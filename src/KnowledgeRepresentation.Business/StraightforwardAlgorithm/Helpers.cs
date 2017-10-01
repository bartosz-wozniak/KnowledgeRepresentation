using System;
using System.Collections.Generic;
using System.Linq;
using KnowledgeRepresentation.Business.Parsing;

namespace KnowledgeRepresentation.Business.StraightforwardAlgorithm
{
    using Expression = Expression<int>;
    using Sentence = Sentence<int>;

    public class ModelState
    {
        public int Time { get; set; }
        public bool[] Fluents { get; set; }
        public List<int> OcclussionFluents { get; set; }
        public ActionState Action { get; set; }
    }

    public class ActionState
    {
        public int ActionId { get; set; }
        public int StartTime { get; set; }
        public List<ActionResult> Results { get; set; }

        public ActionState Clone()
        {
            return new ActionState()
            {
                ActionId = ActionId,
                StartTime = StartTime,
                Results = new List<ActionResult>(Results)
            };
        }
    }

    public class ActionResult
    {
        public int Delay { get; set; }
        public Expression Result { get; set; }
    }

    public static class ExpressionHelper
    {
        public static bool IsExpressionTrue(Expression expression, bool[] fluents)
        {
            switch (expression)
            {
                case Expression<int>.And and:
                    return IsExpressionTrue(and.Left, fluents) && IsExpressionTrue(and.Right, fluents);
                case Expression<int>.Or or:
                    return IsExpressionTrue(or.Left, fluents) || IsExpressionTrue(or.Right, fluents);
                case Expression<int>.Not not:
                    return !IsExpressionTrue(not.Expression, fluents);
                case Expression<int>.Identifier id:
                    return fluents[id.Name];
                case Expression<int>.True _:
                    return true;
                case Expression<int>.False _:
                    return false;
            }
            throw new ArgumentException("Invalid Expression Type");
        }

        public static List<int> GetAllFluentsInExpression(Expression expression)
        {
            List<int> fluents = new List<int>();
            GetAllFluentsInExpression(expression, fluents);
            return fluents.Distinct().ToList();
        }

        private static void GetAllFluentsInExpression(Expression expression, List<int> fluents)
        {
            switch (expression)
            {
                case Expression<int>.And and:
                    GetAllFluentsInExpression(and.Left, fluents);
                    GetAllFluentsInExpression(and.Right, fluents);
                    return;
                case Expression<int>.Or or:
                    GetAllFluentsInExpression(or.Left, fluents);
                    GetAllFluentsInExpression(or.Right, fluents);
                    return;
                case Expression<int>.Not not:
                    GetAllFluentsInExpression(not.Expression, fluents);
                    return;
                case Expression<int>.Identifier id:
                    fluents.Add(id.Name);
                    return;
                case Expression<int>.True _:
                    return;
                case Expression<int>.False _:
                    return;
            }
            throw new ArgumentException("Invalid Expression Type");
        }
    }

    public static class ActionHelper
    {
        public static (bool, ActionState) CanActionBePerformed(IReadOnlyList<Sentence> sentences, int action, bool[] fluents, int time, int scenarioEndTime)
        {
            if (sentences.OfType<Sentence.Impossible>().Any(s => s.Action == action && s.Time == time))
                return (false, null); //zdanie impossible blokuje
            var results = new List<ActionResult>();
            foreach (var causes in sentences.OfType<Sentence.Causes>().Where(s => s.Action == action))
            {
                if (ExpressionHelper.IsExpressionTrue(causes.Condition, fluents))
                    results.Add(new ActionResult() { Delay = causes.Duration, Result = causes.Result });
            }
            foreach (var releases in sentences.OfType<Sentence.Releases>().Where(s => s.Action == action))
            {
                if (ExpressionHelper.IsExpressionTrue(releases.Condition, fluents))
                    results.Add(new ActionResult() { Delay = releases.Duration, Result = new Expression.Or(releases.Result, new Expression.Not(releases.Result)) });
                //TODO: think it over!!!
            }

            if (!results.Any())
                return (false, null); //żadne cause ani release nie pasuje - akcja się nie wykona
            return (true, new ActionState { StartTime = time, Results = results, ActionId = action }); //akcja może się zacząć i możliwe są akie skutki.
        }

        public static List<int> CheckTriggerSentences(IReadOnlyList<Sentence> sentences, bool[] fluents)
        {
            List<int> triggeredActions = new List<int>();
            foreach (var triggers in sentences.OfType<Sentence.Triggers>())
            {
                if (ExpressionHelper.IsExpressionTrue(triggers.Condition, fluents))
                    triggeredActions.Add(triggers.Action);
            }
            return triggeredActions;
        }

        //TODO: endtime!!
        public static List<(int, int)> FindInvokedActions(ExtractionResult result, int action, int actionDuration, bool[] fluents, int time)
        {
            return (from invokes in result.Sentences.OfType<Sentence.Invokes>().Where(s => s.Invoker == action) where ExpressionHelper.IsExpressionTrue(invokes.Condition, fluents) select (invokes.Invokee, invokes.Delay + actionDuration + time)).ToList();
        }

        //Returns id of action to be invoked. -1 if no action should be invoked. null if model is contradictory
        public static int? GetActionToStart(ExtractionResult extractionResult, int scenarioId, int time, bool[] fluents, Dictionary<int, int> invokedActions)
        {
            var actionToInvoke = -1;
            //get actions from scenario
            var actions = ScenarioHelper.GetActionsFromScenario(extractionResult, scenarioId, time);
            //get triggered actions
            var triggeredActions = ActionHelper.CheckTriggerSentences(extractionResult.Sentences, fluents);
            //get invoked action 
            if (!invokedActions.TryGetValue(time, out var invokedActionId))
                invokedActionId = -1;

            if (actions.Count > 1) return null; //error
            if (actions.Any())
                actionToInvoke = actions.FirstOrDefault();

            if (triggeredActions.Count > 1) return null; //error
            if (triggeredActions.Any())
                if (actionToInvoke < 0)
                    actionToInvoke = triggeredActions.FirstOrDefault();
                else if (actionToInvoke != triggeredActions.FirstOrDefault())//still it can be the same action!!!
                    return null; //error - two actions possible 

            if (invokedActionId >= 0)
                if (actionToInvoke < 0)
                    actionToInvoke = invokedActionId;
                else if (actionToInvoke != invokedActionId) //still it can be the same action!!!
                    return null; //error - two actions possible
            return actionToInvoke;
        }
    }

    public static class FluentsHelper
    {
        public static IEnumerable<bool[]> InitializeFluents(int count)
        {
            bool[] fluents = new bool[count];
            foreach (var f in FillFluentsTable(fluents, 0))
                yield return f;
        }

        private static IEnumerable<bool[]> FillFluentsTable(bool[] fluents, int current)
        {
            if (current == fluents.Length)
            {
                yield return (bool[])fluents.Clone();
                yield break;
            }
            fluents[current] = true;
            foreach (var f in FillFluentsTable(fluents, current + 1))
                yield return f;
            fluents[current] = false;
            foreach (var f in FillFluentsTable(fluents, current + 1))
                yield return f;
        }

        public static IEnumerable<bool[]> InitializeFluentsWithOcclusion(bool[] parentFluents, List<int> occlusion)
        {
            List<int> remaining = new List<int>(occlusion);
            bool[] fluents = (bool[])parentFluents.Clone();
            foreach (var fl in FillFluentsTable(fluents, remaining))
                yield return fl;
        }

        private static IEnumerable<bool[]> FillFluentsTable(bool[] fluents, List<int> remaining)
        {
            if (!remaining.Any())
            {
                yield return (bool[])fluents.Clone();
                yield break;
            }
            int fl = remaining.FirstOrDefault();
            remaining.Remove(fl);
            fluents[fl] = true;
            foreach (var f in FillFluentsTable(fluents, remaining))
                yield return f;
            fluents[fl] = false;
            foreach (var f in FillFluentsTable(fluents, remaining))
                yield return f;
            remaining.Add(fl);
        }

        public static List<int> GetFluentsInOcclusion(ActionState actionState)
        {
            var list = new List<int>();
            foreach (var actionStateResult in actionState.Results)
            {
                list.AddRange(ExpressionHelper.GetAllFluentsInExpression(actionStateResult.Result));
            }
            return list.Distinct().ToList();
        }
    }

    public static class ObservationHelper
    {
        public static bool ValidateModelWithObservation(bool[] fluents, int time, ExtractionResult result, int scenarioId)
        {
            var scenario = result.Sentences.OfType<Sentence.ScenarioDefinition>().FirstOrDefault(s => s.Name == scenarioId);
            if (scenario == null) return false;
            foreach (var scenarioObservation in scenario.Observations.Where(o => o.Item2 == time))
            {
                if (!ExpressionHelper.IsExpressionTrue(scenarioObservation.Item1, fluents))
                    return false;
            }
            return true;
        }
    }

    public static class ScenarioHelper
    {
        public static int GetScenarioEndTime(ExtractionResult result, int scenarioId)
        {
            var scenario = result.Sentences.OfType<Sentence.ScenarioDefinition>().FirstOrDefault(s => s.Name == scenarioId);
            if (scenario == null)
                throw new Exception("Scenario not found");
            return scenario.Time;
        }

        public static List<int> GetActionsFromScenario(ExtractionResult result, int scenarioId, int time)
        {
            var scenario = result.Sentences.OfType<Sentence.ScenarioDefinition>().FirstOrDefault(s => s.Name == scenarioId);
            if (scenario == null)
                throw new Exception("Scenario not found");
            return scenario.Actions.Where(a => a.Item2 == time).Select(a => a.Item1).ToList();
        }
    }

    public class Algorithm
    {
        private readonly ExtractionResult _extractionResult;
        private readonly int _scenarioId;
        private readonly IReadOnlyList<Sentence> _queries;
        private readonly int _endTime;
        private readonly Dictionary<int, int> _invokedActions; //key - time, value - actionId
        private readonly IModelSaver _modelSaver;
        private IList<QueryExecutor> _queryExecutors;

        public Algorithm(ExtractionResult result, int scenarioId, IReadOnlyList<Sentence> queries, IModelSaver modelSaver)
        {
            _extractionResult = result;
            _scenarioId = scenarioId;
            _queries = queries;
            _endTime = ScenarioHelper.GetScenarioEndTime(result, scenarioId);
            _invokedActions = new Dictionary<int, int>();
            _modelSaver = modelSaver;
        }

        public IList<QueryResponse> FindModels()
        {
            Stack<ModelState> stack = new Stack<ModelState>();
            var initialFluentValues = FluentsHelper.InitializeFluents(_extractionResult.Fluents.Count).ToList();

            initialFluentValues.RemoveAll(
                x => !ObservationHelper.ValidateModelWithObservation(x, 0, _extractionResult, _scenarioId));

            _queryExecutors = QueriesHelper.CreateQueryExecutors(_queries, initialFluentValues, _scenarioId);

            foreach (var fluents in initialFluentValues)
            {
                ProcessModel(new ModelState
                {
                    Time = 0,
                    Fluents = fluents,
                    Action = null,
                    OcclussionFluents = null
                }, stack);
                stack.Clear();
            }
            return _queryExecutors.Select(x => x.GetResponse()).ToList();
        }

        private bool ProcessModel(ModelState state, Stack<ModelState> stack)
        {
            //Validate model with observations from scenario
            if (!ObservationHelper.ValidateModelWithObservation(state.Fluents, state.Time, _extractionResult, _scenarioId))
                return false;
            List<int> occlussionInNextMoment = new List<int>();
            //Check actions
            if (state.Action != null) //action is invoking
            {
                //Find all matching action results
                var results = state.Action.Results.Where(r => r.Delay + state.Action.StartTime == state.Time);
                foreach (var actionResult in results.ToList())
                {
                    //check if result is satisfied
                    if (!ExpressionHelper.IsExpressionTrue(actionResult.Result, state.Fluents))
                        return false;
                    state.Action.Results.Remove(actionResult);
                }
                if (!state.Action.Results.Any()) //all results have been applied -> action finish
                    state.Action = null;
                else
                    occlussionInNextMoment = FluentsHelper.GetFluentsInOcclusion(state.Action); //set occlusion
            }

            //there is a method ActionHelper.GetActionToBePerformed. Use it!
            var actionId = ActionHelper.GetActionToStart(_extractionResult, _scenarioId, state.Time, state.Fluents, _invokedActions);
            if (!actionId.HasValue || (actionId >= 0 && state.Action != null))
                return false; //error - multiple action to invoke

            if (actionId.Value >= 0) //start new action
            {
                var results = ActionHelper.CanActionBePerformed(_extractionResult.Sentences, actionId.Value,
                    state.Fluents, state.Time, _endTime);
                if (results.Item1)
                {
                    state.Action = results.Item2; //start action
                    occlussionInNextMoment = FluentsHelper.GetFluentsInOcclusion(state.Action);
                    var actionDuration = results.Item2.Results.Max(x => x.Delay);
                    var invoked = ActionHelper.FindInvokedActions(_extractionResult, actionId.Value, actionDuration,
                        state.Fluents, state.Time);
                    foreach (var i in invoked)
                    {
                        if (!_invokedActions.TryGetValue(i.Item2, out int a))
                            _invokedActions.Add(i.Item2, i.Item1);
                        else
                        {
                            if (a != i.Item1) return false; //error - to many invokes
                        }
                    }
                }
                else return false; //powinniśmy wykonać akcję, ale nie udało się
            }

            stack.Push(state);
            //Generate children - only fluents in occlusion can differ!
            if (state.Time < _endTime)
            {
                //if occlusion is empty - nothing changes it is only one child
                if (!occlussionInNextMoment.Any())
                {
                    var r = ProcessModel(new ModelState()
                    {
                        Fluents = state.Fluents,
                        OcclussionFluents = occlussionInNextMoment,
                        Time = state.Time + 1,
                        Action = state.Action?.Clone()
                    }, stack);
                    if (r)
                        stack.Pop();
                    return r;
                }
                foreach (var fluents in FluentsHelper.InitializeFluentsWithOcclusion(state.Fluents, occlussionInNextMoment))
                {
                    var r = ProcessModel(new ModelState()
                    {
                        Fluents = fluents,
                        Time = state.Time + 1,
                        OcclussionFluents = occlussionInNextMoment,
                        Action = state.Action?.Clone()
                    }, stack);
                    if (r)
                        stack.Pop();
                }
                return true;
            }
            else
            {
                var modelStack = new Stack<ModelState>(stack.ToArray());
                ValidModel model = new ValidModel(modelStack, _extractionResult.Fluents.Count);
                foreach (var queryExecutor in _queryExecutors)
                {
                    queryExecutor.CheckModel(model);
                }
                _modelSaver?.SaveModel(model);
                return true;
            }
        }
    }

    /// <summary>
    /// Interface used to remember valid models.
    /// </summary>
    public interface IModelSaver
    {
        void SaveModel(ValidModel model);
    }

    public class SimpleModelSaver : IModelSaver
    {
        // we need to limit the number of saved models, even for simple cases there may be too many of them
        // (skating example - 2048 models)
        public const int MaxModels = 10;

        public IList<ValidModel> SavedModels { get; } = new List<ValidModel>();
        public int TotalModelsCount { get; private set; } = 0;

        public void SaveModel(ValidModel model)
        {
            TotalModelsCount++;
            if (SavedModels.Count < MaxModels)
            {
                SavedModels.Add(model);
            }
        }
    }

    public class ValidModel
    {
        public bool[][] History { get; }
        public List<int>[] Occlusion { get; }
        public int[] Actions { get; }
        public int EndTime { get; set; }

        public ValidModel(Stack<ModelState> modelStack, int numberOfFluents)
        {
            EndTime = modelStack.Count;
            History = new bool[EndTime][];
            for (int i = 0; i < EndTime; i++)
                History[i] = new bool[numberOfFluents];
            Occlusion = new List<int>[EndTime];
            Actions = new int[EndTime];
            while (modelStack.Count > 0)
            {
                var m = modelStack.Pop();
                for (int i = 0; i < numberOfFluents; i++)
                    History[m.Time][i] = m.Fluents[i];
                Occlusion[m.Time] = m.OcclussionFluents;
                Actions[m.Time] = m.Action?.ActionId ?? -1;
            }
        }
    }

    public class QueryResponse
    {
        public Sentence Query { get; }
        public bool Response { get; }

        public QueryResponse(Sentence query, bool response)
        {
            Query = query;
            Response = response;
        }
    }

    public abstract class QueryExecutor
    {
        public abstract void CheckModel(ValidModel model);

        public abstract QueryResponse GetResponse();
    }

    internal abstract class QueryExecutor<TSentence> : QueryExecutor
        where TSentence : Sentence
    {
        public virtual bool Response { get; protected set; }
        public TSentence Query { get; }
        protected QueryExecutor(TSentence sentence)
        {
            Query = sentence;
        }

        public override QueryResponse GetResponse()
        {
            return new QueryResponse(Query, Response);
        }
    }

    internal class EverExecutableQueryExecutor : QueryExecutor<Sentence.EverExecutable>
    {
        internal EverExecutableQueryExecutor(Sentence.EverExecutable query)
            : base(query)
        {
        }

        public override void CheckModel(ValidModel model)
        {
            Response = true;
        }
    }

    internal class AlwaysExecutableQueryExecutor : QueryExecutor<Sentence.AlwaysExecutable>
    {
        private readonly Dictionary<bool[], bool> _initialCorrectFluentsValues;

        internal AlwaysExecutableQueryExecutor(Sentence.AlwaysExecutable query, List<bool[]> initialFluentValues) : base(query)
        {
            _initialCorrectFluentsValues = new Dictionary<bool[], bool>(new FluentsComparer());
            initialFluentValues.ForEach(x => _initialCorrectFluentsValues.Add(x, false));
        }

        public override void CheckModel(ValidModel model)
        {
            var initialModelFluents = model.History[0];
            if (_initialCorrectFluentsValues.ContainsKey(initialModelFluents))
                _initialCorrectFluentsValues[initialModelFluents] = true;
        }

        public override bool Response => _initialCorrectFluentsValues.All(x => x.Value);

        private class FluentsComparer : IEqualityComparer<bool[]>
        {
            public bool Equals(bool[] x, bool[] y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(bool[] obj)
            {
                var hash = 17;
                foreach (var value in obj)
                {
                    hash = hash * 23 + value.GetHashCode();
                }
                return hash;
            }
        }
    }

    internal class EverHoldsQueryExecutor : QueryExecutor<Sentence.EverHolds>
    {
        internal EverHoldsQueryExecutor(Sentence.EverHolds query)
            : base(query)
        {
        }

        public override void CheckModel(ValidModel model)
        {
            if (ExpressionHelper.IsExpressionTrue(Query.Expression, model.History[Query.Time]))
                Response = true;
        }
    }

    internal class AlwaysHoldsQueryExecutor : QueryExecutor<Sentence.AlwaysHolds>
    {
        private bool _response;

        internal AlwaysHoldsQueryExecutor(Sentence.AlwaysHolds query)
            : base(query)
        {
            _response = true;
        }

        public override void CheckModel(ValidModel model)
        {
            if (!ExpressionHelper.IsExpressionTrue(Query.Expression, model.History[Query.Time]))
                Response = false;
        }

        public override bool Response
        {
            get => _response;
            protected set => _response = value;
        }
    }

    internal class EverOccursQueryExecutor : QueryExecutor<Sentence.EverOccurs>
    {
        internal EverOccursQueryExecutor(Sentence.EverOccurs query)
            : base(query)
        {
        }

        public override void CheckModel(ValidModel model)
        {
            if (model.Actions[Query.Time] == Query.Action)
                Response = true;
        }
    }

    internal class AlwaysOccursQueryExecutor : QueryExecutor<Sentence.AlwaysOccurs>
    {
        private bool _response;

        internal AlwaysOccursQueryExecutor(Sentence.AlwaysOccurs query)
            : base(query)
        {
            _response = true;
        }

        public override void CheckModel(ValidModel model)
        {
            if (model.Actions[Query.Time] != Query.Action)
                Response = false;
        }

        public override bool Response
        {
            get => _response;
            protected set => _response = value;
        }
    }

    public static class QueriesHelper
    {
        public static QueryExecutor CreateQueryExecutor(Sentence query, List<bool[]> initialFluentValues, int scenarioId)
        {
            switch (query)
            {
                case Sentence.EverExecutable s when s.Scenario == scenarioId:
                    return new EverExecutableQueryExecutor(s);
                case Sentence.AlwaysExecutable s when s.Scenario == scenarioId:
                    return new AlwaysExecutableQueryExecutor(s, initialFluentValues);
                case Sentence.EverHolds s when s.Scenario == scenarioId:
                    return new EverHoldsQueryExecutor(s);
                case Sentence.AlwaysHolds s when s.Scenario == scenarioId:
                    return new AlwaysHoldsQueryExecutor(s);
                case Sentence.EverOccurs s when s.Scenario == scenarioId:
                    return new EverOccursQueryExecutor(s);
                case Sentence.AlwaysOccurs s when s.Scenario == scenarioId:
                    return new AlwaysOccursQueryExecutor(s);
                default:
                    return null;
            }
        }

        public static IList<QueryExecutor> CreateQueryExecutors(IReadOnlyList<Sentence> sentences,
            List<bool[]> initialFluentValues, int scenarioId)
        {
            var list = new List<QueryExecutor>();
            foreach (var sentence in sentences)
            {
                var exec = CreateQueryExecutor(sentence, initialFluentValues, scenarioId);
                if (exec != null)
                    list.Add(exec);
            }
            return list;
        }
    }
}
