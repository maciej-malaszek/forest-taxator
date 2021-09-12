using System;
using System.Linq;
using Flee.PublicTypes;

namespace ForestTaxator.Application.Utils
{
    public static class FunctionBuilder<T>
    {
        public static Func<object[], T> CreateFunction(ExpressionDefinition expressionDefinition)
        {
            var context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));

            foreach (var variable in expressionDefinition.Variables)
            {
                context.Variables[variable.Name] = Activator.CreateInstance(Type.GetType(variable.Type)!);
            }

            var e = context.CompileDynamic(expressionDefinition.Expression);
            var orderedVariables = expressionDefinition.Variables.OrderBy(variable => variable.Order).ToArray();
            return objects =>
            {
                for (var i = 0; i < objects.Length; i++)
                {
                    context.Variables[orderedVariables[i].Name] = Convert.ChangeType(objects[i], Type.GetType(orderedVariables[i].Type)!);
                }

                return (T) e.Evaluate();
            };
        }
    }
}