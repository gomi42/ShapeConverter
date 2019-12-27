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

namespace ShapeConverter.BusinessLogic.Generators.GeometrySourceGenerator
{
    /// <summary>
    /// The source code generator that creates stream geometry logic
    /// </summary>
    public class StreamGeometrySourceGenerator : GeometrySourceGenerator, IGeometrySourceGenerator
    {
        /// <summary>
        /// Initialize a stream generation
        /// </summary>
        protected override void Init(GraphicVisual visual)
        {
            InitCode(visual);

            var indent2 = Indent2;

            code.AppendLine($"{indent2}StreamGeometry geometry = new StreamGeometry();");
            code.AppendLine($"{indent2}geometry.FillRule = FillRule.EvenOdd;");
            code.AppendLine($"{indent2}StreamGeometryContext ctx = geometry.Open();");
        }

        /// <summary>
        /// Create code for beginning a figure
        /// </summary>
        protected override void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
        {
            code.AppendLine("");
            code.AppendLine(string.Format("{0}ctx.BeginFigure({1}, {2}, {3});", Indent2, FormatPoint(startPoint), FormatBool(isFilled), FormatBool(isClosed)));
        }

        /// <summary>
        /// Create code for a LineTo operation
        /// </summary>
        protected override void LineTo(Point point, bool isStroked, bool isSmoothJoin)
        {
            code.AppendLine(string.Format("{0}ctx.LineTo({1}, {2}, {3});", Indent2, FormatPoint(point), FormatBool(isStroked), FormatBool(isSmoothJoin)));
        }

        /// <summary>
        /// Create code for a bezier operation
        /// </summary>
        protected override void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
        {
            code.AppendLine(string.Format("{0}ctx.BezierTo({1}, {2}, {3}, {4}, {5});", Indent2, FormatPoint(point1), FormatPoint(point2), FormatPoint(point3), FormatBool(isStroked), FormatBool(isSmoothJoin)));
        }

        /// <summary>
        /// Create code for a quadratic bezier operation
        /// </summary>
        protected override void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
        {
            code.AppendLine(string.Format("{0}ctx.QuadraticBezierTo({1}, {2}, {3}, {4});", Indent2, FormatPoint(point1), FormatPoint(point2), FormatBool(isStroked), FormatBool(isSmoothJoin)));
        }

        /// <summary>
        /// Create code for terminating a figure
        /// </summary>
        protected override void Terminate()
        {
            if (this.finalized)
            {
                return;
            }

            this.finalized = true;

            code.AppendLine("");
            code.AppendLine($"{Indent2}ctx.Close();");
            code.AppendLine("        geometry.Freeze();");
            code.AppendLine("");
            code.AppendLine($"{Indent2}return geometry;");

            FinalizeCode();
        }
    }
}
