using System.Collections.Generic;
using System.Linq;
using KnowledgeRepresentation.Business.Parsing;
using KnowledgeRepresentation.Business.StraightforwardAlgorithm;
using Xunit;

namespace KnowledgeRepresentation.UnitTests
{
    public class AcceptanceTests
    {
        [Fact]
        public void OneActionWithoutConditionScenario_ShouldBeEverAndAlwaysExecutable()
        {
            var results = Evaluate(
            @"action0 lasts 3 causes f1 

                Scenariusz1:
                T = 5
                ACS = (action0, 1)
                OBS = (f1, 0)

                ever executable Scenariusz1
                always executable Scenariusz1"
            );
            Assert.Equal(true, results[0].Response);
            Assert.Equal(true, results[1].Response);
        }

        [Fact]
        public void OneActionWithConditionScenario_ShouldBeEverButNotAlwaysExecutable()
        {
            var results = Evaluate(
            @"action0 lasts 3 causes f2 if f1

                Scenariusz1:
                T = 5
                ACS = (action0, 1)
                OBS = 

                ever executable Scenariusz1
                always executable Scenariusz1"
            );
            Assert.Equal(true, results[0].Response);
            Assert.Equal(false, results[1].Response);
        }


        [Fact]
        public void FluentSetInPerformedAction_ShouldAlwaysHolds()
        {
            var results = Evaluate(
            @"action0 lasts 3 causes f1

                Scenariusz1:
                T = 5
                ACS = (action0, 0)
                OBS = 

                ever holds f1 at 4 when Scenariusz1
                always holds f1 at 4 when Scenariusz1"
            );
            Assert.Equal(true, results[0].Response);
            Assert.Equal(true, results[1].Response);
        }

        [Fact]
        public void FluentUnderOcclusion_ShouldEverButNotAlwaysHolds()
        {
            var results = Evaluate(
            @"action0 lasts 3 causes f1

                Scenariusz1:
                T = 5
                ACS = (action0, 0)
                OBS = 

                ever holds f1 at 2 when Scenariusz1
                always holds f1 at 2 when Scenariusz1"
            );
            Assert.Equal(true, results[0].Response);
            Assert.Equal(false, results[1].Response);
        }

        [Fact]
        public void FluentReleasedInAction_ShouldEverButNotAlwaysHolds()
        {
            var results = Evaluate(
            @"action0 lasts 3 releases f1

                Scenariusz1:
                T = 5
                ACS = (action0, 0)
                OBS = (f1, 0)

                ever holds f1 at 4 when Scenariusz1
                always holds f1 at 4 when Scenariusz1"
            );
            Assert.Equal(true, results[0].Response);
            Assert.Equal(false, results[1].Response);
        }

        [Fact]
        public void FluentSetInInvokedAction_ShouldAlwaysHolds()
        {
            var results = Evaluate(
            @"action0 lasts 3 causes f1
              action0 invokes action1 after 1
              action1 lasts 3 causes f2

                Scenariusz1:
                T = 10
                ACS = (action0, 0)
                OBS = 

                ever holds f2 at 8 when Scenariusz1
                always holds f2 at 8 when Scenariusz1"
            );
            Assert.Equal(true, results[0].Response);
            Assert.Equal(true, results[1].Response);
        }

        [Fact]
        public void FluentSetInTriggeredAction_ShouldAlwaysHolds()
        {
            var results = Evaluate(
            @"action0 lasts 3 causes f1
              f1 triggers action1
              action1 lasts 3 causes f2

                Scenariusz1:
                T = 10
                ACS = (action0, 0)
                OBS = 

                ever holds f2 at 8 when Scenariusz1
                always holds f2 at 8 when Scenariusz1"
            );
            //There is actually no models for this scenario... To justify that note that at 3 after action0 finish
            //f1 is true (f2 is true or false). At 3 action1 starts and it changes only f2, so f1 is still true.
            //For this reason every model is contradictory, because at 4 action1 is running, but f1 is true, so action1 
            //should be invoked... The question is whether we accept this situation as valid or not?
            Assert.Equal(false, results[0].Response);
            //If there is no models always returns true of course.
            Assert.Equal(true, results[1].Response);
        }

        [Fact]
        public void AlwaysInvokedAction_ShouldAlwaysOccur()
        {
            var results = Evaluate(
            @"action0 lasts 3 causes f1
              action0 invokes action1 after 1
              action1 lasts 3 causes f2

                Scenariusz1:
                T = 10
                ACS = (action0, 0)
                OBS = 

                action1 ever occurs at 5 when Scenariusz1
                action1 always occurs at 5 when Scenariusz1"
            );
            //there exists 64 models for this situation (algorithm calculates them correctly I think).
            //There was a mistake in algorithm here. I set actionId badly. Thank you!
            Assert.Equal(true, results[0].Response);
            Assert.Equal(true, results[1].Response);
        }

        [Fact]
        public void AlwaysTriggeredAction_ShouldAlwaysOccur()
        {
            var results = Evaluate(
            @"action0 lasts 3 causes f1
              f1 triggers action1
              action1 lasts 3 causes f2

                Scenariusz1:
                T = 10
                ACS = (action0, 0)
                OBS = 

                action1 ever occurs at 5 when Scenariusz1
                action1 always occurs at 5 when Scenariusz1"
            );
            //This is false. See exclamation for FluentSetInTriggeredAction_ShouldAlwaysHolds (no models possible)
            Assert.Equal(false, results[0].Response);
            Assert.Equal(true, results[1].Response);
        }

        [Fact]
        public void ActionWithTwoEffects()
        {
            var results = this.Evaluate(
            @"action0 lasts 3 causes f1
              action0 lasts 4 causes f2
                Scenariusz1:
                T = 6
                ACS = (action0, 0)
                OBS = 
                ever holds f2 at 5 when Scenariusz1
                ever holds f1 at 5 when Scenariusz1"
            );
            // check if both effects have been executed successfully
            Assert.Equal(true, results[0].Response);
            Assert.Equal(true, results[1].Response);
        }

        [Fact]
        public void ActionWithTwoEffects_OccusionCheck()
        {
            var results = this.Evaluate(
            @"action0 lasts 3 causes f1
              action0 lasts 4 causes f2
                Scenariusz1:
                T = 6
                ACS = (action0, 0)
                OBS = 
                always holds f1 at 3 when Scenariusz1
                always holds f2 at 3 when Scenariusz1
                ever holds f2 at 3 when Scenariusz1"
            );
            // no longer under occlusion and set by action0
            Assert.Equal(true, results[0].Response);
            // ought to be false, since it's still under occlusion
            Assert.Equal(false, results[1].Response);
            // still for some models it should hold 
            Assert.Equal(true, results[2].Response);
        }

        [Fact]
        public void ActionWithDifferentEffects()
        {
            var results = this.Evaluate(
            @"action0 lasts 3 causes f1 if f3
              action0 lasts 4 causes f2 if !f3
                Scenariusz1:
                T = 6
                ACS = (action0, 1)
                OBS = (!f3, 0)

                always holds f2 at 5 when Scenariusz1"
            );
            Assert.Equal(true, results[0].Response);
        }

        [Fact]
        public void ComputesInputWithMultipleScenarios()
        {
            var results = Evaluate(@"work lasts 5 causes PROJECT if !TIRED
                work lasts 10 causes PROJECT if TIRED
                sleep lasts 8 causes !TIRED
                skates lasts 1 causes TIRED

                Scenariusz1:
                    T = 16
                    ACS = (skates, 0) (work, 2) (sleep, 7)
                    OBS = (!PROJECT, 0) (!TIRED & PROJECT, 16)

                Scenariusz2:
                    T = 16
                    ACS = (work, 0) (sleep, 6)
                    OBS = (!PROJECT, 0) (!TIRED & PROJECT, 16)

                ever executable Scenariusz2
                ever executable Scenariusz1");

            Assert.Equal(true, results[0].Response);
            Assert.Equal(false, results[1].Response);
        }

        private IList<QueryResponse> Evaluate(string data)
        {
            //var modelSaver = new SimpleModelSaver();
            var parseResult = Parser.Parse(data);
            var succ = Assert.IsType<ParseResult.Success>(parseResult);
            return AlgorithmRunner.EvaluateQueries(succ).queryResults;
        }
    }
}

