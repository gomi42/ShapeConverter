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

using System.Globalization;
using System.Text;
using System.Windows.Media;
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Generators
{
    public static class PathSourceGenerator
    {
        /// <summary>
        /// Generate the XAML Path source code for all given graphic paths
        /// </summary>
        public static string GeneratePath(GraphicVisual visual, GeometryGeneratorType geometryGeneratorType)
        {
            StringBuilder result = new StringBuilder();

            if (visual is GraphicPath graphicPath)
            {
                result.AppendLine(GeneratePath(graphicPath, true, 0, geometryGeneratorType));
            }
            else
            {
                result.AppendLine("<Viewbox>");
                GeneratePathGroup(visual, result, 1, geometryGeneratorType);
                result.AppendLine("</Viewbox>");
            }

            return result.ToString();
        }

        /// <summary>
        /// Generate the XAML Path source code for a visual
        /// </summary>
        private static void GeneratePathGroup(GraphicVisual visual, StringBuilder result, int level, GeometryGeneratorType geometryGeneratorType)
        {
            switch (visual)
            {
                case GraphicGroup group:
                {
                    var tag = "Grid";
                    var indentTag = SourceGeneratorHelper.GetTagIndent(level);
                    result.Append($"{indentTag}<{tag}");

                    bool tagIndent = false;

                    if (!DoubleUtilities.IsEqual(group.Opacity, 1.0))
                    {
                        tagIndent = true;
                        string opac = string.Format(CultureInfo.InvariantCulture, " Opacity=\"{0}\"", DoubleUtilities.FormatString(group.Opacity));
                        result.Append(opac);
                    }

                    if (group.Clip != null)
                    {
                        if (tagIndent)
                        {
                            var indentProperty = SourceGeneratorHelper.GetPropertyIndent(level, tag);
                            result.AppendLine();
                            result.Append(indentProperty);
                        }
                        else
                        {
                            result.Append(" ");
                        }

                        result.Append(string.Format("Clip=\""));
                        var stream = StreamSourceGenerator.GenerateStreamGeometry(group.Clip);
                        result.Append(stream);
                        result.Append("\"");
                    }

                    result.AppendLine(">");

                    foreach (var childVisual in group.Childreen)
                    {
                        GeneratePathGroup(childVisual, result, level + 1, geometryGeneratorType);
                    }

                    result.AppendLine($"{indentTag}</{tag}>");

                    break;
                }

                case GraphicPath graphicPath:
                {
                    result.AppendLine(GeneratePath(graphicPath, false, level, geometryGeneratorType));
                    break;
                }
            }
        }

        /// <summary>
        /// Generate the XAML source code (a <Path/> for a single graphic path
        /// </summary>
        private static string GeneratePath(GraphicPath graphicPath, bool stretch, int level, GeometryGeneratorType geometryGeneratorType)
        {
            var tag = "Path";
            var indentTag = SourceGeneratorHelper.GetTagIndent(level);
            var indentProperty = SourceGeneratorHelper.GetPropertyIndent(level, tag);
            StringBuilder result = new StringBuilder();

            string stretchParam = stretch ? "Uniform" : "None";
            result.Append($"{indentTag}<{tag} Stretch=\"{stretchParam}\"");

            bool fillColorInExtendedProperties = false;
            bool strokeColorInExtendedProperties = false;

            if (graphicPath.FillBrush != null)
            {
                if (graphicPath.FillBrush is GraphicSolidColorBrush solidFillColor)
                {
                    result.AppendLine();

                    Color color = solidFillColor.Color;
                    result.Append(indentProperty);
                    result.Append(string.Format("Fill=\"{0}\"", SourceGeneratorHelper.FormatColorParamter(color)));
                }
                else
                {
                    fillColorInExtendedProperties = true;
                }
            }

            if (graphicPath.StrokeBrush != null)
            {
                if (graphicPath.StrokeBrush is GraphicSolidColorBrush solidStrokeColor)
                {
                    result.AppendLine();

                    Color color = solidStrokeColor.Color;
                    result.Append(indentProperty);
                    result.Append(string.Format("Stroke=\"{0}\" ", SourceGeneratorHelper.FormatColorParamter(color)));
                }
                else
                {
                    strokeColorInExtendedProperties = true;
                }

                result.AppendLine();
                result.Append(indentProperty);
                result.Append(string.Format(CultureInfo.InvariantCulture, "StrokeThickness=\"{0}\" ", DoubleUtilities.FormatString(graphicPath.StrokeThickness)));

                if (graphicPath.StrokeLineCap != GraphicLineCap.Flat)
                {
                    result.AppendLine();
                    result.Append(indentProperty);
                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "StrokeStartLineCap=\"{0}\" ", Converter.ConvertToWPF(graphicPath.StrokeLineCap).ToString()));
                    result.Append(indentProperty);
                    result.Append(string.Format(CultureInfo.InvariantCulture, "StrokeEndLineCap=\"{0}\" ", Converter.ConvertToWPF(graphicPath.StrokeLineCap).ToString()));
                }

                if (graphicPath.StrokeDashes != null)
                {
                    if (graphicPath.StrokeLineCap != GraphicLineCap.Flat)
                    {
                        result.AppendLine();
                        result.Append(indentProperty);
                        result.Append(string.Format(CultureInfo.InvariantCulture, "StrokeDashCap=\"{0}\" ", Converter.ConvertToWPF(graphicPath.StrokeLineCap).ToString()));
                    }

                    result.AppendLine();
                    result.Append(indentProperty);
                    result.Append("StrokeDashArray=\"");

                    for (int i = 0; i < graphicPath.StrokeDashes.Count; i++)
                    {
                        if (i != 0)
                        {
                            result.Append(" ");
                        }

                        result.Append(DoubleUtilities.FormatString(graphicPath.StrokeDashes[i]));
                    }

                    result.AppendLine("\"");

                    if (!DoubleUtilities.IsZero(graphicPath.StrokeDashOffset))
                    {
                        result.AppendLine();
                        result.Append(indentProperty);
                        result.Append(string.Format(CultureInfo.InvariantCulture, "StrokeDashOffset=\"{0}\"", DoubleUtilities.FormatString(graphicPath.StrokeDashOffset)));
                    }
                }

                if (graphicPath.StrokeLineJoin != GraphicLineJoin.Miter)
                {
                    result.AppendLine();
                    result.Append(indentProperty);
                    result.Append(string.Format(CultureInfo.InvariantCulture, "StrokeLineJoin=\"{0}\" ", Converter.ConvertToWpf(graphicPath.StrokeLineJoin).ToString()));
                }
                else
                if (!DoubleUtilities.IsEqual(graphicPath.StrokeMiterLimit, 10))
                {
                    result.AppendLine();
                    result.Append(indentProperty);
                    result.Append(string.Format(CultureInfo.InvariantCulture, "MiterLimit=\"{0}\"", DoubleUtilities.FormatString(graphicPath.StrokeMiterLimit)));
                }
            }

            if (geometryGeneratorType == GeometryGeneratorType.Stream)
            {
                result.AppendLine();
                result.Append(indentProperty);
                result.Append("Data=\"");
                result.Append(StreamSourceGenerator.GenerateStreamGeometry(graphicPath.Geometry));
                result.Append("\"");
            }

            if (geometryGeneratorType == GeometryGeneratorType.PathGeometry || fillColorInExtendedProperties || strokeColorInExtendedProperties)
            {
                result.AppendLine(">");

                if (geometryGeneratorType == GeometryGeneratorType.PathGeometry)
                {
                    var indent1 = SourceGeneratorHelper.GetTagIndent(level + 1);
                    result.Append(indent1);
                    result.AppendLine("<Path.Data>");

                    PathGeometrySourceGenerator.GeneratePathGeometry(result, graphicPath.Geometry, level + 2);

                    result.Append(indent1);
                    result.AppendLine("</Path.Data>");
                }

                if (fillColorInExtendedProperties)
                {
                    SourceGeneratorHelper.GenerateBrush(result, graphicPath.FillBrush, "Path.Fill", level + 1);
                }

                if (strokeColorInExtendedProperties)
                {
                    SourceGeneratorHelper.GenerateBrush(result, graphicPath.StrokeBrush, "Path.Stroke", level + 1);
                }

                result.Append(indentTag);
                result.Append($"</{tag}>");
            }
            else
            {
                result.Append(" />");
            }

            return result.ToString();
        }
    }
}
