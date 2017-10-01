using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable SwitchStatementMissingSomeCases

namespace KnowledgeRepresentation.Business.Parsing
{
    // This is just a series of if statements, I won't test it. ;)
    public static class Transformer
    {
        public static IReadOnlyList<AttributedToken> Transform(IEnumerable<Token> tokens)
        {
            return tokens.Select(Transform).ToList();
        }

        public static AttributedToken Transform(Token token)
        {
            switch (token)
            {
                case Token.NewLine n:
                    return new AttributedToken.NewLine(n.LineNo, n.CharNo);

                case Token.Number n:
                    return new AttributedToken.Number(n.Value, n.LineNo, n.CharNo);

                case Token.Special s:
                    return TransformSpecial(s);

                case Token.String s:
                    return TransformString(s);
            }

            throw new InvalidOperationException("Impossible");
        }

        private static AttributedToken TransformSpecial(Token.Special t)
        {
            switch (t.Value)
            {
                case '(':
                    return new AttributedToken.Expression(SpecialType.Open, t.LineNo, t.CharNo);
                case ')':
                    return new AttributedToken.Expression(SpecialType.Close, t.LineNo, t.CharNo);
                case ',':
                    return new AttributedToken.Expression(SpecialType.Comma, t.LineNo, t.CharNo);
                case '&':
                    return new AttributedToken.Expression(SpecialType.And, t.LineNo, t.CharNo);
                case '|':
                    return new AttributedToken.Expression(SpecialType.Or, t.LineNo, t.CharNo);
                case '!':
                    return new AttributedToken.Expression(SpecialType.Not, t.LineNo, t.CharNo);
                case ':':
                    return new AttributedToken.Keyword(KeywordType.Colon, t.LineNo, t.CharNo);
                case '=':
                    return new AttributedToken.Keyword(KeywordType.Assign, t.LineNo, t.CharNo);
            }

            throw new InvalidOperationException("Impossible");
        }

        private static AttributedToken TransformString(Token.String t)
        {
            // I could use some Enum + Dictionary magic, but that would mean 'logic', and I don't want that :P
            switch (t.Value.ToLower())
            {
                case "lasts":
                    return new AttributedToken.Keyword(KeywordType.Lasts, t.LineNo, t.CharNo);
                case "causes":
                    return new AttributedToken.Keyword(KeywordType.Causes, t.LineNo, t.CharNo);
                case "if":
                    return new AttributedToken.Keyword(KeywordType.If, t.LineNo, t.CharNo);
                case "releases":
                    return new AttributedToken.Keyword(KeywordType.Releases, t.LineNo, t.CharNo);
                case "triggers":
                    return new AttributedToken.Keyword(KeywordType.Triggers, t.LineNo, t.CharNo);
                case "invokes":
                    return new AttributedToken.Keyword(KeywordType.Invokes, t.LineNo, t.CharNo);
                case "after":
                    return new AttributedToken.Keyword(KeywordType.After, t.LineNo, t.CharNo);
                case "impossible":
                    return new AttributedToken.Keyword(KeywordType.Impossible, t.LineNo, t.CharNo);

                case "always":
                    return new AttributedToken.Keyword(KeywordType.Always, t.LineNo, t.CharNo);
                case "ever":
                    return new AttributedToken.Keyword(KeywordType.Ever, t.LineNo, t.CharNo);
                case "executable":
                    return new AttributedToken.Keyword(KeywordType.Executable, t.LineNo, t.CharNo);
                case "holds":
                    return new AttributedToken.Keyword(KeywordType.Holds, t.LineNo, t.CharNo);
                case "occurs":
                    return new AttributedToken.Keyword(KeywordType.Occurs, t.LineNo, t.CharNo);
                case "at":
                    return new AttributedToken.Keyword(KeywordType.At, t.LineNo, t.CharNo);
                case "when":
                    return new AttributedToken.Keyword(KeywordType.When, t.LineNo, t.CharNo);

                case "true":
                    return new AttributedToken.Expression(SpecialType.True, t.LineNo, t.CharNo);
                case "false":
                    return new AttributedToken.Expression(SpecialType.False, t.LineNo, t.CharNo);

                case "t":
                    return new AttributedToken.Keyword(KeywordType.TimeVar, t.LineNo, t.CharNo);
                case "acs":
                    return new AttributedToken.Keyword(KeywordType.ActionsVar, t.LineNo, t.CharNo);
                case "obs":
                    return new AttributedToken.Keyword(KeywordType.ObservationsVar, t.LineNo, t.CharNo);
            }

            return new AttributedToken.Identifier(t.Value, t.LineNo, t.CharNo);
        }
    }
}
