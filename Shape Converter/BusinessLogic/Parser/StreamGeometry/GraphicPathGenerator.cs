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

using System.Windows;
using ShapeConverter.BusinessLogic.ShapeConverter;

namespace ShapeConverter.Parser.StreamGeometry
{
    /// <summary>
    /// The graphic path generator used from the StreamGeometryParser
    /// </summary>
    internal class GraphicPathGenerator : IStreamCodeGenerator
    {
        private GraphicPathGeometry currentPath = null;
        private GraphicMoveSegment lastMove = null;

        /// <summary>
        /// The resulting graphic paths
        /// </summary>
        public GraphicPathGeometry Path { get; set; }

        /// <summary>
        /// Initialize the generator
        /// </summary>
        public void Init()
        {
            Path = new GraphicPathGeometry();
            currentPath = Path;
        }

        /// <summary>
        /// Sets the fill rule.
        /// </summary>
        public void SetFillRule(System.Windows.Media.FillRule fillRule)
        {
            switch (fillRule)
            {
                case System.Windows.Media.FillRule.EvenOdd:
                    currentPath.FillRule = GraphicFillRule.EvenOdd;
                    break;

                case System.Windows.Media.FillRule.Nonzero:
                    currentPath.FillRule = GraphicFillRule.NoneZero;
                    break;
            }
        }

        /// <summary>
        /// Start a new figure
        /// </summary>
        public void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
        {
            // 2 Moves in a row don't make sense -> remove the first
            var num = currentPath.Segments.Count;

            if (num > 0)
            {
                if (currentPath.Segments[num - 1] is GraphicMoveSegment)
                {
                    currentPath.Segments.RemoveAt(num - 1);
                }
            }

            var move = new GraphicMoveSegment { StartPoint = startPoint };
            currentPath.Segments.Add(move);

            lastMove = move;
        }

        /// <summary>
        /// Create  lineto path
        /// </summary>
        public void LineTo(Point point, bool isStroked, bool isSmoothJoin)
        {
            var lineTo = new GraphicLineSegment { To = point };
            currentPath.Segments.Add(lineTo);
        }

        /// <summary>
        /// Unused
        /// </summary>
        public void BeginBezier()
        {
        }

        /// <summary>
        /// Create a bezier path
        /// </summary>
        public void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
        {
            var bezier = new GraphicCubicBezierSegment();
            currentPath.Segments.Add(bezier);

            bezier.ControlPoint1 = point1;
            bezier.ControlPoint2 = point2;
            bezier.EndPoint = point3;
        }

        /// <summary>
        /// Unused
        /// </summary>
        public void EndBezier()
        {
        }

        /// <summary>
        /// Clean up
        /// </summary>
        public void Terminate()
        {
            // some SVGs add a Move as the last statement -> remove it
            var num = currentPath.Segments.Count;

            if (num > 0)
            {
                if (currentPath.Segments[num - 1] is GraphicMoveSegment)
                {
                    currentPath.Segments.RemoveAt(num - 1);
                }
            }
        }

        /// <summary>
        /// Create  quadratic bezier path
        /// </summary>
        public void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
        {
            var bezier = new GraphicQuadraticBezierSegment();
            currentPath.Segments.Add(bezier);

            bezier.ControlPoint = point1;
            bezier.EndPoint = point2;
        }

        /// <summary>
        /// Create an arc path
        /// </summary>
        public void ArcTo(Point startPoint, Point endPoint, Size size, double rotationAngle, bool isLargeArc, bool sweepDirection, bool isStroked, bool isSmoothJoin)
        {
            var arcSegments = ArcToPathSegmentConverter.ArcToPathSegments(startPoint, endPoint, size, rotationAngle, isLargeArc, sweepDirection);
            currentPath.Segments.AddRange(arcSegments);
        }

        /// <summary>
        /// Define the closed state
        /// </summary>
        public void SetClosedState(bool closed)
        {
            lastMove.IsClosed = closed;
        }
    }
}
