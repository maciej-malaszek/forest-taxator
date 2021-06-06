namespace ForestTaxator.Data.GPD
{
    public class GpdHeader
    {
        #region Enums

        public enum EVersion
        {
            V1,
            UNSUPPORTED
        }

        public enum ESize
        {
            Byte = 1,
            Short = 2,
            Single = 4,
            Double = 8,
            UNSUPPORTED = -1
        }

        public enum EType
        {
            I,
            U,
            F,
            UNSUPPORTED
        }

        public enum EDataType
        {
            ASCII,
            BINARY,
            UNSUPPORTED,
            BINARY_COMPRESSED
        }

        #endregion

        public EVersion Version { get; set; }
        public int Groups { get; set; }
        public float Slice { get; set; }
        public string[] Fields { get; set; }
        public ESize[] Size { get; set; }
        public EType[] Type { get; set; }
        public int Points { get; set; }
        public EDataType DataType { get; set; }
    }
}