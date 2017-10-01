using KnowledgeRepresentation.Business.Parsing;
using Xunit;

namespace KnowledgeRepresentation.UnitTests.Parsing
{
    public class TokenizerTests
    {
        [Fact]
        public void Tokenizes_number_as_number()
        {
            AssertTokens("123", new Token.Number(123, 0, 0));
        }

        [Fact]
        public void Tokenizes_string_as_identifier()
        {
            AssertTokens("abcd", new Token.String("abcd", 0, 0));
        }

        [Fact]
        public void Tokenizes_string_with_dash_as_identifier()
        {
            AssertTokens("abcd-abcd", new Token.String("abcd-abcd", 0, 0));
        }

        [Fact]
        public void Tokenizes_string_with_underscore_as_identifier()
        {
            AssertTokens("abcd_abcd", new Token.String("abcd_abcd", 0, 0));
        }

        [Fact]
        public void Tokenizes_string_with_digits_as_following_characters_as_identifier()
        {
            AssertTokens("a123", new Token.String("a123", 0, 0));
        }

        [Fact]
        public void Treats_space_as_separator()
        {
            AssertTokens("abc zxc", new Token.String("abc", 0, 0), new Token.String("zxc", 0, 0));
        }

        [Fact]
        public void Treats_linefeed_as_new_line()
        {
            AssertTokens("abc\nzxc", new Token.String("abc", 0, 0), new Token.NewLine(0, 0), new Token.String("zxc", 0, 0));
        }

        [Fact]
        public void Treats_crlf_as_new_line()
        {
            AssertTokens("abc\r\nzxc", new Token.String("abc", 0, 0), new Token.NewLine(0, 0), new Token.String("zxc", 0, 0));
        }

        [Fact]
        public void Treats_number_starting_with_dash_as_identifier()
        {
            AssertTokens("-123", new Token.String("-123", 0, 0));
        }

        [Fact]
        public void Treats_number_starting_with_underscore_as_identifier()
        {
            AssertTokens("_123", new Token.String("_123", 0, 0));
        }

        [Fact]
        public void Tokenizes_left_parenthesis_as_special_token()
        {
            AssertTokens("(", new Token.Special('(', 0, 0));
        }

        [Fact]
        public void Tokenizes_two_left_parentheses_as_two_special_tokens()
        {
            AssertTokens("((", new Token.Special('(', 0, 0), new Token.Special('(', 0, 0));
        }

        [Fact]
        public void Tokenizes_right_parenthesis_as_special_token()
        {
            AssertTokens(")", new Token.Special(')', 0, 0));
        }

        [Fact]
        public void Tokenizes_two_right_parentheses_as_two_special_tokens()
        {
            AssertTokens("))", new Token.Special(')', 0, 0), new Token.Special(')', 0, 0));
        }

        [Fact]
        public void Tokenizes_exclamation_mark_as_special_token()
        {
            AssertTokens("!", new Token.Special('!', 0, 0));
        }

        [Fact]
        public void Tokenizes_two_exclamation_marks_as_two_special_tokens()
        {
            AssertTokens("!!", new Token.Special('!', 0, 0), new Token.Special('!', 0, 0));
        }

        [Fact]
        public void Tokenizes_pipe_as_special_symbol()
        {
            AssertTokens("|", new Token.Special('|', 0, 0));
        }

        [Fact]
        public void Tokenizes_two_pipes_as_two_special_symbols()
        {
            AssertTokens("||", new Token.Special('|', 0, 0), new Token.Special('|', 0, 0));
        }

        [Fact]
        public void Tokenizes_ampersand_as_special_symbol()
        {
            AssertTokens("&", new Token.Special('&', 0, 0));
        }

        [Fact]
        public void Tokenizes_two_ampersands_as_two_special_symbols()
        {
            AssertTokens("&&", new Token.Special('&', 0, 0), new Token.Special('&', 0, 0));
        }

        [Fact]
        public void Tokenizes_comma_as_special_symbol()
        {
            AssertTokens(",", new Token.Special(',', 0, 0));
        }

        [Fact]
        public void Tokenizes_two_commas_as_two_tokens()
        {
            AssertTokens(",,", new Token.Special(',', 0, 0), new Token.Special(',', 0, 0));
        }

        [Fact]
        public void Tokenizes_colon_as_special_symbol()
        {
            AssertTokens(":", new Token.Special(':', 0, 0));
        }

        [Fact]
        public void Tokenizes_two_colons_as_two_tokens()
        {
            AssertTokens("::", new Token.Special(':', 0, 0), new Token.Special(':', 0, 0));
        }

        [Fact]
        public void Tokenizes_equal_sign_as_special_symbol()
        {
            AssertTokens("=", new Token.Special('=', 0, 0));
        }

        [Fact]
        public void Tokenizes_two_equal_signs_as_two_tokens()
        {
            AssertTokens("==", new Token.Special('=', 0, 0), new Token.Special('=', 0, 0));
        }

        [Fact]
        public void Does_not_require_space_between_identifier_and_special_symbol()
        {
            AssertTokens("abc,", new Token.String("abc", 0, 0), new Token.Special(',', 0, 0));
            AssertTokens("abc(", new Token.String("abc", 0, 0), new Token.Special('(', 0, 0));
            AssertTokens("abc)", new Token.String("abc", 0, 0), new Token.Special(')', 0, 0));
            AssertTokens("abc!", new Token.String("abc", 0, 0), new Token.Special('!', 0, 0));
            AssertTokens("abc&", new Token.String("abc", 0, 0), new Token.Special('&', 0, 0));
            AssertTokens("abc|", new Token.String("abc", 0, 0), new Token.Special('|', 0, 0));
            AssertTokens("abc:", new Token.String("abc", 0, 0), new Token.Special(':', 0, 0));
            AssertTokens("abc=", new Token.String("abc", 0, 0), new Token.Special('=', 0, 0));
        }

        [Fact]
        public void Does_not_require_space_between_number_and_special_symbol()
        {
            AssertTokens("123,", new Token.Number(123, 0, 0), new Token.Special(',', 0, 0));
            AssertTokens("123(", new Token.Number(123, 0, 0), new Token.Special('(', 0, 0));
            AssertTokens("123)", new Token.Number(123, 0, 0), new Token.Special(')', 0, 0));
            AssertTokens("123!", new Token.Number(123, 0, 0), new Token.Special('!', 0, 0));
            AssertTokens("123&", new Token.Number(123, 0, 0), new Token.Special('&', 0, 0));
            AssertTokens("123|", new Token.Number(123, 0, 0), new Token.Special('|', 0, 0));
            AssertTokens("123:", new Token.Number(123, 0, 0), new Token.Special(':', 0, 0));
            AssertTokens("123=", new Token.Number(123, 0, 0), new Token.Special('=', 0, 0));
        }

        [Fact]
        public void Does_not_require_space_between_special_symbol_and_identifier()
        {
            AssertTokens(",abc", new Token.Special(',', 0, 0), new Token.String("abc", 0, 0));
            AssertTokens("(abc", new Token.Special('(', 0, 0), new Token.String("abc", 0, 0));
            AssertTokens(")abc", new Token.Special(')', 0, 0), new Token.String("abc", 0, 0));
            AssertTokens("!abc", new Token.Special('!', 0, 0), new Token.String("abc", 0, 0));
            AssertTokens("&abc", new Token.Special('&', 0, 0), new Token.String("abc", 0, 0));
            AssertTokens("|abc", new Token.Special('|', 0, 0), new Token.String("abc", 0, 0));
            AssertTokens(":abc", new Token.Special(':', 0, 0), new Token.String("abc", 0, 0));
            AssertTokens("=abc", new Token.Special('=', 0, 0), new Token.String("abc", 0, 0));
        }

        [Fact]
        public void Does_not_require_space_between_special_sumbol_and_number()
        {
            AssertTokens(",123", new Token.Special(',', 0, 0), new Token.Number(123, 0, 0));
            AssertTokens("(123", new Token.Special('(', 0, 0), new Token.Number(123, 0, 0));
            AssertTokens(")123", new Token.Special(')', 0, 0), new Token.Number(123, 0, 0));
            AssertTokens("!123", new Token.Special('!', 0, 0), new Token.Number(123, 0, 0));
            AssertTokens("&123", new Token.Special('&', 0, 0), new Token.Number(123, 0, 0));
            AssertTokens("|123", new Token.Special('|', 0, 0), new Token.Number(123, 0, 0));
            AssertTokens(":123", new Token.Special(':', 0, 0), new Token.Number(123, 0, 0));
            AssertTokens("=123", new Token.Special('=', 0, 0), new Token.Number(123, 0, 0));
        }

        [Fact]
        public void Returns_error_when_number_contains_letters()
        {
            AssertFailure("123a");
        }

        [Fact]
        public void Returns_error_when_number_contains_dash()
        {
            AssertFailure("123-");
        }

        [Fact]
        public void Returns_error_when_number_contains_underscore()
        {
            AssertFailure("123_123");
        }

        [Fact]
        public void Tokenizes_multiple_tokens_correctly()
        {
            AssertTokens("123 num1 & v |\nprepare !2 lift off",
                new Token.Number(123, 0, 0),
                new Token.String("num1", 0, 0),
                new Token.Special('&', 0, 0),
                new Token.String("v", 0, 0),
                new Token.Special('|', 0, 0),
                new Token.NewLine(0, 0),
                new Token.String("prepare", 0, 0),
                new Token.Special('!', 0, 0),
                new Token.Number(2, 0, 0),
                new Token.String("lift", 0, 0),
                new Token.String("off", 0, 0)
            );
        }

        [Fact]
        public void Treats_CRLF_as_new_line()
        {
            AssertTokens("\r\n", new Token.NewLine(0, 0));
        }

        [Fact]
        public void Correctly_records_token_locations()
        {
            var result = Tokenizer.Tokenize("abc 123\n\n123 a");
            var tokens = Assert.IsType<TokenizationResult.Success>(result).Tokens;

            Assert.Equal(6, tokens.Count);

            Assert.Equal(1, tokens[0].LineNo);
            Assert.Equal(1, tokens[0].CharNo);

            Assert.Equal(1, tokens[1].LineNo);
            Assert.Equal(5, tokens[1].CharNo);

            Assert.Equal(1, tokens[2].LineNo);
            Assert.Equal(8, tokens[2].CharNo);

            Assert.Equal(2, tokens[3].LineNo);
            Assert.Equal(1, tokens[3].CharNo);

            Assert.Equal(3, tokens[4].LineNo);
            Assert.Equal(1, tokens[4].CharNo);

            Assert.Equal(3, tokens[5].LineNo);
            Assert.Equal(5, tokens[5].CharNo);
        }

        private static void AssertTokens(string input, params Token[] expected)
        {
            var result = Tokenizer.Tokenize(input);
            var success = Assert.IsType<TokenizationResult.Success>(result).Tokens;
            Assert.Equal(expected.Length, success.Count);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.IsType(expected[i].GetType(), success[i]);

                switch (expected[i])
                {
                    case Token.String id:
                        Assert.Equal(id.Value, ((Token.String)success[i]).Value);
                        break;

                    case Token.Number id:
                        Assert.Equal(id.Value, ((Token.Number)success[i]).Value);
                        break;

                    case Token.Special id:
                        Assert.Equal(id.Value, ((Token.Special)success[i]).Value);
                        break;
                }
            }
        }

        private static void AssertFailure(string input)
        {
            var result = Tokenizer.Tokenize(input);
            Assert.IsType<TokenizationResult.Failure>(result);
        }
    }
}
