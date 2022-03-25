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
            g.CompositingMode = CompositingMode.SourceOver;
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.Clear(Color.Transparent);

            var ringColor = Color.DarkRed;
            var innerColor = Color.IndianRed;
            var gradientCenter = Color.Red;
            var gradientEdge = Color.Green;

            if (state == "new")
            {
                ringColor = Color.Yellow;
                innerColor = Color.Yellow;
            }
            else if(state == "preview")
            {
                ringColor = Color.LightGreen;
                innerColor = Color.LightGreen;
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
                gradientCenter = Color.DimGray;
                gradientEdge = Color.DarkGray;
            }

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
            var previousDb = int.MinValue;
            var startAngle = 0;
            for (var i = 0; i < 360; i++)
            {
                // Request gain for 360-i, since GraphicsPath draws in the opposite direction (and is very
                // peculiar on the sequence of arcs and their direction).
                var db = (int) Math.Round((centerRadius + power + gain.GetGainAtAngle(360 - i)) * dbFactor);
                if (previousDb == int.MinValue) previousDb = db;

                if (db != previousDb)
                {
                    path.AddArc(new Rectangle((s - previousDb) / 2, (s - previousDb) / 2, previousDb, previousDb), startAngle, (i - 1 - startAngle));
                    Debug.WriteLine(startAngle + " to " + (i - 1) + ": " + previousDb + ", " + (s - previousDb) / 2);
                    startAngle = i;
                }

                previousDb = db;
            }

            if (startAngle != 359)
            {
                var i = 359;
                path.AddArc(new Rectangle((s - previousDb) / 2, (s - previousDb) / 2, previousDb, previousDb), startAngle, (i - 1 - startAngle));
                Debug.WriteLine(startAngle + " to " + (i - 1) + ": " + previousDb + ", " + (s - previousDb) / 2);
            }

            path.CloseFigure();

            path.AddEllipse(new Rectangle((s - centerRadius) / 2, (s - centerRadius) / 2, centerRadius, centerRadius));

            var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(255, gradientCenter),
                SurroundColors = new[] {Color.FromArgb(215, gradientEdge), Color.FromArgb(175, gradientEdge) }
            };

            g.FillPath(new SolidBrush(Color.FromArgb(175, gradientEdge)), path);
            g.FillPath(brush, path);

            g.FillEllipse(new SolidBrush(Color.FromArgb(255, ringColor)), new Rectangle(54, 54, 20, 20));
            g.FillEllipse(new SolidBrush(Color.FromArgb(255, innerColor)), new Rectangle(57, 57, 14, 14));

            using var memoryStream = new MemoryStream();
            bm.Save(memoryStream, ImageFormat.Png);
            return File(memoryStream.ToArray(), "image/png");
        }
    }
}