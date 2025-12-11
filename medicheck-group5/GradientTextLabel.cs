using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace MediCheck_UI_
{
    public class GradientTextLabel : Label
    {

        public Color GradientStartColor { get; set; } = ColorTranslator.FromHtml("#004059");
        public Color GradientEndColor { get; set; } = ColorTranslator.FromHtml("#26FFEA");

        public GradientTextLabel()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // Optional, but helps


            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            using (LinearGradientBrush brush = new LinearGradientBrush(
                rect,
                this.GradientStartColor,
                this.GradientEndColor,
                LinearGradientMode.Horizontal))
            {

                using (StringFormat format = new StringFormat())
                {

                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Near;


                    switch (this.TextAlign)
                    {
                        case ContentAlignment.TopCenter:
                        case ContentAlignment.MiddleCenter:
                        case ContentAlignment.BottomCenter:
                            format.Alignment = StringAlignment.Center;
                            break;
                        case ContentAlignment.TopRight:
                        case ContentAlignment.MiddleRight:
                        case ContentAlignment.BottomRight:
                            format.Alignment = StringAlignment.Far;
                            break;
                    }

                    switch (this.TextAlign)
                    {
                        case ContentAlignment.TopLeft:
                        case ContentAlignment.TopCenter:
                        case ContentAlignment.TopRight:
                            format.LineAlignment = StringAlignment.Near;
                            break;
                        case ContentAlignment.MiddleLeft:
                        case ContentAlignment.MiddleCenter:
                        case ContentAlignment.MiddleRight:
                            format.LineAlignment = StringAlignment.Center;
                            break;
                        case ContentAlignment.BottomLeft:
                        case ContentAlignment.BottomCenter:
                        case ContentAlignment.BottomRight:
                            format.LineAlignment = StringAlignment.Far;
                            break;
                    }
                    e.Graphics.DrawString(
                        this.Text,
                        this.Font,
                        brush,
                        rect,
                        format
                    );
                }
            }
        }
    }
}