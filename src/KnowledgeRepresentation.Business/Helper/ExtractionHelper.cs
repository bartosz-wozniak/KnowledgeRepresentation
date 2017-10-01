using KnowledgeRepresentation.Business.Models;
using KnowledgeRepresentation.Business.Parsing;
using System.Collections.Generic;

namespace KnowledgeRepresentation.Business.Helper
{
    using SenI = Sentence<int>;

    public static class ExtractionHelper
    {
        public static List<ActionDescription> ExtractActions(this ExtractionResult result)
        {
            var actions = new List<ActionDescription>();
            foreach(var a in result.Sentences)
            {
                switch (a)
                {
                    case SenI.Causes c:
                        actions.Add(new ActionDescription(c.Action, c.Duration, c.Result, c.Condition));
                        break;
                    case SenI.Releases r:
                        actions.Add(new ActionDescription(r.Action, r.Duration, r.Result, r.Condition));
                        break;
                }
            }
            return actions;
        }
    }
}
