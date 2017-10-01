using KnowledgeRepresentation.Business.Parsing;
using Xunit;

namespace KnowledgeRepresentation.UnitTests.Parsing
{
    using Expression = Expression<string>;
    using Sentence = Sentence<string>;

    public class ParserTests
    {
        [Fact]
        public void Parses_full_causes_sentence()
        {
            var result = Parse<Sentence.Causes>("action lasts 5 causes true if true");

            Assert.Equal("action", result.Action);
            Assert.Equal(5, result.Duration);
        }

        [Fact]
        public void Parses_causes_sentence_without_if_clause()
        {
            var result = Parse<Sentence.Causes>("action lasts 5 causes true");

            Assert.Equal("action", result.Action);
            Assert.Equal(5, result.Duration);
            Assert.IsType<Expression.True>(result.Condition);
        }

        [Fact]
        public void Parses_full_releases_sentence()
        {
            var result = Parse<Sentence.Releases>("action lasts 3 releases a if true");

            Assert.Equal("action", result.Action);
            Assert.Equal(3, result.Duration);
        }

        [Fact]
        public void Parses_releases_sentence_without_if_clause()
        {
            var result = Parse<Sentence.Releases>("action lasts 4 releases a");

            Assert.Equal("action", result.Action);
            Assert.Equal(4, result.Duration);
            Assert.IsType<Expression.True>(result.Condition);
        }

        [Fact]
        public void Parses_triggers_sentence()
        {
            var result = Parse<Sentence.Triggers>("b triggers action");

            Assert.Equal("action", result.Action);
        }

        [Fact]
        public void Parses_full_invokes_sentence()
        {
            var result = Parse<Sentence.Invokes>("action invokes action2 after 6 if true");

            Assert.Equal("action", result.Invoker);
            Assert.Equal("action2", result.Invokee);
            Assert.Equal(6, result.Delay);
        }

        [Fact]
        public void Parses_invokes_sentence_without_if_clause()
        {
            var result = Parse<Sentence.Invokes>("action invokes action2 after 6");

            Assert.Equal("action", result.Invoker);
            Assert.Equal("action2", result.Invokee);
            Assert.Equal(6, result.Delay);
            Assert.IsType<Expression.True>(result.Condition);
        }

        [Fact]
        public void Parses_impossible_sentence()
        {
            var result = Parse<Sentence.Impossible>("action impossible at 123");

            Assert.Equal("action", result.Action);
            Assert.Equal(123, result.Time);
        }

        [Fact]
        public void Parses_always_executable_query()
        {
            var result = Parse<Sentence.AlwaysExecutable>("always executable scenario");

            Assert.Equal("scenario", result.Scenario);
        }

        [Fact]
        public void Parses_ever_executable_query()
        {
            var result = Parse<Sentence.EverExecutable>("ever executable scenario");

            Assert.Equal("scenario", result.Scenario);
        }

        [Fact]
        public void Parses_always_holds_query()
        {
            var result = Parse<Sentence.AlwaysHolds>("always holds true at 3 when scenario");

            Assert.Equal(3, result.Time);
            Assert.Equal("scenario", result.Scenario);
        }

        [Fact]
        public void Parses_ever_holds_query()
        {
            var result = Parse<Sentence.EverHolds>("ever holds true at 3 when scenario");

            Assert.Equal(3, result.Time);
            Assert.Equal("scenario", result.Scenario);
        }

        [Fact]
        public void Parses_always_occurs_query()
        {
            var result = Parse<Sentence.AlwaysOccurs>("action always occurs at 2 when scenario");

            Assert.Equal("action", result.Action);
            Assert.Equal(2, result.Time);
            Assert.Equal("scenario", result.Scenario);
        }

        [Fact]
        public void Parses_scenario()
        {
            var result = Parse<Sentence.ScenarioDefinition>("scenario:\nT=12\nACS=\nOBS=");

            Assert.Equal("scenario", result.Name);
            Assert.Equal(12, result.Time);
        }

        [Fact]
        public void Parses_single_action_in_scenario()
        {
            var result = Parse<Sentence.ScenarioDefinition>("scenario:\nT=1\nACS=(A,3)\nOBS=");

            Assert.Equal(1, result.Actions.Count);
            Assert.Equal(("A", 3), result.Actions[0]);
        }

        [Fact]
        public void Parses_multiple_actions_in_scenario()
        {
            var result = Parse<Sentence.ScenarioDefinition>("scenario:\nT=1\nACS=(A,3) (action, 10)\nOBS=");

            Assert.Equal(2, result.Actions.Count);
            Assert.Equal(("A", 3), result.Actions[0]);
            Assert.Equal(("action", 10), result.Actions[1]);
        }

        [Fact]
        public void Parses_single_observation_in_scenario()
        {
            var result = Parse<Sentence.ScenarioDefinition>("scenario:\nT=1\nACS=\nOBS=(a,10)");

            Assert.Equal(1, result.Observations.Count);
            Assert.Equal(10, result.Observations[0].Item2);
        }

        [Fact]
        public void Parses_multiple_observations_in_scenario()
        {
            var result = Parse<Sentence.ScenarioDefinition>("scenario:\nT=1\nACS=\nOBS=(a,10) (b, 22)");

            Assert.Equal(2, result.Observations.Count);
            Assert.Equal(10, result.Observations[0].Item2);
            Assert.Equal(22, result.Observations[1].Item2);
        }

        [Fact]
        public void Parses_compound_expression_in_observations()
        {
            var result = Parse<Sentence.ScenarioDefinition>("scenario:\nT=1\nACS=\nOBS=(a | b & (c | !d),10)");

            Assert.Equal(1, result.Observations.Count);
            Assert.Equal(10, result.Observations[0].Item2);
        }

        [Fact]
        public void Parses_two_sentences()
        {
            var result = Parser.Parse("action lasts 5 causes true if true\naction lasts 5 causes true if true");

            var success = Assert.IsType<ParseResult.Success>(result);
            Assert.Equal(2, success.Sentences.Count);
            Assert.IsType<Sentence.Causes>(success.Sentences[0]);
            Assert.IsType<Sentence.Causes>(success.Sentences[1]);
        }

        [Fact]
        public void Parses_multiple_sentences()
        {
            var result = Parser.Parse("action lasts 5 causes true if true\naction lasts 5 causes true if true\naction2 lasts 6 causes true if true");

            var success = Assert.IsType<ParseResult.Success>(result);
            Assert.Equal(3, success.Sentences.Count);
            Assert.IsType<Sentence.Causes>(success.Sentences[0]);
            Assert.IsType<Sentence.Causes>(success.Sentences[1]);
            Assert.IsType<Sentence.Causes>(success.Sentences[2]);
        }

        [Fact]
        public void Ignores_empty_lines_between_sentences()
        {
            var result = Parser.Parse("action lasts 5 causes true if true\n\naction lasts 5 causes true if true");

            var success = Assert.IsType<ParseResult.Success>(result);
            Assert.Equal(2, success.Sentences.Count);
            Assert.IsType<Sentence.Causes>(success.Sentences[0]);
            Assert.IsType<Sentence.Causes>(success.Sentences[1]);
        }

        private static T Parse<T>(string input)
        {
            var result = Parser.Parse(input);
            var success = Assert.IsType<ParseResult.Success>(result);
            Assert.Equal(1, success.Sentences.Count);
            return Assert.IsType<T>(success.Sentences[0]);
        }
    }
}
