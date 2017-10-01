using System.Linq;
using KnowledgeRepresentation.Business.Algorithm;
using KnowledgeRepresentation.Business.Parsing;

namespace KnowledgeRepresentation.Business.Models.Queries
{
    public abstract class HoldsQuery : Query
    {
        public int Time { get; set; }
        public Expression<int> Expression { get; set; }
    }

    public class EverHoldsQuery : HoldsQuery
    {
        public override bool EvaluateQuery()
        {
            return Model.States.Any(EvaluateChildren);
        }

        private bool EvaluateChildren(State state)
        {
            if (Time == state.CurrentTime)
            {
                return ExpressionMethods.CheckExpressionSatisfaction(Expression, state.Fluents) == FluentValue.True;
            }
            return state.Children.Any(EvaluateChildren);
        }
    }
    public class AlwaysHoldsQuery : HoldsQuery
    {
        public override bool EvaluateQuery()
        {
            return Model.States.All(EvaluateChildren);
        }

        private bool EvaluateChildren(State state)
        {
            if (Time == state.CurrentTime)
            {
                return ExpressionMethods.CheckExpressionSatisfaction(Expression, state.Fluents) == FluentValue.True;
            }
            return state.Children.All(EvaluateChildren);
        }
    }
}