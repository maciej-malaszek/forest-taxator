using System.Collections.Generic;
using System.Threading.Tasks;
using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public interface IPointSetFilter
    {
        IList<PointSet> Filter(IList<PointSet> pointSets);
    }
}