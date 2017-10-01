using System.Collections.Generic;

namespace KnowledgeRepresentation.Business.Models
{
    public class Model
    {
        public List<State> States { get; set; }
        public int FluentCount { get; set; }

        public Model(State rootState)
        {
            States = rootState.Children;
            FluentCount = rootState.Fluents.Length;
        }
    }
}
