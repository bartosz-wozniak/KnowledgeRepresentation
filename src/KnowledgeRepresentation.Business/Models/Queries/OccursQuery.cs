using System.Linq;

namespace KnowledgeRepresentation.Business.Models.Queries
{
    public abstract class OccursQuery : Query
    {
        public int Time { get; set; }
        public int ActionIndex { get; set; }
    }


    public class EverOccursQuery : OccursQuery
    {
        public override bool EvaluateQuery()
        {
            //gdzie Model.States jest węzłem w chwili -1
            return Model.States.Any(CheckIfModelIsCorrectFromState);
        }

        private bool CheckIfModelIsCorrectFromState(State state)
        {
            if (state.CurrentTime == Time)
                return state.CurrentAction.HasValue && state.CurrentAction.Value == ActionIndex;

            return state.Children.Any(CheckIfModelIsCorrectFromState);
        }

    }

    public class AlwaysOccursQuery : OccursQuery
    {
        public override bool EvaluateQuery()
        {
            //gdzie Model.States jest węzłem w chwili -1
            return Model.States.All(CheckIfModelIsCorrectFromState);
        }

        private bool CheckIfModelIsCorrectFromState(State state)
        {
            if (state.CurrentTime == Time)
                return state.CurrentAction.HasValue && state.CurrentAction.Value == ActionIndex;

            return state.Children.All(CheckIfModelIsCorrectFromState);
        }
    }
}
