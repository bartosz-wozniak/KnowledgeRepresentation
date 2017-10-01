using System.Collections.Generic;
using System.Linq;
using KnowledgeRepresentation.Business.Models;
using KnowledgeRepresentation.Business.Parsing;

namespace KnowledgeRepresentation.Business.Algorithm
{
    using SenI = Sentence<int>;
    using Scenario = Sentence<int>.ScenarioDefinition;
    using Expression = Expression<int>;

    public class ActionEffect
    {
        public Expression Expression { get; set; }
        public int Time { get; set; }
    }

    internal class ActionExecution
    {
        public int Action { get; set; } = -1;
        public int StartTime { get; set; } = -1;
        public List<ActionEffect> Effects { get; set; } = new List<ActionEffect>();
    }

    public class ModelsBuilder
    {
        private readonly ActionsToPerform _actionsToPerform = new ActionsToPerform();
        private readonly IReadOnlyCollection<SenI> _sentences;
        private readonly Scenario _scenario;
        public static int FluentsCount;
        private ActionExecution _currentAction;

        public ModelsBuilder(IReadOnlyCollection<SenI> sentences, Scenario scenario, int fluentsCount)
        {
            _sentences = sentences;
            _scenario = scenario;
            FluentsCount = fluentsCount;
            //fill in actionToPerform
            _actionsToPerform.Observations = new List<Expression>[scenario.Time + 1];
            _actionsToPerform.Actions = new List<int>[scenario.Time + 1];
            for (var i = 0; i <= scenario.Time; i++)
            {
                _actionsToPerform.Actions[i] = new List<int>();
                _actionsToPerform.Observations[i] = new List<Expression>();
            }
            foreach (var scenarioAction in scenario.Actions)
            {
                _actionsToPerform.Actions[scenarioAction.Item2].Add(scenarioAction.Item1);
            }
            foreach (var scenarioObservation in scenario.Observations)
            {
                _actionsToPerform.Observations[scenarioObservation.Item2].Add(scenarioObservation.Item1);
            }
        }

        public State Build()
        {
            var root = new State
            {
                CurrentTime = -1,
                Fluents = new FluentValue[FluentsCount]
            };
            ProcessState(root);
            return root;
        }

        private bool ProcessState(State s)
        {
            if (s.CurrentTime > _scenario.Time)
                return false;
            var result = FillState(s);
            if (!result)
                return false;
            if (s.CurrentTime == _scenario.Time)
                return true;
            var branches = FindBranches(s);
            foreach (var branch in branches)
            {
                if (ProcessState(branch))
                    s.Children.Add(branch);
            }
            return s.Children.Count > 0;
        }

        private IList<State> FindBranches(State s)
        {
            var branches = new List<State>();
            var exp = _actionsToPerform.Observations[s.CurrentTime + 1].ToList();
            var effect =
                _currentAction?.Effects.FirstOrDefault(e => e.Time == s.CurrentTime + 1 - _currentAction.StartTime); //check
            if (effect != null)
            {
                exp.Add(effect.Expression);
            }
            if (exp.Any())
            {
                //get conjunction    
                var fullExp = exp[0];
                for (var i = 1; i < exp.Count; i++)
                    fullExp = new Expression.And(fullExp, exp[i]);
                //convert expression to branches
                //get all true evaluation -> foreach generate new state
                var list = ExpressionMethods.GetAllFluentEvaluationsSatisfyingExpression(fullExp);
                branches.AddRange(list.Select(fluentValues => new State
                {
                    CurrentTime = s.CurrentTime + 1,
                    Fluents = fluentValues,
                    CurrentAction = _currentAction?.Action
                }));
            }
            else
            {
                //fluents are derieved from parent
                branches.Add(new State { CurrentTime = s.CurrentTime + 1, Fluents = s.Fluents, CurrentAction = _currentAction?.Action});
            }
            return branches;
        }

        private bool FillState(State s)
        {
            if (s.CurrentTime < 0)
                return true; //accept the root artificial state at -1 time.
            if (_currentAction != null)
            {
                if (_currentAction.StartTime + (_currentAction.Effects.Any() ? _currentAction.Effects.Max(e => e.Time) : 0) == s.CurrentTime)
                {
                    _currentAction = null; //finish action
                }
                else
                {
                    //set dependent fluents to occlusion
                    var expressions = _currentAction.Effects
                        .Where(e => e.Time + _currentAction.StartTime > s.CurrentTime).Select(e => e.Expression);
                    //foreach exp get all fluents to single list
                    //set each fluent from the list to occlusion
                    var fluents = new List<int>();
                    foreach (var expression in expressions)
                    {
                        fluents.AddRange(ExpressionMethods.FindAllFluentsInExpression(expression));
                    }
                    foreach (var i in fluents.Distinct())
                    {
                        s.Fluents[i] = FluentValue.UnderOcclusion;
                    }
                }
            }
            //add triggers for state check all triggers sentence condition
            foreach (var triggers in _sentences.Where(sen => sen is SenI.Triggers).Cast<SenI.Triggers>())
            {
                if (triggers.Condition == null ||
                    ExpressionMethods.CheckExpressionSatisfaction(triggers.Condition, s.Fluents) == FluentValue.True)
                {
                    _actionsToPerform.Actions[s.CurrentTime].Add(triggers.Action);
                }
            }
            //add releases observation
            foreach (var releases in _sentences.Where(sen => sen is SenI.Releases).Cast<SenI.Releases>())
            {
                if (releases.Condition != null &&
                    ExpressionMethods.CheckExpressionSatisfaction(releases.Condition, s.Fluents) !=
                    FluentValue.True) continue;
                if (releases.Duration + s.CurrentTime <= _scenario.Time)
                    _actionsToPerform.Observations[releases.Duration + s.CurrentTime].Add(releases.Result);
            }
            if (GetActionsForTime(s.CurrentTime, out var action) && action > 0)
            {
                //start action
                _currentAction = new ActionExecution
                {
                    Action = action,
                    StartTime = s.CurrentTime
                };
                foreach (var causes in _sentences.OfType<SenI.Causes>().Where(se => se.Action == action))
                {
                    if(ExpressionMethods.CheckExpressionSatisfaction(causes.Condition, s.Fluents) != FluentValue.True)
                        continue;
                    _currentAction.Effects.Add(new ActionEffect
                    {
                        Expression = causes.Result,
                        Time = causes.Duration
                    });
                }
                //invokes
                foreach (var invokes in _sentences.OfType<SenI.Invokes>().Where(i => i.Invoker == action &&
                  (i.Condition == null || ExpressionMethods.CheckExpressionSatisfaction(i.Condition, s.Fluents) == FluentValue.True)))
                {
                    var time = _currentAction.StartTime + _currentAction.Effects.Max(e => e.Time) + invokes.Delay;
                    if (time <= _scenario.Time)
                        _actionsToPerform.Actions[time].Add(invokes.Invokee);
                }
            }
            //check state!
            //check observations
            var obs = _actionsToPerform.Observations[s.CurrentTime];
            if (obs?.Any(o => ExpressionMethods.CheckExpressionSatisfaction(o, s.Fluents) != FluentValue.True) ?? false)
                return false; //model contradictory!
            //check actions
            var acs = _actionsToPerform.Actions[s.CurrentTime] ?? new List<int>();
            return acs.Count <= 1 || acs[0] == _currentAction.Action;
        }

        private bool GetActionsForTime(int time, out int action)
        {
            var list = _actionsToPerform.Actions[time];
            action = list.Any() ? list.FirstOrDefault() : -1;
            return true;
        }
    }
}
