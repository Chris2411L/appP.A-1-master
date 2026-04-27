using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace appP.A.Controls;

public class RadialBarsDrawable : IDrawable
{
    public IList<appP.A.DashboardPage.ChartItem>? Items { get; set; }

    public string? SelectedLabel { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        var center = new PointF(dirtyRect.Center.X, dirtyRect.Center.Y);
        var outer = Math.Min(dirtyRect.Width, dirtyRect.Height) * 0.45f;
        var inner = outer * 0.2f;

        var values = Items?.Select(i => i.Valor).ToArray() ?? new double[0];
        if (values.Length == 0)
        {
            canvas.FillColor = Colors.LightGray;
            canvas.FillCircle(center, outer);
            canvas.RestoreState();
            return;
        }

        var max = values.Max();
        if (max <= 0) max = 1;

        var count = values.Length;
        var step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            var v = (float)(values[i] / max);
            var stroke = Math.Max(6f, inner + v * (outer - inner)) * 0.08f;
            var color = Items![i].Color;

            var sweep = step * 0.9f;
            var angle = -90f + i * step + (step - sweep) / 2f;

            // draw arc as stroked circle segment
            canvas.StrokeColor = color;
            canvas.StrokeSize = stroke;
            canvas.DrawArc(center.X - outer, center.Y - outer, outer * 2, outer * 2, angle, sweep, false, false);

            if (!string.IsNullOrEmpty(SelectedLabel) && SelectedLabel == Items[i].Label)
            {
                // draw highlight (bigger stroke and slightly larger radius)
                canvas.StrokeColor = Colors.Black;
                canvas.StrokeSize = stroke + 2f;
                canvas.DrawArc(center.X - (outer + 3), center.Y - (outer + 3), (outer + 3) * 2, (outer + 3) * 2, angle, sweep, false, false);
            }
        }

        canvas.RestoreState();
    }
}
