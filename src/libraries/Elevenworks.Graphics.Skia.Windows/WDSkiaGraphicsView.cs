﻿using System.Windows;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace Elevenworks.Graphics
{
    public class WDSkiaGraphicsView : SKElement
    {
        private readonly EWRectangle _dirtyRect = new EWRectangle();
        private EWDrawable _drawable;
        private ISkiaGraphicsRenderer _renderer;

        public WDSkiaGraphicsView()
        {
            Renderer = CreateDefaultRenderer();
        }

        public ISkiaGraphicsRenderer Renderer
        {
            get { return _renderer; }

            set
            {
                if (_renderer != null)
                {
                    _renderer.Drawable = null;
                    _renderer.GraphicsView = null;
                    _renderer.Dispose();
                }

                _renderer = value ?? CreateDefaultRenderer();
                _renderer.GraphicsView = this;
                _renderer.Drawable = _drawable;
                _renderer.SizeChanged((int) CanvasSize.Width, (int) CanvasSize.Height);
            }
        }

        private ISkiaGraphicsRenderer CreateDefaultRenderer()
        {
            return new WDSkiaDirectRenderer();
        }

        public EWColor BackgroundColor
        {
            get { return _renderer.BackgroundColor; }
            set { _renderer.BackgroundColor = value; }
        }

        public EWDrawable Drawable
        {
            get { return _drawable; }
            set
            {
                _drawable = value;
                _renderer.Drawable = _drawable;
            }
        }

        protected override void OnPaintSurface(
            SKPaintSurfaceEventArgs e)
        {
            _renderer?.Draw(e.Surface.Canvas, _dirtyRect);
        }

        protected override void OnRenderSizeChanged(
            SizeChangedInfo sizeInfo)
        {
            _dirtyRect.Width = (float) sizeInfo.NewSize.Width;
            _dirtyRect.Height = (float) sizeInfo.NewSize.Height;
            _renderer?.SizeChanged((int) sizeInfo.NewSize.Width, (int) sizeInfo.NewSize.Height);
        }

        public void Invalidate()
        {
            InvalidateVisual();
        }
    }
}