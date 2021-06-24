using System;

namespace LambdaModel.Terrain
{
    internal class TiffTileDownloadException : Exception
    {
        public TiffTileDownloadException(string message) : base(message)
        {
        }
    }
}