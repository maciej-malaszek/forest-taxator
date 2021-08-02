using System;
using System.Collections.Generic;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Utils
{
    public static class ProgressTracker
    {
        public static Dictionary<EProgressStage, IEnumerable<Action<string, double, double>>> Actions { get; set; } = new();

        public static void Progress(EProgressStage progressStage, string status, double step, double total)
        {
            foreach (var action in Actions[progressStage])
            {
                action(status, step, total);
            }
        }
    }
}