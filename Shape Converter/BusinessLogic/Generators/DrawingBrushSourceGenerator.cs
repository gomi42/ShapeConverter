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

using System.Globalization;
using System.Text;
using System.Windows.Media;
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Generators
{
    public class DrawingBrushSourceGenerator
    {
        /// <summary>
        /// Generates the drawing brush source code for a given graphic drawing.
        /// </summary>
        public static string Generate(GraphicVisual visual, bool generatePathFigure)
        {
            StringBuilder result = new StringBuilder();
            var indentTag = SourceGeneratorHelper.GetTagIndent(1);

            result.AppendLine("<DrawingBrush Stretch=\"Uniform\">");
            result.AppendLine($"{indentTag}<DrawingBrush.Drawing>");

            Generate(visual, result, 2, generatePathFigure);

            result.AppendLine($"{indentTag}</DrawingBrush.Drawing>");
            result.AppendLine("</DrawingBrush>");

            return result.ToString();
        }

        /// <summary>
        /// Generates the drawing brush source code for a given geometry.
        /// </summary>
        private static void Generate(GraphicVisual visual, StringBuilder result, int level, bool generatePathFigure)
        {
            switch (visual)
            {
                case GraphicGroup group:
                {
                    var tag = "DrawingGroup";
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
                            result.AppendLine("");
                            result.Append(indentProperty);
                        }
                        else
                        {
                            result.Append(" ");
                        }

                        result.Append(string.Format("ClipGeometry=\""));
                        var stream = StreamSourceGenerator.GenerateStreamGeometry(group.Clip);
                        result.Append(stream);
                        result.Append("\"");
                    }

                    result.AppendLine(">");

                    foreach (var childVisual in group.Childreen)
                    {
                        Generate(childVisual, result, level + 1, generatePathFigure);
                    }

                    result.AppendLine($"{indentTag}</{tag}>");

                    break;
                }

                case GraphicPath graphicPath:
                {
                    GeneratePath(graphicPath, result, level, generatePathFigure);
                    break;
                }
            }
        }

        /// <summary>
        /// Generate path
        /// </summary>
        private static void GeneratePath(GraphicPath graphicPath, StringBuilder result, int level, bool generatePathFigure)
        {
            var tag = "GeometryDrawing";
            var indentTag = SourceGeneratorHelper.GetTagIndent(level);
            var indentProperty = SourceGeneratorHelper.GetPropertyIndent(level, tag);

            result.Append($"{indentTag}<{tag} ");

            bool fillColorInExtendedProperties = false;
            bool firstAttributSet = false;

            if (graphicPath.FillBrush != null)
            {
                if (graphicPath.FillBrush is GraphicSolidColorBrush solidFillColor)
                {
                    firstAttributSet = true;

                    Color color = solidFillColor.Color;
                    var brush = string.Format("Brush=\"{0}\"", SourceGeneratorHelper.FormatColorParamter(color));
                    result.Append(brush);
                }
                else
                {
                    fillColorInExtendedProperties = true;
                }
            }

            if (!generatePathFigure)
            {
                if (firstAttributSet)
                {
                    result.AppendLine();
                    result.Append(indentProperty);
                }

                result.Append(string.Format("Geometry=\""));
                var stream = StreamSourceGenerator.GenerateStreamGeometry(graphicPath.Geometry);
                result.Append(stream);
                result.Append("\"");
            }

            if (generatePathFigure || fillColorInExtendedProperties || graphicPath.StrokeBrush != null)
            {
                result.AppendLine(">");

                if (generatePathFigure)
                {
                    var indent1 = SourceGeneratorHelper.GetTagIndent(level + 1);
                    result.Append(indent1);
                    result.AppendLine("<GeometryDrawing.Geometry>");

                    StreamSourceGenerator.GeneratePathGeometry(result, graphicPath.Geometry, level + 2);

                    result.Append(indent1);
                    result.AppendLine("</GeometryDrawing.Geometry>");
                }

                if (fillColorInExtendedProperties)
                {
                    SourceGeneratorHelper.GenerateBrush(result, graphicPath.FillBrush, "GeometryDrawing.Brush", level + 1);
                }

                if (graphicPath.StrokeBrush != null)
                {
                    GeneratePen(result, graphicPath, level + 1);
                }

                result.Append(indentTag);
                result.AppendLine("</GeometryDrawing>");
            }
            else
            {
                result.AppendLine("/>");
            }
        }

        /// <summary>
        /// Generate the pen for a GeometryDrawing
        /// </summary>
        private static void GeneratePen(StringBuilder result, GraphicPath graphicPath, int level)
        {
            if (graphicPath.StrokeBrush != null)
            {
                var indent1 = SourceGeneratorHelper.GetTagIndent(level);
                var indent2 = SourceGeneratorHelper.GetTagIndent(level + 1);

                bool strokeColorInExtendedProperties = false;

                result.Append(indent1);
                result.AppendLine("<GeometryDrawing.Pen>");

                var tag = "Pen";
                var indentPenProperty = SourceGeneratorHelper.GetPropertyIndent(level + 1, tag);

                result.Append(indent2);
                result.Append(string.Format(CultureInfo.InvariantCulture, "<{0} Thickness=\"{1}\"", tag, DoubleUtilities.FormatString(graphicPath.StrokeThickness)));

                if (graphicPath.StrokeLineCap != GraphicLineCap.Flat)
                {
                    result.AppendLine("");
                    result.Append(indentPenProperty);
                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "StartLineCap=\"{0}\" ", Converter.ConvertToWPF(graphicPath.StrokeLineCap).ToString()));
                    result.Append(indentPenProperty);
                    result.Append(string.Format(CultureInfo.InvariantCulture, "EndLineCap=\"{0}\" ", Converter.ConvertToWPF(graphicPath.StrokeLineCap).ToString()));
                }

                if (graphicPath.StrokeDashes != null)
                {
                    if (graphicPath.StrokeLineCap != GraphicLineCap.Square)
                    {
                        result.AppendLine("");
                        result.Append(indentPenProperty);
                        result.Append(string.Format(CultureInfo.InvariantCulture, "DashCap=\"{0}\" ", Converter.ConvertToWPF(graphicPath.StrokeLineCap).ToString()));
                    }
                }

                if (graphicPath.StrokeLineJoin != GraphicLineJoin.Miter)
                {
                    result.AppendLine("");
                    result.Append(indentPenProperty);
                    result.Append(string.Format(CultureInfo.InvariantCulture, "LineJoin=\"{0}\" ", Converter.ConvertToWpf(graphicPath.StrokeLineJoin).ToString()));
                }
                else
                if (!DoubleUtilities.IsEqual(graphicPath.StrokeMiterLimit, 10))
                {
                    result.AppendLine("");
                    result.Append(indentPenProperty);
                    result.Append(string.Format(CultureInfo.InvariantCulture, "MiterLimit=\"{0}\"", DoubleUtilities.FormatString(graphicPath.StrokeMiterLimit)));
                }

                if (graphicPath.StrokeBrush is GraphicSolidColorBrush strokeColor)
                {
                    result.AppendLine("");

                    Color color = strokeColor.Color;
                    result.Append(indentPenProperty);
                    result.Append(string.Format("Brush=\"{0}\"", SourceGeneratorHelper.FormatColorParamter(color)));
                }
                else
                {
                    strokeColorInExtendedProperties = true;
                }

                if (strokeColorInExtendedProperties || graphicPath.StrokeDashes != null)
                {
                    result.Append(">");
                    result.AppendLine("");

                    if (graphicPath.StrokeDashes != null)
                    {
                        var indent3 = SourceGeneratorHelper.GetTagIndent(level + 2);
                        var indent4 = SourceGeneratorHelper.GetTagIndent(level + 3);

                        var tagDashStyle = "DashStyle";

                        result.Append(indent3);
                        result.AppendLine($"<Pen.{tagDashStyle}>");

                        result.Append(indent4);
                        result.Append($"<{tagDashStyle} Dashes=\"");

                        for (int i = 0; i < graphicPath.StrokeDashes.Count; i++)
                        {
                            if (i != 0)
                            {
                                result.Append(" ");
                            }

                            result.Append(DoubleUtilities.FormatString(graphicPath.StrokeDashes[i]));
                        }

                        result.Append("\"");

                        if (!DoubleUtilities.IsZero(graphicPath.StrokeDashOffset))
                        {
                            var indentDashStyleProperty = SourceGeneratorHelper.GetPropertyIndent(level + 3, tagDashStyle);
                            result.AppendLine("");
                            result.Append(indentDashStyleProperty);
                            result.Append(string.Format(CultureInfo.InvariantCulture, "Offset=\"{0}\"", DoubleUtilities.FormatString(graphicPath.StrokeDashOffset)));
                        }

                        result.AppendLine("/>");
                        result.Append(indent3);
                        result.AppendLine($"</Pen.{tagDashStyle}>");
                    }

                    if (strokeColorInExtendedProperties)
                    {
                        SourceGeneratorHelper.GenerateBrush(result, graphicPath.StrokeBrush, "Pen.Brush", level + 3);
                    }

                    result.AppendLine($"{indent2}</{tag}>");
                }
                else
                {
                    result.AppendLine(" />");
                }

                result.Append(indent1);
                result.AppendLine("</GeometryDrawing.Pen>");
            }
        }
    }
}
