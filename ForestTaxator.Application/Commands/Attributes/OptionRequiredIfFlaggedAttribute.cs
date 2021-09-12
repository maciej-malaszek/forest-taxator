using System;

namespace ForestTaxator.Application.Commands.Attributes
{
    public class OptionRequiredIfFlaggedAttribute : Attribute
    {
        public string FlagName { get; set; }
        public bool FlagValue { get; set; }
        public OptionRequiredIfFlaggedAttribute(string flagName, bool value = true)
        {
            FlagName = flagName;
            FlagValue = value;
        }
    }
}