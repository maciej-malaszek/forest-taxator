using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ForestTaxator.Model;

namespace ForestTaxator.Data.GPD
{
    public class GpdReader : ICloudStreamReader
    {
        private readonly BinaryReader _binaryReader;
        private readonly FileInfo _fileInfo;
        private readonly FileStream _fileStream;
        public long Size => _fileInfo.Length;

        private int _pointId = 0;
        private int _pointGroupId = 0;
        private int _sliceId = 0;
        private long _pointsInCurrentGroup = 0;
        private const byte MetaDataChar = 0x23;

        public GpdHeader Header { get; }

        public bool IsValid =>
            Header != null &&
            Header.Version != GpdHeader.EVersion.UNSUPPORTED &&
            !Header.Type.Contains(GpdHeader.EType.UNSUPPORTED) &&
            !Header.Size.Contains(GpdHeader.ESize.UNSUPPORTED) &&
            Header.DataType != GpdHeader.EDataType.UNSUPPORTED;

        public GpdReader(string inputFile)
        {
            _fileInfo = new FileInfo(inputFile);
            _fileStream = File.Open(inputFile, FileMode.Open);
            var bufferedStream = new BufferedStream(_fileStream);
            _binaryReader = new BinaryReader(bufferedStream);
            Header = new GpdHeader(_fileStream);
            _fileStream.Seek(Header.HeaderBytes, SeekOrigin.Begin);
        }

        public GpdGroupMeta ReadMetaData()
        {
            var stringBuilder = new StringBuilder();
            // Meta is ASCII encoded string, so we can read it as chars
            byte metaDataTagRead = 0;
            do
            {
                var readChar = _binaryReader.ReadChar();
                if (readChar == MetaDataChar)
                {
                    metaDataTagRead++;
                }
                stringBuilder.Append(readChar);
            } while (metaDataTagRead != 2);
            var metadataLine = stringBuilder.ToString();
            var metaData = GpdGroupMeta.Parse(metadataLine);
            _sliceId = metaData.Slice;
            _pointId = 0;
            _pointGroupId = metaData.Id;
            _pointsInCurrentGroup = metaData.Points;
            return metaData;
        }

        public CloudPoint ReadPoint()
        {
            if (_pointId >= _pointsInCurrentGroup)
            {
                return null;
            }

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
                    (GpdHeader.EType.I, GpdHeader.ESize.Byte) => bytes[0] - 128,
                    (GpdHeader.EType.I, GpdHeader.ESize.Short) => BitConverter.ToInt16(bytes),
                    (GpdHeader.EType.I, GpdHeader.ESize.Single) => BitConverter.ToInt32(bytes),
                    (GpdHeader.EType.I, GpdHeader.ESize.Double) => BitConverter.ToInt64(bytes),
                    (GpdHeader.EType.U, GpdHeader.ESize.Byte) => bytes[0],
                    (GpdHeader.EType.U, GpdHeader.ESize.Short) => BitConverter.ToUInt16(bytes),
                    (GpdHeader.EType.U, GpdHeader.ESize.Single) => BitConverter.ToUInt32(bytes),
                    (GpdHeader.EType.U, GpdHeader.ESize.Double) => BitConverter.ToUInt64(bytes),
                    (GpdHeader.EType.F, GpdHeader.ESize.Byte) => BitConverter.ToSingle(bytes),
                    (GpdHeader.EType.F, GpdHeader.ESize.Short) => BitConverter.ToSingle(bytes),
                    (GpdHeader.EType.F, GpdHeader.ESize.Single) => BitConverter.ToSingle(bytes),
                    (GpdHeader.EType.F, GpdHeader.ESize.Double) => BitConverter.ToDouble(bytes),
                    _ => p[i]
                };
            }

            _pointId++;
            return p;
        }

        public PointSet ReadPointSet()
        {
            if (_pointGroupId >= Header.Groups)
            {
                return null;
            }

            var metaData = ReadMetaData();
            var pointSet = new PointSet((int) metaData.Points);
            while (_pointId < metaData.Points)
            {
                pointSet.Points.Add(ReadPoint());
            }

            _pointGroupId++;

            return pointSet;
        }

        public IEnumerable<PointSlice> ReadPointSlices(float sliceHeight = 0.1f)
        {
            var pointSlices = new Dictionary<int, PointSlice>();
            PointSet pointSet;
            do
            {
                pointSet = ReadPointSet();
                pointSlices[_sliceId] ??= new PointSlice
                {
                    PointSets = new List<PointSet>()
                };
                if (pointSet != null)
                {
                    pointSlices[_sliceId].PointSets.Add(pointSet);
                }
            } while (pointSet != null);

            return pointSlices.OrderBy(x => x.Key).Select(x => x.Value).ToList();
        }

        public Cloud ReadCloud()
        {
            var pointSet = ReadPointSet();
            var cloud = new Cloud(pointSet);
            while (pointSet != null)
            {
                pointSet = ReadPointSet();
                if (pointSet != null)
                {
                    cloud.Points.AddRange(pointSet.Points);
                }
            }

            return cloud;
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
            _binaryReader?.Dispose();
        }
    }
}