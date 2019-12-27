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

using Ntreev.Library.Psd.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Ntreev.Library.Psd.Readers.LayerResources
{
    /// <summary>
    /// Base class for a basic color definition of a specific
    /// color space
    /// </summary>
    public abstract class ColorSpaceColor
    {
    }

    /// <summary>
    /// An RGB color definition
    /// </summary>
    public class ColorRGB : ColorSpaceColor
    {
        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }
    }

    /// <summary>
    /// A CMYK color definition
    /// </summary>
    public class ColorCMYK : ColorSpaceColor
    {
        public double C { get; set; }
        public double M { get; set; }
        public double Y { get; set; }
        public double K { get; set; }
    }

    /// <summary>
    /// A Lab color definition
    /// </summary>
    public class ColorLab : ColorSpaceColor
    {
        public double L { get; set; }
        public double A { get; set; }
        public double B { get; set; }
    }

    /// <summary>
    /// A gray color definition
    /// </summary>
    public class ColorGray : ColorSpaceColor
    {
        public double Gray { get; set; }
    }

    /// <summary>
    /// A color from an unknown color space
    /// </summary>
    public class ColorUnknown : ColorSpaceColor
    {
    }

    /// <summary>
    /// Base class for all color definitions
    /// </summary>
    public abstract class ColorBase
    {
    }

    /// <summary>
    /// A solid color definition
    /// </summary>
    public class SolidColor : ColorBase
    {
        public ColorSpaceColor Color { get; set; }
    }

    /// <summary>
    /// A gradient stop definition
    /// </summary>
    public class GradientColorStop
    {
        /// <summary>
        /// The color of this stop
        /// </summary>
        public ColorSpaceColor Color { get; set; }

        /// <summary>
        /// position range 0..MaxPosition
        /// </summary>
        public double Position { get; set; }

        /// <summary>
        /// Transition middle point range 0..100
        /// </summary>
        public double MiddlePoint { get; set; }
    }

    /// <summary>
    /// Base class for a gradient color definition
    /// </summary>
    public abstract class GradientColor : ColorBase
    {
        public double Angle { get; set; }
        public bool IsReverse { get; set; }
        public double Scale { get; set; }
        public double MaxPosition { get; set; }
        public List<GradientColorStop> GradientStops { get; set; }
    }

    /// <summary>
    /// A linear gradient color definition
    /// </summary>
    public class LinearGradientColor : GradientColor
    {
    }

    /// <summary>
    /// A radial gradient color definition
    /// </summary>
    public class RadialGradientColor : GradientColor
    {
    }

    /// <summary>
    /// Read a color from a resource tree
    /// </summary>
    internal static class ColorReader
    {
        /// <summary>
        /// Gets a solid color definition from a descriptor that contains a Clr definition.
        /// </summary>
        public static SolidColor GetSolidColor(StructureDescriptor descriptor)
        {
            var colorDescriptor = (StructureDescriptor)descriptor.Items["Clr"];

            var solidColor = new SolidColor();
            solidColor.Color = GetColor(colorDescriptor);

            return solidColor;
        }

        /// <summary>
        /// Gets a linear gradient definition.
        /// </summary>
        public static LinearGradientColor GetLinearGradientColor(StructureDescriptor descriptor)
        {
            var linearGradientColor = new LinearGradientColor();
            linearGradientColor.GradientStops = new List<GradientColorStop>();

            linearGradientColor.GradientStops = GetGradientColor(descriptor, linearGradientColor);

            return linearGradientColor;
        }

        /// <summary>
        /// Gets a linear gradient definition.
        /// </summary>
        public static RadialGradientColor GetRadialGradientColor(StructureDescriptor descriptor)
        {
            var radialGradientColor = new RadialGradientColor();

            radialGradientColor.GradientStops = GetGradientColor(descriptor, radialGradientColor);

            return radialGradientColor;
        }

        /// <summary>
        /// Get a the gradient color definition
        /// </summary>
        private static List<GradientColorStop> GetGradientColor(StructureDescriptor descriptor, GradientColor gradientColor)
        {
            StructureItem item = null;

            if (descriptor.Items.TryGetValue(ref item, "Angl"))
            {
                var anglDesc = (StructureUnitFloat)item;
                gradientColor.Angle = anglDesc.Value;
            }

            if (descriptor.Items.TryGetValue(ref item, "Rvrs"))
            {
                var rvrsDesc = (StructureBool)item;
                gradientColor.IsReverse = rvrsDesc.Value;
            }

            if (descriptor.Items.TryGetValue(ref item, "Scl"))
            {
                var sclDesc = (StructureUnitFloat)item;
                gradientColor.Scale = sclDesc.Value;
            }
            else
            {
                gradientColor.Scale = 100;
            }

            var gradDesc = (StructureDescriptor)descriptor.Items["Grad"];

            var intrDescriptor = (StructureDouble)gradDesc.Items["Intr"];
            gradientColor.MaxPosition = intrDescriptor.Value;

            var clrsDesc = (StructureList)gradDesc.Items["Clrs"];
            List<GradientColorStop> stops = new List<GradientColorStop>();

            foreach (var clrtItem in clrsDesc.Items)
            {
                var clrtDesc = (StructureDescriptor)clrtItem;

                GradientColorStop stop = new GradientColorStop();
                stops.Add(stop);

                var colorDescriptor = (StructureDescriptor)clrtDesc.Items["Clr"];
                stop.Color = GetColor(colorDescriptor);

                var positionDescriptor = (StructureLong)clrtDesc.Items["Lctn"];
                stop.Position = positionDescriptor.Value;

                var mdpnDescriptor = (StructureLong)clrtDesc.Items["Mdpn"];
                stop.MiddlePoint = mdpnDescriptor.Value;
            }

            return stops;
        }

        /// <summary>
        /// Gets the color.
        /// </summary>
        public static ColorSpaceColor GetColor(StructureDescriptor colorDescriptor)
        {
            ColorSpaceColor colorBase = null;

            var colorSpace = colorDescriptor.ClassId;

            switch (colorSpace)
            {
                case "RGBC":
                    colorBase = GetRgbColor(colorDescriptor);
                    break;

                case "CMYC":
                    colorBase = GetCmykColor(colorDescriptor);
                    break;

                case "LbCl":
                    colorBase = GetLabColor(colorDescriptor);
                    break;

                case "Grsc":
                    colorBase = GetGrayColor(colorDescriptor);
                    break;

                default:
                    colorBase = GetUnknownColor();
                    break;
            }

            return colorBase;
        }

        /// <summary>
        /// Read an RGB color
        /// </summary>
        private static ColorRGB GetRgbColor(StructureDescriptor colorDescriptor)
        {
            var red = (int)((StructureDouble)colorDescriptor.Items["Rd"]).Value;
            var green = (int)((StructureDouble)colorDescriptor.Items["Grn"]).Value;
            var blue = (int)((StructureDouble)colorDescriptor.Items["Bl"]).Value;

            return new ColorRGB { R = red, G = green, B = blue };
        }

        /// <summary>
        /// Ready a CMYK color
        /// </summary>
        public static ColorCMYK GetCmykColor(StructureDescriptor colorDescriptor)
        {
            var c = (double)(((StructureDouble)colorDescriptor.Items["Cyn"]).Value);
            var m = (double)(((StructureDouble)colorDescriptor.Items["Mgnt"]).Value);
            var y = (double)(((StructureDouble)colorDescriptor.Items["Ylw"]).Value);
            var k = (double)(((StructureDouble)colorDescriptor.Items["Blck"]).Value);

            return new ColorCMYK { C = c, M = m, Y = y, K = k };
        }

        /// <summary>
        /// Ready a Lab color
        /// </summary>
        private static ColorLab GetLabColor(StructureDescriptor colorDescriptor)
        {
            var L = (double)((StructureDouble)colorDescriptor.Items["Lmnc"]).Value;
            var A = (double)((StructureDouble)colorDescriptor.Items["A"]).Value;
            var B = (double)((StructureDouble)colorDescriptor.Items["B"]).Value;

            return new ColorLab { L = L, A = A, B = B };
        }

        /// <summary>
        /// Ready a gray color
        /// </summary>
        private static ColorGray GetGrayColor(StructureDescriptor colorDescriptor)
        {
            var gray = (double)((StructureDouble)colorDescriptor.Items["Gry"]).Value;

            return new ColorGray { Gray = gray };
        }

        /// <summary>
        /// Create an unknow color definition
        /// </summary>
        public static ColorUnknown GetUnknownColor()
        {
            return new ColorUnknown();
        }
    }
}
