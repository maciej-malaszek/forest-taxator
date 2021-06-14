using System;
using System.Globalization;
using System.IO;
using System.Text;
using ForestTaxator.Model;

namespace ForestTaxator.Data.GPD
{
    public class GpdWriter : ICloudStreamWriter, IDisposable
    {
        private readonly FileInfo _fileInfo;
        private readonly StreamWriter _streamWriter;
        private readonly BinaryWriter _binaryWriter;
        private readonly bool _binaryMode;
        private int _groupId;
        public int SliceId { get; set; }

        public GpdWriter(string filePath, bool binaryMode = true)
        {
            _binaryMode = binaryMode;
            try
            {
                _fileInfo = new FileInfo(filePath);
                var fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var bufferedStream = new BufferedStream(fileStream, 4096);
                if (_binaryMode)
                {
                    _binaryWriter = new BinaryWriter(bufferedStream);
                }
                else
                {
                    _streamWriter = new StreamWriter(bufferedStream, Encoding.ASCII);
                }

                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void WriteLine(string data)
        {
            if (_binaryMode)
            {
                _binaryWriter.Write(data);
                return;
            }

            _streamWriter.WriteLine(data);
        }

        public void WriteGroupMeta(GpdGroupMeta metadata)
        {
            WriteLine(metadata.ToString());
        }

        public void Dispose()
        {
            _streamWriter?.Flush();
            _streamWriter?.Dispose();
            _binaryWriter?.Flush();
            _binaryWriter?.Dispose();
        }

        public void WritePoint(CloudPoint point)
        {
            if (_binaryMode)
            {
                var data = point.BinarySerialized();
                _binaryWriter.Write(data);
                return;
            }

            _streamWriter.WriteLine(point.StringSerialized());
        }

        public void WritePoint(Point point)
        {
            if (_binaryMode)
            {
                var data = point.BinarySerialized();
                _binaryWriter.Write(data);
                return;
            }

            _streamWriter.WriteLine(point.StringSerialized());
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

        public void WritePointSetGroup(PointSetGroup pointSetGroup, int sliceId)
        {
            foreach (var pointSet in pointSetGroup.PointSets)
            {
                WritePointSet(pointSet, sliceId);
            }
        }
        
        public void WritePointSetGroup(PointSetGroup pointSetGroup)
        {
            WritePointSetGroup(pointSetGroup, SliceId);
        }
    }
}