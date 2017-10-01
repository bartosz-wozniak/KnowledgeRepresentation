using System.Collections.Generic;
// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace KnowledgeRepresentation.Business.Parsing
{
    using Expression = Expression<string>;

    public class ExpressionParser
    {
        public static Expression Parse(IReadOnlyList<AttributedToken> tokens) => new ExpressionParser(tokens).Parse();

        private readonly IReadOnlyList<AttributedToken> _tokens;
        private int _index;

        private ExpressionParser(IReadOnlyList<AttributedToken> tokens)
        {
            _tokens = tokens;
        }

        private Expression Parse()
        {
            var r = Exp();
            if (_index < _tokens.Count)
            {
                return null;
            }
            return r;
        }

        private Expression Exp()
        {
            var prev = Not();
            if (prev == null)
            {
                return null;
            }

            while (_index < _tokens.Count && _tokens[_index] is AttributedToken.Expression e && (e.Type == SpecialType.And || e.Type == SpecialType.Or))
            {
                var op = e.Type;
                _index++;

                var second = Not();

                if (second == null)
                {
                    return null;
                }

                if (op == SpecialType.And)
                {
                    prev = new Expression.And(prev, second);
                }
                else
                {
                    prev = new Expression.Or(prev, second);
                }
            }
            return prev;
        }

        private Expression Not()
        {
            if (Match(SpecialType.Not))
            {
                var sub = Factor();
                if (sub != null)
                {
                    return new Expression.Not(sub);
                }
            }
            return Factor();
        }

        private Expression Factor()
        {
            var result = Id() ?? TrueFalse();
            if (result == null)
            {
                if (Match(SpecialType.Open))
                {
                    result = Exp();
                    if (!Match(SpecialType.Close))
                    {
                        result = null;
                    }
                }
            }
            return result;
        }

        private Expression.Identifier Id()
        {
            if (_index < _tokens.Count && _tokens[_index] is AttributedToken.Identifier i)
            {
                _index++;
                return new Expression.Identifier(i.Name);
            }
            return null;
        }

        private Expression TrueFalse()
        {
            if (_index < _tokens.Count && _tokens[_index] is AttributedToken.Expression e)
            {
                if (e.Type == SpecialType.True)
                {
                    _index++;
                    return new Expression.True();
                }
                if (e.Type == SpecialType.False)
                {
                    _index++;
                    return new Expression.False();
                }
            }
            return null;
        }

        private bool Match(SpecialType type)
        {
            if (_index < _tokens.Count && _tokens[_index] is AttributedToken.Expression e && e.Type == type)
            {
                _index++;
                return true;
            }
            return false;
        }
    }
}
