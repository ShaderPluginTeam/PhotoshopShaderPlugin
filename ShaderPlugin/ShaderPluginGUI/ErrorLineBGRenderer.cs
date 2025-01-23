using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace ShaderPluginGUI
{
    public class ErrorLineBackgroundRenderer : IBackgroundRenderer
    {
        public List<int> ErrorLines = new List<int>();
        SolidColorBrush ErrorBrush = new SolidColorBrush(Color.FromArgb(0xA0, 0xA0, 0x30, 0x30));
        Pen ErrorPen = new Pen(new SolidColorBrush(Color.FromArgb(0xA0, 0xC0, 0x30, 0x30)), 1f);

        public KnownLayer Layer => KnownLayer.Background;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            foreach (VisualLine visualLine in textView.VisualLines)
            {
                if (ErrorLines == null || ErrorLines.Count <= 0 || !ErrorLines.Contains(visualLine.FirstDocumentLine.LineNumber))
                    continue;

                Rect rc = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, visualLine, 0, visualLine.GetVisualColumn(0)).First();
                drawingContext.DrawRectangle(ErrorBrush, ErrorPen, new Rect(rc.Left, rc.Top, textView.HorizontalOffset + textView.ActualWidth, rc.Height));
            }
        }
    }
}
