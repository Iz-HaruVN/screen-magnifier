using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ScreenMagnifier
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(Point pt, uint dwFlags);
        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

        private Timer timer;
        private PictureBox pictureBox;
        private const int captureSize = 100;
        private const int zoom = 3;

        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.TopMost = true;
            this.Width = captureSize * zoom;
            this.Height = captureSize * zoom;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(10, 10);

            // Tạo PictureBox
            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            this.Controls.Add(pictureBox);

            // Tạo Timer
            timer = new Timer { Interval = 20 };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private Point GetPhysicalCursorPos()
        {
            // Luôn trả về tọa độ desktop gốc
            GetCursorPos(out Point pt);
            return pt;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                Point cursor = GetPhysicalCursorPos();

                Rectangle rect = new Rectangle(
                    cursor.X - captureSize / 2,
                    cursor.Y - captureSize / 2,
                    captureSize,
                    captureSize
                );

                // Giới hạn trong screen hiện tại
                Screen scr = Screen.FromPoint(cursor);
                rect.Intersect(scr.Bounds);

                if (rect.Width <= 0 || rect.Height <= 0) return;

                using (Bitmap bmp = new Bitmap(rect.Width, rect.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(rect.Location, Point.Empty, rect.Size);
                    }

                    Bitmap zoomed = new Bitmap(bmp, new Size(rect.Width * zoom, rect.Height * zoom));
                    pictureBox.Image?.Dispose();
                    pictureBox.Image = zoomed;
                }
            }
            catch (Exception ex)
            {
                this.Text = $"Lỗi: {ex.Message}";
            }
        }
    }
}
