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

using System.Windows;

namespace ShapeConverter.BusinessLogic.Generators.GeometryCSharpSourceGenerator
{
    /// <summary>
    /// The source code generator that creates path geometry logic
    /// </summary>
    internal class PathGeometryCSharpSourceGenerator : GeometryCSharpSourceGeneratorBase
    {
        private bool lineSegmentCreated;
        private bool bezierSegmentCreated;
        private bool quadraticBezierSegmentCreated;
        private bool figureCreated;

        /// <summary>
        /// Initialize a stream generation
        /// </summary>
        protected override void Init(GraphicVisual visual)
        {
            var code = Code;
            var indent = MethodBodyIndent;

            code.AppendLine($"{indent}PathGeometry pathGeometry = new PathGeometry();");
            code.AppendLine($"{indent}PathFigureCollection pathFigureCollection = new PathFigureCollection();");
            code.AppendLine($"{indent}pathGeometry.Figures = pathFigureCollection;");
        }

        /// <summary>
        /// Create code for beginning a figure
        /// </summary>
        protected override void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
        {
            var code = Code;
            var indent = MethodBodyIndent;

            if (!figureCreated)
            {
                this.figureCreated = true;
                code.AppendLine($"{indent}PathFigure pathFigure;");
                code.AppendLine($"{indent}PathSegmentCollection pathSegmentCollection;");
            }

            code.AppendLine($"{indent}pathFigure = new PathFigure();");
            code.AppendLine($"{indent}pathFigureCollection.Add (pathFigure);");
            code.AppendLine(string.Format("{0}pathFigure.IsClosed = {1};", indent, FormatBool(isClosed)));
            code.AppendLine(string.Format("{0}pathFigure.StartPoint = {1};", indent, FormatPoint(startPoint)));
            code.AppendLine();
            code.AppendLine($"{indent}pathSegmentCollection = new PathSegmentCollection();");
            code.AppendLine($"{indent}pathFigure.Segments = pathSegmentCollection;");
            code.AppendLine();
        }

        /// <summary>
        /// Create code for a LineTo operation
        /// </summary>
        protected override void LineTo(Point point, bool isStroked, bool isSmoothJoin)
        {
            var code = Code;
            var indent = MethodBodyIndent;

            if (!lineSegmentCreated)
            {
                lineSegmentCreated = true;
                code.AppendLine($"{indent}LineSegment lineSegment;");
            }

            code.AppendLine($"{indent}lineSegment = new LineSegment();");
            code.AppendLine($"{indent}pathSegmentCollection.Add(lineSegment);");
            code.AppendLine(string.Format("{0}lineSegment.Point = {1};", indent, FormatPoint(point)));
            code.AppendLine();
        }

        /// <summary>
        /// Create code for a bezier operation
        /// </summary>
        protected override void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
        {
            var code = Code;
            var indent = MethodBodyIndent;

            if (!bezierSegmentCreated)
            {
                bezierSegmentCreated = true;
                code.AppendLine($"{indent}BezierSegment bezierSegment;");
            }

            code.AppendLine($"{indent}bezierSegment = new BezierSegment();");
            code.AppendLine($"{indent}pathSegmentCollection.Add(bezierSegment);");
            code.AppendLine(string.Format("{0}bezierSegment.Point1 = {1};", indent, FormatPoint(point1)));
            code.AppendLine(string.Format("{0}bezierSegment.Point2 = {1};", indent, FormatPoint(point2)));
            code.AppendLine(string.Format("{0}bezierSegment.Point3 = {1};", indent, FormatPoint(point3)));
            code.AppendLine();
        }

        /// <summary>
        /// Create code for a quadratic bezier operation
        /// </summary>
        protected override void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
        {
            var code = Code;
            var indent = MethodBodyIndent;

            if (!quadraticBezierSegmentCreated)
            {
                quadraticBezierSegmentCreated = true;
                code.AppendLine($"{indent}QuadraticBezierSegment quadraticBezierSegment;");
            }

            code.AppendLine($"{indent}quadraticBezierSegment = new QuadraticBezierSegment();");
            code.AppendLine($"{indent}pathSegmentCollection.Add(quadraticBezierSegment);");
            code.AppendLine(string.Format("{0}quadraticBezierSegment.Point1 = {1};", indent, FormatPoint(point1)));
            code.AppendLine(string.Format("{0}quadraticBezierSegment.Point2 = {1};", indent, FormatPoint(point2)));
            code.AppendLine();
        }

        /// <summary>
        /// Create code for terminating a figure
        /// </summary>
        protected override void Terminate()
        {
            var code = Code;
            var indent = MethodBodyIndent;

            code.AppendLine($"{indent}pathGeometry.Freeze();");
            code.AppendLine();
            code.AppendLine($"{indent}return pathGeometry;");
        }
    }
}
