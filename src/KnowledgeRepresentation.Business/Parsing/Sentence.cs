using System.Collections.Generic;

namespace KnowledgeRepresentation.Business.Parsing
{
    public interface IQuerySentence<out TId>
    {
        TId Scenario { get; }
    }

    public abstract class Sentence<TId>
    {
        public sealed class Causes : Sentence<TId>
        {
            public TId Action { get; }
            public int Duration { get; }
            public Expression<TId> Result { get; }
            public Expression<TId> Condition { get; }

            public Causes(TId action, int time, Expression<TId> result, Expression<TId> condition)
            {
                Action = action;
                Duration = time;
                Result = result;
                Condition = condition;
            }
        }

        public sealed class Releases : Sentence<TId>
        {
            public TId Action { get; }
            public int Duration { get; }
            public Expression<TId> Result { get; }
            public Expression<TId> Condition { get; }

            public Releases(TId action, int time, Expression<TId> result, Expression<TId> condition)
            {
                Action = action;
                Duration = time;
                Result = result;
                Condition = condition;
            }
        }

        public sealed class Triggers : Sentence<TId>
        {
            public Expression<TId> Condition { get; }
            public TId Action { get; }

            public Triggers(Expression<TId> condition, TId action)
            {
                Condition = condition;
                Action = action;
            }
        }

        public sealed class Invokes : Sentence<TId>
        {
            public TId Invoker { get; }
            public TId Invokee { get; }
            public int Delay { get; }
            public Expression<TId> Condition { get; }

            public Invokes(TId invoker, TId invokee, int delay, Expression<TId> condition)
            {
                Invoker = invoker;
                Invokee = invokee;
                Delay = delay;
                Condition = condition;
            }
        }

        public sealed class Impossible : Sentence<TId>
        {
            public TId Action { get; }
            public int Time { get; }

            public Impossible(TId action, int time)
            {
                Action = action;
                Time = time;
            }
        }

        public sealed class AlwaysExecutable : Sentence<TId>, IQuerySentence<TId>
        {
            public TId Scenario { get; }

            public AlwaysExecutable(TId scenario)
            {
                Scenario = scenario;
            }
        }

        public sealed class EverExecutable : Sentence<TId>, IQuerySentence<TId>
        {
            public TId Scenario { get; }

            public EverExecutable(TId scenario)
            {
                Scenario = scenario;
            }
        }

        public sealed class AlwaysHolds : Sentence<TId>, IQuerySentence<TId>
        {
            public Expression<TId> Expression { get; }
            public int Time { get; }
            public TId Scenario { get; }

            public AlwaysHolds(Expression<TId> expression, int time, TId scenario)
            {
                Expression = expression;
                Time = time;
                Scenario = scenario;
            }
        }

        public sealed class EverHolds : Sentence<TId>, IQuerySentence<TId>
        {
            public Expression<TId> Expression { get; }
            public int Time { get; }
            public TId Scenario { get; }

            public EverHolds(Expression<TId> expression, int time, TId scenario)
            {
                Expression = expression;
                Time = time;
                Scenario = scenario;
            }
        }

        public sealed class AlwaysOccurs : Sentence<TId>, IQuerySentence<TId>
        {
            public TId Action { get; }
            public int Time { get; }
            public TId Scenario { get; }

            public AlwaysOccurs(TId action, int time, TId scenario)
            {
                Action = action;
                Time = time;
                Scenario = scenario;
            }
        }

        public sealed class EverOccurs : Sentence<TId>, IQuerySentence<TId>
        {
            public TId Action { get; }
            public int Time { get; }
            public TId Scenario { get; }

            public EverOccurs(TId action, int time, TId scenario)
            {
                Action = action;
                Time = time;
                Scenario = scenario;
            }
        }

        public sealed class ScenarioDefinition : Sentence<TId>
        {
            public TId Name { get; }
            public int Time { get; }
            public IReadOnlyList<(TId, int)> Actions { get; }
            public IReadOnlyList<(Expression<TId>, int)> Observations { get; }

            public ScenarioDefinition(TId name, int time, IReadOnlyList<(TId, int)> actions, IReadOnlyList<(Expression<TId>, int)> observations)
            {
                Name = name;
                Time = time;
                Actions = actions;
                Observations = observations;
            }
        }

        private Sentence() { }
    }
}
