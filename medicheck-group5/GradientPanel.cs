using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace medicheck_group5
{
    public class GradientPanel : Panel
    {
        // --- Color and Position Properties (No change) ---
        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        public Color Color3 { get; set; }

        private float _middleColorPosition = 0.5f;
        public float MiddleColorPosition
        {
            get { return _middleColorPosition; }
            set
            {
                _middleColorPosition = Math.Max(0f, Math.Min(1f, value));
                this.Invalidate();
            }
        }

        // --- NEW Property for Gradient Angle (0-360 degrees) ---
        private float _angle = 0f;

        [DefaultValue(0f)]
        public float Angle
        {
            get { return _angle; }
            set
            {
                // Keep the angle between 0 and 360 degrees
                _angle = value % 360f;
                if (_angle < 0) _angle += 360f;
                this.Invalidate(); // Redraw
            }
        }

        // --- Override OnPaintBackground to Draw the Gradient ---
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.ClientRectangle.Width > 0 && this.ClientRectangle.Height > 0)
            {
                Rectangle rect = this.ClientRectangle;

                // 1. Initialize the LinearGradientBrush using the Rectangle and the Angle
                // We must use the (Rectangle, Color, Color, float) constructor for custom angles.
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect,
                    Color1, // Starting Color (Required, but overwritten by ColorBlend)
                    Color3, // Ending Color (Required, but overwritten by ColorBlend)
                    _angle)) // Use the new Angle property
                {
                    // 2. Define the three colors and their positions
                    Color[] colors = { Color1, Color2, Color3 };
                    float[] positions = { 0.0f, _middleColorPosition, 1.0f };

                    // 3. Create and set the ColorBlend
                    ColorBlend colorBlend = new ColorBlend();
                    colorBlend.Colors = colors;
                    colorBlend.Positions = positions;
                    brush.InterpolationColors = colorBlend;

                    // 4. Draw the gradient
                    e.Graphics.FillRectangle(brush, rect);
                }
            }
        }
    }
}