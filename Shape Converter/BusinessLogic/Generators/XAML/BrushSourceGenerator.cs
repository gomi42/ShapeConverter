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
using System.Windows.Media;
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Generators
{
        /// <summary>
        /// Helper to generate the source code for a complex brush
        /// </summary>
    internal static class BrushSourceGenerator
    {
        /// <summary>
        /// Generate the source code for a complex brush
        /// </summary>
        public static void GenerateBrush(StringBuilder result, GraphicBrush graphicBrush, string element, int level)
        {
            var indentTag = SourceFormatterHelper.GetTagIndent(level);
            var indent1Tag = SourceFormatterHelper.GetTagIndent(level + 1);
            var indent2Tag = SourceFormatterHelper.GetTagIndent(level + 2);

            result.AppendLine(string.Format("{0}<{1}>", indentTag, element));

            switch (graphicBrush)
            {
                case GraphicLinearGradientBrush linearGradientBrush:
                {
                    var tag = "LinearGradientBrush";
                    var indentProperty = SourceFormatterHelper.GetPropertyIndent(level + 1, tag);

                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}<{1} StartPoint=\"{2}\"",
                                                    indent1Tag,
                                                    tag,
                                                    SourceFormatterHelper.FormatPointParamter(linearGradientBrush.StartPoint)));

                    result.Append(string.Format(CultureInfo.InvariantCulture, "{0}EndPoint=\"{1}\"",
                                                indentProperty,
                                                SourceFormatterHelper.FormatPointParamter(linearGradientBrush.EndPoint)));

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
                    var indentProperty = SourceFormatterHelper.GetPropertyIndent(level + 1, tag);

                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}<{1} GradientOrigin=\"{2}\"",
                                                    indent1Tag,
                                                    tag,
                                                    SourceFormatterHelper.FormatPointParamter(radialGradientBrush.StartPoint)));

                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}Center=\"{1}\"",
                                                    indentProperty,
                                                    SourceFormatterHelper.FormatPointParamter(radialGradientBrush.EndPoint)));

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
                                                SourceFormatterHelper.FormatColorParamter(color),
                                                DoubleUtilities.FormatString(offset)));
            }
        }
    }
}
