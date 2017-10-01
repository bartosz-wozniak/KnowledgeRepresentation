using KnowledgeRepresentation.Business.Parsing;

namespace KnowledgeRepresentation.Business.Models
{
    using Expression = Expression<int>;

    public class ActionDescription
    {
        public int ActionId { get; set; }
        public int Duration { get; set; }
        public Expression Result { get; set; }
        public Expression Condition { get; set; }

        public ActionDescription(int action, Expression result, Expression condition)
        {
            ActionId = action;
            Result = result;
            Condition = condition;
        }

        public ActionDescription(int action, int duration, Expression result, Expression condition)
            : this(action, result, condition)
        {
            Duration = duration;
        }

        public ActionDescription()
        {
        }
    }
}
