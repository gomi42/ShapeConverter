//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019-2020
//
// This file is part of ShapeConverter.
//
// ShapeConverter is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Windows.Media;

namespace ShapeConverter.BusinessLogic.ShapeConverter
{
    /// <summary>
    /// The CMYK to RGB converter
    /// </summary>
    internal static class ColorSpaceConverter
    {
        /// <summary>
        /// Convert a CMYK color given as individual values to RGB
        /// </summary>
        public static Color ConvertCmykToRgb(double c, double m, double y, double k)
        {
            return ConvertCmykToRgb(1.0, c, m, y, k);
        }

        /// <summary>
        /// Convert a CMYK color given as individual values to RGB
        /// </summary>
        public static Color ConvertCmykToRgb(double alpha, double c, double m, double y, double k)
        {
            // The following Adobe ICC profile would do a great job here:
            //
            // var cmykColor = new float[] { (float)c, (float)m, (float)y, (float)k };
            // return Color.FromAValues((float)alpha, cmykColor, new Uri(@"pack://application:,,,/WebCoatedSWOP2006Grade3.icc"));
            //
            // The license conditions for that profile are a bit vague, so I don't use it.
            //
            // There is a very good replacement from color.org named SWOP2006_Coated3v2.icc and it is free. The drawback is it's
            // size is almost 3 MB. This size doesn't relate to the importance of the CMYK -> RGB conversion and the overall size
            // of the ShapeConverter. The CMYK -> RGB conversion should never happen. And if it is needed it is known it cannot be
            // done right (with any ICC profile) because of the nature of the 2 color spaces. We just want to give a good guess
            // about the color.
            //
            // The following magic formulas emulate the Adobe ICC profile. It is not perfect but better than the simple
            // conversion formulas (1 - c/m/y)*(1 - k) found overall the internet. The emulation serves our needs quite well. 
            // The formulas and their factors were found by experiments in a special test application. 

            double r = 1 - ((0.2 * c + 1 - 0.25 * m - 0.25 * y - 0.7 * k) * c
                            + (0.09 + 0.08 * y - 0.05 * k) * m
                            + (-0.05 + 0.05 * k) * y
                            + (0.15 * k + 0.7) * k);
            double g = 1 - ((0.34 - 0.3 * m - 0.18 * k) * c
                            + (0.1 * m + 0.8 - 0.05 * y - 0.62 * k) * m
                            + (0.1 - 0.1 * k) * y
                            + (0.15 * k + 0.7) * k);
            double b = 1 - ((0.09 - 0.1 * m - 0.1 * y) * c
                            + (0.48 - 0.3 * y - 0.2 * k) * m
                            + (0.1 * y + 0.8 - 0.74 * k ) * y
                            + (0.15 * k + 0.7) * k);

            return Color.FromArgb(MakeColorByte(alpha), MakeColorByte(r), MakeColorByte(g), MakeColorByte(b));
        }

        /// <summary>
        /// Convert a color from HSL model to RGB model
        /// </summary>
        public static Color ConvertHlsToRgb(double alpha, double hue, double saturation, double luminosity)
        {
            byte red, green, blue;

            if (saturation == 0)
            {
                red = (byte)Math.Round(luminosity * 255.0);
                green = (byte)Math.Round(luminosity * 255.0);
                blue = (byte)Math.Round(luminosity * 255.0);
            }
            else
            {
                double t1, t2;
                double th = hue / 360.0;

                if (luminosity < 0.5)
                {
                    t2 = luminosity * (1d + saturation);
                }
                else
                {
                    t2 = (luminosity + saturation) - (luminosity * saturation);
                }

                t1 = 2d * luminosity - t2;

                double tr, tg, tb;
                tr = th + (1.0 / 3.0);
                tg = th;
                tb = th - (1.0 / 3.0);

                red = ConvertHlsColorChannel(tr, t1, t2);
                green = ConvertHlsColorChannel(tg, t1, t2);
                blue = ConvertHlsColorChannel(tb, t1, t2);
            }

            return Color.FromArgb((byte)(alpha * 255), red, green, blue);
        }

        /// <summary>
        /// Convert a single color channel
        /// </summary>
        private static byte ConvertHlsColorChannel(double c, double t1, double t2)
        {
            double retVal = t1;

            if (c < 0)
            {
                c += 1.0;
            }
            else
            if (c > 1)
            {
                c -= 1.0;
            }

            if (c < 1.0 / 6.0)
            {
                retVal = t1 + (t2 - t1) * 6.0 * c;
            }
            else
            if (c < 1.0 / 2.0)
            {
                retVal = t2;
            }
            else
            if (c < 2.0 / 3.0)
            {
                retVal = t1 + (t2 - t1) * (2.0 / 3.0 - c) * 6.0;
            }

            return MakeColorByte(retVal);
        }

        /// <summary>
        /// Convert the relative color in range [0..1] to [0..255] and check the limits
        /// </summary>
        /// <param name="relativeColor"></param>
        /// <returns></returns>
        private static byte MakeColorByte(double relativeColor)
        {
            byte colorValue;

            relativeColor = Math.Round(relativeColor * 255);

            if (relativeColor > 255)
            {
                colorValue = 255;
            }
            else
            if (relativeColor < 0)
            {
                colorValue = 0;
            }
            else
            {
                colorValue = (byte)relativeColor;
            }

            return colorValue;
        }
    }
}
