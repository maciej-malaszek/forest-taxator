using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ForestTaxator.Model;

namespace ForestTaxator.Data.GPD
{
    public class GpdWriter : ICloudStreamWriter
    {
        private readonly GpdHeader _header;
        private readonly BinaryWriter _binaryWriter;
        private int _groupId;
        private uint _pointCounter;
        public int SliceId { get; set; }

        public GpdWriter(string filePath, string[] fields, float sliceHeight)
        {
            _header = new GpdHeader
            {
                Version = GpdHeader.EVersion.V1,
                Slice = sliceHeight,
                Fields = fields ?? new[] {"x", "y", "z", "intensity"},
                Format = GpdHeader.EFormat.GPD,
                Size = new[] {GpdHeader.ESize.Double, GpdHeader.ESize.Double, GpdHeader.ESize.Double, GpdHeader.ESize.Double},
                Type = new[] {GpdHeader.EType.F, GpdHeader.EType.F, GpdHeader.EType.F, GpdHeader.EType.F},
                DataType = GpdHeader.EDataType.BINARY
            };
            try
            {
                var fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var bufferedStream = new BufferedStream(fileStream, 4096);
                _binaryWriter = new BinaryWriter(bufferedStream);
                PrependMetadataHeader();
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void WriteLine(string data)
        {
            _binaryWriter.Write(Encoding.ASCII.GetBytes(data));
        }

        public void PrependMetadataHeader()
        {
            _binaryWriter.Flush();
            _header.Points = _pointCounter;
            _header.Groups = (uint) _groupId;
            var header = _header.BinarySerialize();
            _binaryWriter.Seek(0, SeekOrigin.Begin);
            _binaryWriter.Write(header);
            _binaryWriter.Flush();
        }

        public void WriteGroupMeta(GpdGroupMeta metadata)
        {
            WriteLine(metadata.ToString());
        }

        public void Dispose()
        {
            PrependMetadataHeader();
            _binaryWriter?.Flush();
            _binaryWriter?.Dispose();
        }

        public void WritePoint(CloudPoint point)
        {
            _pointCounter++;
            var data = point.BinarySerialized();
            _binaryWriter.Write(data);
        }

        public void WritePoint(Point point)
        {
            _pointCounter++;
            var data = point.BinarySerialized();
            _binaryWriter.Write(data);
        }

        public void WritePointSet(PointSet pointSet)
        {
            WritePointSet(pointSet, SliceId);
        }

        public void WritePointSet(PointSet pointSet, int sliceId)
        {
            var groupMeta = new GpdGroupMeta
            {
                Points = pointSet.Count,
                Slice = sliceId,
                Id = _groupId++
            };
            WriteGroupMeta(groupMeta);
            foreach (var point in pointSet)
            {
                WritePoint(point);
            }
        }

        public void WritePointSetGroup(IPointSetGroup pointSetGroup, int sliceId)
        {
            foreach (var pointSet in pointSetGroup.PointSets)
            {
                WritePointSet(pointSet, sliceId);
            }
        }

        public void WritePointSetGroup(IPointSetGroup pointSetGroup)
        {
            WritePointSetGroup(pointSetGroup, SliceId);
        }
        
        public void WriteSlices(IEnumerable<IPointSetGroup> slices)
        {
            foreach (var slice in slices)
            {
                WritePointSetGroup(slice, SliceId++);                
            }
            
        }
    }
}