namespace System.Graphics
{
    public class EWPaint
    {
        private float _angle;
        private EWSize _endLocationRatio;
        private EWImage _image;
        private EWPaintType _paintType = EWPaintType.LINEAR_GRADIENT;
        private IPattern _pattern;
        private EWSize _startLocationRatio;

        private EWPaintStop[] _stops =
        {
            new EWPaintStop(0, Colors.White),
            new EWPaintStop(1, Colors.White)
        };

        public EWPaint()
        {
        }

        public EWPaint(EWPaint source)
        {
            if (source != null)
            {
                _stops = new EWPaintStop[source.Stops.Length];
                for (var i = 0; i < _stops.Length; i++)
                {
                    _stops[i] = new EWPaintStop(source.Stops[i]);
                }

                _paintType = source.PaintType;
                _angle = source._angle;

                if (source.StartLocationRatio != null)
                {
                    _startLocationRatio = new EWSize(source.StartLocationRatio);
                }

                if (source.EndLocationRatio != null)
                {
                    _endLocationRatio = new EWSize(source.EndLocationRatio);
                }

                _pattern = source._pattern;
                _image = source._image;
            }
        }

        public EWPaintStop[] Stops
        {
            get => _stops;
            set
            {
                _stops = value;
                if (_stops == null || _stops.Length == 0)
                {
                    _stops = new[] {new EWPaintStop(0, Colors.White), new EWPaintStop(1, Colors.White)};
                }
            }
        }

        public Color StartColor
        {
            get => _stops[StartColorIndex].Color;

            set
            {
                var startColorIndex = StartColorIndex;
                _stops[startColorIndex].Color = value ?? Colors.White;
            }
        }

        public Color EndColor
        {
            get => _stops[EndColorIndex].Color;

            set
            {
                var endColorIndex = EndColorIndex;
                _stops[endColorIndex].Color = value ?? Colors.White;
            }
        }

        public Color ForegroundColor
        {
            get => _stops[0].Color;

            set
            {
                var startColorIndex = StartColorIndex;
                _stops[startColorIndex].Color = value ?? Colors.White;
            }
        }

        public Color BackgroundColor
        {
            get => _stops[_stops.Length - 1].Color;

            set => _stops[_stops.Length - 1].Color = value;
        }

        public IPattern Pattern
        {
            get => _pattern;

            set
            {
                _pattern = value;
                _paintType = EWPaintType.PATTERN;

                if (!(_pattern is PaintPattern))
                {
                    var paintPattern = new PaintPattern(_pattern);
                    paintPattern.Paint = this;
                    _pattern = paintPattern;
                }
            }
        }

        public EWImage Image
        {
            get => _image;
            set
            {
                _image = value;
                _paintType = EWPaintType.IMAGE;
            }
        }

        private int EndColorIndex
        {
            get
            {
                var index = -1;
                float offset = 0;
                for (var i = 0; i < _stops.Length; i++)
                {
                    if (_stops[i].Offset >= offset)
                    {
                        index = i;
                        offset = _stops[i].Offset;
                    }
                }

                return index >= 0 ? index : _stops.Length - 1;
            }
        }

        public int StartColorIndex
        {
            get
            {
                var index = -1;
                float offset = 1;
                for (var i = 0; i < _stops.Length; i++)
                {
                    if (_stops[i].Offset <= offset)
                    {
                        index = i;
                        offset = _stops[i].Offset;
                    }
                }

                return index >= 0 ? index : 0;
            }
        }

        public EWPaintType PaintType
        {
            get => _paintType;
            set => _paintType = value;
        }

        public float Angle
        {
            get => _angle;
            set => _angle = value;
        }

        public EWSize StartLocationRatio
        {
            get => _startLocationRatio;
            set => _startLocationRatio = value;
        }

        public EWSize EndLocationRatio
        {
            get => _endLocationRatio;
            set => _endLocationRatio = value;
        }

        public EWSize FocalPointRatio { get; set; }

        public EWPaintStop[] GetSortedStops()
        {
            var vStops = new EWPaintStop[_stops.Length];
            Array.Copy(_stops, vStops, _stops.Length);
            Array.Sort(vStops);
            return vStops;
        }

        public void AddOffset(float offset)
        {
            AddOffset(offset, GetColorAt(offset));
        }

        public void AddOffset(float offset, Color color)
        {
            var oldStops = Stops;
            var newStops = new EWPaintStop[oldStops.Length + 1];
            
            for (var i = 0; i < oldStops.Length; i++)
                newStops[i] = oldStops[i];

            newStops[oldStops.Length] = new EWPaintStop(offset, color);

            Stops = newStops;
        }

        public void RemoveOffset(int index)
        {
            if (index < 0 || index >= Stops.Length)
            {
                return;
            }

            var oldStops = Stops;
            var newStops = new EWPaintStop[oldStops.Length - 1];
            for (var i = 0; i < oldStops.Length; i++)
            {
                if (i < index)
                {
                    newStops[i] = oldStops[i];
                }
                else if (i > index)
                {
                    newStops[i - 1] = oldStops[i];
                }
            }

            Stops = newStops;
        }

        public Color GetColorAt(float offset)
        {
            var stops = Stops;
            if (stops.Length == 1)
            {
                return stops[0].Color;
            }

            var before = float.MaxValue;
            var beforeIndex = -1;
            var after = float.MaxValue;
            var afterIndex = -1;

            for (var i = 0; i < stops.Length; i++)
            {
                var currentOffset = stops[i].Offset;

                if (Math.Abs(currentOffset - offset) < Geometry.Epsilon)
                {
                    return stops[i].Color;
                }

                if (currentOffset < offset)
                {
                    var dx = offset - currentOffset;
                    if (dx < before)
                    {
                        before = currentOffset;
                        beforeIndex = i;
                    }
                }
                else if (currentOffset > offset)
                {
                    var dx = currentOffset - offset;
                    if (dx < after)
                    {
                        after = currentOffset;
                        afterIndex = i;
                    }
                }
            }

            if (afterIndex == -1)
            {
                return EndColor;
            }

            if (beforeIndex == -1)
            {
                return StartColor;
            }

            var f = Geometry.GetFactor(before, after, offset);
            return BlendStartAndEndColors(stops[beforeIndex].Color, stops[afterIndex].Color, f);
        }

        public Color BlendStartAndEndColors()
        {
            if (_stops == null || _stops.Length < 2)
            {
                return Colors.White;
            }

            return BlendStartAndEndColors(StartColor, EndColor, .5f);
        }

        public Color BlendStartAndEndColors(Color startColor, Color endColor, float factor)
        {
            if (startColor == null) startColor = Colors.White;
            if (endColor == null) endColor = Colors.White;

            var r = Geometry.GetLinearValue(startColor.Red, endColor.Red, factor);
            var g = Geometry.GetLinearValue(startColor.Green, endColor.Green, factor);
            var b = Geometry.GetLinearValue(startColor.Blue, endColor.Blue, factor);
            var a = Geometry.GetLinearValue(startColor.Alpha, endColor.Alpha, factor);

            return new Color(r, g, b, a);
        }

        public override string ToString()
        {
            return $"[{nameof(EWPaint)}: StartColor={StartColor}, EndColor={EndColor}, PaintType={PaintType}, Angle={Angle}]";
        }
        
        public void SetStops(float[] offsets, Color[] colors)
        {
            var stopCount = Math.Min(colors.Length, offsets.Length);
            _stops = new EWPaintStop[stopCount];
            for (var p = 0; p < stopCount; p++)
            {
                _stops[p] = new EWPaintStop(offsets[p], colors[p]);
            }
        }

        public bool IsTransparent
        {
            get
            {
                if (_paintType == EWPaintType.SOLID)
                {
                    return StartColor.Alpha < 1;
                }

                if (_paintType == EWPaintType.LINEAR_GRADIENT || _paintType == EWPaintType.RADIAL_GRADIENT)
                {
                    foreach (var stop in Stops)
                    {
                        if (stop.Color != null && stop.Color.Alpha < 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                if (_paintType == EWPaintType.PATTERN)
                {
                    if (BackgroundColor == null || BackgroundColor.Alpha < 1)
                        return true;

                    return ForegroundColor.Alpha < 1;
                }

                return false;
            }
        }
    }
}