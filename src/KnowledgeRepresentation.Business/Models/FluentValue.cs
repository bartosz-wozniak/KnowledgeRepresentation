namespace KnowledgeRepresentation.Business.Models
{
    public enum FluentValue
    {
        Unknown, // default
        True,
        False,
        UnderOcclusion
    }

    public static class FluentValueMethods
    {
        public static FluentValue And(FluentValue a, FluentValue b)
        {
            if (a == FluentValue.True && b == FluentValue.True)
                return FluentValue.True;

            if (a == FluentValue.False || b == FluentValue.False)
                return FluentValue.False;

            if (a == FluentValue.UnderOcclusion || b == FluentValue.UnderOcclusion)
                return FluentValue.UnderOcclusion;

            return FluentValue.Unknown;
        }

        public static FluentValue Or(FluentValue a, FluentValue b)
        {
            if (a == FluentValue.True || b == FluentValue.True)
                return FluentValue.True;

            if (a == FluentValue.False && b == FluentValue.False)
                return FluentValue.False;

            if (a == FluentValue.UnderOcclusion || b == FluentValue.UnderOcclusion)
                return FluentValue.UnderOcclusion;

            return FluentValue.Unknown;
        }

        public static FluentValue Not(FluentValue a)
        {
            if (a == FluentValue.True)
                return FluentValue.False;

            return a == FluentValue.False ? FluentValue.True : a;
        }
    }
}
