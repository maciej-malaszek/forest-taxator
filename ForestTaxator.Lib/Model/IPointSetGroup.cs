using System.Collections.Generic;

namespace ForestTaxator.Lib.Model
{
    public interface IPointSetGroup
    {
        public IList<PointSet> PointSets { get; set; }
    }
}