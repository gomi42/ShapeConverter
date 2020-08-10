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

namespace ShapeConverter.BusinessLogic.Generators
{
    /// <summary>
    /// Generate the XAML source code for a PathGeometry
    /// </summary>
    public static class PathGeometrySourceGenerator
    {
        /// <summary>
        /// Generate the XAML source code for a PathGeometry
        /// </summary>
        public static string GeneratePathGeometry(GraphicVisual visual)
        {
            StringBuilder result = new StringBuilder();

            int xKey = 1;
            GeneratePathGeometry(result, visual, ref xKey);

            return result.ToString();
        }

        /// <summary>
        /// Generate the XAML source code for a PathGeometry
        /// </summary>
        private static void GeneratePathGeometry(StringBuilder result, GraphicVisual visual, ref int xKey)
        {
            switch (visual)
            {
                case GraphicGroup group:
                {
                    foreach (var childVisual in group.Childreen)
                    {
                        GeneratePathGeometry(result, childVisual, ref xKey);
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    var shapeName = $"shape{xKey}";
                    GeneratePathGeometry(result, graphicPath.Geometry, 0, shapeName);
                    xKey++;

                    break;
                }
            }
        }

        /// <summary>
        /// Generate the XAML source code for a PathGeometry
        /// </summary>
        public static void GeneratePathGeometry(StringBuilder result, GraphicPathGeometry geometry, int level)
        {
            GeneratePathGeometry(result, geometry, level, string.Empty);
        }

        /// <summary>
        /// Generate the XAML source code for a PathGeometry plus an optional xKey attribute
        /// </summary>
        private static void GeneratePathGeometry(StringBuilder result, GraphicPathGeometry geometry, int level, string xKey)
        {
            bool finalizeLastFigure = false;

            var indent = SourceFormatterHelper.GetTagIndent(level);
            var indent1 = SourceFormatterHelper.GetTagIndent(level + 1);
            var indent2 = SourceFormatterHelper.GetTagIndent(level + 2);

            var pathGeometryTag = "PathGeometry";

            string fillRule = string.Empty;

            switch (geometry.FillRule)
            {
                case GraphicFillRule.EvenOdd:
                    fillRule = "EvenOdd";
                    break;

                case GraphicFillRule.NoneZero:
                    fillRule = "Nonzero";
                    break;
            }

            if (string.IsNullOrEmpty(xKey))
            {
                result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}<{1} FillRule=\"{2}\">", indent, pathGeometryTag, fillRule));
            }
            else
            {
                var indentPathGeometryProperty = SourceFormatterHelper.GetPropertyIndent(level, pathGeometryTag);
                result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}<{1} x:Key=\"{2}\"", indent, pathGeometryTag, xKey));
                result.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}FillRule=\"{1}\">", indentPathGeometryProperty, fillRule));
            }

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

                        var indentPathFigureProperty = SourceFormatterHelper.GetPropertyIndent(level + 1, pathFigureTag);

                        result.Append(string.Format(CultureInfo.InvariantCulture, "{0}<{1} IsClosed=", indent1, pathFigureTag));
                        SourceFormatterHelper.AppendXamlBool(result, graphicMove.IsClosed);
                        result.AppendLine();

                        result.Append(indentPathFigureProperty);
                        result.Append("StartPoint=");
                        SourceFormatterHelper.AppendXamlPoint(result, graphicMove.StartPoint.X, graphicMove.StartPoint.Y);

                        result.AppendLine(">");

                        finalizeLastFigure = true;
                        break;
                    }

                    case GraphicLineSegment graphicLineTo:
                    {
                        result.Append(indent2);
                        result.Append("<LineSegment Point=");
                        SourceFormatterHelper.AppendXamlPoint(result, graphicLineTo.To);
                        result.AppendLine(" />");
                        break;
                    }

                    case GraphicCubicBezierSegment graphicCubicBezier:
                    {
                        var tag = "BezierSegment";
                        var indentProperty = SourceFormatterHelper.GetPropertyIndent(level + 2, tag);

                        result.Append(string.Format(CultureInfo.InvariantCulture, "{0}<{1} Point1=", indent2, tag));
                        SourceFormatterHelper.AppendXamlPoint(result, graphicCubicBezier.ControlPoint1);
                        result.AppendLine();

                        result.Append(indentProperty);
                        result.Append("Point2=");
                        SourceFormatterHelper.AppendXamlPoint(result, graphicCubicBezier.ControlPoint2);
                        result.AppendLine();

                        result.Append(indentProperty);
                        result.Append("Point3=");
                        SourceFormatterHelper.AppendXamlPoint(result, graphicCubicBezier.EndPoint);

                        result.AppendLine(" />");
                        break;
                    }

                    case GraphicQuadraticBezierSegment graphicQuadraticBezier:
                    {
                        var tag = "QuadraticBezierSegment";
                        var indentProperty = SourceFormatterHelper.GetPropertyIndent(level + 2, tag);

                        result.Append(string.Format(CultureInfo.InvariantCulture, "{0}<{1} Point1=", indent2, tag));
                        SourceFormatterHelper.AppendXamlPoint(result, graphicQuadraticBezier.ControlPoint);
                        result.AppendLine();

                        result.Append(indentProperty);
                        result.Append("Point2=");
                        SourceFormatterHelper.AppendXamlPoint(result, graphicQuadraticBezier.EndPoint);

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
        }
    }
}
