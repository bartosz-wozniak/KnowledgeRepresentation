using System.Collections.Generic;
using System.Linq;
using KnowledgeRepresentation.Business.Parsing;
using KnowledgeRepresentation.Business.StraightforwardAlgorithm;
using Xunit;

namespace KnowledgeRepresentation.UnitTests.Algorithm
{
    public class AlgorithmHelperTests
    {
        public string FileData => @"rozmowa lasts 1 causes ZAKOCHANA if !ZAKOCHANA
            gotowanie lasts 2 causes OBIAD
            gotowanie lasts 3 causes OBIAD & SPALONY if ZAKOCHANA

            Scenariusz1:
            T = 4
            ACS = (gotowanie, 1)
            OBS = (!OBIAD & !SPALONY, 0) (OBIAD, 4)

            always holds !SPALONY & ZAKOCHANA at 4 when Scenariusz1
            ever holds !SPALONY & !ZAKOCHANA at 4 when Scenariusz1";

        [Fact]
        public void Can_Gotowanie_Be_Performed()
        {
            var result = Extract(FileData);
            //czy gotowanie może się zacząć gdy jest zakochana?
            var actionResult = ActionHelper.CanActionBePerformed(result.Sentences, 1, new[] {true, false, false}, 1, 4);
            Assert.True(actionResult.Item1);
            Assert.Equal(1, actionResult.Item2.StartTime);
            Assert.Equal(2, actionResult.Item2.Results.Count);
            Assert.True(actionResult.Item2.Results.Any(r => r.Delay == 2));
            Assert.True(actionResult.Item2.Results.Any(r => r.Delay == 3));
        }

        [Fact]
        public void Get_All_Values_For_Fluents()
        {
            var list = FluentsHelper.InitializeFluents(3).ToList();
            Assert.Equal(8, list.Count);
        }

        [Fact]
        public void Get_All_Values_For_Fluents_With_Occlusion()
        {
            bool[] fluents = new bool[]{true, false, false};
            List<int> occlusion = new List<int>(){0,2};
            var list = FluentsHelper.InitializeFluentsWithOcclusion(fluents, occlusion).ToList();
            Assert.Equal(4, list.Count);
        }

        [Fact]
        public void Is_Scenario_Ever_Executable()
        {
            var modelSaver = new SimpleModelSaver();
            var result = Extract(FileData);
            Business.StraightforwardAlgorithm.Algorithm alg = new Business.StraightforwardAlgorithm.Algorithm(result, result.Scenarios.First().Value, 
                new List<Sentence<int>>(){new Sentence<int>.EverExecutable(result.Scenarios.FirstOrDefault().Value)}, modelSaver);
            alg.FindModels();
            Assert.Equal(10, modelSaver.SavedModels.Count);
        }

        [Fact]
        public void Are_Queries_Response_Valid()
        {
            var result = Extract(FileData);
            Business.StraightforwardAlgorithm.Algorithm alg = new Business.StraightforwardAlgorithm.Algorithm(result, result.Scenarios.First().Value,
                result.Sentences, null);
            var results = alg.FindModels();
            Assert.Equal(2, results.Count);
            Assert.Equal(false, results[0].Response);
            Assert.Equal(true, results[1].Response);
        }

        private static ExtractionResult Extract(string input)
        {
            var result = Parser.Parse(input);
            var succ = Assert.IsType<ParseResult.Success>(result);
            return IdentifierExtractor.Extract(succ.Sentences);
        }
    }
}
