using System;
using System.Collections.Generic;
using ForestTaxator.TestApp.Utils;
using NUnit.Framework;

namespace ForestTaxator.UnitTests.Utils
{
    public class FunctionBuilderTests
    {
        [Test]
        public void FunctionBuilder_Executes_Simple_Math_Expressions()
        {
            var expressionDefinition = new ExpressionDefinition
            {
                Variables = new List<ExpressionDefinition.VariableDefinition>
                {
                    new()
                    {
                        Name = "height",
                        Order = 1,
                        Type = "System.Double"
                    },
                },
                Expression = "pow(height + 1,2)"
            };
            var functor = FunctionBuilder<double>.CreateFunction(expressionDefinition);
            var result = functor(new object[] {2});
            Assert.AreEqual(9, result);
        }
    }
}