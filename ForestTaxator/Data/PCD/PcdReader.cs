using System;
using System.IO;
using System.Linq;
using ForestTaxator.Model;

namespace ForestTaxator.Data.PCD
{
    public class PcdReader : ICloudStreamReader, IDisposable
    {
        private readonly StreamReader _streamReader;
        private readonly BinaryReader _binaryReader;
        private readonly FileInfo _fileInfo;

        public void Dispose()
        {
            _streamReader.Dispose();
        }

        private CloudPoint ReadAsciiPoint()
        {
            var p = new CloudPoint();
            var fields = _streamReader.ReadLine()?.Split(" ");
            if (fields == null)
            {
                return null;
            }


            for (var i = 0; i < fields.Length; i++)
            {
                p[i] = (Header.Type[i], Header.Size[i]) switch
                {
                    (PcdHeader.EType.I, PcdHeader.ESize.Byte) => char.Parse(fields[i]) - 128,
                    (PcdHeader.EType.I, PcdHeader.ESize.Short) => short.Parse(fields[i]),
                    (PcdHeader.EType.I, PcdHeader.ESize.Single) => int.Parse(fields[i]),
                    (PcdHeader.EType.I, PcdHeader.ESize.Double) => long.Parse(fields[i]),
                    (PcdHeader.EType.U, PcdHeader.ESize.Byte) => char.Parse(fields[i]),
                    (PcdHeader.EType.U, PcdHeader.ESize.Short) => ushort.Parse(fields[i]),
                    (PcdHeader.EType.U, PcdHeader.ESize.Single) => uint.Parse(fields[i]),
                    (PcdHeader.EType.U, PcdHeader.ESize.Double) => ulong.Parse(fields[i]),
                    (PcdHeader.EType.F, PcdHeader.ESize.Byte) => float.Parse(fields[i]),
                    (PcdHeader.EType.F, PcdHeader.ESize.Short) => float.Parse(fields[i]),
                    (PcdHeader.EType.F, PcdHeader.ESize.Single) => float.Parse(fields[i]),
                    (PcdHeader.EType.F, PcdHeader.ESize.Double) => double.Parse(fields[i]),
                    _ => p[i]
                };
            }

            return p;
        }

        private CloudPoint ReadBinaryPoint()
        {
            var p = new CloudPoint();
            for (var i = 0; i < Header.Fields.Length; i++)
            {
                var bytes = _binaryReader.ReadBytes((int) Header.Size[i]);
                if (i >= 3)
                {
                    continue;
                }

                p[i] = (Header.Type[i], Header.Size[i]) switch
                {
                    (PcdHeader.EType.I, PcdHeader.ESize.Byte) => bytes[0] - 128,
                    (PcdHeader.EType.I, PcdHeader.ESize.Short) => BitConverter.ToInt16(bytes),
                    (PcdHeader.EType.I, PcdHeader.ESize.Single) => BitConverter.ToInt32(bytes),
                    (PcdHeader.EType.I, PcdHeader.ESize.Double) => BitConverter.ToInt64(bytes),
                    (PcdHeader.EType.U, PcdHeader.ESize.Byte) => bytes[0],
                    (PcdHeader.EType.U, PcdHeader.ESize.Short) => BitConverter.ToUInt16(bytes),
                    (PcdHeader.EType.U, PcdHeader.ESize.Single) => BitConverter.ToUInt32(bytes),
                    (PcdHeader.EType.U, PcdHeader.ESize.Double) => BitConverter.ToUInt64(bytes),
                    (PcdHeader.EType.F, PcdHeader.ESize.Byte) => BitConverter.ToSingle(bytes),
                    (PcdHeader.EType.F, PcdHeader.ESize.Short) => BitConverter.ToSingle(bytes),
                    (PcdHeader.EType.F, PcdHeader.ESize.Single) => BitConverter.ToSingle(bytes),
                    (PcdHeader.EType.F, PcdHeader.ESize.Double) => BitConverter.ToDouble(bytes),
                    _ => p[i]
                };
            }

            return p;
        }

        public CloudPoint ReadPoint()
        {
            if (IsValid == false)
            {
                return null;
            }

            return Header.DataType switch
            {
                PcdHeader.EDataType.ASCII => ReadAsciiPoint(),
                PcdHeader.EDataType.BINARY => ReadBinaryPoint(),
                PcdHeader.EDataType.UNSUPPORTED => null,
                _ => null
            };
        }

        public PointSet ReadPointSet()
        {
            if (IsValid == false)
            {
                return null;
            }

            var points = new CloudPoint[Header.Points];
            for (ulong i = 0; i < Header.Points; i++)
            {
                points[i] = ReadPoint();
            }

            return new PointSet(points);
        }

        public Cloud ReadCloud()
        {
            return new(ReadPointSet());
        }

        public long Size => _fileInfo.Length;

        public PcdHeader Header { get; }

        public bool IsValid =>
            Header != null &&
            Header.Version != PcdHeader.EVersion.UNSUPPORTED &&
            !Header.Type.Contains(PcdHeader.EType.UNSUPPORTED) &&
            !Header.Size.Contains(PcdHeader.ESize.UNSUPPORTED) &&
            Header.DataType != PcdHeader.EDataType.UNSUPPORTED;

        public PcdReader(string path)
        {
            _fileInfo = new FileInfo(path);
            var fileStream = File.Open(path, FileMode.Open);
            var bufferedStream = new BufferedStream(fileStream, 4096);
            _streamReader = new StreamReader(bufferedStream);
            Header = new PcdHeader(_streamReader);
            if (Header.DataType == PcdHeader.EDataType.BINARY)
            {
                _binaryReader = new BinaryReader(bufferedStream);
            }
        }
    }
}