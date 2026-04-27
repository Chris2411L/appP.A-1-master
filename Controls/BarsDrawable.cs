using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace appP.A.Controls;

public class BarsDrawable : IDrawable
{
    public IList<appP.A.DashboardPage.ChartItem>? Items { get; set; }
    public string? SelectedLabel { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.FontSize = 12;
        canvas.FontColor = Colors.Black;

        var items = Items?.ToArray() ?? new appP.A.DashboardPage.ChartItem[0];
        if (items.Length == 0)
        {
            canvas.FillColor = Colors.LightGray;
            canvas.FillRectangle(dirtyRect);
            canvas.RestoreState();
            return;
        }

        var max = (float)items.Max(i => i.Valor);
        if (max <= 0) max = 1;

        var padding = 8f;
        var availableHeight = dirtyRect.Height - padding * 2;
        var rowHeight = availableHeight / items.Length;
        var barLeft = dirtyRect.Left + 80; // reserve left area for labels in UI
        var barRight = dirtyRect.Right - 8;

        for (int i = 0; i < items.Length; i++)
        {
            var it = items[i];
            var y = dirtyRect.Top + padding + i * rowHeight;
            var centerY = y + rowHeight / 2f;

            // background bar
            canvas.FillColor = Color.FromArgb("#E5E7EB");
            canvas.FillRoundedRectangle(barLeft, centerY - 10, barRight - barLeft, 20, 10);

            // filled bar
            var width = (float)(it.Valor / max) * (barRight - barLeft);
            var color = it.Color;
            canvas.FillColor = color;
            canvas.FillRoundedRectangle(barLeft, centerY - 10, width, 20, 10);

            // draw value at end
            canvas.FontColor = Colors.Black;
            var valueText = it.Valor.ToString("C0");
            var valueRect = new RectF(barRight - 80, centerY - 10, 80, 20);
            canvas.DrawString(valueText, valueRect, HorizontalAlignment.Right, VerticalAlignment.Center);

            // highlight if selected
            if (!string.IsNullOrEmpty(SelectedLabel) && SelectedLabel == it.Label)
            {
                canvas.StrokeColor = Colors.Black;
                canvas.StrokeSize = 1.5f;
                canvas.DrawRoundedRectangle(barLeft - 2, centerY - 12, Math.Max(4, width) + 4, 24, 12);
            }
        }

        canvas.RestoreState();
    }
}
