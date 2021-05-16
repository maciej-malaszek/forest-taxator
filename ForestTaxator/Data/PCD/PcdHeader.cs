using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ForestTaxator.Data.PCD
{
    public class PcdHeader
    {
        #region Enums

        public enum EVersion
        {
            V7,
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
            UNSUPPORTED
        }

        #endregion

        #region Properties

        public EVersion Version { get; set; }
        public string[] Fields { get; set; }
        public ESize[] Size { get; set; }
        public EType[] Type { get; set; }
        public int[] Count { get; set; }
        public ulong Width { get; set; }
        public ulong Height { get; set; }
        public double[] Viewpoint { get; set; }
        public ulong Points { get; set; }
        public EDataType DataType { get; set; }

        #endregion

        public PcdHeader(TextReader reader)
        {
            var headerFields = ReadHeader(reader);

            Version = ParseVersion(headerFields["VERSION"][0]);
            Size = ParseSize(headerFields["SIZE"]);
            Fields = headerFields["FIELDS"];
            Count = headerFields["COUNT"].Select(x => Convert.ToInt32(x)).ToArray();
            Viewpoint = headerFields["VIEWPOINT"].Select(Convert.ToDouble).ToArray();
            Width = Convert.ToUInt64(headerFields["WIDTH"][0]);
            Height = Convert.ToUInt64(headerFields["HEIGHT"][0]);
            Points = Convert.ToUInt64(headerFields["POINTS"][0]);
            Type = headerFields["TYPE"].Select(EnumParse<EType>).ToArray();
            DataType = EnumParse<EDataType>(headerFields["DATA"][0].ToUpper());
        }

        #region Functions

        private static string ReadHeaderLine(TextReader streamReader, string name)
        {
            return streamReader.ReadLine()?[$"{name} ".Length..];
        }

        private static EVersion ParseVersion(string version)
        {
            return version switch
            {
                "0.7" => EVersion.V7,
                _ => EVersion.UNSUPPORTED
            };
        }

        private static ESize[] ParseSize(IReadOnlyList<string> sizeLines)
        {
            var sizes = new ESize[sizeLines.Count];
            for (var i = 0; i < sizeLines.Count; i++)
            {
                sizes[i] = (ESize) Convert.ToInt32(sizeLines[i]);
            }

            return sizes;
        }

        private static T EnumParse<T>(string line) where T : struct
        {
            var success = Enum.TryParse<T>(line, out var result);
            return success ? result : Enum.Parse<T>("UNSUPPORTED");
        }

        private static Dictionary<string, string[]> ReadHeader(TextReader reader)
        {
            var header = new Dictionary<string, string[]>();
            do
            {
                var line = reader.ReadLine()?.Split(" ");
                if (line == null)
                {
                    return header;
                }

                if (line[0].StartsWith("#"))
                {
                    continue;
                }

                header.Add(line[0], line.Skip(1).ToArray());
            } while (header.Count == 0 || header.Last().Key.StartsWith("DATA") == false);

            return header;
        }

        #endregion
    }
}