using BitMiracle.LibTiff.Classic;
using ConsoleUtilities.ConsoleInfoPanel;
using ConsoleUtilities.ConsoleInfoPanel.Items;

namespace LambdaModel.Utilities
{
    internal class LambdaTiffErrorHandler : TiffErrorHandler
    {
        private readonly ConsoleInformationPanel _cip;

        private AppendableStringInfoItem _log = null;

        public LambdaTiffErrorHandler(ConsoleInformationPanel cip)
        {
            _cip = cip;
        }

        private void Log(string text)
        {
            return;
            if (_log == null)
            {
                _log = _cip.Log("Tiff parsing", text, sequence: 999);
            }
            else
            {
                _log.AppendLine(text);
            }
        }

        public override void ErrorHandler(Tiff tif, string method, string format, params object[] args)
        {
            //Log("ERROR: " + string.Format(format, args) + " (" + method + ", " + System.IO.Path.GetFileName(tif.FileName()) + ")");
        }

        public override void ErrorHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
        {
            Log("ERROR: " + string.Format(format, args) + " (" + method + ", " + System.IO.Path.GetFileName(tif.FileName()) + ")");
        }

        public override void WarningHandler(Tiff tif, string method, string format, params object[] args)
        {
            //Log("WARNING: " + string.Format(format, args) + " (" + method + ", " + System.IO.Path.GetFileName(tif.FileName()) + ")");
        }

        public override void WarningHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
        {
            Log("WARNING: " + string.Format(format, args) + " (" + method + ", " + System.IO.Path.GetFileName(tif.FileName()) + ")");
        }
    }
}