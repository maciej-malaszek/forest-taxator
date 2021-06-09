using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandLine;
using ForestTaxator.TestApp.Commands.Attributes;

namespace ForestTaxator.TestApp.Commands.Extensions
{
    public static class ParserExtensions
    {
        private static bool ValidateRequiredAttributes<T>(T command)
        {
            var properties = typeof(T).GetProperties();
            var propertiesWithRequiredIfFlagged = properties.Where(p =>
                p.GetCustomAttribute<OptionRequiredIfFlaggedAttribute>() != null
            );

            var errors = new List<string>();
            foreach (var propertyInfo in propertiesWithRequiredIfFlagged)
            {
                var attributeData = propertyInfo.GetCustomAttribute<OptionRequiredIfFlaggedAttribute>();
                var optionData = propertyInfo.GetCustomAttribute<OptionAttribute>();
                if (attributeData == null || optionData == null) 
                {
                    continue;
                }
                var flagValue = (bool)(typeof(T).GetProperty(attributeData.FlagName)?.GetValue(command) ?? false);
                if (flagValue != attributeData.FlagValue)
                {
                    continue;
                }
                var value = propertyInfo.GetValue(command);
                if (value is null || value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
                {
                    errors.Add($"Required option '{optionData.ShortName}, {optionData.LongName}' is missing.");
                }
                
            }

            if (errors.Count <= 0)
            {
                return true;
            }

            Console.WriteLine("ERROR(S):");
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }

            return false;

        }
        public static TResult ExecuteMapping<T, TResult>(this Parser parser, T command, Func<T, TResult> func)
        {
            if (ValidateRequiredAttributes(command))
            {
                return func(command);
            }
            Environment.Exit(0);
            return default;
        }
    }
}