using System.Collections.Generic;

namespace ForestTaxator.TestApp.Utils
{
    public class ExpressionDefinition
    {
        public class VariableDefinition
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public int Order { get; set; }
        }

        public List<VariableDefinition> Variables { get; set; }
        public string Expression { get; set; }
    }
}