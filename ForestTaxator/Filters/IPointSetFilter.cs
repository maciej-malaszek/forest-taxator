using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public interface IPointSetFilter
    {
        void Filter( PointSet[] groups);
    }
}