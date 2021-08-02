using System.IO;
using System.Linq;
using System.Text;
using ForestTaxator.Lib.Data.GPD;
using ForestTaxator.Lib.Data.PCD;
using ForestTaxator.Lib.Data.XYZ;

namespace ForestTaxator.Lib.Data
{
    public static class FileFormatDetector
    {
        private const int ProbeSize = 16;
        private static readonly byte[] _gpdMagic = {0x46, 0x4f, 0x52, 0x4d, 0x41, 0x54, 0x20, 0x47, 0x50, 0x44, 0x0a, 0x56, 0x45, 0x52, 0x53, 0x49};
        private static readonly byte[] _pcdMagic = {0x23, 0x20, 0x2e, 0x50, 0x43, 0x44, 0x20, 0x76, 0x30, 0x2e, 0x37, 0x20, 0x2d, 0x20, 0x50, 0x6f};

        public static ESupportedFormat Detect(string inputPath)
        {
            using var stream = File.Open(inputPath, FileMode.Open);
            
            var buffer = new byte[ProbeSize];
            stream.Read(buffer, 0, ProbeSize);
            if (buffer.SequenceEqual(_gpdMagic))
            {
                return ESupportedFormat.GPD;
            }

            stream.Seek(0, SeekOrigin.Begin);

            return buffer.SequenceEqual(_pcdMagic) ? ESupportedFormat.PCD : ESupportedFormat.XYZ;
        }

        public static ICloudStreamReader GetCloudStreamReader(string inputPath)
        {
            return Detect(inputPath) switch
            {
                ESupportedFormat.GPD => new GpdReader(inputPath),
                ESupportedFormat.PCD => new PcdReader(inputPath),
                ESupportedFormat.XYZ => new XyzReader(inputPath, Encoding.ASCII), 
                _ => null
            };
        }
        
        public static ICloudStreamReader GetCloudStreamReader(string inputPath, ESupportedCloudFormat format)
        {
            return format switch
            {
                ESupportedCloudFormat.GPD => new GpdReader(inputPath),
                ESupportedCloudFormat.PCD => new PcdReader(inputPath),
                ESupportedCloudFormat.XYZ => new XyzReader(inputPath, Encoding.ASCII), 
                _ => null
            };
        }
        
        public static ICloudStreamWriter GetCloudStreamWriter(string path, ESupportedCloudFormat format)
        {
            return format switch
            {
                ESupportedCloudFormat.GPD => new GpdWriter(path, null, 0),
                ESupportedCloudFormat.PCD => new PcdWriter(path),
                ESupportedCloudFormat.XYZ => new XyzWriter(path), 
                _ => null
            };
        }
    }
}