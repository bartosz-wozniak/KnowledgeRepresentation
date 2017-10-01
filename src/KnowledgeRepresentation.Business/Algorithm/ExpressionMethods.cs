using System;
using System.Collections.Generic;
using KnowledgeRepresentation.Business.Models;
using KnowledgeRepresentation.Business.Parsing;

namespace KnowledgeRepresentation.Business.Algorithm
{
    public static class ExpressionMethods
    {

        /// <summary>
        /// Checks weather given fluents satisfy the expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="fluents"></param>
        /// <returns></returns>
        /// 
        /// // okluzja jest mocniejsza niż undefined
        public static FluentValue CheckExpressionSatisfaction(Expression<int> expression, FluentValue[] fluents)
        {
            switch (expression)
            {
                case Expression<int>.And and:
                    {
                        var left = CheckExpressionSatisfaction(and.Left, fluents);
                        var right = CheckExpressionSatisfaction(and.Right, fluents);
                        return FluentValueMethods.And(left, right);
                    }
                case Expression<int>.Or or:
                    {
                        var left = CheckExpressionSatisfaction(or.Left, fluents);
                        var right = CheckExpressionSatisfaction(or.Right, fluents);
                        return FluentValueMethods.Or(left, right);
                    }
                case Expression<int>.Not not:
                    var toNegate = CheckExpressionSatisfaction(not.Expression, fluents);
                    return FluentValueMethods.Not(toNegate);
                case Expression<int>.Identifier identifier:
                    return fluents[identifier.Name];
                case Expression<int>.True _:
                    return FluentValue.True;
                case Expression<int>.False _:
                    return FluentValue.False;
                default:
                    //todo: Catch me if you can ;) 
                    throw new InvalidOperationException("Given expression is not supported in conditions");
            }
        }

        public static List<FluentValue[]> GetAllFluentEvaluationsSatisfyingExpression(Expression<int> expression)
        {
            var fluentsInExpression = new List<int>();

            FindAllFluentsInExpression(expression, fluentsInExpression);
            FluentValue[] fluents = new FluentValue[ModelsBuilder.FluentsCount];
            var fluentNumber = 0;
            var correctValues = new List<FluentValue[]>();

            FindEvaluations(expression, fluentsInExpression, fluents, fluentNumber, correctValues);

            return correctValues;
        }

        private static void FindEvaluations(Expression<int> expression, List<int> fluentsInExpression, FluentValue[] fluents, int fluentNumber, List<FluentValue[]> correctValues)
        {
            if (fluentNumber == fluentsInExpression.Count)
            {
                var result = CheckExpressionSatisfaction(expression, fluents);

                if (result == FluentValue.True)
                    correctValues.Add((FluentValue[])fluents.Clone());

                return;
            }

            foreach (FluentValue fluentValue in Enum.GetValues(typeof(FluentValue)))
            {
                var fluentIndex = fluentsInExpression[fluentNumber];

                fluents[fluentIndex] = fluentValue;

                FindEvaluations(expression, fluentsInExpression, fluents, fluentNumber + 1, correctValues);
            }
        }

        public static List<int> FindAllFluentsInExpression(Expression<int> expression)
        {
            List<int> fluents = new List<int>();
            FindAllFluentsInExpression(expression, fluents);
            return fluents;
        }

        private static void FindAllFluentsInExpression(Expression<int> expression, List<int> fluents)
        {
            switch (expression)
            {
                case Expression<int>.And and:
                    FindAllFluentsInExpression(and.Left, fluents);
                    FindAllFluentsInExpression(and.Right, fluents);
                    break;
                case Expression<int>.Or or:
                    FindAllFluentsInExpression(or.Left, fluents);
                    FindAllFluentsInExpression(or.Right, fluents);
                    break;
                case Expression<int>.Not not:
                    FindAllFluentsInExpression(not.Expression, fluents);
                    break;
                case Expression<int>.Identifier identifier:
                    fluents.Add(identifier.Name);
                    break;
                default:
                    //todo: Catch me if you can ;) 
                    throw new InvalidOperationException("Given expression is not supported in conditions");
            }
        }
    }
}
