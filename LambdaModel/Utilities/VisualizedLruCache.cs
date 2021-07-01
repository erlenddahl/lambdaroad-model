using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace LambdaModel.Utilities
{
    public class VisualizedLruCache<K, T> : LruCache<K, T>
    {
        private readonly string _saveToDirectory;
        private readonly int _tileSize;
        private int _step = 0;
        private Bitmap _bitmap;
        private Graphics _g;
        private readonly Font _font;
        private readonly int _drawSize;

        public VisualizedLruCache(string saveToDirectory, int tileSize, int maxItems, int removeItemsWhenFull) : base(maxItems, removeItemsWhenFull)
        {
            _saveToDirectory = saveToDirectory;
            _tileSize = tileSize;
            _drawSize = 20;
            _font = new Font("Arial", 8);
        }

        public override bool TryGetValue(K key, out T value)
        {
            var res = base.TryGetValue(key, out value);

            var genericKeys = _cache.Keys.ToArray();
            if (genericKeys is (int x, int y)[] keys && keys.Any())
            {
                var maxX = int.MinValue;
                var minX = int.MaxValue;
                var maxY = int.MinValue;
                var minY = int.MaxValue;

                foreach (var k in keys)
                {
                    if (k.x > maxX) maxX = k.x;
                    if (k.x < minX) minX = k.x;
                    if (k.y > maxY) maxY = k.y;
                    if (k.y < minY) minY = k.y;
                }

                var (bw, bh) = (maxX - minX + _tileSize, maxY - minY + _tileSize);
                bw = bw / _tileSize * _drawSize;
                bh = bh / _tileSize * _drawSize;
                if (_bitmap == null || _bitmap.Width != bw || _bitmap.Height != bh)
                {
                    _bitmap = new Bitmap(bw, bh);
                    _g = Graphics.FromImage(_bitmap);
                }

                _g.Clear(Color.White);
                for (var i = 0; i < keys.Length; i++)
                {
                    var k = keys[i];
                    var (x, y) = (k.x - minX, k.y - minY);
                    x = x / _tileSize * _drawSize;
                    y = y / _tileSize * _drawSize;
                    _g.FillRectangle(Brushes.Gray, x, y, _drawSize, _drawSize);
                    _g.DrawString(GetCreationAge(genericKeys[i]).ToString(), _font, Brushes.Black, x, y);
                }

                _bitmap.Save(System.IO.Path.Combine(_saveToDirectory, _step.ToString("00000000") + ".jpg"), ImageFormat.Jpeg);
            }

            _step++;
            return res;
        }
    }
}
