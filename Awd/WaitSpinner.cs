using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Awd
{
    public static class WaitSpinner
    {
        public static IEnumerable<IEnumerable<object>> Spinner
        {
            get
            {
                var array = new[]
                {
                    new object[] 
                    {
                        0d,
                        new LinearGradientBrush(new GradientStopCollection(new [] {new GradientStop(Colors.White, 4d), new GradientStop(Colors.Transparent, 0d)})){StartPoint = new Point(0,0), EndPoint= new Point(1,0)}
                    },
                    new object[] 
                    {
                        90d,
                        new LinearGradientBrush(new GradientStopCollection(new [] {new GradientStop(Colors.White, 3d), new GradientStop(Colors.Transparent, -1d)})){StartPoint = new Point(0,0), EndPoint= new Point(1,0)}
                    },
                    new object[] 
                    {
                        180d,
                        new LinearGradientBrush(new GradientStopCollection(new [] {new GradientStop(Colors.White, 2d), new GradientStop(Colors.Transparent, -2d)})){StartPoint = new Point(0,0), EndPoint= new Point(1,0)}
                    },
                    new object[] 
                    {
                        270d,
                        new LinearGradientBrush(new GradientStopCollection(new [] {new GradientStop(Colors.White, 1d), new GradientStop(Colors.Transparent, -3d)})){StartPoint = new Point(0,0), EndPoint= new Point(1,0)}
                    },


                };
                return array;
            }
        }
    }
}