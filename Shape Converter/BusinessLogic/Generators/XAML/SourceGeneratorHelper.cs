//
// Author:
//   Michael Göricke
//
// Copyright (c) 2020
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

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Generators
{
    internal static class SourceGeneratorHelper
    {
        /// <summary>
        /// Gets the indent according to the given level
        /// </summary>
        public static string GetTagIndent(int level)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < level; i++)
            {
                sb.Append("    ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the indent according to the given level
        /// </summary>
        public static string GetPropertyIndent(int level, string tagName)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < level; i++)
            {
                sb.Append("    ");
            }

            int tagIndent = tagName.Length + 2;

            for (int i = 0; i < tagIndent; i++)
            {
                sb.Append(" ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the indent according to the given level
        /// </summary>
        public static string GetSourceIndent(int level)
        {
            return GetTagIndent(level);
        }

        /// <summary>
        /// Generate the source code for a complex brush
        /// </summary>
        public static void GenerateBrush(StringBuilder result, GraphicBrush graphicBrush, string element, int level)
        {
            var indentTag = GetTagIndent(level);
            var indent1Tag = GetTagIndent(level + 1);
            var indent2Tag = GetTagIndent(level + 2);

            result.AppendLine(string.Format("{0}<{1}>", indentTag, element));

            switch (graphicBrush)
            {
                case GraphicLinearGradientBrush linearGradientBrush:
                {
                    var tag = "LinearGradientBrush";
                    var indentProperty = GetPropertyIndent(level + 1, tag);

                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}<{1} StartPoint=\"{2}\"",
                                                    indent1Tag,
                                                    tag,
                                                    FormatPointParamter(linearGradientBrush.StartPoint)));

                    result.Append(string.Format(CultureInfo.InvariantCulture, "{0}EndPoint=\"{1}\"",
                                                indentProperty,
                                                FormatPointParamter(linearGradientBrush.EndPoint)));

                    if (linearGradientBrush.MappingMode != GraphicBrushMappingMode.RelativeToBoundingBox)
                    {
                        result.AppendLine();
                        result.Append(string.Format(CultureInfo.InvariantCulture, "{0}MappingMode=\"{1}\"",
                                                    indentProperty,
                                                    Converter.ConvertToWpf(linearGradientBrush.MappingMode).ToString()));
                    }

                    result.AppendLine(">");

                    GenerateGradientStops(result, indent2Tag, linearGradientBrush.GradientStops);

                    result.AppendLine(string.Format("{0}</{1}>", indent1Tag, tag));
                    break;
                }

                case GraphicRadialGradientBrush radialGradientBrush:
                {
                    var tag = "RadialGradientBrush";
                    var indentProperty = GetPropertyIndent(level + 1, tag);

                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}<{1} GradientOrigin=\"{2}\"",
                                                    indent1Tag,
                                                    tag,
                                                    FormatPointParamter(radialGradientBrush.StartPoint)));

                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}Center=\"{1}\"",
                                                    indentProperty,
                                                    FormatPointParamter(radialGradientBrush.EndPoint)));

                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}RadiusX=\"{1}\"",
                                                    indentProperty,
                                                    DoubleUtilities.FormatString(radialGradientBrush.RadiusX)));

                    result.Append(string.Format(CultureInfo.InvariantCulture, "{0}RadiusY=\"{1}\"",
                                                    indentProperty,
                                                    DoubleUtilities.FormatString(radialGradientBrush.RadiusY)));

                    if (radialGradientBrush.MappingMode != GraphicBrushMappingMode.RelativeToBoundingBox)
                    {
                        result.AppendLine();
                        result.Append(string.Format(CultureInfo.InvariantCulture, "{0}MappingMode=\"{1}\"",
                                                        indentProperty,
                                                        Converter.ConvertToWpf(radialGradientBrush.MappingMode).ToString()));
                    }

                    result.AppendLine(">");

                    GenerateGradientStops(result, indent2Tag, radialGradientBrush.GradientStops);

                    result.AppendLine(string.Format("{0}</{1}>", indent1Tag, tag));
                    break;
                }

                default:
                {
                    result.AppendLine(string.Format("{0}<SolidColorBrush Color=\"#FFB0B0B0\" />", indent1Tag));
                    break;
                }
            }

            result.AppendLine(string.Format("{0}</{1}>", indentTag, element));
        }

        /// <summary>
        /// Generate gradient stops
        /// </summary>
        private static void GenerateGradientStops(StringBuilder result, string indent, List<GraphicGradientStop> gradientStops)
        {
            foreach (var stop in gradientStops)
            {
                Color color = stop.Color;
                var offset = stop.Position;
                result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}<GradientStop Color=\"{1}\" Offset=\"{2}\"/>",
                                                indent,
                                                FormatColorParamter(color),
                                                DoubleUtilities.FormatString(offset)));
            }
        }

        /// <summary>
        /// Formats a single point
        /// </summary>
        public static string FormatPointParamter(Point point)
        {
            var xStr = DoubleUtilities.FormatString(point.X);
            var yStr = DoubleUtilities.FormatString(point.Y);

            return $"{xStr},{yStr}";
        }

        /// <summary>
        /// Formats a given color
        /// </summary>
        public static string FormatColorParamter(Color color)
        {
            return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// append a point to the stream
        /// </summary>
        public static void AppendPoint(StringBuilder output, double x, double y)
        {
            string xstr = DoubleUtilities.FormatString(x);
            string ystr = DoubleUtilities.FormatString(y);

            output.Append(xstr);
            output.Append(',');
            output.Append(ystr);
            output.Append(' ');
        }

        /// <summary>
        /// append a point to the stream
        /// </summary>
        public static void AppendPoint(StringBuilder output, Point point)
        {
            AppendPoint(output, point.X, point.Y);
        }

        /// <summary>
        /// Append a numeric value to the stream
        /// </summary>
        public static void AppendValue(StringBuilder output, double val)
        {
            string valstr = DoubleUtilities.FormatString(val);
            output.Append(valstr);
            output.Append(' ');
        }

        /// <summary>
        /// append a point to the stream
        /// </summary>
        public static void AppendXamlPoint(StringBuilder output, double x, double y)
        {
            string xstr = DoubleUtilities.FormatString(x);
            string ystr = DoubleUtilities.FormatString(y);

            output.Append("\"");
            output.Append(xstr);
            output.Append(',');
            output.Append(ystr);
            output.Append("\"");
        }

        /// <summary>
        /// append a point to the stream
        /// </summary>
        public static void AppendXamlPoint(StringBuilder output, Point point)
        {
            AppendXamlPoint(output, point.X, point.Y);
        }

        /// <summary>
        /// Append a bool to the stream
        /// </summary>
        public static void AppendXamlBool(StringBuilder output, bool boolVal)
        {
            string boolstr = boolVal ? "True" : "False";
            output.Append("\"");
            output.Append(boolstr);
            output.Append("\"");
        }
    }
}
