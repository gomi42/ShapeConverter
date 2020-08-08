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

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Generators
{
    /// <summary>
    /// The stream source code generator
    /// </summary>
    public static class StreamSourceGenerator
    {
        /// <summary>
        /// Generate a list of raw (pure) geometry streams without any code around
        /// </summary>
        public static List<string> GenerateStreamGeometries(GraphicVisual visual)
        {
            List<string> list = new List<string>();

            GenerateStreamGeometries(visual, list);

            return list;
        }

        /// <summary>
        /// Generate a list of raw (pure) geometry streams without any code around recursively
        /// </summary>
        private static void GenerateStreamGeometries(GraphicVisual visual, List<string> list)
        {
            switch (visual)
            {
                case GraphicGroup group:
                {
                    foreach (var childVisual in group.Childreen)
                    {
                        GenerateStreamGeometries(childVisual, list);
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    var path = GenerateStreamGeometry(graphicPath.Geometry);
                    list.Add(path);

                    break;
                }
            }
        }

        /// <summary>
        /// Generate the XAML source code (a <Path/> for a single graphic path
        /// </summary>
        public static string GenerateGeometry(GraphicVisual visual)
        {
            var tag = "Geometry";
            StringBuilder result = new StringBuilder();

            var geometries = GenerateStreamGeometries(visual);
            int i = 1;

            foreach (var geometry in geometries)
            {
                result.Append($"<{tag} x:Key=\"shape{i}\">");
                result.Append(geometry);
                result.AppendLine($"\"</{tag}>");
                i++;
            }

            return result.ToString();
        }

        /// <summary>
        /// Generate the XAML path source code for all given graphic paths
        /// </summary>
        /// <returns></returns>
        public static string GeneratePath(GraphicVisual visual)
        {
            StringBuilder result = new StringBuilder();

            if (visual is GraphicPath graphicPath)
            {
                result.AppendLine(GeneratePath(graphicPath, true, 0));
            }
            else
            {
                result.AppendLine("<Viewbox>");
                GeneratePathGroup(visual, result, 1);
                result.AppendLine("</Viewbox>");
            }

            return result.ToString();
        }

        /// <summary>
        /// Generate the XAML source code for a visual
        /// </summary>
        private static void GeneratePathGroup(GraphicVisual visual, StringBuilder result, int level)
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
                            result.AppendLine("");
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
                        GeneratePathGroup(childVisual, result, level + 1);
                    }

                    result.AppendLine($"{indentTag}</{tag}>");

                    break;
                }

                case GraphicPath graphicPath:
                {
                    result.AppendLine(GeneratePath(graphicPath, false, level));
                    break;
                }
            }
        }

        /// <summary>
        /// Generate the XAML source code (a <Path/> for a single graphic path
        /// </summary>
        public static string GeneratePath(GraphicPath graphicPath, bool stretch, int level)
        {
            var tag = "Path";
            var indentTag = SourceGeneratorHelper.GetTagIndent(level);
            var indentProperty = SourceGeneratorHelper.GetPropertyIndent(level, tag);
            StringBuilder result = new StringBuilder();

            string stretchParam = stretch ? "Uniform" : "None";
            result.AppendLine($"{indentTag}<{tag} Stretch=\"{stretchParam}\"");

            bool fillColorInExtendedProperties = false;
            bool strokeColorInExtendedProperties = false;

            if (graphicPath.FillBrush != null)
            {
                if (graphicPath.FillBrush is GraphicSolidColorBrush solidFillColor)
                {
                    Color color = solidFillColor.Color;
                    result.Append(indentProperty);
                    result.AppendLine(string.Format("Fill=\"{0}\"", SourceGeneratorHelper.FormatColorParamter(color)));
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
                    Color color = solidStrokeColor.Color;
                    result.Append(indentProperty);
                    result.AppendLine(string.Format("Stroke=\"{0}\" ", SourceGeneratorHelper.FormatColorParamter(color)));
                }
                else
                {
                    strokeColorInExtendedProperties = true;
                }

                result.Append(indentProperty);
                result.AppendLine(string.Format(CultureInfo.InvariantCulture, "StrokeThickness=\"{0}\" ", DoubleUtilities.FormatString(graphicPath.StrokeThickness)));

                if (graphicPath.StrokeLineCap != GraphicLineCap.Flat)
                {
                    result.Append(indentProperty);
                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "StrokeStartLineCap=\"{0}\" ", Converter.ConvertToWPF(graphicPath.StrokeLineCap).ToString()));
                    result.Append(indentProperty);
                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "StrokeEndLineCap=\"{0}\" ", Converter.ConvertToWPF(graphicPath.StrokeLineCap).ToString()));
                }

                if (graphicPath.StrokeDashes != null)
                {
                    if (graphicPath.StrokeLineCap != GraphicLineCap.Flat)
                    {
                        result.Append(indentProperty);
                        result.AppendLine(string.Format(CultureInfo.InvariantCulture, "StrokeDashCap=\"{0}\" ", Converter.ConvertToWPF(graphicPath.StrokeLineCap).ToString()));
                    }

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
                        result.Append(indentProperty);
                        result.AppendLine(string.Format(CultureInfo.InvariantCulture, "StrokeDashOffset=\"{0}\"", DoubleUtilities.FormatString(graphicPath.StrokeDashOffset)));
                    }
                }

                if (graphicPath.StrokeLineJoin != GraphicLineJoin.Miter)
                {
                    result.Append(indentProperty);
                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "StrokeLineJoin=\"{0}\" ", Converter.ConvertToWpf(graphicPath.StrokeLineJoin).ToString()));
                }
                else
                if (!DoubleUtilities.IsEqual(graphicPath.StrokeMiterLimit, 10))
                {
                    result.Append(indentProperty);
                    result.AppendLine(string.Format(CultureInfo.InvariantCulture, "MiterLimit=\"{0}\"", DoubleUtilities.FormatString(graphicPath.StrokeMiterLimit)));
                }
            }

            result.Append(indentProperty);
            result.Append("Data=\"");
            result.Append(GenerateStreamGeometry(graphicPath.Geometry));
            result.Append("\"");

            if (fillColorInExtendedProperties || strokeColorInExtendedProperties)
            {
                result.AppendLine(">");

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

        /// <summary>
        /// Generate the raw (pure) stream geometry for a single graphic path
        /// </summary>
        public static string GenerateStreamGeometry(GraphicPathGeometry geometry, bool includeFillRule = true)
        {
            StringBuilder result = new StringBuilder();
            bool closeLastPath = false;

            if (includeFillRule)
            {
                switch (geometry.FillRule)
                {
                    case GraphicFillRule.EvenOdd:
                        result.Append("F0");
                        break;

                    case GraphicFillRule.NoneZero:
                        result.Append("F1");
                        break;
                }
            }

            foreach (var segment in geometry.Segments)
            {
                switch (segment)
                {
                    case GraphicMoveSegment graphicMove:
                    {
                        if (closeLastPath)
                        {
                            result.Append("z ");
                        }

                        result.Append("M");
                        AppendPoint(result, graphicMove.StartPoint.X, graphicMove.StartPoint.Y);
                        closeLastPath = graphicMove.IsClosed;
                        break;
                    }

                    case GraphicLineSegment graphicLineTo:
                    {
                        result.Append("L");
                        AppendPoint(result, graphicLineTo.To);
                        break;
                    }

                    case GraphicCubicBezierSegment graphicCubicBezier:
                    {
                        result.Append("C");
                        AppendPoint(result, graphicCubicBezier.ControlPoint1);
                        AppendPoint(result, graphicCubicBezier.ControlPoint2);
                        AppendPoint(result, graphicCubicBezier.EndPoint);
                        break;
                    }

                    case GraphicQuadraticBezierSegment graphicQuadraticBezier:
                    {
                        result.Append("Q");
                        AppendPoint(result, graphicQuadraticBezier.ControlPoint);
                        AppendPoint(result, graphicQuadraticBezier.EndPoint);
                        break;
                    }

                    default:
                        break;
                }
            }

            if (closeLastPath)
            {
                result.Append("z");
            }

            var len = result.Length;

            if (result[len - 1] == ' ')
            {
                result.Remove(len - 1, 1);
            }

            return result.ToString();
        }









        public static string GeneratePathGeometry(GraphicVisual visual)
        {
            StringBuilder result = new StringBuilder();

            GeneratePathGeometry(result, visual);

            return result.ToString();
        }

        /// <summary>
        /// Generate a list of raw (pure) geometry streams without any code around recursively
        /// </summary>
        private static void GeneratePathGeometry(StringBuilder result, GraphicVisual visual)
        {
            switch (visual)
            {
                case GraphicGroup group:
                {
                    foreach (var childVisual in group.Childreen)
                    {
                        GeneratePathGeometry(result, childVisual);
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    GeneratePathGeometry(result, graphicPath.Geometry, 0);

                    break;
                }
            }
        }

        /// <summary>
        /// Generate the XAML source code for a PathFigure
        /// </summary>
        public static string GeneratePathGeometry(StringBuilder result, GraphicPathGeometry geometry, int level)
        {
            bool finalizeLastFigure = false;

            var indent = SourceGeneratorHelper.GetTagIndent(level);
            var indent1 = SourceGeneratorHelper.GetTagIndent(level + 1);
            var indent2 = SourceGeneratorHelper.GetTagIndent(level + 2);

            var pathGeometryTag = "PathGeometry";

            string fillRule = string.Empty;

            switch (geometry.FillRule)
            {
                case GraphicFillRule.EvenOdd:
                    fillRule = "EvenOdd";
                    break;

                case GraphicFillRule.NoneZero:
                    fillRule = "NoneZero";
                    break;
            }

            result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}<{1} FillRule=\"{2}\">", indent, pathGeometryTag, fillRule));

            var pathFigureTag = "PathFigure";

            foreach (var segment in geometry.Segments)
            {
                switch (segment)
                {
                    case GraphicMoveSegment graphicMove:
                    {
                        if (finalizeLastFigure)
                        {
                            result.Append(indent1);
                            result.AppendLine("</PathFigure>");
                        }

                        var indentPathFigureProperty = SourceGeneratorHelper.GetPropertyIndent(level +1, pathFigureTag);

                        result.Append(string.Format(CultureInfo.InvariantCulture, "{0}<{1} IsClosed=", indent1, pathFigureTag));
                        AppendXamlBool(result, graphicMove.IsClosed);
                        result.AppendLine();

                        result.Append(indentPathFigureProperty);
                        result.Append("StartPoint=");
                        AppendXamlPoint(result, graphicMove.StartPoint.X, graphicMove.StartPoint.Y);

                        result.AppendLine(">");

                        finalizeLastFigure = true;
                        break;
                    }

                    case GraphicLineSegment graphicLineTo:
                    {
                        result.Append(indent2);
                        result.Append("<LineSegment Point=");
                        AppendXamlPoint(result, graphicLineTo.To);
                        result.AppendLine(" />");
                        break;
                    }

                    case GraphicCubicBezierSegment graphicCubicBezier:
                    {
                        var tag = "BezierSegment";
                        var indentProperty = SourceGeneratorHelper.GetPropertyIndent(level + 2, tag);

                        result.Append(string.Format(CultureInfo.InvariantCulture, "{0}<{1} Point1=", indent2, tag));
                        AppendXamlPoint(result, graphicCubicBezier.ControlPoint1);
                        result.AppendLine();

                        result.Append(indentProperty);
                        result.Append("Point2=");
                        AppendXamlPoint(result, graphicCubicBezier.ControlPoint2);
                        result.AppendLine();

                        result.Append(indentProperty);
                        result.Append("Point3=");
                        AppendXamlPoint(result, graphicCubicBezier.EndPoint);

                        result.AppendLine(" />");
                        break;
                    }

                    case GraphicQuadraticBezierSegment graphicQuadraticBezier:
                    {
                        var tag = "QuadraticBezierSegment";
                        var indentProperty = SourceGeneratorHelper.GetPropertyIndent(level + 2, tag);

                        result.Append(string.Format(CultureInfo.InvariantCulture, "{0}<{1} Point1=", indent2, tag));
                        AppendXamlPoint(result, graphicQuadraticBezier.ControlPoint);
                        result.AppendLine();

                        result.Append(indentProperty);
                        result.Append("Point2=");
                        AppendXamlPoint(result, graphicQuadraticBezier.EndPoint);

                        result.AppendLine(" />");
                        break;
                    }

                    default:
                        break;
                }
            }

            if (finalizeLastFigure)
            {
                result.Append(indent1);
                result.AppendLine($"</{pathFigureTag}>");
            }

            result.AppendLine($"{indent}</{pathGeometryTag}>");

            return result.ToString();
        }

        /// <summary>
        /// append a point to the stream
        /// </summary>
        private static void AppendPoint(StringBuilder output, double x, double y)
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
        private static void AppendPoint(StringBuilder output, Point point)
        {
            AppendPoint(output, point.X, point.Y);
        }

        /// <summary>
        /// Append a numeric value to the stream
        /// </summary>
        private static void AppendValue(StringBuilder output, double val)
        {
            string valstr = DoubleUtilities.FormatString(val);
            output.Append(valstr);
            output.Append(' ');
        }

        /// <summary>
        /// append a point to the stream
        /// </summary>
        private static void AppendXamlPoint(StringBuilder output, double x, double y)
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
        private static void AppendXamlPoint(StringBuilder output, Point point)
        {
            AppendXamlPoint(output, point.X, point.Y);
        }

        /// <summary>
        /// Append a bool to the stream
        /// </summary>
        private static void AppendXamlBool(StringBuilder output, bool boolVal)
        {
            string boolstr = boolVal ? "True" : "False";
            output.Append("\"");
            output.Append(boolstr);
            output.Append("\"");
        }
    }
}
