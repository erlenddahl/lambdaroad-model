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
        public int Width;
        public int Height;
        public int StartX;
        public int StartY;

        /// <summary>
        /// Non-inclusive end-coordinate
        /// </summary>
        public int EndX;

        /// <summary>
        /// Non-inclusive end-coordinate
        /// </summary>
        public int EndY;

        public void SetEnds()
        {
            EndX = StartX + Width;
            EndY = StartY + Height;
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

        public float GetAltitudeNoCheck(int pX, int pY)
        {
            return GetAltitudeInternal(pX - StartX, pY - StartY);
        }

        protected abstract float GetAltitudeInternal(int x, int y);

        protected (int x, int y) ToLocal(double pX, double pY)
        {
            return (QuickMath.Round(pX - StartX), QuickMath.Round(pY - StartY));
        }

        public Point4D[] GetAltitudeVector(Point3D a, Point3D b, int incMeter = 1)
        {
            return GetAltitudeVector(a.X,a.Y, b.X, b.Y, incMeter);
        }

        public Point4D[] GetAltitudeVector(double aX, double aY, double bX, double bY, int incMeter = 1)
        {
            (aX, aY) = ToLocal(aX, aY);
            (bX, bY) = ToLocal(bX, bY);
            
            var dx = bX - aX;
            var dy = bY - aY;
			var l = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            var v = new Point4D[(int)l + 1];

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0;

            var (x, y) = (aX, aY);

            while (m <= l)
            {
                v[m] = new Point4D(x, y, GetAltitudeInternal(QuickMath.Round(x), QuickMath.Round(y)));

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
            return x >= StartX && x < EndX && y >= StartY && y < EndY;
        }

        public bool Contains(int x, int y)
        {
            return x >= StartX && x < EndX && y >= StartY && y < EndY;
        }

        public GeoTiff GetSubset(int bottomLeftX, int bottomLeftY, int size)
        {
            var tiff = new GeoTiff
            {
                HeightMap = new float[size, size], 
                StartX = bottomLeftX, 
                StartY = bottomLeftY,
                Width = size,
                Height = size
            };
            tiff.SetEnds();
            for (var y = bottomLeftY; y < bottomLeftY + size; y++)
            for (var x = bottomLeftX; x < bottomLeftX + size; x++)
                tiff.HeightMap[size - y + bottomLeftY - 1, x - bottomLeftX] = GetAltitudeNoCheck(x, y);

            return tiff;
        }

        public int FillVector(Point4D[] vector, double aX, double aY, double bX, double bY, int incMeter = 1, bool withHeights = false)
        {
            var dx = bX - aX;
            var dy = bY - aY;
            var l = Math.Sqrt(dx * dx + dy * dy);

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0;

            var (x, y) = (aX, aY);

            while (m <= l)
            {
                var vm = vector[m];

                vm.X = x;
                vm.Y = y;

                vm.RoundedX = QuickMath.Round(x);
                vm.RoundedY = QuickMath.Round(y);

                if (withHeights)
                    vm.Z = GetAltitudeNoCheck(vm.RoundedX, vm.RoundedY);
                else
                    vm.Z = double.NaN;

                vm.M = m;

                m += incMeter;
                x += xInc;
                y += yInc;
            }

            return (int)l + 1;
        }

        public abstract void Dispose();
    }
}
