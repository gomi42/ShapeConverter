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
    /// The source code generator that creates stream geometry logic
    /// </summary>
    internal class StreamGeometryCSharpSourceGenerator : GeometryCSharpSourceGeneratorBase
    {
        /// <summary>
        /// Initialize a stream generation
        /// </summary>
        protected override void Init(GraphicVisual visual)
        {
            var code = Code;
            var indent = MethodBodyIndent;

            code.AppendLine($"{indent}StreamGeometry geometry = new StreamGeometry();");
            code.AppendLine($"{indent}geometry.FillRule = FillRule.EvenOdd;");
            code.AppendLine($"{indent}StreamGeometryContext ctx = geometry.Open();");
        }

        /// <summary>
        /// Create code for beginning a figure
        /// </summary>
        protected override void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
        {
            var code = Code;

            code.AppendLine();
            code.AppendLine(string.Format("{0}ctx.BeginFigure({1}, {2}, {3});", MethodBodyIndent, FormatPoint(startPoint), FormatBool(isFilled), FormatBool(isClosed)));
        }

        /// <summary>
        /// Create code for a LineTo operation
        /// </summary>
        protected override void LineTo(Point point, bool isStroked, bool isSmoothJoin)
        {
            Code.AppendLine(string.Format("{0}ctx.LineTo({1}, {2}, {3});", MethodBodyIndent, FormatPoint(point), FormatBool(isStroked), FormatBool(isSmoothJoin)));
        }

        /// <summary>
        /// Create code for a bezier operation
        /// </summary>
        protected override void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
        {
            Code.AppendLine(string.Format("{0}ctx.BezierTo({1}, {2}, {3}, {4}, {5});", MethodBodyIndent, FormatPoint(point1), FormatPoint(point2), FormatPoint(point3), FormatBool(isStroked), FormatBool(isSmoothJoin)));
        }

        /// <summary>
        /// Create code for a quadratic bezier operation
        /// </summary>
        protected override void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
        {
            Code.AppendLine(string.Format("{0}ctx.QuadraticBezierTo({1}, {2}, {3}, {4});", MethodBodyIndent, FormatPoint(point1), FormatPoint(point2), FormatBool(isStroked), FormatBool(isSmoothJoin)));
        }

        /// <summary>
        /// Create code for terminating a figure
        /// </summary>
        protected override void Terminate()
        {
            var code = Code;

            code.AppendLine();
            code.AppendLine($"{MethodBodyIndent}ctx.Close();");
            code.AppendLine($"{MethodBodyIndent}geometry.Freeze();");
            code.AppendLine();
            code.AppendLine($"{MethodBodyIndent}return geometry;");
        }
    }
}
