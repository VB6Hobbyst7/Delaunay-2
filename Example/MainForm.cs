using Delaunay;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Example
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void preview_Paint(object sender, PaintEventArgs e)
        {
            List<PointF> points = GetRandomPoints();
            RectangleF size = new RectangleF(0, 0, preview.Width, preview.Height);
            Voronoi voronoi = new Voronoi(points, null, size);
            foreach (PointF point in points)
            {
                List<PointF> region = voronoi.Region(point);
                e.Graphics.DrawRectangle(Pens.Red, point.X, point.Y, 10, 10);
                e.Graphics.DrawLines(Pens.Black, region.ToArray());
            }
        }

        private List<PointF> GetRandomPoints()
        {
            List<PointF> points = new List<PointF>();
            Random random = new Random();
            int border = 10;
            for (int i = 0; i < 20; i++)
            {
                float x = random.Next(border, preview.Width - border * 2);
                float y = random.Next(border, preview.Height - border * 2);
                points.Add(new PointF(x, y));
            }
            return points;
        }
    }
}