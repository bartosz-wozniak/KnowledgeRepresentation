using System.Collections.Generic;
using KnowledgeRepresentation.Business.Parsing;

namespace KnowledgeRepresentation.Business.Models
{
    using Expression = Expression<int>;

    public class ActionsToPerform
    {
        public List<int>[] Actions { get; set; } //indeksowane chwilami czasu
        public List<Expression>[] Observations { get; set; } //to też
    }
}
