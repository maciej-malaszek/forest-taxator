using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Data.XYZ
{
    public class XyzReader : ICloudStreamReader, IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly BufferedStream _bufferedStream;
        private readonly StreamReader _streamReader;
        private readonly FileInfo _fileInfo;
        public long Size => _fileInfo.Length;

        public XyzReader(string filePath, Encoding encoding)
        {
            _fileInfo = new FileInfo(filePath);
            _fileStream = File.Open(filePath, FileMode.Open);
            _bufferedStream = new BufferedStream(_fileStream, 4096);
            _streamReader = new StreamReader(_bufferedStream);
            for (var i = 0; i < 11; i++)
            {
                _streamReader.ReadLine();
            }
        }

        public CloudPoint ReadPoint()
        {
            var data = _streamReader.ReadLine()?.Split(' ');
            if (data == null)
            {
                return null;
            }

            var p = new CloudPoint
            {
                X = Convert.ToSingle(data[0], CultureInfo.InvariantCulture),
                Y = Convert.ToSingle(data[1], CultureInfo.InvariantCulture),
                Z = Convert.ToSingle(data[2], CultureInfo.InvariantCulture)
            };

            return p;
        }

        public PointSet ReadPointSet()
        {
            var pointSet = new PointSet();
            CloudPoint p;
            do
            {
                p = ReadPoint();
                if (p != null)
                {
                    pointSet.Add(p);
                }
            } while (p != null);

            return pointSet;
        }

        public IEnumerable<PointSlice> ReadPointSlices(float sliceHeight)
        {
            return ReadCloud().Slice(sliceHeight);
        }

        public Cloud ReadCloud()
        {
            return new(ReadPointSet());
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
            _bufferedStream?.Dispose();
            _streamReader?.Dispose();
        } }
}