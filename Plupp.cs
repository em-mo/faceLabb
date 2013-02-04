using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;

namespace FaceTrackingBasics
{
    class Plupp
    {
        private int rightDirection; // 0= right, 1 = left
        private int speed;
        private float maxWidth;
        private float maxHeight;
        private DateTime TTL;
        public Ellipse ellipse;

        public Plupp(int speed, float maxWidth, float maxHeight)
        {
           
           
            this.speed = speed;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            ellipse = new Ellipse();
            ellipse.Visibility = Visibility.Visible;
            ellipse.Width = 50;
            ellipse.Height = 50;
            ellipse.Fill = new SolidColorBrush(Colors.DarkCyan);

            TTL = DateTime.Now.AddSeconds(15);

            Thickness margin = new Thickness();
            Random random = new Random();
            this.rightDirection = random.Next(0, 2);
            float height = random.Next(0, 70);
            System.Console.WriteLine(rightDirection);
            if(rightDirection == 0)
                margin = new Thickness(-maxWidth + ellipse.Width, -height * 3.5, 0, 0);
            else
                margin = new Thickness(maxWidth - ellipse.Width, -height * 3.5, 0, 0);
            this.ellipse.Margin = margin;
        }

        public Rect returnRectangle()
        {
            Size size = new Size(ellipse.Width, ellipse.Height);
            Rect rect = new Rect(new Point((maxWidth + ellipse.Margin.Left - ellipse.Width) / 2, (maxHeight + ellipse.Margin.Top - ellipse.Height) / 2), size);
            return rect;
        }

        public bool Update()
        {
            Thickness margin;
            switch (rightDirection)
            {
                case 0:
                    margin = ellipse.Margin;
                    margin.Left += 3;
                    ellipse.Margin = margin;
                    break;
                case 1:
                    margin = ellipse.Margin;
                    margin.Left -= 3;
                    ellipse.Margin = margin;
                    break;
                default:
                    break;
            }

            if (DateTime.Now > TTL)
                return true;
            else
                return false;
        }
    }
}
