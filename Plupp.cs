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
        private bool rightDirection; // 0= right, 1 = left
        private int speed;
        private float maxWidth;
        public Ellipse ellipse;

        public Plupp(bool rightDirection, int speed, float maxWidth)
        {
           
            this.rightDirection = rightDirection;
            this.speed = speed;
            this.maxWidth = maxWidth;
            ellipse = new Ellipse();
            ellipse.Visibility = Visibility.Visible;
            ellipse.Width = 50;
            ellipse.Height = 50;
            ellipse.Fill = new SolidColorBrush(Colors.DarkCyan);

            Thickness margin = new Thickness();
            Random random = new Random();
            float height = random.Next(0, 50);
            if(rightDirection)
                margin = new Thickness(-maxWidth, height,0,0);
            else if(!rightDirection)
                margin = new Thickness(maxWidth, height,0,0);
            this.ellipse.Margin = margin;
        }

        public void Update()
        {
            Thickness margin;
            switch (rightDirection)
            {
                case true:
                    margin = ellipse.Margin;
                    margin.Left += 3;
                    ellipse.Margin = margin;
                    break;
                case false:
                    margin = ellipse.Margin;
                    margin.Left += 3;
                    ellipse.Margin = margin;
                    break;
                default:
                    break;
            }    
        }
    }
}
