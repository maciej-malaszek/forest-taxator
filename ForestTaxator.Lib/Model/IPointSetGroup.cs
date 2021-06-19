using System.Collections.Generic;

namespace ForestTaxator.Model
{
    public interface IPointSetGroup
    {
        public IList<PointSet> PointSets { get; set; }
    }
}