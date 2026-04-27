using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace appP.A.Controls;

public class PieDrawable : IDrawable
{
    public IList<appP.A.DashboardPage.ChartItem>? Items { get; set; }

    public string? SelectedLabel { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        var center = new PointF(dirtyRect.Center.X, dirtyRect.Center.Y);
        var radius = Math.Min(dirtyRect.Width, dirtyRect.Height) * 0.4f;
        var total = (float)(Items?.Sum(i => i.Valor) ?? 0.0);
        if (total <= 0)
        {
            // draw empty circle
            canvas.FillColor = Colors.LightGray;
            canvas.FillCircle(center, radius);
            canvas.RestoreState();
            return;
        }

        float startAngle = -90f;
        for (int i = 0; i < Items!.Count; i++)
        {
            var it = Items[i];
            var sweep = (float)(it.Valor / total) * 360f;
            var color = it.Color;

            // compute points along arc to build a polygon (avoid ArcTo portability issues)
            var points = new List<PointF>();
            points.Add(center);
            var steps = Math.Max(6, (int)(sweep / 6));
            for (int s = 0; s <= steps; s++)
            {
                var angle = startAngle + (s / (float)steps) * sweep;
                var rad = angle * (float)System.Math.PI / 180f;
                var px = center.X + (float)System.Math.Cos(rad) * radius;
                var py = center.Y + (float)System.Math.Sin(rad) * radius;
                points.Add(new PointF(px, py));
            }

            // apply slight offset if selected
            if (!string.IsNullOrEmpty(SelectedLabel) && SelectedLabel == it.Label)
            {
                var mid = startAngle + sweep / 2f;
                var dx = (float)(System.Math.Cos(mid * System.Math.PI / 180.0) * 8);
                var dy = (float)(System.Math.Sin(mid * System.Math.PI / 180.0) * 8);
                for (int p = 0; p < points.Count; p++)
                {
                    points[p] = new PointF(points[p].X + dx, points[p].Y + dy);
                }
            }

            using (var path = new PathF())
            {
                path.MoveTo(points[0]);
                for (int p = 1; p < points.Count; p++) path.LineTo(points[p]);
                path.Close();
                canvas.FillColor = color;
                canvas.FillPath(path);
            }

            startAngle += sweep;
        }

        // draw center label if selected
        if (!string.IsNullOrEmpty(SelectedLabel))
        {
            var selected = Items.FirstOrDefault(x => x.Label == SelectedLabel);
            if (selected != null)
            {
                canvas.FontColor = Colors.Black;
                canvas.FontSize = 14;
                var text = $"{selected.Label}\n{selected.PorcentajeTexto}";
                var rect = new RectF(center.X - radius * 0.6f, center.Y - radius * 0.6f, radius * 1.2f, radius * 1.2f);
                canvas.DrawString(text, rect, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }

        canvas.RestoreState();
    }
}
