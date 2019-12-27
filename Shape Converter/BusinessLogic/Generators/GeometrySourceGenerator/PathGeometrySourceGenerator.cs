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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ShapeConverter.BusinessLogic.Generators.GeometrySourceGenerator
{
    /// <summary>
    /// The source code generator that creates path geometry logic
    /// </summary>
    public class PathGeometrySourceGenerator : GeometrySourceGenerator, IGeometrySourceGenerator
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
            InitCode(visual);

            var indent2 = Indent2;

            code.AppendLine($"{indent2}PathGeometry pathGeometry = new PathGeometry();");
            code.AppendLine($"{indent2}PathFigureCollection pathFigureCollection = new PathFigureCollection();");
            code.AppendLine($"{indent2}pathGeometry.Figures = pathFigureCollection;");
        }

        /// <summary>
        /// Create code for beginning a figure
        /// </summary>
        protected override void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
        {
            var indent2 = Indent2;

            if (!this.figureCreated)
            {
                this.figureCreated = true;
                code.AppendLine($"{indent2}PathFigure pathFigure;");
                code.AppendLine($"{indent2}PathSegmentCollection pathSegmentCollection;");
            }

            code.AppendLine($"{indent2}pathFigure = new PathFigure();");
            code.AppendLine($"{indent2}pathFigureCollection.Add (pathFigure);");
            code.AppendLine(string.Format("        pathFigure.StartPoint = {0};", FormatPoint(startPoint)));
            code.AppendLine("");
            code.AppendLine($"{indent2}pathSegmentCollection = new PathSegmentCollection();");
            code.AppendLine($"{indent2}pathFigure.Segments = pathSegmentCollection;");
            code.AppendLine("");

            this.isClosed = isClosed;
        }

        /// <summary>
        /// Create code for a LineTo operation
        /// </summary>
        protected override void LineTo(Point point, bool isStroked, bool isSmoothJoin)
        {
            var indent2 = Indent2;

            if (!lineSegmentCreated)
            {
                lineSegmentCreated = true;
                code.AppendLine($"{indent2}LineSegment lineSegment;");
            }

            code.AppendLine($"{indent2}lineSegment = new LineSegment();");
            code.AppendLine($"{indent2}pathSegmentCollection.Add(lineSegment);");
            code.AppendLine(string.Format("{0}lineSegment.Point = {1};", indent2, FormatPoint(point)));
            code.AppendLine("");
        }

        /// <summary>
        /// Create code for a bezier operation
        /// </summary>
        protected override void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
        {
            var indent2 = Indent2;

            if (!bezierSegmentCreated)
            {
                bezierSegmentCreated = true;
                code.AppendLine("        BezierSegment bezierSegment;");
            }

            code.AppendLine($"{indent2}bezierSegment = new BezierSegment();");
            code.AppendLine($"{indent2}pathSegmentCollection.Add(bezierSegment);");
            code.AppendLine(string.Format("{0}bezierSegment.Point1 = {1};", indent2, FormatPoint(point1)));
            code.AppendLine(string.Format("{0}bezierSegment.Point2 = {1};", indent2, FormatPoint(point2)));
            code.AppendLine(string.Format("{0}bezierSegment.Point3 = {1};", indent2, FormatPoint(point3)));
            code.AppendLine("");
        }

        /// <summary>
        /// Create code for a quadratic bezier operation
        /// </summary>
        protected override void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
        {
            var indent2 = Indent2;

            if (!quadraticBezierSegmentCreated)
            {
                quadraticBezierSegmentCreated = true;
                code.AppendLine("QuadraticBezierSegment quadraticBezierSegment;");
            }

            code.AppendLine($"{indent2}quadraticBezierSegment = new QuadraticBezierSegment();");
            code.AppendLine($"{indent2}pathSegmentCollection.Add(quadraticBezierSegment);");
            code.AppendLine(string.Format("{0}quadraticBezierSegment.Point1 = {1};", indent2, FormatPoint(point1)));
            code.AppendLine(string.Format("{0}quadraticBezierSegment.Point2 = {1};", indent2, FormatPoint(point2)));
            code.AppendLine("");
        }

        /// <summary>
        /// Create code for terminating a figure
        /// </summary>
        protected override void Terminate()
        {
            var indent2 = Indent2;

            if (this.finalized)
            {
                return;
            }

            this.finalized = true;

            code.AppendLine(string.Format("{0}pathFigure.IsClosed = {1};", indent2, FormatBool(this.isClosed)));
            code.AppendLine($"{indent2}pathGeometry.Freeze();");
            code.AppendLine("");
            code.AppendLine($"{indent2}return pathGeometry;");

            FinalizeCode();
        }
    }
}
