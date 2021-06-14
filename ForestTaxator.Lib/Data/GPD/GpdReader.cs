using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ForestTaxator.Model;

namespace ForestTaxator.Data.GPD
{
    public class GpdReader : ICloudStreamReader, IDisposable
    {
        private readonly StreamReader _streamReader;
        private readonly BinaryReader _binaryReader;
        private readonly FileInfo _fileInfo;
        private readonly FileStream _fileStream;
        public long Size => _fileInfo.Length;

        private int pointId = 0;
        private int pointGroupId = 0;
        private int sliceId = 0;
        private long pointsInCurrentGroup = 0;

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
            _binaryReader = new BinaryReader(_fileStream);
            _streamReader = new StreamReader(_fileStream);
            Header = new GpdHeader(_streamReader);
            _fileStream.Seek(Header.HeaderBytes, SeekOrigin.Begin);
        }

        public GpdGroupMeta ReadMetaData()
        {
            var metadataLine = _streamReader.ReadLine();
            var metaData = GpdGroupMeta.Parse(metadataLine);
            sliceId = metaData.Slice;
            pointId = 0;
            pointGroupId = metaData.Id;
            pointsInCurrentGroup = metaData.Points;
            return metaData;
        }

        public CloudPoint ReadPoint()
        {
            if (pointId >= pointsInCurrentGroup)
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

            pointId++;
            return p;
        }

        public PointSet ReadPointSet()
        {
            if (pointGroupId >= Header.Groups)
            {
                return null;
            }

            var metaData = ReadMetaData();
            var pointSet = new PointSet();
            while (pointId < metaData.Points)
            {
                pointSet.Points.Add(ReadPoint());
            }

            pointGroupId++;

            return pointSet;
        }

        public IEnumerable<PointSlice> ReadPointSlices(float sliceHeight = 0.1f)
        {
            var pointSlices = new Dictionary<int, PointSlice>();
            PointSet pointSet;
            do
            {
                pointSet = ReadPointSet();
                pointSlices[sliceId] ??= new PointSlice
                {
                    PointSets = new List<PointSet>()
                };
                if (pointSet != null)
                {
                    pointSlices[sliceId].PointSets.Add(pointSet);
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
            _streamReader?.Dispose();
        }
    }
}