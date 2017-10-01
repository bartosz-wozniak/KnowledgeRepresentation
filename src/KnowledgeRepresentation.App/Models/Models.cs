using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using KnowledgeRepresentation.Business.Parsing;
using KnowledgeRepresentation.Business.StraightforwardAlgorithm;

namespace KnowledgeRepresentation.App.Models
{
    public class AlgorithmResultModel
    {
        public IList<ScenarioResultsModel> ScenarioResults { get; set; } = new List<ScenarioResultsModel>();

        public AlgorithmResultModel(Dictionary<int, SimpleModelSaver> models, ExtractionResult extractionResult)
        {
            var reverseFluents = extractionResult.Fluents.ToDictionary(x => x.Value, x => x.Key);
            var reverseActions = extractionResult.Actions.ToDictionary(x => x.Value, x => x.Key);
            var reverseScenarios = extractionResult.Scenarios.ToDictionary(x => x.Value, x => x.Key);

            foreach (var scenario in models)
            {
                var name = reverseScenarios[scenario.Key];
                var result = new ScenarioResultsModel { Name = name, ModelsCount = scenario.Value.TotalModelsCount};
                for (var i = 0; i < scenario.Value.SavedModels.Count; i++)
                {
                    var model = scenario.Value.SavedModels[i];
                    var modelModel = new ModelModel
                    {
                        Name = $"Model {i + 1}",
                        HistoryTimeColumns = GetDataGridColumns(model.EndTime, true),
                        OcclusionTimeColumns = GetDataGridColumns(model.EndTime),
                        ActionsTimeColumns = GetDataGridColumns(model.EndTime)
                    };
                    modelModel.Actions.Add(new HistoryModel
                    {
                        Values = model.Actions.Select(a => a == -1 ? "" : reverseActions[a]).ToArray()
                    });
                    modelModel.Occlusion.Add(new HistoryModel
                    {
                        Values = model.Occlusion.Select(x => x != null ? string.Join(",", x.Select(o => reverseFluents[o])) : "").ToArray()
                    });
                    for (var j = 0; j < reverseFluents.Count; j++)
                    {
                        modelModel.History.Add(new HistoryModel
                        {
                            Name = reverseFluents[j],
                            Values = model.History.Select(h => h[j].ToString()).ToArray()
                        });
                    }
                    result.Models.Add(modelModel);
                }
                if (result.Models.Any())
                {
                    ScenarioResults.Add(result);
                }
            }
        }

        // breaks MVVM so badly, but works ;)
        private static ObservableCollection<DataGridColumn> GetDataGridColumns(int endTime, bool withTitle = false)
        {
            var collection = new ObservableCollection<DataGridColumn>();
            if (withTitle)
            {
                collection.Add(new DataGridTextColumn {Header = "", Binding = new Binding("Name")});
            }
            foreach (var n in Enumerable.Range(0, endTime))
            {
                collection.Add(new DataGridTextColumn
                {
                    Header = n.ToString(),
                    Binding = new Binding($"Values[{n}]"),
                });
            }
            return collection;
        }
    }

    public class ScenarioResultsModel
    {
        public string Name { get; set; }
        public int ModelsCount { get; set; }
        public IList<ModelModel> Models { get; set; } = new List<ModelModel>();
    }

    public class ModelModel
    {
        public string Name { get; set; }

        public ObservableCollection<DataGridColumn> HistoryTimeColumns { get; set; }
        public ObservableCollection<DataGridColumn> OcclusionTimeColumns { get; set; }
        public ObservableCollection<DataGridColumn> ActionsTimeColumns { get; set; }

        public IList<HistoryModel> History { get; set; } = new List<HistoryModel>();
        public IList<HistoryModel> Actions { get; set; } = new List<HistoryModel>();
        public IList<HistoryModel> Occlusion { get; set; } = new List<HistoryModel>();
    }

    public class HistoryModel
    {
        public string Name { get; set; }
        public string[] Values { get; set; }
    }
}
