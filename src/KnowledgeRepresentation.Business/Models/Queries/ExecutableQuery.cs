using System;

namespace KnowledgeRepresentation.Business.Models.Queries
{
    public class EverExecutableQuery : Query
    {
        public override bool EvaluateQuery()
        {
            return Model.States != null && Model.States.Count > 0;
        }
    }

    public class AlwaysExecutableQuery : Query
    {
        public override bool EvaluateQuery()
        {
            var createdModelsCount = Model.States.Count;

            var allPossibleModelsCount = (int)Math.Round(Math.Pow(2, Model.FluentCount));

            return createdModelsCount == allPossibleModelsCount;
        }
    }
}
