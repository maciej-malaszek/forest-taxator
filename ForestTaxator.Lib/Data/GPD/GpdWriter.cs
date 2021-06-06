using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ForestTaxator.Model;

namespace ForestTaxator.Data.GPD
{
    public class GpdWriter : IDisposable
    {
        private readonly FileInfo _fileInfo;
        private readonly StreamWriter _streamWriter;
        private readonly BinaryWriter _binaryWriter;
        private readonly bool _binaryMode;
        private int _groupId;
        
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
        private async Task WriteLine(string data)
        {
            if (_binaryMode)
            {
                _binaryWriter.Write(data);
                return;
            }

            await _streamWriter.WriteLineAsync(data);
        }

        private async Task WritePoint<T>(T point) where T : Point
        {
            if (_binaryMode)
            {
                var data = point.BinarySerialized();
                _binaryWriter.Write(data);
                return;
            }

            await _streamWriter.WriteLineAsync(point.StringSerialized());
        }

        public async Task WriteGroupMeta(GpdGroupMeta metadata)
        {
            await WriteLine(metadata.ToString());
        }

        public async Task WritePointSet(PointSet pointSet, int sliceId = 0)
        {
            var groupMeta = new GpdGroupMeta
            {
                Points = pointSet.Count,
                Slice = sliceId,
                Id = _groupId++
            };
            await WriteGroupMeta(groupMeta);
            foreach (var point in pointSet)
            {
                await WritePoint(point);
            }
        }

        public async Task WritePointSetGroup(PointSetGroup pointSetGroup, int sliceId = 0)
        {
            foreach (var pointSet in pointSetGroup.PointSets)
            {
                await WritePointSet(pointSet, sliceId);
            }
        }

        public void Dispose()
        {
            _streamWriter?.Flush();
            _streamWriter?.Dispose();
            _binaryWriter?.Flush();
            _binaryWriter?.Dispose();
        }
    }
}