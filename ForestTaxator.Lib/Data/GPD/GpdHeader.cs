using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ForestTaxator.Model;

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
        public uint Groups { get; set; }
        public float Slice { get; set; }
        public string[] Fields { get; set; }
        public ESize[] Size { get; set; }
        public EType[] Type { get; set; }
        public uint Points { get; set; }
        public EDataType DataType { get; set; }
        public long HeaderBytes { get; private set; }

        public GpdHeader()
        {
        }

        public GpdHeader(Stream reader)
        {
            // We cannot use dispose those, because it will close file and it is too soon to do that
            var textReader = new StreamReader(reader);
            var binaryReader = new BinaryReader(reader);
            
            var headerFields = ReadHeader(textReader);
            Format = ParseFormat(headerFields["FORMAT"][0]);
            Version = ParseVersion(headerFields["VERSION"][0]);

            Slice = Convert.ToSingle(headerFields["SLICE"][0]);
            Fields = headerFields["FIELDS"];
            Size = ParseSize(headerFields["SIZE"]);
            Type = headerFields["TYPE"].Select(EnumParse<EType>).ToArray();
            DataType = EnumParse<EDataType>(headerFields["DATA"][0].ToUpper());

            reader.Seek(HeaderBytes, SeekOrigin.Begin);
            reader.Seek("GROUPS".Length, SeekOrigin.Current);
            Groups = binaryReader.ReadUInt32();
            reader.Seek("POINTS".Length, SeekOrigin.Current);
            Points = binaryReader.ReadUInt32();
            HeaderBytes = reader.Position;

            reader.Seek(0, SeekOrigin.Begin);
        }

        public byte[] BinarySerialize()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"FORMAT {Format.ToString().ToUpperInvariant()}");
            stringBuilder.AppendLine($"VERSION {Version.ToString().ToUpperInvariant()}");
            stringBuilder.AppendLine($"SLICE {Slice.ToString(CultureInfo.InvariantCulture).ToUpperInvariant()}");
            stringBuilder.AppendLine($"FIELDS {string.Join(' ', Fields.Select(x => x.ToString().ToUpperInvariant()))}");
            stringBuilder.AppendLine($"SIZE {string.Join(' ', Size.Select(x => (int) x))}");
            stringBuilder.AppendLine($"TYPE {string.Join(' ', Type.Select(x => x.ToString().ToUpperInvariant()))}");
            stringBuilder.AppendLine($"DATA {DataType.ToString().ToUpperInvariant()}");
            var asciiHeader = Encoding.ASCII.GetBytes(stringBuilder.ToString());
            var groupsHeader = Encoding.ASCII.GetBytes("GROUPS");
            var pointsHeader = Encoding.ASCII.GetBytes("POINTS");

            var headerSize = asciiHeader.Length + groupsHeader.Length + pointsHeader.Length + sizeof(uint) + sizeof(uint);
            var header = new byte[headerSize];
            var offset = 0;
            Buffer.BlockCopy(asciiHeader, 0, header, offset, asciiHeader.Length);
            offset += asciiHeader.Length;
            Buffer.BlockCopy(groupsHeader, 0, header, offset, groupsHeader.Length);
            offset += groupsHeader.Length;
            Buffer.BlockCopy(BitConverter.GetBytes(Groups), 0, header, offset, sizeof(uint));
            offset += sizeof(uint);
            Buffer.BlockCopy(pointsHeader, 0, header, offset, pointsHeader.Length);
            offset += pointsHeader.Length;
            Buffer.BlockCopy(BitConverter.GetBytes(Points), 0, header, offset, sizeof(uint));

            return header;
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