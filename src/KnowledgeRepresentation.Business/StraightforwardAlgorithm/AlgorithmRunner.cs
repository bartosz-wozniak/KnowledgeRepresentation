using System.Collections.Generic;
using System.Linq;
using KnowledgeRepresentation.Business.Parsing;

namespace KnowledgeRepresentation.Business.StraightforwardAlgorithm
{
    /// <summary>
    /// Wrapper around the whole algorithm
    /// </summary>
    public static class AlgorithmRunner
    {
        public static (IList<QueryResponse> queryResults, Dictionary<int, SimpleModelSaver> modelsByScenario, ExtractionResult extracted)
            EvaluateQueries(ParseResult.Success parseResult)
        {
            var extracted = IdentifierExtractor.Extract(parseResult.Sentences);
            var results = new List<QueryResponse>();

            var queriesByScenarios = new Dictionary<int, List<Sentence<int>>>();
            foreach (var sentence in extracted.Sentences)
            {
                switch (sentence)
                {
                    case Sentence<int>.ScenarioDefinition scenario:
                        queriesByScenarios.Add(scenario.Name, new List<Sentence<int>>());
                        break;
                    case IQuerySentence<int> query:
                        queriesByScenarios[query.Scenario].Add((Sentence<int>) query);
                        break;
                }
            }

            var modelsByScenario = new Dictionary<int, SimpleModelSaver>();
            foreach (var entry in queriesByScenarios)
            {
                var modelSaver = new SimpleModelSaver();
                var alg = new Algorithm(extracted, entry.Key, entry.Value, modelSaver);
                results.AddRange(alg.FindModels());
                modelsByScenario.Add(entry.Key, modelSaver);
            }

            var queryResults = results
                .OrderBy(r => ((IList<Sentence<int>>) extracted.Sentences).IndexOf(r.Query))
                .ToList();

            return (queryResults, modelsByScenario, extracted);
        }
    }

}