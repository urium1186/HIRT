using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for SpinnerAdvControl.xaml
    /// </summary>
    public partial class SpinnerAdvControl : UserControl
    {
        public SpinnerAdvControl()
        {
            InitializeComponent();
            animate();
        }

        public void animate()
        {
            DoubleAnimation animation1 = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(1)));
            animation1.RepeatBehavior = RepeatBehavior.Forever;
            rotateTransform1.BeginAnimation(RotateTransform.AngleProperty, animation1);

            DoubleAnimation animation2 = new DoubleAnimation(360, 0, new Duration(TimeSpan.FromSeconds(2)));
            animation2.RepeatBehavior = RepeatBehavior.Forever;
            rotateTransform2.BeginAnimation(RotateTransform.AngleProperty, animation2);

            DoubleAnimation animation3 = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(3)));
            animation3.RepeatBehavior = RepeatBehavior.Forever;
            rotateTransform3.BeginAnimation(RotateTransform.AngleProperty, animation3);
        }

    }
}
