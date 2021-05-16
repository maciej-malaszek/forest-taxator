using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ForestTaxator.Model;

namespace ForestTaxator.Data.XYZ
{
    public class XyzWriter : ICloudStreamWriter, IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly BufferedStream _bufferedStream;
        private readonly StreamWriter _streamWriter;
        private readonly FileInfo _fileInfo;
        public long Size => _fileInfo.Length;

        public XyzWriter(string filePath)
        {
            try
            {
                _fileInfo = new FileInfo(filePath);
                _fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                _bufferedStream = new BufferedStream(_fileStream, 4096);
                _streamWriter = new StreamWriter(_bufferedStream, Encoding.ASCII);
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void WritePoint(CloudPoint point)
        {
            _streamWriter.WriteLine($"{point.X} {point.Y} {point.Z} {point.Intensity}");
        }

        public void WritePoint(Point point)
        {
            _streamWriter.WriteLine($"{point.X} {point.Y} {point.Z}");
        }

        public void WritePointSet(PointSet pointSet)
        {
            foreach (var point in pointSet)
            {
                WritePoint(point);
            }

            _streamWriter.Flush();
        }

        public void WritePointSetGroup(PointSetGroup pointSetGroup)
        {
            var pointSetSize = pointSetGroup.PointSets.Select(pointSet => pointSet.Count).Sum();
            Console.WriteLine(pointSetSize);
            foreach (var pointSet in pointSetGroup.PointSets)
            {
                foreach (var point in pointSet)
                {
                    WritePoint(point);
                }
            }
            _streamWriter.Flush();
        }
        

        public void Dispose()
        {
            _streamWriter.Flush();
            _bufferedStream.Flush();
            _streamWriter.Close();
            _bufferedStream?.Dispose();
            _fileStream?.Dispose();
        }
    }
}