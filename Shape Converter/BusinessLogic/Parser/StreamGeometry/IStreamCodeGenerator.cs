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
using System.Windows.Media;

namespace ShapeConverter.Parser.StreamGeometry
{
    /// <summary>
    /// Stream code generator interface
    /// </summary>
    internal interface IStreamCodeGenerator
    {
        /// <summary>
        /// Initialize the generator
        /// </summary>
        void Init();

        /// <summary>
        /// Sets the fill rule.
        /// </summary>
        void SetFillRule(FillRule fillRule);

        /// <summary>
        /// Terminate the generator
        /// </summary>
        void Terminate();

        /// <summary>
        /// Start a new figure
        /// </summary>
        void BeginFigure(Point startPoint, bool isFilled, bool isClosed);

        /// <summary>
        /// Create  lineto path
        /// </summary>
        void LineTo(Point point, bool isStroked, bool isSmoothJoin);

        /// <summary>
        /// Start a bezier path
        /// </summary>
        void BeginBezier();

        void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin);

        /// <summary>
        /// End a bezier path
        /// </summary>
        void EndBezier();

        /// <summary>
        /// Create  quadratic bezier path
        /// </summary>
        void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin);

        /// <summary>
        /// Create an arc path
        /// </summary>
        void ArcTo(
              Point startPoint,
              Point endPoint,
              Size size,
              double rotationAngle,
              bool isLargeArc,
              bool sweepDirection,
              bool isStroked,
              bool isSmoothJoin
              );

        /// <summary>
        /// Sets the state to closed.
        /// </summary>
        void SetClosedState(bool closed);
    }

}
