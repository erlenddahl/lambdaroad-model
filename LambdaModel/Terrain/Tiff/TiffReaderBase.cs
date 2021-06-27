using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BitMiracle.LibTiff.Classic;
using LambdaModel.General;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Terrain.Tiff
{
	public abstract class TiffReaderBase: ITiffReader, IDisposable
	{
		public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int StartX { get; protected set; }
        public int StartY { get; protected set; }
        public int EndX { get; protected set; }
        public int EndY { get; protected set; }

        protected void SetEnds()
        {
            EndX = StartX + Width;
            EndY = StartY - Height;
        }

        public float GetAltitude(Point3D p)
        {
            return GetAltitude(p.X, p.Y);
        }

        public virtual float GetAltitude(double pX, double pY)
        {
            var (x, y) = ToLocal(pX, pY);
            if (!Contains(pX, pY))
                throw new Exception("Requested point is not inside this TIFF file.");

            return GetAltitudeInternal(x, y);
        }

        protected abstract float GetAltitudeInternal(int x, int y);

        protected (int x, int y) ToLocal(double pX, double pY)
        {
            return (QuickMath.Round(pX - StartX), QuickMath.Round(StartY - pY));
        }

        public Point3D[] GetAltitudeVector(Point3D a, Point3D b, int incMeter = 1)
        {
            return GetAltitudeVector(a.X,a.Y, b.X, b.Y, incMeter);
        }

        public Point3D[] GetAltitudeVector(double aX, double aY, double bX, double bY, int incMeter = 1)
        {
            (aX, aY) = ToLocal(aX, aY);
            (bX, bY) = ToLocal(bX, bY);
            
            var dx = bX - aX;
            var dy = bY - aY;
			var l = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            var v = new Point3D[(int)l + 1];

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0;

            var (x, y) = (aX, aY);

            while (m <= l)
            {
                v[m] = new Point3D(x, y, GetAltitudeInternal(QuickMath.Round(x), QuickMath.Round(y)));

                m += incMeter;
                x += xInc;
                y += yInc;
            }

            return v;
        }

        public bool Contains(double pX, double pY)
        {
            var x = QuickMath.Round(pX);
            var y = QuickMath.Round(pY);
            return x >= StartX && x < EndX && y > EndY && y <= StartY;
        }

        public abstract void Dispose();
    }
}
