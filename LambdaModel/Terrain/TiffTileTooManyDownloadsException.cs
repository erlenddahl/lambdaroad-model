namespace LambdaModel.Terrain
{
    internal class TiffTileTooManyDownloadsException : TiffTileDownloadException
    {
        public TiffTileTooManyDownloadsException(string message) : base(message)
        {
        }
    }
}