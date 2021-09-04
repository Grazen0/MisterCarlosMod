using System;
using Microsoft.Xna.Framework;

namespace MisterCarlosMod
{
    public class Utils
    {
        // Code "borrowed" from StackOverflow
        public static Color HsvToColor(float h, float s, float v)
        {
            double H = h % 360;
            while (H < 0) { H += 360; };
            double R, G, B;
            if (v <= 0)
            { R = G = B = 0; }
            else if (s <= 0)
            {
                R = G = B = v;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = v * (1 - s);
                double qv = v * (1 - s * f);
                double tv = v * (1 - s * (1 - f));
                switch (i)
                {
                    case 0:
                        R = v;
                        G = tv;
                        B = pv;
                        break;
                    case 1:
                        R = qv;
                        G = v;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = v;
                        B = tv;
                        break;
                    case 3:
                        R = pv;
                        G = qv;
                        B = v;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = v;
                        break;
                    case 5:
                        R = v;
                        G = pv;
                        B = qv;
                        break;
                    case 6:
                        R = v;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = v;
                        G = pv;
                        B = qv;
                        break;
                    default:
                        R = G = B = v;
                        break;
                }
            }
            int r = (int)MathHelper.Clamp((float)R * 255f, 0f, 255f);
            int g = (int)MathHelper.Clamp((float)G * 255f, 0f, 255f);
            int b = (int)MathHelper.Clamp((float)B * 255f, 0f, 255f);

            return new Color(r, g, b);
        }
    }
}
