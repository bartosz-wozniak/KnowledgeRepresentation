namespace KnowledgeRepresentation.Business.Parsing
{
    public enum KeywordType
    {
        Lasts,
        Causes,
        If,
        Releases,
        Triggers,
        Invokes,
        After,
        Impossible,

        Always,
        Ever,
        Executable,
        Holds,
        Occurs,
        At,
        When,

        TimeVar,
        ActionsVar,
        ObservationsVar,
        Colon,
        Assign
    }

    public enum SpecialType
    {
        Open, Close, Comma,
        And, Or, Not,
        True, False
    }

    public abstract class AttributedToken
    {
        public int LineNo { get; }
        public int CharNo { get; }

        public sealed class Keyword : AttributedToken
        {
            public KeywordType Type { get; }

            public Keyword(KeywordType type, int lineNo, int charNo)
                : base(lineNo, charNo)
            {
                Type = type;
            }
        }

        public sealed class Expression : AttributedToken
        {
            public SpecialType Type { get; }

            public Expression(SpecialType type, int lineNo, int charNo)
                : base(lineNo, charNo)
            {
                Type = type;
            }
        }

        public sealed class Identifier : AttributedToken
        {
            public string Name { get; }

            public Identifier(string name, int lineNo, int charNo)
                : base(lineNo, charNo)
            {
                Name = name;
            }
        }

        public sealed class Number : AttributedToken
        {
            public int Value { get; }

            public Number(int v, int lineNo, int charNo)
                : base(lineNo, charNo)
            {
                Value = v;
            }
        }

        public sealed class NewLine : AttributedToken
        {
            public NewLine(int lineNo, int charNo)
                : base(lineNo, charNo)
            { }
        }

        private AttributedToken(int lineNo, int charNo)
        {
            LineNo = lineNo;
            CharNo = charNo;
        }
    }
}
