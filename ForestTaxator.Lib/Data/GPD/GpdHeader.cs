using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ForestTaxator.Data.GPD
{
    public class GpdHeader
    {
        #region Enums
        
        public enum EFormat
        {
            GPD,
            UNSUPPORTED
        }

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

        public EFormat Format { get; set; }
        public EVersion Version { get; set; }
        public int Groups { get; set; }
        public float Slice { get; set; }
        public string[] Fields { get; set; }
        public ESize[] Size { get; set; }
        public EType[] Type { get; set; }
        public int Points { get; set; }
        public EDataType DataType { get; set; }
        public int HeaderBytes { get; private set; }

        public GpdHeader(StreamReader reader)
        {
            var headerFields = ReadHeader(reader);
            Format = ParseFormat(headerFields["FORMAT"][0]);
            Version = ParseVersion(headerFields["VERSION"][0]);
            Groups = Convert.ToInt32(headerFields["GROUPS"]);
            Slice = Convert.ToSingle(headerFields["SLICE"]);
            Fields = headerFields["FIELDS"];
            Size = ParseSize(headerFields["SIZE"]);
            Type = headerFields["TYPE"].Select(EnumParse<EType>).ToArray();
            Points = Convert.ToInt32(headerFields["POINTS"][0]);
            DataType = EnumParse<EDataType>(headerFields["DATA"][0].ToUpper());
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
        }

        private static EFormat ParseFormat(string s)
        {
            return s switch
            {
                "GPD" => EFormat.GPD,
                _ => EFormat.UNSUPPORTED
            };
        }

        private static string ReadHeaderLine(TextReader streamReader, string name)
        {
            return streamReader.ReadLine()?[$"{name} ".Length..];
        }

        private static EVersion ParseVersion(string version)
        {
            return version switch
            {
                "0.1" => EVersion.V1,
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

        private Dictionary<string, string[]> ReadHeader(TextReader reader)
        {
            var header = new Dictionary<string, string[]>();
            do
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    return header;
                }

                HeaderBytes += line.Length + 1; // number of characters + new line. DO NOT multiply by char, because we read ASCII file
                var fields = line?.Split(" ");

                if (fields[0].StartsWith("#"))
                {
                    continue;
                }

                header.Add(fields[0], fields.Skip(1).ToArray());
            } while (header.Count == 0 || header.Keys.Contains("DATA") == false);

            return header;
        }

    }
}