﻿using Elevenworks.Graphics;

namespace GraphicsTester.Scenarios
{
    public class ArcScenario2 : AbstractScenario
    {
        public ArcScenario2()
            : base(720, 1024)
        {
        }

        private void DrawArc(EWCanvas canvas, float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed)
        {
            canvas.FillColor = StandardColors.Black;
            canvas.FillArc(x, y, width, height, startAngle, endAngle, clockwise);

            var path = new EWPath();
            path.AddArc(x, y + 400, x + width, y + 400 + width, startAngle, endAngle, clockwise);
            path.Close();
            canvas.FillPath(path);
        }

        public override void Draw(EWCanvas canvas, float zoom, float ppu)
        {
            canvas.SaveState();
            DrawArc(canvas, 400, 100, 80, 80, -315, 300, false, false);
            canvas.RestoreState();
        }
    }
}