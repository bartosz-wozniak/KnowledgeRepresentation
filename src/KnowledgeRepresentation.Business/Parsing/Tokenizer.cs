using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgeRepresentation.Business.Parsing
{
    public static class Tokenizer
    {
        private enum State
        {
            Waiting,
            Number,
            Identifier
        }

        public static TokenizationResult Tokenize(string input)
        {
            // Nothing smells as good as a class without a class ^^

            var state = State.Waiting;
            var lineNo = 1;
            var charNo = 1;
            var i = 0;
            char c;

            var tokens = new List<Token>();
            var lastToken = new StringBuilder();

            void Proceed()
            {
                charNo++;
                i++;
            }

            void ProceedToNewLine()
            {
                charNo = 1;
                lineNo++;
                i++;
            }

            TokenizationResult.Failure Unknown()
            {
                return new TokenizationResult.Failure($"Unknown character {c} at line {lineNo}, position {charNo}.");
            }

            TokenizationResult.Failure Unexpected(string expected)
            {
                return new TokenizationResult.Failure($"Unexpected character {c} at line {lineNo}, position {charNo}. {expected} expected.");
            }


            input += '\0'; // Simplifies things _a lot_
            while (i < input.Length)
            {
                c = input[i];
                switch (state)
                {
                    case State.Waiting:
                        if (IsWhiteSpace(c))
                        {
                            Proceed();
                        }
                        else if (IsNewLine(c))
                        {
                            tokens.Add(new Token.NewLine(lineNo, charNo));
                            ProceedToNewLine();
                        }
                        else if (IsSpecial(c))
                        {
                            tokens.Add(new Token.Special(c, lineNo, charNo));
                            Proceed();
                        }
                        else if (IsNumber(c))
                        {
                            state = State.Number;
                        }
                        else if (IsIdentifier(c))
                        {
                            state = State.Identifier;
                        }
                        else if (IsEol(c))
                        {
                            Proceed();
                        }
                        else
                        {
                            return Unknown();
                        }
                        break;

                    case State.Number:
                        if (IsNumber(c))
                        {
                            lastToken.Append(c);
                            Proceed();
                        }
                        else if (IsIdentifier(c))
                        {
                            return Unexpected("Digit or whitespace");
                        }
                        else
                        {
                            var num = int.Parse(lastToken.ToString());
                            tokens.Add(new Token.Number(num, lineNo, charNo - lastToken.Length));
                            lastToken = new StringBuilder();
                            state = State.Waiting;
                        }
                        break;

                    case State.Identifier:
                        if (IsIdentifier(c))
                        {
                            lastToken.Append(c);
                            Proceed();
                        }
                        else
                        {
                            tokens.Add(new Token.String(lastToken.ToString(), lineNo, charNo - lastToken.Length));
                            lastToken = new StringBuilder();
                            state = State.Waiting;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new TokenizationResult.Success(tokens);
        }

        private static bool IsNewLine(char c) => c == '\n';
        private static bool IsNumber(char c) => char.IsDigit(c);
        private static bool IsIdentifier(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '-';
        private static bool IsSpecial(char c) => c == '(' || c == ')' || c == '!' || c == '|' || c == '&' || c == ',' || c == ':' || c == '=';
        private static bool IsWhiteSpace(char c) => char.IsWhiteSpace(c) && c != '\n';
        private static bool IsEol(char c) => c == '\0';
    }

    public abstract class TokenizationResult
    {
        public sealed class Success : TokenizationResult
        {
            public IReadOnlyList<Token> Tokens { get; }

            public Success(IReadOnlyList<Token> tokens)
            {
                Tokens = tokens;
            }
        }

        public sealed class Failure : TokenizationResult
        {
            public string Error { get; }

            public Failure(string error)
            {
                Error = error;
            }
        }

        private TokenizationResult()
        { }
    }
}
