using KnowledgeRepresentation.Business.Parsing;
using System;
using Xunit;

namespace KnowledgeRepresentation.UnitTests.Parsing
{
    using Expression = Expression<string>;

    public class ExpressionParserTests
    {
        [Fact]
        public void Parses_identifier_as_identifier()
        {
            AssertExpression<Expression.Identifier>("identifier", e => Assert.Equal("identifier", e.Name));
        }

        [Fact]
        public void Parses_true_as_True()
        {
            AssertExpression<Expression.True>("true");
        }

        [Fact]
        public void Parses_false_as_False()
        {
            AssertExpression<Expression.False>("false");
        }

        [Fact]
        public void Parses_and_as_And()
        {
            AssertExpression<Expression.And>("a & false");
        }

        [Fact]
        public void Parses_or_as_Or()
        {
            AssertExpression<Expression.Or>("a | false");
        }

        [Fact]
        public void Parses_not_as_not()
        {
            AssertExpression<Expression.Not>("!a");
        }

        [Fact]
        public void Parses_not_with_parentheses()
        {
            AssertExpression<Expression.Not>("!(a)");
        }

        [Fact]
        public void Parses_and_and_ors_right_to_left()
        {
            AssertExpression<Expression.And>("a & b | c & d", e =>
            {
                var or = Assert.IsType<Expression.Or>(e.Left);
                var and = Assert.IsType<Expression.And>(or.Left);

                Assert.IsType<Expression.Identifier>(e.Right);
                Assert.IsType<Expression.Identifier>(or.Right);
                Assert.IsType<Expression.Identifier>(and.Left);
            });
        }

        [Fact]
        public void Parses_sides_of_and()
        {
            AssertExpression<Expression.And>("a & false", a =>
            {
                Assert.IsType<Expression.Identifier>(a.Left);
                Assert.IsType<Expression.False>(a.Right);
            });
        }

        [Fact]
        public void Parses_sides_of_or()
        {
            AssertExpression<Expression.Or>("a | false", a =>
            {
                Assert.IsType<Expression.Identifier>(a.Left);
                Assert.IsType<Expression.False>(a.Right);
            });
        }

        [Fact]
        public void Respects_parentheses()
        {
            AssertExpression<Expression.And>("(a | b) & (c | d)", e =>
            {
                Assert.IsType<Expression.Or>(e.Left);
                Assert.IsType<Expression.Or>(e.Right);
            });
        }

        [Fact]
        public void Returns_null_if_expression_is_malformed()
        {
            var result = Parse("a |");

            Assert.Null(result);
        }

        [Fact]
        public void Returns_null_if_expression_is_malformed_excessive_data()
        {
            var result = Parse("a | b c");

            Assert.Null(result);
        }

        private static void AssertExpression<T>(string input, Action<T> assert = null)
        {
            var expr = Parse(input);
            var typed = Assert.IsType<T>(expr);
            assert?.Invoke(typed);
        }

        private static Expression Parse(string input)
        {
            var tokens = Tokenizer.Tokenize(input);
            var attribs = Transformer.Transform(((TokenizationResult.Success)tokens).Tokens);
            return ExpressionParser.Parse(attribs);
        }
    }
}
