namespace KnowledgeRepresentation.Business.Parsing
{
    static class ErrorExtensions
    {
        public static string Describe(this KeywordType type)
        {
            return type.ToString().ToLower();
        }

        public static string Describe(this SpecialType type)
        {
            switch (type)
            {
                case SpecialType.Open:
                    return "')'";
                case SpecialType.Close:
                    return "'('";
                case SpecialType.Comma:
                    return "','";
                case SpecialType.And:
                    return "'&'";
                case SpecialType.Or:
                    return "'|'";
                case SpecialType.Not:
                    return "'!'";
                case SpecialType.True:
                    return "'true'";
                case SpecialType.False:
                    return "'false'";
            }
            return string.Empty;
        }

        public static string Describe(this AttributedToken token)
        {
            switch (token)
            {
                case AttributedToken.Keyword k:
                    return $"keyword '{k.Type.Describe()}'";
                case AttributedToken.Expression e:
                    return $"'{e.Type.Describe()}' character";
                case AttributedToken.Identifier i:
                    return $"identifier '{i.Name}'";
                case AttributedToken.Number n:
                    return $"number '{n.Value}'";
                case AttributedToken.NewLine _:
                    return "new line";
            }
            return string.Empty;
        }
    }
}
