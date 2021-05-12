namespace ForestTaxator.Model
{
    public class Terrain
    {
        public const int MeshesCount = 500;
        public const float MeshSize = 2.5f;

        public double[,] Mesh { get; } = new double[MeshesCount,MeshesCount];

        public Terrain()
        {
            for (var x = 0; x < MeshesCount; x++)
            for (var y = 0; y < MeshesCount; y++)
            {
                Mesh[x,y] = float.MaxValue;
            }
        }
    }
}
