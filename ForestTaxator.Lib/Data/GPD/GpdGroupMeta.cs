namespace ForestTaxator.Data.GPD
{
    public class GpdGroupMeta
    {
        public int Id { get; set; }
        public int Slice { get; set; }
        public long Points { get; set; }
        public string Comment { get; set; }

        public override string ToString()
        {
            return $"#{Id}.{Slice}.{Points}.{Comment}#";
        }

        public static GpdGroupMeta Parse(string data)
        {
            if (data.StartsWith('#') && data.EndsWith('#') == false)
            {
                return null;
            }
            
            var fields = data.Substring(1, data.Length - 2).Split('.');
            return new GpdGroupMeta
            {
                Id = int.Parse(fields[0]),
                Slice = int.Parse(fields[1]),
                Points = long.Parse(fields[2]),
                Comment = fields[3]
            };
        } 
    }
}