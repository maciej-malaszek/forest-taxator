using System.Collections.Generic;
using ForestTaxator.Application.Utils;
using GeneticToolkit.Genotypes.Collective;
using GeneticToolkit.Phenotypes.Collective;
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

        //[Test]
        // public void FunctionBuilder_Executes_Complex_Delegates()
        // {
        //     var geneticDistributionFilter = new GeneticDistributionFilter(new GeneticDistributionFilterParams
        //     {
        //         GetTrunkThreshold = _ => 0.22
        //     });
        //     var fitnessFunction = geneticDistributionFilter.GetFitnessFunction();
        //     var factory = new CollectivePhenotypeFactory<ParabolicParameters>();
        //     var individualFactory =
        //         new IndividualFactory<CollectiveGenotype<ParabolicParameters>, CollectivePhenotype<ParabolicParameters>>(factory, fitnessFunction);
        //     var individual = individualFactory.CreateRandomIndividual();
        //     
        //     
        //     var expressionDefinition = new ExpressionDefinition
        //     {
        //         Variables = new List<ExpressionDefinition.VariableDefinition>
        //         {
        //             new()
        //             {
        //                 Name = "phenotype",
        //                 Order = 1,
        //                 Type = "GeneticToolkit.Interfaces.IPhenotype"
        //             },
        //             new()
        //             {
        //                 Name = "handler",
        //                 Order = 1,
        //                 Type = "System.Func`2[GeneticToolkit.Interfaces.IPhenotype,System.Double]"
        //             },
        //         },
        //         Expression = "handler(phenotype)"
        //     };
        //     var functor = FunctionBuilder<double>.CreateFunction(expressionDefinition);
        // }
    }
}