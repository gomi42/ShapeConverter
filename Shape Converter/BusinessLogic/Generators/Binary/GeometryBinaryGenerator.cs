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

using System.Windows.Media;

namespace ShapeConverter.BusinessLogic.Generators
{
    /// <summary>
    /// Create a binary geometry
    /// </summary>
    public static class GeometryBinaryGenerator
    {
        /// <summary>
        /// Generates a WPF geometry for a single graphic path geometry
        /// </summary>
        public static Geometry GenerateGeometry(GraphicPathGeometry graphicPathGeometry)
        {
            if (graphicPathGeometry == null)
            {
                return null;
            }

            StreamGeometry geometry = new StreamGeometry();
            geometry.FillRule = ConvertFillRule(graphicPathGeometry.FillRule);
            StreamGeometryContext ctx = geometry.Open();

            foreach (var segment in graphicPathGeometry.Segments)
            {
                switch (segment)
                {
                    case GraphicMoveSegment graphicMove:
                    {
                        ctx.BeginFigure(graphicMove.StartPoint, true, graphicMove.IsClosed);
                        break;
                    }

                    case GraphicLineSegment graphicLineTo:
                    {
                        ctx.LineTo(graphicLineTo.To, true, false);
                        break;
                    }

                    case GraphicCubicBezierSegment graphicCubicBezier:
                    {
                        ctx.BezierTo(graphicCubicBezier.ControlPoint1, graphicCubicBezier.ControlPoint2, graphicCubicBezier.EndPoint, true, false);
                        break;
                    }

                    case GraphicQuadraticBezierSegment graphicQuadraticBezier:
                    {
                        ctx.QuadraticBezierTo(graphicQuadraticBezier.ControlPoint, graphicQuadraticBezier.EndPoint, true, false);
                        break;
                    }
                }
            }

            ctx.Close();
            geometry.Freeze();

            return geometry;
        }

        /// <summary>
        /// Converts the fill rule.
        /// </summary>
        private static FillRule ConvertFillRule(GraphicFillRule graphicFillRule)
        {
            FillRule fillRule = FillRule.EvenOdd;

            switch (graphicFillRule)
            {
                case GraphicFillRule.EvenOdd:
                    fillRule = FillRule.EvenOdd;
                    break;

                case GraphicFillRule.NoneZero:
                    fillRule = FillRule.Nonzero;
                    break;
            }

            return fillRule;
        }
    }
}
