using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Microsoft.Win32;
using KnowledgeRepresentation.App.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KnowledgeRepresentation.Business.Parsing;
using KnowledgeRepresentation.Business.Serializing;
using KnowledgeRepresentation.Business.StraightforwardAlgorithm;
using System.Text;
using KnowledgeRepresentation.App.Models;

namespace KnowledgeRepresentation.App.ViewModels
{
    internal sealed class MainWindowViewModel : Screen
    {
        /// <summary>
        ///     Input Text Sentence
        /// </summary>
        public string InputTextSentence { get; set; } = Consts.Labels.InputHello;

        /// <summary>
        ///     Input Text Scenario
        /// </summary>
        public string InputTextScenario { get; set; } = Consts.Labels.InputHello;

        /// <summary>
        ///     Input Text Query
        /// </summary>
        public string InputTextQuery { get; set; } = Consts.Labels.InputHello;

        /// <summary>
        ///     Output Text
        /// </summary>
        public string OutputText { get; set; } = Consts.Labels.OutputText;

        public AlgorithmResultModel AlgorithmResult { get; set; }

        private readonly ICustomDialogManager _dialogManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dialogManager">Dialog Manager</param>
        public MainWindowViewModel(ICustomDialogManager dialogManager)
        {
            _dialogManager = dialogManager;
        }

        /// <summary>
        ///     Compute button onclick handler
        /// </summary>
        public async void Compute()
        {
            try
            {
                var result = Business.Parsing.Parser.Parse(InputTextSentence + Environment.NewLine + InputTextScenario + Environment.NewLine + InputTextQuery);
                switch (result)
                {
                    case ParseResult.Success success:
                        var results = AlgorithmRunner.EvaluateQueries(success);
                        OutputText = string.Join("\n", results.queryResults.Select(q => q.Response).ToArray());
                        AlgorithmResult = new AlgorithmResultModel(results.modelsByScenario, results.extracted);
                        break;
                    case ParseResult.Failure failure:
                        await ShowError(BuildErrorMessage(failure));
                        break;
                }

            }
            catch (Exception ex)
            {
                await ShowError(ex.Message);
            }
        }

        /// <summary>
        ///     Load file button onclick handler
        /// </summary>
        public async void Load()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = Consts.FileFilters.Txt
            };
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            try
            {
                (InputTextSentence, InputTextScenario, InputTextQuery) = Serializer.ReadFile(openFileDialog.FileName);
            }
            catch (Exception ex)
            {
                await ShowError(ex.Message);
            }
        }

        /// <summary>
        ///     Save button onclick handler
        /// </summary>
        public async void Save()
        {
            var openFileDialog = new SaveFileDialog
            {
                Filter = Consts.FileFilters.Txt
            };
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            try
            {
                File.WriteAllText(openFileDialog.FileName, InputTextSentence + Environment.NewLine + InputTextScenario + Environment.NewLine + InputTextQuery);
            }
            catch (Exception ex)
            {
                await ShowError(ex.Message);
            }
        }

        /// <summary>
        ///     Causes button onclick handler
        /// </summary>
        public void Causes()
        {
            InputTextSentence += Environment.NewLine + Consts.Language.Sentences.Causes;
        }

        /// <summary>
        ///     Releases button onclick handler
        /// </summary>
        public void Releases()
        {
            InputTextSentence += Environment.NewLine + Consts.Language.Sentences.Releases;
        }

        /// <summary>
        ///     Triggers button onclick handler
        /// </summary>
        public void Triggers()
        {
            InputTextSentence += Environment.NewLine + Consts.Language.Sentences.Triggers;
        }

        /// <summary>
        ///     Invokes button onclick handler
        /// </summary>
        public void Invokes()
        {
            InputTextSentence += Environment.NewLine + Consts.Language.Sentences.Invokes;
        }

        /// <summary>
        ///     Impossible button onclick handler
        /// </summary>
        public void Impossible()
        {
            InputTextSentence += Environment.NewLine + Consts.Language.Sentences.Impossible;
        }

        /// <summary>
        ///     Scenario button onclick handler
        /// </summary>
        public void Scenario()
        {
            InputTextScenario += Environment.NewLine + Consts.Language.Scenario.Text;
        }

        /// <summary>
        ///     AlwaysExecutable button onclick handler
        /// </summary>
        public void AlwaysExecutable()
        {
            InputTextQuery += Environment.NewLine + Consts.Language.Queries.AlwaysExecutable;
        }

        /// <summary>
        ///     EverExecutable button onclick handler
        /// </summary>
        public void EverExecutable()
        {
            InputTextQuery += Environment.NewLine + Consts.Language.Queries.EverExecutable;
        }

        /// <summary>
        ///     AlwaysHolds button onclick handler
        /// </summary>
        public void AlwaysHolds()
        {
            InputTextQuery += Environment.NewLine + Consts.Language.Queries.AlwaysHolds;
        }

        /// <summary>
        ///     EverHolds button onclick handler
        /// </summary>
        public void EverHolds()
        {
            InputTextQuery += Environment.NewLine + Consts.Language.Queries.EverHolds;
        }

        /// <summary>
        ///     AlwaysOccurs button onclick handler
        /// </summary>
        public void AlwaysOccurs()
        {
            InputTextQuery += Environment.NewLine + Consts.Language.Queries.AlwaysOccurs;
        }

        /// <summary>
        ///     EverOccurs button onclick handler
        /// </summary>
        public void EverOccurs()
        {
            InputTextQuery += Environment.NewLine + Consts.Language.Queries.EverOccurs;
        }

        private async Task ShowError(string message)
        {
            await _dialogManager.DisplayMessageBox(Consts.Errors.Title, Consts.Errors.Info + message);
        }

        private static string BuildErrorMessage(ParseResult.Failure failure)
        {
            var builder = new StringBuilder();
            // If you really want to extract this to consts - go ahead, it should be done like that but I'm lazy
            builder.AppendLine("We couldn't parse the input. Errors:");
            foreach (var item in failure.Errors)
            {
                var (line, msg) = item;
                if (line == 0)
                {
                    builder.AppendLine(msg);
                }
                else
                {
                    builder
                        .Append("Line ")
                        .Append(line)
                        .Append(": ")
                        .AppendLine(msg);
                }
            }
            return builder.Remove(builder.Length - 1, 1).ToString();
        }
    }
}
