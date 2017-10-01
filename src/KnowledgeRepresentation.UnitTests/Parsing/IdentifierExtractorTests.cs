using KnowledgeRepresentation.Business.Parsing;
using Xunit;

namespace KnowledgeRepresentation.UnitTests.Parsing
{
    using ExprI = Expression<int>;
    using SenI = Sentence<int>;

    public class IdentifierExtractorTests
    {
        [Fact]
        public void Extracts_actions_from_sentences()
        {
            var result = Extract("action1 lasts 1 causes a\naction2 lasts 2 releases b");

            Assert.Equal(2, result.Actions.Count);
            Assert.Contains("action1", result.Actions.Keys);
            Assert.Contains("action2", result.Actions.Keys);

            Assert.Equal(2, result.Sentences.Count);

            Assert.Equal(result.Actions["action1"], ((SenI.Causes)result.Sentences[0]).Action);
            Assert.Equal(result.Actions["action2"], ((SenI.Releases)result.Sentences[1]).Action);
        }

        [Fact]
        public void Unifies_actions()
        {
            var result = Extract("action1 lasts 1 causes a\naction1 lasts 2 releases b");

            Assert.Equal(1, result.Actions.Count);
            Assert.Contains("action1", result.Actions.Keys);

            Assert.Equal(2, result.Sentences.Count);

            Assert.Equal(result.Actions["action1"], ((SenI.Causes)result.Sentences[0]).Action);
            Assert.Equal(result.Actions["action1"], ((SenI.Releases)result.Sentences[1]).Action);
        }

        [Fact]
        public void Extracts_scenarios_from_sentences()
        {
            var result = Extract("ever executable sc1\nalways executable sc2");

            Assert.Equal(2, result.Scenarios.Count);
            Assert.Contains("sc1", result.Scenarios.Keys);
            Assert.Contains("sc2", result.Scenarios.Keys);

            Assert.Equal(2, result.Sentences.Count);

            Assert.Equal(result.Scenarios["sc1"], ((SenI.EverExecutable)result.Sentences[0]).Scenario);
            Assert.Equal(result.Scenarios["sc2"], ((SenI.AlwaysExecutable)result.Sentences[1]).Scenario);
        }

        [Fact]
        public void Unifies_scenarios()
        {
            var result = Extract("ever executable sc1\nalways executable sc1");

            Assert.Equal(1, result.Scenarios.Count);
            Assert.Contains("sc1", result.Scenarios.Keys);

            Assert.Equal(2, result.Sentences.Count);

            Assert.Equal(result.Scenarios["sc1"], ((SenI.EverExecutable)result.Sentences[0]).Scenario);
            Assert.Equal(result.Scenarios["sc1"], ((SenI.AlwaysExecutable)result.Sentences[1]).Scenario);
        }

        [Fact]
        public void Extracts_fluents_from_expressions()
        {
            var result = Extract("action1 lasts 1 causes a | b\naction2 lasts 2 releases c & d");

            Assert.Equal(4, result.Fluents.Count);
            Assert.Contains("a", result.Fluents.Keys);
            Assert.Contains("b", result.Fluents.Keys);
            Assert.Contains("c", result.Fluents.Keys);
            Assert.Contains("d", result.Fluents.Keys);

            Assert.Equal(2, result.Sentences.Count);

            Assert.Equal(result.Fluents["a"], ((ExprI.Identifier)((ExprI.Or)((SenI.Causes)result.Sentences[0]).Result).Left).Name);
            Assert.Equal(result.Fluents["b"], ((ExprI.Identifier)((ExprI.Or)((SenI.Causes)result.Sentences[0]).Result).Right).Name);
            Assert.Equal(result.Fluents["c"], ((ExprI.Identifier)((ExprI.And)((SenI.Releases)result.Sentences[1]).Result).Left).Name);
            Assert.Equal(result.Fluents["d"], ((ExprI.Identifier)((ExprI.And)((SenI.Releases)result.Sentences[1]).Result).Right).Name);
        }

        [Fact]
        public void Unifies_fluents_in_expressions()
        {
            var result = Extract("action1 lasts 1 causes a | b\naction2 lasts 2 releases b & a");

            Assert.Equal(2, result.Actions.Count);
            Assert.Contains("a", result.Fluents.Keys);
            Assert.Contains("b", result.Fluents.Keys);

            Assert.Equal(2, result.Sentences.Count);

            Assert.Equal(result.Fluents["a"], ((ExprI.Identifier)((ExprI.Or)((SenI.Causes)result.Sentences[0]).Result).Left).Name);
            Assert.Equal(result.Fluents["b"], ((ExprI.Identifier)((ExprI.Or)((SenI.Causes)result.Sentences[0]).Result).Right).Name);
            Assert.Equal(result.Fluents["b"], ((ExprI.Identifier)((ExprI.And)((SenI.Releases)result.Sentences[1]).Result).Left).Name);
            Assert.Equal(result.Fluents["a"], ((ExprI.Identifier)((ExprI.And)((SenI.Releases)result.Sentences[1]).Result).Right).Name);
        }

        private static ExtractionResult Extract(string input)
        {
            var result = Parser.Parse(input);
            var succ = Assert.IsType<ParseResult.Success>(result);
            return IdentifierExtractor.Extract(succ.Sentences);
        }
    }
}
