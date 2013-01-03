using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgateLib;
using AgateLib.DisplayLib;
using AgateLib.Geometry;

namespace AgateDemo
{
    public class Chroma
    {

        private static int Blend(int a, int b, double coef)
        {
            return (int)(a + (b - a) * coef);
        }

        /**
    * Returns an Color that is the given distance from the first color to the
    * second color.
    *
    * @param color1 The first color
    * @param color2 The second color
    * @param coef The percent towards the second color, as 0.0 to 1.0
    * @return
    */
        public static Color Blend(Color color1, Color color2, double coef)
        {
            return Color.FromArgb(Blend(color1.R, color2.R, coef),
                    Blend(color1.G, color2.G, coef),
                    Blend(color1.B, color2.B, coef));
        }

        /**
    * Returns an Color that is randomly chosen from the color line between the
    * two provided colors from the two provided points.
    *
    * @param color1
    * @param color2
    * @param min The minimum percent towards the second color, as 0.0 to 1.0
    * @param max The maximum percent towards the second color, as 0.0 to 1.0
    * @return
    */
        public static Random rng = new Random();
        public static Color RandomBlend(Color color1, Color color2, double min, double max)
        {
            return Blend(color1, color2, rng.NextDouble() * (max - min) + min);
        }
    }
}
