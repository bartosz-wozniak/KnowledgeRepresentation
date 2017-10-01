using System;
using System.Collections.Generic;
using System.Linq;

namespace KnowledgeRepresentation.Business.Parsing
{
    using ExprI = Expression<int>;
    using ExprS = Expression<string>;
    using SenI = Sentence<int>;
    using SenS = Sentence<string>;

    public sealed class ExtractionResult
    {
        public IReadOnlyList<SenI> Sentences { get; }
        public IReadOnlyDictionary<string, int> Actions { get; }
        public IReadOnlyDictionary<string, int> Fluents { get; }
        public IReadOnlyDictionary<string, int> Scenarios { get; }

        public ExtractionResult(
            IReadOnlyList<SenI> sentences,
            IReadOnlyDictionary<string, int> actions,
            IReadOnlyDictionary<string, int> fluents,
            IReadOnlyDictionary<string, int> scenarios)
        {
            Sentences = sentences;
            Actions = actions;
            Fluents = fluents;
            Scenarios = scenarios;
        }
    }

    public sealed class IdentifierExtractor
    {
        private readonly Dictionary<string, int> _actions = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _fluents = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _scenarios = new Dictionary<string, int>();

        public static ExtractionResult Extract(IEnumerable<SenS> sentences)
        {
            var extractor = new IdentifierExtractor();
            var rewritten = sentences.Select(extractor.Rewrite).ToList();
            return new ExtractionResult(rewritten, extractor._actions, extractor._fluents, extractor._scenarios);
        }

        private SenI Rewrite(SenS sen)
        {
            switch (sen)
            {
                case SenS.Causes c:
                    return new SenI.Causes(_actions.GetOrAddNext(c.Action), c.Duration, Rewrite(c.Result), Rewrite(c.Condition));

                case SenS.Releases r:
                    return new SenI.Releases(_actions.GetOrAddNext(r.Action), r.Duration, Rewrite(r.Result), Rewrite(r.Condition));

                case SenS.Triggers t:
                    return new SenI.Triggers(Rewrite(t.Condition), _actions.GetOrAddNext(t.Action));

                case SenS.Invokes i:
                    return new SenI.Invokes(_actions.GetOrAddNext(i.Invoker), _actions.GetOrAddNext(i.Invokee), i.Delay, Rewrite(i.Condition));

                case SenS.Impossible i:
                    return new SenI.Impossible(_actions.GetOrAddNext(i.Action), i.Time);

                case SenS.AlwaysExecutable e:
                    return new SenI.AlwaysExecutable(_scenarios.GetOrAddNext(e.Scenario));

                case SenS.EverExecutable e:
                    return new SenI.EverExecutable(_scenarios.GetOrAddNext(e.Scenario));

                case SenS.AlwaysHolds h:
                    return new SenI.AlwaysHolds(Rewrite(h.Expression), h.Time, _scenarios.GetOrAddNext(h.Scenario));

                case SenS.EverHolds h:
                    return new SenI.EverHolds(Rewrite(h.Expression), h.Time, _scenarios.GetOrAddNext(h.Scenario));

                case SenS.AlwaysOccurs o:
                    return new SenI.AlwaysOccurs(_actions.GetOrAddNext(o.Action), o.Time, _scenarios.GetOrAddNext(o.Scenario));

                case SenS.EverOccurs o:
                    return new SenI.EverOccurs(_actions.GetOrAddNext(o.Action), o.Time, _scenarios.GetOrAddNext(o.Scenario));

                case SenS.ScenarioDefinition d:
                    {
                        var name = _scenarios.GetOrAddNext(d.Name);
                        var acs = d.Actions.Select(a => (_actions.GetOrAddNext(a.Item1), a.Item2)).ToArray();
                        var obs = d.Observations.Select(o => (Rewrite(o.Item1), o.Item2)).ToArray();
                        return new SenI.ScenarioDefinition(name, d.Time, acs, obs);
                    }
            }
            throw new InvalidOperationException("Impossible");
        }

        private ExprI Rewrite(ExprS expr)
        {
            switch (expr)
            {
                case ExprS.Identifier i:
                    return new ExprI.Identifier(_fluents.GetOrAddNext(i.Name));

                case ExprS.True _:
                    return new ExprI.True();

                case ExprS.False _:
                    return new ExprI.False();

                case ExprS.Not n:
                    return new ExprI.Not(Rewrite(n.Expression));

                case ExprS.And a:
                    return new ExprI.And(Rewrite(a.Left), Rewrite(a.Right));

                case ExprS.Or o:
                    return new ExprI.Or(Rewrite(o.Left), Rewrite(o.Right));
            }
            throw new InvalidOperationException("Impossible");
        }
    }

    internal static class DictionaryExtensions
    {
        public static int GetOrAddNext(this Dictionary<string, int> dict, string key)
        {
            if (dict.TryGetValue(key, out var val))
            {
                return val;
            }
            dict.Add(key, dict.Count);
            return dict.Count - 1;
        }
    }
}
