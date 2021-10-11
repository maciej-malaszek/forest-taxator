using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ForestTaxator.Lib.Data.Compression;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Data.PCD
{
    public class PcdReader : ICloudStreamReader
    {
        private readonly StreamReader _streamReader;
        private readonly BinaryReader _binaryReader;
        private readonly FileInfo _fileInfo;
        private long _pointIndex;

        public void Dispose()
        {
            _streamReader.Dispose();
        }

        private CloudPoint ReadAsciiPoint()
        {
            var p = new CloudPoint();
            var line = _streamReader.ReadLine();
            var fields = line?.Split(" "); 
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
                    (PcdHeader.EType.F, PcdHeader.ESize.Byte) => float.Parse(fields[i], CultureInfo.InvariantCulture),
                    (PcdHeader.EType.F, PcdHeader.ESize.Short) => float.Parse(fields[i], CultureInfo.InvariantCulture),
                    (PcdHeader.EType.F, PcdHeader.ESize.Single) => float.Parse(fields[i], CultureInfo.InvariantCulture),
                    (PcdHeader.EType.F, PcdHeader.ESize.Double) => double.Parse(fields[i], CultureInfo.InvariantCulture),
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
        
        private CloudPoint ReadBinaryPointFromOrdered()
        {
            var p = new CloudPoint();
            // Points are stored as XXXYYYZZZIII instead of XYZIXYZIXYZI
            if (_pointIndex / (long)Header.Size[2] >= (long)Header.Points)
            {
                return null;
            }
            for (var i = 0; i < Header.Fields.Length; i++)
            {
                var position = (long)Header.Points * (long) Header.Size[i] * i; // skip all points for previous dimensions
                position += _pointIndex;
                _binaryReader.BaseStream.Seek(position, SeekOrigin.Begin);
                var bytes = _binaryReader.ReadBytes((int) Header.Size[i]);
                switch (i)
                {
                    case 2:
                        _pointIndex+=(long)Header.Size[i];
                        break;
                    case >= 3:
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
                PcdHeader.EDataType.BINARY_COMPRESSED => ReadBinaryPointFromOrdered(),
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
                if (points[i] is null)
                {
                    break;
                }
            }

            return points[0] is null ? null : new PointSet(points);
        }

        public IEnumerable<PointSlice> ReadPointSlices(float sliceHeight = 0.1f)
        {
            return ReadCloud().Slice(sliceHeight);
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
            _streamReader = new StreamReader(fileStream);
            Header = new PcdHeader(_streamReader);
            
            switch (Header.DataType)
            {
                case PcdHeader.EDataType.ASCII:
                {
                    _streamReader.BaseStream.Seek(Header.HeaderBytes, SeekOrigin.Begin);
                    _streamReader.DiscardBufferedData();
                    break;
                }
                case PcdHeader.EDataType.BINARY:
                {
                    _binaryReader = new BinaryReader(bufferedStream);
                    _binaryReader.ReadBytes(Header.HeaderBytes);
                    break;
                }
                case PcdHeader.EDataType.BINARY_COMPRESSED:
                {
                    _pointIndex = 0;
                    using var binaryReader = new BinaryReader(bufferedStream);
                    var headerBytes = binaryReader.ReadBytes(Header.HeaderBytes);
                    var compressedDataBytes = binaryReader.ReadUInt32();
                    var uncompressedDataBytes = binaryReader.ReadUInt32();
                    if (Header.HeaderBytes + compressedDataBytes + 8 != bufferedStream.Length)
                    {
                        // Add some validation log here
                    }

                    var fileContent = binaryReader.ReadBytes((int) compressedDataBytes);
                    var decompressed = CLZF2.Decompress(fileContent);
                    var memoryStream = new MemoryStream(decompressed);
                    _binaryReader = new BinaryReader(memoryStream);
                    break;
                }
            }
        }
    }
}