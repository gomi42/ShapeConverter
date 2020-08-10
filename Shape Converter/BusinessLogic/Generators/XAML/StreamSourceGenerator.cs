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
using System.Text;

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
                        SourceFormatterHelper.AppendPoint(result, graphicMove.StartPoint.X, graphicMove.StartPoint.Y);
                        closeLastPath = graphicMove.IsClosed;
                        break;
                    }

                    case GraphicLineSegment graphicLineTo:
                    {
                        result.Append("L");
                        SourceFormatterHelper.AppendPoint(result, graphicLineTo.To);
                        break;
                    }

                    case GraphicCubicBezierSegment graphicCubicBezier:
                    {
                        result.Append("C");
                        SourceFormatterHelper.AppendPoint(result, graphicCubicBezier.ControlPoint1);
                        SourceFormatterHelper.AppendPoint(result, graphicCubicBezier.ControlPoint2);
                        SourceFormatterHelper.AppendPoint(result, graphicCubicBezier.EndPoint);
                        break;
                    }

                    case GraphicQuadraticBezierSegment graphicQuadraticBezier:
                    {
                        result.Append("Q");
                        SourceFormatterHelper.AppendPoint(result, graphicQuadraticBezier.ControlPoint);
                        SourceFormatterHelper.AppendPoint(result, graphicQuadraticBezier.EndPoint);
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
    }
}
