using System;
using System.Collections.Generic;
using System.Linq;
using KnowledgeRepresentation.Business.Parsing;
// ReSharper disable TailRecursiveCall

namespace KnowledgeRepresentation.Business.StraightforwardAlgorithm
{
    public static class ModelsBuilder
    {
        public static IList<Model> BuildModels(ExtractionResult extractionResult, IEnumerable<Sentence<int>> queries)
        {
            var scenarios = new List<Model>();

            for (var i = 0; i < extractionResult.Scenarios.Count; ++i)
            {
                scenarios[i] = BuildModelsForScenario(extractionResult, i, queries);
            }
            return scenarios;
        }


        private static Model BuildModelsForScenario(ExtractionResult extractionResult, int scenarioId, IEnumerable<Sentence<int>> queries)
        {
            var root = new Model
            {
                Time = -1,
                Fluents = null
            };

            var initialModels = new List<Model>();

            foreach (var fluents in InitializeFluents(extractionResult.Fluents.Keys.Count()))
            {
                initialModels.Add(new Model
                {
                    Fluents = fluents,
                    Time = 0,
                    CurrentActionId = -1 //wyciągnąć do jakiegoś ładnego defaultu
                });
            }

            foreach (var initModel in initialModels)
            {
                TryInvokeAction(initModel, extractionResult, scenarioId);
            }
            return root;
        }

        private static void TryInvokeAction(Model model, ExtractionResult extractionResult, int scenarioId)
        {
            //walidujemy czy model jest poprawny - stany fluentów zgodne są z obserwacjami
            if (!ValidateModel(model, extractionResult, scenarioId))
                return; //jeżeli nie, model jest zły, odrzucamy, wracamy rekurencją do jego rodzica

            var childModels = new List<Model>(); //modele do kolejnego poziomu

            //jeżeli jest okej, to mamy dwie możliwości: wykonwana jest akcja lub nie
            //przypadek gdy wykonywana jest jakaś akcja:
            if (model.CurrentActionId != -1)
            {
                //sprawdzamy, czy możemy zakończyć jakąś akcję
                foreach (var actionDescription in model.CurrentActionDescriptions)
                {
                    if (actionDescription.Value == 0) //skończył się czas wykonywania podakcji; musi zachodzić jej efekt
                        //sprawdzić, czy obecne wartościowanie fluentów jest poprawne, jak tak, to idziemy dalej, jak nie odrzucamy
                        if (!ValidateModelForCondition(model, actionDescription.Key.Result))
                            return;
                        else
                        {
                            model.CurrentActionDescriptions
                                .Remove(actionDescription.Key); //usuwamy akcję po jej zakończeniu
                        }
                }

                //koniec akcji 
                if (model.CurrentActionDescriptions == null || model.CurrentActionDescriptions.Count == 0)
                    model.CurrentActionId = -1;
                else //akcja trwa dalej
                {
                    //todo: powinniśmy wygenerować 2^k nowych modeli, gdzie k jest liczbą fluentów pod okluzją
                    var oclusionFluents = new List<int>();
                    foreach (var action in model.CurrentActionDescriptions)
                    {
                        GetOclusionFluents(action.Key.Result, oclusionFluents);
                    }

                    var childFluents = InitializeOcclusionFluents(model.Fluents, oclusionFluents.Distinct());

                    foreach (var childFluent in childFluents)
                    {
                        childModels.Add(new Model
                        {
                            CurrentActionDescriptions = model.CurrentActionDescriptions,
                            CurrentActionId = model.CurrentActionId,
                            Fluents = childFluent,
                            Time = model.Time + 1,
                            TimeToActionEnd = model.TimeToActionEnd - 1
                        });
                    }
                }
            }



            //szukamy akcji do wykonania
            var actionsToPerform = new List<Sentence<int>.Causes>();

            var actionsToFromScenario = new List<Sentence<int>.Causes>();

            //todo: sprawdzić, czy zgodnie z obserwacjami nie powinniśmy byli wykonywać jakiejś akcji
            //akcje ze scenariusza które rozpoczynają się w czasie t
            var actionsFromScenarioForCurrentTime =
                extractionResult.Sentences.OfType<Sentence<int>.ScenarioDefinition>().ToList()[scenarioId].Actions
                    .Where(item => item.Item2 == model.Time); //to powinna być zawsze jedna akcja

            if (actionsFromScenarioForCurrentTime.Count() > 1) //błąd w scenariuszu - scenariusz wskazuje, że w danej chwili powinny rozpocząć się kilka akcji - sprzeczność
                return;


            if (actionsFromScenarioForCurrentTime != null && actionsFromScenarioForCurrentTime.Count() == 1)
            {
                var action = actionsFromScenarioForCurrentTime.First();

                model.CurrentActionId = action.Item1;
                //wszystkie zdania causes
                foreach (var sentence in extractionResult.Sentences.OfType<Sentence<int>.Causes>().ToList())
                {
                    if (sentence.Action == action.Item1) //czy zdanie causes jest dla poszukiwanej akcji ze scenariusza
                    {
                        //czy warunek rozpoczęcia akcji jest spełniony
                        if (ValidateObservation(sentence.Condition, model.Fluents))
                        {
                            actionsToFromScenario.Add(sentence);
                        }
                    }
                }
            }


            //todo: sprawdzić, czy zgodnie ze stanem nie powinniśmy rozpocząć wykonywania akcji (triggers)
            var actionsFromTriggers = new List<Sentence<int>.Causes>();

            foreach (var triggerSentence in extractionResult.Sentences.OfType<Sentence<int>.Triggers>().ToList())
            {
                //czy warunek rozpoczęcia akcji jest spełniony
                if (ValidateObservation(triggerSentence.Condition, model.Fluents))
                {
                    //wszystkie zdania causes
                    foreach (var sentence in extractionResult.Sentences.OfType<Sentence<int>.Causes>().ToList())
                    {
                        if (sentence.Action == triggerSentence.Action) //czy zdanie causes jest dla poszukiwanej akcji z triggersa
                        {
                            //czy warunek rozpoczęcia akcji jest spełniony
                            if (ValidateObservation(sentence.Condition, model.Fluents))
                            {
                                actionsFromTriggers.Add(sentence);
                            }
                        }
                    }
                }
            }

            if (actionsFromTriggers.Select(x => x.Action).Distinct().Count()>1)
                return; //błąd, stan powoduje wykonanie kilku akcji

            //todo: sprawdzić, czy zgodnie ze wcześniejszym wystąpieniem akcji A nie powinniśmy w tej chwili rozpocząć wykonywania akcji B (invokes)

            var actionsToInvokeAtCurrentTime = model.InvokedActionsInTime?.Where(x => x.Key == model.Time);

            //powinna być tylko jedna taka akcja (jedno id) inaczej błąd
            if (actionsToInvokeAtCurrentTime?.Count() > 1)
                return;


            var actionsFromInvoke = new List<Sentence<int>.Causes>();


            if (actionsToInvokeAtCurrentTime?.Count() == 1)
            {
                var actionToInvoke = actionsToInvokeAtCurrentTime.First();

                foreach (var sentence in extractionResult.Sentences.OfType<Sentence<int>.Causes>().ToList())
                {
                    if (sentence.Action == actionToInvoke.Value) //czy zdanie causes jest dla poszukiwanej akcji
                    {
                        //czy warunek rozpoczęcia akcji jest spełniony
                        if (ValidateObservation(sentence.Condition, model.Fluents))
                        {
                            actionsFromInvoke.Add(sentence);
                        }
                    }
                }

                //jeżeli żadne ze zdań nie może się wykonać to błąd, bo zdanie invokes sugerowało, że akcja powinna zacząć się wykonywać
                if (actionsFromInvoke.Count == 0)
                {
                    return;
                }
            }

            //todo:sprawdzić czy przypadkiem nie wykonujemy jakiejś akcji, a wyszło nam, że powinniśmy rozpocząć już kolejną
            //jeżeli z 3 powyższych todosów wychodzi, że powinniśmy wykonać jakąś akcję, to ją wykonajmy
            //jeżeli jest więcej niż jedna akcja - błąd i return
            //jeżeli takiej nie ma, to nie rób nic (bez rozgałęzień)
            var actionIdFromScenario = actionsToFromScenario.FirstOrDefault()?.Action ?? -1;
            var actionIdFromTriggers = actionsFromTriggers.FirstOrDefault()?.Action ?? -1;
            var actionIdFromInvoke = actionsFromInvoke.FirstOrDefault()?.Action ?? -1;

            if (actionIdFromScenario != -1)
            {
                if (actionIdFromTriggers != -1 && actionIdFromScenario != actionIdFromTriggers)
                    return;//sprzeczność, chcemy wywołać dwie różne akcje
                if (actionIdFromInvoke != -1 && actionIdFromScenario != actionIdFromInvoke)
                    return;//sprzeczność, chcemy wywołać dwie różne akcje

                //todo: zapamiętaj akcje do wykonania
                actionsToPerform = actionsToFromScenario;
            }

            if (actionIdFromTriggers != -1)
            {
                if (actionIdFromInvoke != -1 && actionIdFromTriggers != actionIdFromInvoke)
                    return;//sprzeczność, chcemy wywołać dwie różne akcje

                //todo: zapamiętaj akcje do wykonania
                actionsToPerform = actionsFromTriggers;
            }

            if (actionIdFromInvoke != -1)
            {
                //todo: zapamiętaj akcje do wykonania
                actionsToPerform = actionsFromInvoke;
            }





            //sprawdzamy czas wykonania scenariusza dobiegł końca
            if (model.Time == extractionResult.Sentences.OfType<Sentence<int>.ScenarioDefinition>().ToList()[scenarioId]
                    .Time)
            {
                if (actionsToFromScenario == null || actionsToFromScenario.Count == 0)
                {
                    //todo: sprawdzić kwerendy
                    return; //zwrócić odpowiedź na kwerendy
                }
                return; //błąd, już nic nie powinniśmy robić
            }

            //todo: wykonujemy akcje z actionsToPerform tworząc 2^k modeli
            //the end

            #region OldVersion
            //    {
            //    return new List<Model>
            //    {
            //        new Model
            //        {
            //            NextModels = new List<Model>(),
            //            Fluents = model.Fluents.ToArray(),
            //            Time = model.Time + 1,
            //            State = model.State,
            //            CurrentActionId = model.CurrentActionId,
            //            TimeToActionEnd = model.TimeToActionEnd - 1
            //        }
            //    };
            //}

            //            else
            //            {
            //                foreach (var action in extractionResult.Sentences.OfType<Sentence<int>.ScenarioDefinition>().ToList()[scenarioId].Actions.Where(item => item.Item2 == model.Time))
            //                {
            //                    foreach (var sentence in extractionResult.Sentences.OfType<Sentence<int>.Causes>().ToList())
            //                    {
            //                        if (sentence.Action == action.Item1)
            //                        {
            //                            if (ValidateObservation(sentence.Condition, model.Fluents))
            //                            {
            //                                var oclusionFluents = new List<int>();
            //                                GetOclusionFluents(sentence.Result, oclusionFluents);
            //        var fluents = model.Fluents.ToArray();
            //                                foreach (var fluent in oclusionFluents)
            //                                {
            //                                    //todo: tu trzeba wygenerować 2^k modeli, gdzie k to liczba fluentów w okluzji
            //                                    // fluents[fluent] = null;
            //                                }
            //                                model.NextModels = new List<Model>
            //                                {
            //                                    new Model
            //                                    {
            //                                        NextModels = new List<Model>(),
            //                                        State = State.ActionInProgress,
            //                                        CurrentActionId = action.Item1,
            //                                        Time = model.Time + 1,
            //                                        TimeToActionEnd = sentence.Duration,
            //                                        Fluents = fluents
            //    }
            //};
            //                            }
            //                        }
            //                    }
            //                    foreach (var sentence in extractionResult.Sentences.OfType<Sentence<int>.Releases>().ToList())
            //                    {
            //                        if (sentence.Action == action.Item1)
            //                        {
            //                            if (ValidateObservation(sentence.Condition, model.Fluents))
            //                            {
            //                                var oclusionFluents = new List<int>();
            //                                GetOclusionFluents(sentence.Result, oclusionFluents);
            //var fluents = model.Fluents.ToArray();
            //                                foreach (var fluent in oclusionFluents)
            //                                {
            //                                    //todo: tu trzeba wygenerować 2^k modeli, gdzie k to liczba fluentów w okluzji
            //                                    //fluents[fluent] = null;
            //                                }
            //                                model.NextModels = new List<Model>
            //                                {
            //                                    new Model
            //                                    {
            //                                        NextModels = new List<Model>(),
            //                                        State = State.ActionInProgress,
            //                                        CurrentActionId = action.Item1,
            //                                        Time = model.Time + 1,
            //                                        TimeToActionEnd = sentence.Duration,
            //                                        Fluents = fluents
            //                                    }
            //                                };
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            return new List<Model>(); 
            #endregion
        }

        private static void GetOclusionFluents(Expression<int> sentenceResult, ICollection<int> oclusionFluents)
        {
            switch (sentenceResult)
            {
                case Expression<int>.Identifier id:
                    oclusionFluents.Add(id.Name);
                    return;
                case Expression<int>.And and:
                    GetOclusionFluents(and.Right, oclusionFluents);
                    GetOclusionFluents(and.Left, oclusionFluents);
                    return;
                case Expression<int>.Or or:
                    GetOclusionFluents(or.Right, oclusionFluents);
                    GetOclusionFluents(or.Left, oclusionFluents);
                    return;
                case Expression<int>.Not not:
                    GetOclusionFluents(not.Expression, oclusionFluents);
                    return;
            }
        }

        private static bool ValidateModel(Model model, ExtractionResult extractionResult, int scenarioId)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var observation in extractionResult.Sentences.OfType<Sentence<int>.ScenarioDefinition>().ToList()[scenarioId]
                .Observations.Where(item => item.Item2 == model.Time))
            {
                if (!ValidateObservation(observation.Item1, model.Fluents))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ValidateModelForCondition(Model model, Expression<int> condition)
        {
            return ValidateObservation(condition, model.Fluents);
        }

        private static bool ValidateObservation(Expression<int> observation, IReadOnlyList<bool> fluents)
        {
            switch (observation)
            {
                case Expression<int>.And and:
                    {
                        return ValidateObservation(and.Left, fluents) && ValidateObservation(and.Right, fluents);
                    }
                case Expression<int>.Or or:
                    {
                        return ValidateObservation(or.Left, fluents) || ValidateObservation(or.Right, fluents);
                    }
                case Expression<int>.Not not:
                    {
                        return !ValidateObservation(not.Expression, fluents);
                    }
                case Expression<int>.Identifier id:
                    {
                        return fluents[id.Name] == true;
                    }
                case Expression<int>.True _:
                    {
                        return true;
                    }
                case Expression<int>.False _:
                    {
                        return false;
                    }
            }
            throw new ArgumentException("Invalid Expression Type");
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private static IList<bool[]> InitializeFluents(int count)
        {
            var ret = new List<bool[]>();
            for (var i = 0; i < Math.Pow(2, count); i++)
            {
                ret.Add(new bool[count]);
                for (var j = 0; j < count; ++j)
                {
                    ret[i][j] = ((i >> j) & 1) == 1;
                }
            }
            return ret;
        }

        private static IList<bool[]> InitializeOcclusionFluents(bool[] originalFluentValues, IEnumerable<int> occlusionFluents)
        {
            var occlusionFluentsPermutations = InitializeFluents(occlusionFluents.Count());

            var ret = new List<bool[]>();

            foreach (var permutation in occlusionFluentsPermutations)
            {
                var fluentsForSingleModel = (bool[])originalFluentValues.Clone();
                var i = 0;
                foreach (var fluentId in occlusionFluents)
                {
                    fluentsForSingleModel[fluentId] = permutation[i];
                    i++;
                }
                ret.Add(fluentsForSingleModel);
            }
            return ret;
        }
    }
}
