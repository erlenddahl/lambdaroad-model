using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using LambdaModel.Config;
using LambdaModel.Stations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LambdaRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IconController : ControllerBase
    {
        private readonly IConfiguration _config;

        public IconController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public object DrawIcon(double power, string gainDefinition, string state)
        {
            var s = 128;
            using var bm = new Bitmap(s, s);
            using var g = Graphics.FromImage(bm);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.High;

            g.Clear(Color.Transparent);

            if (state != "new" && state != "edit" && state != "preview")
            {
                AntennaGain gain;
                try
                {
                    gain = AntennaGain.FromDefinition(gainDefinition);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    gain = AntennaGain.FromConstant(0);
                }

                var path = new GraphicsPath(FillMode.Alternate);
                var centerRadius = 25;

                var dbFactor = Math.Min(1, s / (centerRadius + power + gain.GetMaxGain()));

                path.StartFigure();
                for (var i = 0; i <= 360; i++)
                {
                    var db = (int) Math.Round((centerRadius + power + gain.GetGainAtAngle(i)) * dbFactor);
                    path.AddArc(new Rectangle((s - db) / 2, (s - db) / 2, db, db), 360 - i, 1);
                }

                path.CloseFigure();

                path.AddEllipse(new Rectangle((s - centerRadius) / 2, (s - centerRadius) / 2, centerRadius, centerRadius));

                var brush = new PathGradientBrush(path)
                {
                    CenterColor = Color.FromArgb(255, Color.Red),
                    SurroundColors = new[] {Color.FromArgb(215, Color.Green), Color.FromArgb(175, Color.Green)}
                };

                g.FillPath(brush, path);
            }

            var ringColor = Color.DarkRed;
            var innerColor = Color.IndianRed;

            if (state == "new")
            {
                ringColor = Color.GreenYellow;
                innerColor = Color.Yellow;
            }
            else if (state == "selected")
            {
                ringColor = Color.DarkBlue;
                innerColor = Color.DeepSkyBlue;
            }
            else if (state == "edit")
            {
                ringColor = Color.Black;
                innerColor = Color.Gray;
            }

            g.FillEllipse(new SolidBrush(Color.FromArgb(255, ringColor)), new Rectangle(54, 54, 20, 20));
            g.FillEllipse(new SolidBrush(Color.FromArgb(255, innerColor)), new Rectangle(57, 57, 14, 14));

            using var memoryStream = new MemoryStream();
            bm.Save(memoryStream, ImageFormat.Png);
            return File(memoryStream.ToArray(), "image/png");
        }
    }
}