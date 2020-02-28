//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
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
using System.Globalization;
using System.Windows.Media;
using ShapeConverter.BusinessLogic.ShapeConverter;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Helper
{
    internal static class ColorParser
    {
        /// <summary>
        //  Try to parse a color given in different flavours (hex, rgb, name)
        /// </summary>
        public static bool TryParseColor(string strVal, double alpha, out Color color)
        {
            // 1: color is specified in rgb values
            if (strVal.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
            {
                var str = strVal.Substring(4, strVal.Length - 5);
                string[] parts = str.Split(',');

                if (parts.Length != 3)
                {
                    color = Colors.Black;
                    return false;
                }

                try
                {
                    string redStr = parts[0].Trim();
                    string greenStr = parts[1].Trim();
                    string blueStr = parts[2].Trim();
                    byte red;
                    byte green;
                    byte blue;

                    if (string.IsNullOrWhiteSpace(redStr)
                        || string.IsNullOrWhiteSpace(greenStr)
                        || string.IsNullOrWhiteSpace(blueStr))
                    {
                        red = 0;
                        green = 0;
                        blue = 0;
                    }
                    else
                    {
                        red = (byte)ParseColorValue(redStr);
                        green = (byte)ParseColorValue(greenStr);
                        blue = (byte)ParseColorValue(blueStr);
                    }

                    color = Color.FromArgb((byte)(alpha * 255), red, green, blue);
                    return true;
                }
                catch
                {
                }

                color = Colors.Black;
                return false;
            }

            // 2: color is specified in rgba values
            if (strVal.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase))
            {
                var str = strVal.Substring(5, strVal.Length - 6);
                string[] parts = str.Split(',');

                if (parts.Length != 4)
                {
                    color = Colors.Black;
                    return false;
                }

                try
                {
                    string redStr = parts[0].Trim();
                    string greenStr = parts[1].Trim();
                    string blueStr = parts[2].Trim();
                    string alphaStr = parts[3].Trim();
                    byte red;
                    byte green;
                    byte blue;
                    double alphaD;

                    if (string.IsNullOrWhiteSpace(alphaStr)
                        || string.IsNullOrWhiteSpace(redStr)
                        || string.IsNullOrWhiteSpace(greenStr)
                        || string.IsNullOrWhiteSpace(blueStr))
                    {
                        alphaD = 1;
                        red = 0;
                        green = 0;
                        blue = 0;
                    }
                    else
                    {
                        red = (byte)ParseColorValue(redStr);
                        green = (byte)ParseColorValue(greenStr);
                        blue = (byte)ParseColorValue(blueStr);
                        alphaD = DoubleWithUnitParser.Parse(alphaStr);
                    }

                    color = Color.FromArgb((byte)(alpha * alphaD * 255), red, green, blue);
                    return true;
                }
                catch
                {
                }

                color = Colors.Black;
                return false;
            }

            // 3: color is specified in hsl values
            if (strVal.StartsWith("hsl(", StringComparison.OrdinalIgnoreCase))
            {
                var str = strVal.Substring(4, strVal.Length - 5);
                string[] parts = str.Split(',');

                if (parts.Length != 3)
                {
                    color = Colors.Black;
                    return false;
                }
                try
                {
                    string hueStr = parts[0].Trim();
                    string saturationStr = parts[1].Trim();
                    string luminosityStr = parts[2].Trim();

                    if (string.IsNullOrWhiteSpace(hueStr)
                        || string.IsNullOrWhiteSpace(saturationStr)
                        || string.IsNullOrWhiteSpace(luminosityStr))
                    {
                        color = Colors.Black;
                        return true;
                    }
                    else
                    {
                        var hue = DoubleWithUnitParser.Parse(hueStr);
                        var saturation = DoubleWithUnitParser.Parse(saturationStr);
                        var luminosity = DoubleWithUnitParser.Parse(luminosityStr);

                        color = ColorModelConverter.ConvertHlsToRgb(alpha, hue, saturation, luminosity);
                        return true;
                    }
                }
                catch
                {
                }

                color = Colors.Black;
                return false;
            }

            // 4: color is either given as name or hex values
            try
            {
                var color2 = (Color)ColorConverter.ConvertFromString(strVal);
                color = Color.FromArgb((byte)(alpha * 255), color2.R, color2.G, color2.B);
                return true;
            }
            catch
            {
            }

            color = Colors.Black;
            return false;
        }

        /// <summary>
        /// Parse a single color value
        /// </summary>
        private static int ParseColorValue(string colorValue)
        {
            int retVal;

            var hasPercent = colorValue.EndsWith("%", StringComparison.OrdinalIgnoreCase);

            if (hasPercent)
            {
                colorValue = colorValue.Substring(0, colorValue.Length - 1);
                retVal = int.Parse(colorValue, CultureInfo.InvariantCulture);
                retVal = (int)Math.Round(255.0 * (double)retVal / 100.0);
            }
            else
            {
                retVal = int.Parse(colorValue, CultureInfo.InvariantCulture);
            }

            if (retVal > 255)
            {
                retVal = 255;
            }
            else
            if (retVal < 0)
            {
                retVal = 0;
            }

            return retVal;
        }
    }
}
