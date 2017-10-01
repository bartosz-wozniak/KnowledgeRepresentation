namespace KnowledgeRepresentation.Business.Models.Queries
{
    public abstract class Query
    {
        public Model Model { get; set; }

        public bool Result { get; set; }

        public abstract bool EvaluateQuery();
    }
}
