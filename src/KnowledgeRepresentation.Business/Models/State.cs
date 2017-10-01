using System.Collections.Generic;

namespace KnowledgeRepresentation.Business.Models
{
    public class State
    {
        public int CurrentTime { get; set; }
        /// <summary>
        /// Action that starts at this state
        /// </summary>
        public int? CurrentAction { get; set; }
        public FluentValue[] Fluents { get; set; }
        public List<State> Children { get; set; } = new List<State>();
    }
}
