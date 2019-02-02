using System;
using System.Collections;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class VolumeControl : UserControl
    {

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, (double)value); }
        }
        public static DependencyProperty StartAngleProperty = 
                      DependencyProperty.Register("StartAngle", typeof(double), typeof(VolumeControl), null);

        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, (double)value); }
        }
        public static DependencyProperty EndAngleProperty = 
                      DependencyProperty.Register("EndAngle", typeof(double), typeof(VolumeControl), null);

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, (double)value); }
        }
        public static DependencyProperty RadiusProperty = 
                      DependencyProperty.Register("Radius", typeof(double), typeof(VolumeControl), null);

        public Point  Center
        {
            get { return (Point)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, (Point)value); }
        }
        public static DependencyProperty CenterProperty = 
                      DependencyProperty.Register("Center", typeof(Point), typeof(VolumeControl), null);

        public bool   SmallAngle
        {
            get { return (bool)GetValue(SmallAngleProperty); }
            set { SetValue(SmallAngleProperty, (bool)value); }

        }
        public static DependencyProperty SmallAngleProperty = 
                      DependencyProperty.Register("SmallAngle", typeof(bool), typeof(VolumeControl), null);

        public VolumeControl()
        {
            this.InitializeComponent();

            //Color red = new Color();
            //red.R = 0xFF;
            //red.G = 0x00;
            //red.B = 0x00;
            //red.A = 0xFF;
            //VolumePath.Stroke = new SolidColorBrush(red);
            //VolumePath.StrokeThickness = 3;


            //StartAngle = 0;
            //EndAngle = 210;
            //double w = this.ActualWidth;

            //Radius = 50;
            //Center = new Point(90, 90);
            ////SmallAngle = true;

            //VolumePath.Data = ValueGeometry(100);

            //EllipseGeometry geometry = new EllipseGeometry();
            //geometry.RadiusX = geometry.RadiusY = 50;
            //geometry.Center = new Point(70, 70);

            //VolumePath.Data = geometry;

        }

        public void SetVolume(int volume)
        {
            double w = this.ActualWidth;

            Radius = Math.Min(this.ActualWidth, this.ActualHeight)/2 - 20;
            Center = new Point(this.ActualWidth/2, this.ActualHeight/2);
            //SmallAngle = true;

            VolumePath.Data = ValueGeometry(90);

        }

        public Geometry ValueGeometry(int endAngle)
        {
            Point p0, p1;
            Radius = 100;
            bool largeAngle = true;
            endAngle = 60;

            p0.X = Center.X + Radius * Math.Cos(0);
            p0.Y = Center.Y + Radius * Math.Sin(0);

            p1.X = Center.X + Radius * Math.Cos(endAngle);
            p1.Y = Center.Y + Radius * Math.Sin(endAngle);

            ArcSegment arcSegment = new ArcSegment();
            arcSegment.IsLargeArc = largeAngle;
            arcSegment.Point = p1;
            arcSegment.Size = new Size(Radius, Radius);
            arcSegment.SweepDirection = SweepDirection.Clockwise;
            arcSegment.RotationAngle = 0.0;

            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = p0;
            pathFigure.Segments.Add(arcSegment);
            pathFigure.IsClosed = false;
            pathFigure.IsFilled = false;

            List<PathFigure> figures = new List<PathFigure>();
            figures.Add(pathFigure);

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            pathGeometry.FillRule = FillRule.EvenOdd;

            return pathGeometry;

        }


        public Geometry ArcGeometry
        {
            get
            {
                double a0 = StartAngle < 0 ? StartAngle + 2 * Math.PI : StartAngle;
                double a1 = EndAngle < 0 ? EndAngle + 2 * Math.PI : EndAngle;

                if (a1 < a0)
                    a1 += Math.PI * 2;

                SweepDirection sweepDirection = SweepDirection.Counterclockwise;
                bool large;  // large == true если дуга больше 180 градусов
                if (SmallAngle)
                {
                    large = false;
                    sweepDirection = (a1 - a0) > Math.PI ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
                }
                else
                    large = (Math.Abs(a1 - a0) < Math.PI);

                Point p0, p1;
                p0.X = Center.X + Radius * Math.Cos(a0);
                p0.Y = Center.Y + Radius * Math.Sin(a0);

                p1.X = Center.X + Radius * Math.Cos(a1);
                p1.Y = Center.Y + Radius * Math.Sin(a1);

                ArcSegment arcSegment = new ArcSegment();
                arcSegment.IsLargeArc = large;
                arcSegment.Point = p1;
                arcSegment.Size = new Size(Radius, Radius);
                arcSegment.SweepDirection = sweepDirection;
                arcSegment.RotationAngle = 0.0;

                PathFigure pathFigure = new PathFigure();
                pathFigure.StartPoint = p0;
                pathFigure.Segments.Add(arcSegment);
                pathFigure.IsClosed = false;
                pathFigure.IsFilled = false;

                List<PathFigure> figures = new List<PathFigure>();
                figures.Add(pathFigure);

                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);
                pathGeometry.FillRule = FillRule.EvenOdd;

                return pathGeometry;
            }

        }

        private void Control_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetVolume(100);
        }

        private void VolumePath_Loaded(object sender, RoutedEventArgs e)
        {
            SetVolume(100);
        }
    }
}
