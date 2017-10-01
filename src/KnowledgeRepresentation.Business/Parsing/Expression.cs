namespace KnowledgeRepresentation.Business.Parsing
{
    public abstract class Expression<TId>
    {
        public sealed class Identifier : Expression<TId>
        {
            public TId Name { get; }

            public Identifier(TId name)
            {
                Name = name;
            }
        }

        public sealed class True : Expression<TId>
        {}

        public sealed class False : Expression<TId>
        {}

        public sealed class Not : Expression<TId>
        {
            public Expression<TId> Expression { get; }

            public Not(Expression<TId> expression)
            {
                Expression = expression;
            }
        }

        public sealed class And : Expression<TId>
        {
            public Expression<TId> Left { get; }
            public Expression<TId> Right { get; }

            public And(Expression<TId> left, Expression<TId> right)
            {
                Left = left;
                Right = right;
            }
        }

        public sealed class Or : Expression<TId>
        {
            public Expression<TId> Left { get; }
            public Expression<TId> Right { get; }

            public Or(Expression<TId> left, Expression<TId> right)
            {
                Left = left;
                Right = right;
            }
        }
 
        private Expression() { }
    }
}
