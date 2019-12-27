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
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Ntreev.Library.Psd.Readers.LayerResources;
using ShapeConverter.BusinessLogic.ShapeConverter;
using ShapeConverter.Helper;

namespace ShapeConverter.Parser.Psd
{
    /// <summary>
    /// The color space manager
    /// </summary>
    class ColorSpaceManager
    {
        bool isProfileAvailable;
        string filename;

        /// <summary>
        /// Set the ICC profile
        /// </summary>
        public void SetProfile(byte[] profileData)
        {
            isProfileAvailable = true;
            filename = CommonHelper.GetTempProfileFilename();

            // don't worry about the created file, it gets deleted
            // latest when closing the app
            using (var stream = File.Open(filename, FileMode.Create))
            {
                stream.Write(profileData, 0, profileData.Length);
            }
        }

        /// <summary>
        /// Get a GraphicBrush from the given psd color definition
        /// If the color definition is unknown return a default color
        /// </summary>
        public (GraphicBrush, GraphicColorPrecision) GetBrush(ColorBase colorBase, double aspectRatio, bool returnDarkDefaultColor)
        {
            GraphicBrush color;
            GraphicColorPrecision colorPrecision;

            switch (colorBase)
            {
                case SolidColor solidColor:
                    (color, colorPrecision) = GetSolidColorBrush(solidColor, returnDarkDefaultColor);
                    break;

                case LinearGradientColor linearGradientColor:
                    (color, colorPrecision) = GetLinearGradientBrush(linearGradientColor, aspectRatio, returnDarkDefaultColor);
                    break;

                case RadialGradientColor radialGradientColor:
                    (color, colorPrecision) = GetRadialGradientBrush(radialGradientColor, aspectRatio, returnDarkDefaultColor);
                    break;

                default:
                    color = new GraphicSolidColorBrush { Color = returnDarkDefaultColor ? Colors.Black : Colors.LightGray };
                    colorPrecision = GraphicColorPrecision.Placeholder;
                    break;
            }

            return (color, colorPrecision);
        }


        /// <summary>
        /// Get our intermediate solid color definition from the given psd color definition
        /// </summary>
        private (GraphicSolidColorBrush, GraphicColorPrecision) GetSolidColorBrush(SolidColor solidColor, bool returnDarkDefaultColor)
        {
            var (color, precision) = GetColor(solidColor.Color, returnDarkDefaultColor);
            return (new GraphicSolidColorBrush { Color = color }, precision);
        }

        /// <summary>
        /// Get our intermediate linear gradient color definition from the given psd color definition
        /// </summary>
        private (GraphicLinearGradientBrush, GraphicColorPrecision) GetLinearGradientBrush(LinearGradientColor linearGradientColor, double aspectRatio, bool returnDarkDefaultColor)
        {
            var gradient = new GraphicLinearGradientBrush();

            double angle = linearGradientColor.Angle;

            if (linearGradientColor.IsReverse)
            {
                angle += 180;

                if (angle > 180)
                {
                    angle -= 360;
                }
            }

            var scale = linearGradientColor.Scale / 100;
            double angleFullAbs = Math.Abs(angle);

            double x2;
            double x1;

            double y2;
            double y1;

            if (angle == 90)
            {
                x1 = 0.5;
                x2 = 0.5;

                y1 = 0.5 + scale * 0.5;
                y2 = 0.5 - scale * 0.5;
            }
            else
            if (angle == -90)
            {
                x1 = 0.5;
                x2 = 0.5;

                y1 = 0.5 - scale * 0.5;
                y2 = 0.5 + scale * 0.5;
            }
            else
            {
                var angleAbs = angleFullAbs;

                if (angleFullAbs > 90)
                {
                    angleAbs = 180 - angleFullAbs;
                }

                double angleRad = angleAbs * Math.PI / 180.0;
                var beta = Math.Atan(aspectRatio);

                if (angleRad < beta)
                {
                    var a = 0.5 * Math.Tan(angleRad) * scale;

                    if (angleFullAbs <= 90)
                    {
                        x1 = 0.5 - scale * 0.5;
                        x2 = 0.5 + scale * 0.5;
                    }
                    else
                    {
                        x1 = 0.5 + scale * 0.5;
                        x2 = 0.5 - scale * 0.5;
                    }

                    if (angle >= 0)
                    {
                        y1 = 0.5 + a / aspectRatio;
                        y2 = 0.5 - a / aspectRatio;
                    }
                    else
                    {
                        y1 = 0.5 - a / aspectRatio;
                        y2 = 0.5 + a / aspectRatio;
                    }
                }
                else
                {
                    var a = 0.5 * Math.Tan(Math.PI / 2 - angleRad) * scale;

                    if (angleFullAbs <= 90)
                    {
                        x1 = 0.5 - a * aspectRatio;
                        x2 = 0.5 + a * aspectRatio;
                    }
                    else
                    {
                        x1 = 0.5 + a * aspectRatio;
                        x2 = 0.5 - a * aspectRatio;
                    }

                    if (angle >= 0)
                    {
                        y1 = 0.5 + scale * 0.5;
                        y2 = 0.5 - scale * 0.5;
                    }
                    else
                    {
                        y1 = 0.5 - scale * 0.5;
                        y2 = 0.5 + scale * 0.5;
                    }
                }
            }

            gradient.StartPoint = new Point(x1, y1);
            gradient.EndPoint = new Point(x2, y2);

            gradient.GradientStops = new List<GraphicGradientStop>();
            GraphicColorPrecision precision;

            foreach (var stop in linearGradientColor.GradientStops)
            {
                var gs = new GraphicGradientStop();
                gradient.GradientStops.Add(gs);

                (gs.Color, precision) = GetColor(stop.Color, returnDarkDefaultColor);
                gs.Position = stop.Position / linearGradientColor.MaxPosition;
            }

            return (gradient, GraphicColorPrecision.Estimated);
        }

        /// <summary>
        /// Get our intermediate radial gradient color definition from the given psd color definition
        /// </summary>
        private (GraphicRadialGradientBrush, GraphicColorPrecision) GetRadialGradientBrush(RadialGradientColor radialGradientColor, double aspectRatio, bool returnDarkDefaultColor)
        {
            var gradient = new GraphicRadialGradientBrush();

            gradient.StartPoint = new Point(0.5, 0.5);
            gradient.EndPoint = new Point(0.5, 0.5);

            var alpha = Math.Abs(radialGradientColor.Angle) % 90;
            double angleRad = alpha * Math.PI / 180.0;
            var beta = Math.Atan(aspectRatio);

            if (angleRad < beta)
            {
                var radius = 0.5 / Math.Cos(angleRad);

                gradient.RadiusX = radius;
                gradient.RadiusY = radius / aspectRatio;
            }
            else
            {
                var radius = 0.5 / Math.Cos(Math.PI / 2 - angleRad);

                gradient.RadiusY = radius;
                gradient.RadiusX = radius * aspectRatio;
            }

            gradient.GradientStops = new List<GraphicGradientStop>();
            GraphicColorPrecision precision;
            var scale = radialGradientColor.Scale / 100;

            foreach (var stop in radialGradientColor.GradientStops)
            {
                var gs = new GraphicGradientStop();
                gradient.GradientStops.Add(gs);

                (gs.Color, precision) = GetColor(stop.Color, returnDarkDefaultColor);
                gs.Position = stop.Position / radialGradientColor.MaxPosition;

                if (radialGradientColor.IsReverse)
                {
                    gs.Position = 1 - gs.Position;
                }

                if (radialGradientColor.Scale != 100)
                {
                    gs.Position *= scale;
                }
            }

            return (gradient, GraphicColorPrecision.Estimated);
        }

        /// <summary>
        /// Get the WPF color from a basic color
        /// </summary>
        public (Color, GraphicColorPrecision) GetColor(ColorSpaceColor colorBase, bool returnDarkDefaultColor)
        {
            Color color;
            GraphicColorPrecision colorPrecision;

            switch (colorBase)
            {
                case ColorRGB rgbColor:
                    (color, colorPrecision) = GetColor(rgbColor);
                    break;

                case ColorCMYK cmykColor:
                    (color, colorPrecision) = GetColor(cmykColor);
                    break;

                case ColorLab labColor:
                    (color, colorPrecision) = GetColor(labColor);
                    break;

                case ColorGray grayColor:
                    (color, colorPrecision) = GetColor(grayColor);
                    break;

                default:
                    color = returnDarkDefaultColor ? Colors.Black : Colors.LightGray;
                    colorPrecision = GraphicColorPrecision.Placeholder;
                    break;
            }

            return (color, colorPrecision);
        }

        /// <summary>
        /// Get the uri of the ICC profile file
        /// </summary>
        /// <returns></returns>
        private Uri GetProfileUri()
        {
            var filename = this.filename.Replace('\\', '/');
            return new Uri(filename);
        }

        /// <summary>
        /// Get the WPF color from a RGB color definition
        /// </summary>
        private (Color, GraphicColorPrecision) GetColor(ColorRGB rgbColor)
        {
            Color color;

            if (isProfileAvailable)
            {
                float[] colorValues = { (float)(rgbColor.R / 255.0), (float)(rgbColor.G / 255.0), (float)(rgbColor.B / 255.0) };

                color = Color.FromValues(colorValues, GetProfileUri());
            }
            else
            {
                color = Color.FromRgb((byte)rgbColor.R, (byte)rgbColor.G, (byte)rgbColor.B);
            }

            return (color, GraphicColorPrecision.Precise);
        }

        /// <summary>
        /// Get the WPF color from a CMKY color definition
        /// </summary>
        private (Color, GraphicColorPrecision) GetColor(ColorCMYK cmykColor)
        {
            Color color;
            GraphicColorPrecision colorPrecision;

            if (isProfileAvailable)
            {
                float[] colorValues = { (float)(cmykColor.C / 100.0), (float)(cmykColor.M / 100.0), (float)(cmykColor.Y / 100.0), (float)(cmykColor.K / 100.0) };
                var uri = GetProfileUri();
                colorPrecision = GraphicColorPrecision.Precise;
                color = Color.FromValues(colorValues, uri);
            }
            else
            {
                colorPrecision = GraphicColorPrecision.Estimated;
                color = CmykToRgbConverter.Convert(cmykColor.C / 100.0, cmykColor.M / 100.0, cmykColor.Y / 100.0, cmykColor.K / 100.0);
            }

            return (color, colorPrecision);
        }

        /// <summary>
        /// Get the WPF color from a Lab color definition
        /// </summary>
        private (Color, GraphicColorPrecision) GetColor(ColorLab labColor)
        {
            Color color;

            double L = labColor.L;
            double a = labColor.A;
            double b = labColor.B;

            // http://www.easyrgb.com/en/math.php#text8
            double x, y, z, fx, fy, fz;

            fy = Math.Pow((L + 16.0) / 116.0, 3.0);

            if (fy < 0.008856)
            {
                fy = L / 903.3;
            }

            y = fy;

            if (fy > 0.008856)
            {
                fy = Math.Pow(fy, 1.0 / 3.0);
            }
            else
            {
                fy = 7.787 * fy + 16.0 / 116.0;
            }

            fx = a / 500.0 + fy;

            if (fx > 0.206893)
            {
                x = Math.Pow(fx, 3.0);
            }
            else
            {
                x = (fx - 16.0 / 116.0) / 7.787;
            }

            fz = fy - b / 200.0;

            if (fz > 0.206893)
            {
                z = Math.Pow(fz, 3.0);
            }
            else
            {
                z = (fz - 16.0 / 116.0) / 7.787;
            }

            // reference point for D65
            x *= 0.950456;
            y *= 1;
            z *= 1.088754;

            // matrix for D65
            double rd = 3.240479 * x - 1.537150 * y - 0.498535 * z;
            double gd = -0.969256 * x + 1.875992 * y + 0.041556 * z;
            double bd = 0.055648 * x - 0.204043 * y + 1.057311 * z;

            int ri = (int)((rd * 255.0) + 0.5);
            int gi = (int)((gd * 255.0) + 0.5);
            int bi = (int)((bd * 255.0) + 0.5);

            ri = ri < 0 ? 0 : ri > 255 ? 255 : ri;
            gi = gi < 0 ? 0 : gi > 255 ? 255 : gi;
            bi = bi < 0 ? 0 : bi > 255 ? 255 : bi;

            color = Color.FromRgb((byte)ri, (byte)gi, (byte)bi);

            return (color, GraphicColorPrecision.Estimated);
        }

        /// <summary>
        /// Get the WPF color from a gray color definition
        /// </summary>
        private (Color, GraphicColorPrecision) GetColor(ColorGray grayColor)
        {
            int gray = (int)(grayColor.Gray / 100.0 * 255.0 + 0.5);
            Color color = Color.FromRgb((byte)gray, (byte)gray, (byte)gray);

            return (color, GraphicColorPrecision.Precise);
        }
    }
}
