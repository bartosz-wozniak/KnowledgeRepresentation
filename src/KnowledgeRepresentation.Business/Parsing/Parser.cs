using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KnowledgeRepresentation.Business.Parsing
{
    using Expression = Expression<string>;
    using Sentence = Sentence<string>;

    public static class Parser
    {
        private static readonly Func<SentenceParser, SentenceParser>[] AllParsers =
        {
            ParseFullCauses,
            ParseCausesWithoutCondition,
            ParseFullReleases,
            ParseReleasesWithoutCondition,
            ParseTriggers,
            ParseFullInvokes,
            ParseInvokesWithoutCondition,
            ParseImpossible,

            ParseAlwaysExecutable,
            ParseEverExecutable,
            ParseAlwaysHolds,
            ParseEverHolds,
            ParseAlwaysOccurs,
            ParseEverOccurs,

            ParseScenario
        };

        private static readonly Func<SentenceParser, SentenceParser>[] SentencesParsers =
        {
            ParseFullCauses,
            ParseCausesWithoutCondition,
            ParseFullReleases,
            ParseReleasesWithoutCondition,
            ParseTriggers,
            ParseFullInvokes,
            ParseInvokesWithoutCondition,
            ParseImpossible,
        };

        private static readonly Func<SentenceParser, SentenceParser>[] QueriesParsers =
        {
            ParseAlwaysExecutable,
            ParseEverExecutable,
            ParseAlwaysHolds,
            ParseEverHolds,
            ParseAlwaysOccurs,
            ParseEverOccurs
        };

        private static readonly Func<SentenceParser, SentenceParser>[] ScenarioParsers = { ParseScenario };

        public static ParseResult Parse(string input) => Parse(input, AllParsers);
        public static ParseResult ParseSentences(string input) => Parse(input, SentencesParsers);
        public static ParseResult ParseQueries(string input) => Parse(input, QueriesParsers);
        public static ParseResult ParseScenario(string input) => Parse(input, ScenarioParsers);

        private static ParseResult Parse(string input, Func<SentenceParser, SentenceParser>[] availableParser)
        {
            var result = Tokenizer.Tokenize(input);
            switch (result)
            {
                case TokenizationResult.Failure f:
                    return new ParseResult.Failure(new[] { (0, "Tokenization failed: " + f.Error) });

                case TokenizationResult.Success s:
                    return Parse(Transformer.Transform(s.Tokens), availableParser);
            }
            throw new InvalidOperationException("Impossible");
        }

        private static ParseResult Parse(IReadOnlyList<AttributedToken> tokens, Func<SentenceParser, SentenceParser>[] availableParser)
        {
            // Per-line list of errors
            var errors = new List<List<SentenceParseResult<Sentence>.Failure>>();

            var sentences = new List<Sentence>();
            var workingBuffer = new List<AttributedToken>(tokens);
            workingBuffer.Add(new AttributedToken.NewLine(tokens[tokens.Count - 1].LineNo, tokens[tokens.Count - 1].CharNo + 1));

            var offset = 0;
            while (offset < tokens.Count)
            {
                // Handling empty lines is easier here
                if (tokens[offset] is AttributedToken.NewLine)
                {
                    offset++;
                    continue;
                }

                var (sentence, consumed, parseErrors) = Consume(workingBuffer, offset, availableParser);
                if (parseErrors != null)
                {
                    errors.Add(parseErrors);

                    // Skip this line, try parse next - this will result in gathering all the errors
                    while (!(workingBuffer[offset] is AttributedToken.NewLine))
                    {
                        offset++;
                    }
                }
                else
                {
                    offset = consumed;
                    sentences.Add(sentence);
                }
            }

            if (errors.Count > 0)
            {
                return BuildFailure(errors, workingBuffer);
            }
            else
            {
                return new ParseResult.Success(sentences);
            }
        }

        private static (Sentence, int, List<SentenceParseResult<Sentence>.Failure>) Consume(IReadOnlyList<AttributedToken> tokens, int startOffset, Func<SentenceParser, SentenceParser>[] availableParser)
        {
            var errors = new List<SentenceParseResult<Sentence>.Failure>();
            foreach (var builder in availableParser)
            {
                var parser = builder(new SentenceParser(tokens, startOffset));
                var parseResult = parser.Parse<Sentence>();

                switch (parseResult)
                {
                    case SentenceParseResult<Sentence>.Success s:
                        return (s.Result, s.NextTokenIndex, null);

                    case SentenceParseResult<Sentence>.Failure f:
                        errors.Add(f);
                        break;
                }
            }
            return (null, 0, errors);
        }

        private static ParseResult.Failure BuildFailure(List<List<SentenceParseResult<Sentence>.Failure>> errors, IReadOnlyList<AttributedToken> workingBuffer)
        {
            var messages = new List<(int, string)>();

            foreach (var perLine in errors)
            {
                var mostParsed = perLine.Max(e => e.FailedAtIndex);
                var probableSentences = perLine.Where(e => e.FailedAtIndex == mostParsed);

                var unexpected = workingBuffer[probableSentences.First().FailedAtIndex];

                var builder = new StringBuilder("Unexpected ")
                    .Append(unexpected.Describe())
                    .Append(" at position ")
                    .Append(unexpected.CharNo)
                    .AppendLine(". If you wanted");

                foreach (var s in probableSentences)
                {
                    builder
                        .Append('\t')
                        .Append(s.SentenceName)
                        .Append(" sentence (e.g. '")
                        .Append(s.Example)
                        .Append("') then ")
                        .Append(s.ExpectedToken)
                        .AppendLine(" is expected.");
                }

                messages.Add((unexpected.LineNo, builder.ToString()));
            }

            return new ParseResult.Failure(messages);
        }

        // No one won - it is a monadic (well... applicative, but it's almost the same) parser ;)
        private static SentenceParser ParseFullCauses(SentenceParser p) => p
            .Named("causes")
            .Example("A lasts 1 causes B if C")
            .Identifier()
            .Keyword(KeywordType.Lasts)
            .Number()
            .Keyword(KeywordType.Causes)
            .Expression()
            .Keyword(KeywordType.If)
            .Expression()
            .NewLine()
            .Finish<Sentence.Causes>();

        private static SentenceParser ParseCausesWithoutCondition(SentenceParser p) => p
            .Named("causes")
            .Example("A lasts 1 causes B")
            .Identifier()
            .Keyword(KeywordType.Lasts)
            .Number()
            .Keyword(KeywordType.Causes)
            .Expression()
            .Substitute(new Expression.True())
            .NewLine()
            .Finish<Sentence.Causes>();

        private static SentenceParser ParseFullReleases(SentenceParser p) => p
            .Named("releases")
            .Example("A lasts 1 releases B if C")
            .Identifier()
            .Keyword(KeywordType.Lasts)
            .Number()
            .Keyword(KeywordType.Releases)
            .Expression()
            .Keyword(KeywordType.If)
            .Expression()
            .NewLine()
            .Finish<Sentence.Releases>();

        private static SentenceParser ParseReleasesWithoutCondition(SentenceParser p) => p
            .Named("releases")
            .Example("A lasts 1 releases B")
            .Identifier()
            .Keyword(KeywordType.Lasts)
            .Number()
            .Keyword(KeywordType.Releases)
            .Expression()
            .Substitute(new Expression.True())
            .NewLine()
            .Finish<Sentence.Releases>();

        private static SentenceParser ParseTriggers(SentenceParser p) => p
            .Named("triggers")
            .Example("A & B triggers C")
            .Expression()
            .Keyword(KeywordType.Triggers)
            .Identifier()
            .NewLine()
            .Finish<Sentence.Triggers>();

        private static SentenceParser ParseFullInvokes(SentenceParser p) => p
            .Named("invokes")
            .Example("A invokes B after 1 if C")
            .Identifier()
            .Keyword(KeywordType.Invokes)
            .Identifier()
            .Keyword(KeywordType.After)
            .Number()
            .Keyword(KeywordType.If)
            .Expression()
            .NewLine()
            .Finish<Sentence.Invokes>();

        private static SentenceParser ParseInvokesWithoutCondition(SentenceParser p) => p
            .Named("invokes")
            .Example("A invokes B after 1")
            .Identifier()
            .Keyword(KeywordType.Invokes)
            .Identifier()
            .Keyword(KeywordType.After)
            .Number()
            .Substitute(new Expression.True())
            .NewLine()
            .Finish<Sentence.Invokes>();

        private static SentenceParser ParseImpossible(SentenceParser p) => p
            .Named("impossible")
            .Example("A impossible at 1")
            .Identifier()
            .Keyword(KeywordType.Impossible)
            .Keyword(KeywordType.At)
            .Number()
            .NewLine()
            .Finish<Sentence.Impossible>();

        private static SentenceParser ParseAlwaysExecutable(SentenceParser p) => p
            .Named("always executable")
            .Example("always executable Sc1")
            .Keyword(KeywordType.Always)
            .Keyword(KeywordType.Executable)
            .Identifier()
            .NewLine()
            .Finish<Sentence.AlwaysExecutable>();

        private static SentenceParser ParseEverExecutable(SentenceParser p) => p
            .Named("ever executable")
            .Example("ever executable Sc1")
            .Keyword(KeywordType.Ever)
            .Keyword(KeywordType.Executable)
            .Identifier()
            .NewLine()
            .Finish<Sentence.EverExecutable>();

        private static SentenceParser ParseAlwaysHolds(SentenceParser p) => p
            .Named("always holds")
            .Example("always holds A & B at 1 when Sc1")
            .Keyword(KeywordType.Always)
            .Keyword(KeywordType.Holds)
            .Expression()
            .Keyword(KeywordType.At)
            .Number()
            .Keyword(KeywordType.When)
            .Identifier()
            .NewLine()
            .Finish<Sentence.AlwaysHolds>();

        private static SentenceParser ParseEverHolds(SentenceParser p) => p
            .Named("ever holds")
            .Example("ever holds A & B at 1 when Sc1")
            .Keyword(KeywordType.Ever)
            .Keyword(KeywordType.Holds)
            .Expression()
            .Keyword(KeywordType.At)
            .Number()
            .Keyword(KeywordType.When)
            .Identifier()
            .NewLine()
            .Finish<Sentence.EverHolds>();

        private static SentenceParser ParseAlwaysOccurs(SentenceParser p) => p
            .Named("always occurs")
            .Example("A always occurs at 1 when Sc1")
            .Identifier()
            .Keyword(KeywordType.Always)
            .Keyword(KeywordType.Occurs)
            .Keyword(KeywordType.At)
            .Number()
            .Keyword(KeywordType.When)
            .Identifier()
            .NewLine()
            .Finish<Sentence.AlwaysOccurs>();

        private static SentenceParser ParseEverOccurs(SentenceParser p) => p
            .Named("ever occurs")
            .Example("A ever occurs at 1 when Sc1")
            .Identifier()
            .Keyword(KeywordType.Ever)
            .Keyword(KeywordType.Occurs)
            .Keyword(KeywordType.At)
            .Number()
            .Keyword(KeywordType.When)
            .Identifier()
            .NewLine()
            .Finish<Sentence.EverOccurs>();

        private static SentenceParser ParseScenario(SentenceParser p) => p
            .Named("scenario")
            .Example("\n\t\tSc1:\n\t\t\tT = 1\n\t\t\tACS = (A1, 1) (A2, 2)\n\t\t\tOBS = (A, 1) (A & B, 2)\n\t")
            .Identifier()
            .Keyword(KeywordType.Colon)
            .NewLine()

            .Keyword(KeywordType.TimeVar)
            .Keyword(KeywordType.Assign)
            .Number()
            .NewLine()

            .Keyword(KeywordType.ActionsVar)
            .Keyword(KeywordType.Assign)
            .Expression(ParseActions, expected: "action occurence")
            .NewLine()

            .Keyword(KeywordType.ObservationsVar)
            .Keyword(KeywordType.Assign)
            .Expression(ParseObservations, expected: "observation")
            .NewLine()

            .Finish<Sentence.ScenarioDefinition>();

        private static object ParseActions(List<AttributedToken> tokens)
        {
            return ParseListOf<(string, int)>(tokens, p => p
                .Special(SpecialType.Open)
                .Identifier()
                .Special(SpecialType.Comma)
                .Number()
                .Special(SpecialType.Close)
                .Finish<(string, int)>()
            );
        }

        private static object ParseObservations(List<AttributedToken> tokens)
        {
            return ParseListOf<(Expression, int)>(tokens, p => p
                .Special(SpecialType.Open)
                .Expression(until: t => t is AttributedToken.Expression e && e.Type == SpecialType.Comma)
                .Special(SpecialType.Comma)
                .Number()
                .Special(SpecialType.Close)
                .Finish<(Expression, int)>());
        }

        private static List<T> ParseListOf<T>(IReadOnlyList<AttributedToken> tokens, Func<SentenceParser, SentenceParser> parser)
        {
            var result = new List<T>();
            var offset = 0;
            while (offset < tokens.Count)
            {
                var p = parser(new SentenceParser(tokens, offset));
                var parseResult = p.Parse<T>();
                switch (parseResult)
                {
                    case SentenceParseResult<T>.Success s:
                        result.Add(s.Result);
                        offset = s.NextTokenIndex;
                        break;

                    case SentenceParseResult<T>.Failure f:
                        return null;
                }
            }
            return result;
        }

        // Bartek - 'R# all the things' is the worst case of refactoring. Esp. w/ the "_" thing for private fields (and some other settings) on by default.
        private sealed class SentenceParser
        {
            private readonly IReadOnlyList<AttributedToken> _tokens;

            // Bartek - Case in readability - "default" value means "I don't care", explicitly setting the value to default means "I care and I want it to be exactly like this"
            private bool _failed;
            private int _index; // Except this, this was leftover from previous version that was offset-based

            private readonly List<object> _captured = new List<object>();
            private Type _builder;

            // Error-reporting related
            private readonly int _baseIndex;
            private string _name = string.Empty;
            private string _expectedToken = string.Empty;
            private string _example = string.Empty;

            private bool HasData => _index < _tokens.Count;

            public SentenceParser(IReadOnlyList<AttributedToken> tokens, int startFrom)
            {
                _tokens = tokens;
                _baseIndex = _index = startFrom;
            }

            public SentenceParser Named(string name)
            {
                _name = name;
                return this;
            }

            public SentenceParser Example(string example)
            {
                _example = example;
                return this;
            }


            public SentenceParser Keyword(KeywordType type)
            {
                if (HasData && !_failed && _tokens[_index] is AttributedToken.Keyword k && k.Type == type)
                {
                    _index++;
                }
                else if (!_failed)
                {
                    _expectedToken = type.Describe();
                    _failed = true;
                }
                return this;
            }

            public SentenceParser Identifier()
            {
                if (HasData && !_failed && _tokens[_index] is AttributedToken.Identifier i)
                {
                    _index++;
                    _captured.Add(i.Name);
                }
                else if (!_failed)
                {
                    _expectedToken = "identifier";
                    _failed = true;
                }
                return this;
            }

            public SentenceParser Number()
            {
                if (HasData && !_failed && _tokens[_index] is AttributedToken.Number n)
                {
                    _index++;
                    _captured.Add(n.Value);
                }
                else if (!_failed)
                {
                    _expectedToken = "number";
                    _failed = true;
                }
                return this;
            }

            public SentenceParser Special(SpecialType type)
            {
                if (HasData && !_failed && _tokens[_index] is AttributedToken.Expression e && e.Type == type)
                {
                    _index++;
                }
                else if (!_failed)
                {
                    _expectedToken = type.Describe();
                    _failed = true;
                }
                return this;
            }

            public SentenceParser Expression(Func<List<AttributedToken>, object> transformUsing = null, Func<AttributedToken, bool> until = null, string expected = "expression")
            {
                if (!HasData || _failed)
                {
                    return this;
                }

                var exprs = new List<AttributedToken>();
                while (
                    (_tokens[_index] is AttributedToken.Expression ||
                     _tokens[_index] is AttributedToken.Identifier ||
                     _tokens[_index] is AttributedToken.Number) &&
                     (until == null || !until(_tokens[_index])))
                {
                    exprs.Add(_tokens[_index]);
                    _index++;
                }

                var cap =
                    transformUsing != null ? transformUsing(exprs) :
                    exprs.Count > 0 ? ExpressionParser.Parse(exprs) : null;

                if (cap != null)
                {
                    _captured.Add(cap);
                }
                else
                {
                    _expectedToken = expected;
                    _failed = true;
                }
                return this;
            }

            public SentenceParser NewLine()
            {
                if (HasData && !_failed && _tokens[_index] is AttributedToken.NewLine)
                {
                    _index++;
                }
                else if (!_failed)
                {
                    _expectedToken = "new line";
                    _failed = true;
                }
                return this;
            }

            public SentenceParser Substitute(object param)
            {
                _captured.Add(param);
                return this;
            }

            public SentenceParser Finish<T>()
            {
                if (!_failed)
                {
                    _builder = typeof(T);
                }
                return this;
            }

            public SentenceParseResult<TResult> Parse<TResult>()
            {
                if (_failed)
                {
                    return new SentenceParseResult<TResult>.Failure(_index, _name, _expectedToken, _example);
                }
                else if (_builder == null)
                {
                    throw new InvalidOperationException("Finish first.");
                }
                else
                {
                    var obj = (TResult)Activator.CreateInstance(_builder, _captured.ToArray());
                    return new SentenceParseResult<TResult>.Success(obj, _index);
                }
            }
        }

        private abstract class SentenceParseResult<TResult>
        {
            public sealed class Success : SentenceParseResult<TResult>
            {
                public TResult Result { get; }
                public int NextTokenIndex { get; }

                public Success(TResult result, int nextTokenIndex)
                {
                    Result = result;
                    NextTokenIndex = nextTokenIndex;
                }
            }

            public sealed class Failure : SentenceParseResult<TResult>
            {
                public int FailedAtIndex { get; }
                public string SentenceName { get; }
                public string ExpectedToken { get; }
                public string Example { get; }

                public Failure(int failedAt, string sentenceName, string expected, string example)
                {
                    FailedAtIndex = failedAt;
                    SentenceName = sentenceName;
                    ExpectedToken = expected;
                    Example = example;
                }
            }

            private SentenceParseResult() { }
        }
    }

    public abstract class ParseResult
    {
        public sealed class Success : ParseResult
        {
            public IReadOnlyList<Sentence> Sentences { get; }

            public Success(IReadOnlyList<Sentence> sentences)
            {
                Sentences = sentences;
            }
        }

        public sealed class Failure : ParseResult
        {
            // (line, message)
            public IReadOnlyList<(int, string)> Errors { get; }

            public Failure(IReadOnlyList<(int, string)> errors)
            {
                Errors = errors;
            }
        }

        private ParseResult() { }
    }
}
