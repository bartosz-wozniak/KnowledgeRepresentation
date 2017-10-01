namespace KnowledgeRepresentation.Business.Parsing
{
    public abstract class Token
    {
        public int LineNo { get; }
        public int CharNo { get; }

        public sealed class Number : Token
        {
            public int Value { get; }

            public Number(int v, int lineNo, int charNo)
                : base(lineNo, charNo)
            {
                Value = v;
            }
        }

        public sealed class String : Token
        {
            public string Value { get; }

            public String(string v, int lineNo, int charNo)
                : base(lineNo, charNo)
            {
                Value = v;
            }
        }

        public sealed class Special : Token
        {
            public char Value { get; }

            public Special(char v, int lineNo, int charNo)
                : base(lineNo, charNo)
            {
                Value = v;
            }
        }

        public sealed class NewLine : Token
        {
            public NewLine(int lineNo, int charNo)
                : base(lineNo, charNo)
            { }
        }

        private Token(int lineNo, int charNo)
        {
            LineNo = lineNo;
            CharNo = charNo;
        }
    }
}