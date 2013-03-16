using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public class Gaussian
    {
        /*
        private static bool uselast = true;
        private static double next_gaussian = 0.0;

        public static double BoxMuller()
        {
            if (uselast)
            {
                uselast = false;
                return next_gaussian;
            }
            else
            {
                double v1, v2, s;
                do
                {
                    v1 = 2.0 * Game.Random.NextDouble() - 1.0;
                    v2 = 2.0 * Game.Random.NextDouble() - 1.0;
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1.0 || s == 0);

                s = System.Math.Sqrt((-2.0 * System.Math.Log(s)) / s);

                next_gaussian = v2 * s;
                uselast = true;
                return v1 * s;
            }
        }*/

        public static double NextGaussianDouble()
        {
            double U, u, v, S;

            do
            {
                u = 2.0 * Game.Random.NextDouble() - 1.0;
                v = 2.0 * Game.Random.NextDouble() - 1.0;
                S = u * u + v * v;
            }
            while (S >= 1.0);

            double fac = Math.Sqrt(-2.0 * Math.Log(S) / S);
            return u * fac;
        }

        public static double BoxMuller(double mean, double standard_deviation)
        {
            return mean + NextGaussianDouble() * standard_deviation;
        }
    }
}
