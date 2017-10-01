using System.Collections.Generic;
using KnowledgeRepresentation.Business.Parsing;

namespace KnowledgeRepresentation.Business.StraightforwardAlgorithm
{
    public class Model
    {
        public int Time { get; set; }

        public bool[] Fluents { get; set; }

        public State State { get; set; }

        public int TimeToActionEnd { get; set; }

        public int CurrentActionId { get; set; }

        public Dictionary<Sentence<int>.Causes, int> CurrentActionDescriptions { get; set; } //konkretne opisy wykonywanej akcji - zdania causes
        //todo: dodać jeszcze releasees

        public Dictionary<int, int> InvokedActionsInTime { get; set; } //czas w którym powinna się rozpocząć akcja, id akcji do wykonania
    }
    

    public enum State
    {
        Idle = 0,
        ActionInProgress = 1
    }
}
