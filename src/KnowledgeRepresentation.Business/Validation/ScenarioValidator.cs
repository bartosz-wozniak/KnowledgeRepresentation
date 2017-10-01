using System.Collections.Generic;
using System.Linq;
using KnowledgeRepresentation.Business.Parsing;

namespace KnowledgeRepresentation.Business.Validation
{
    using Sentence = Sentence<int>;

    public static class ScenarioValidator
    {

        private interface IValidator
        {
            bool Validate(IReadOnlyList<Sentence> sentences);
        }

        private class CausesCollisionValidator : IValidator
        {
            /// <summary>
            /// This validator checks whether there are more cause sentences with the same duration for an action
            /// TODO: Actually if the results aren't contradicting eachother we could allow multiple causes sentences 
            /// </summary>
            /// <param name="sentences"></param>
            /// <returns></returns>
            public bool Validate(IReadOnlyList<Sentence> sentences)
            {

                string GetShortCut(Sentence sentence)
                {

                    var cause = (Sentence.Causes)sentence;
                    return $"{cause.Action}-{cause.Duration}";

                }
                // extract causes sentences
                var causes = sentences.Where(
                    s =>
                    {
                        switch (s)
                        {
                            case Sentence.Causes c:
                                return true;
                            default:
                                return false;
                        }
                    }
                );
                var set = new HashSet<string>();

                foreach (var sentence in causes)
                {
                    var key = GetShortCut(sentence);
                    // cause sentence with the same Action and Duration already exists, therefore fail the validation process
                    if (set.Contains(key))
                    {
                        return false;
                    }
                    set.Add(key);
                }
                return true;
            }
        }

        // TODO: more validators possibly

        public static bool Validate(IReadOnlyList<Sentence> scenario)
        {
            // fold on all validators
            return new IValidator[] {new CausesCollisionValidator() /* add more validators here */}
                .Aggregate(true, (res, v) =>
                    res && v.Validate(scenario)
                );   

        }
    }
}