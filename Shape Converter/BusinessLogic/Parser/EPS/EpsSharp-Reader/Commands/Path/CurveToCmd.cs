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
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;
using ShapeConverter;
using ShapeConverter.BusinessLogic.Helper;

namespace EpsSharp.Eps.Commands.Path
{
    /// <summary>
    /// CurveTo
    /// </summary>
    internal class CurveToCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var y3 = operandStack.PopRealValue();
            var x3 = operandStack.PopRealValue();
            var y2 = operandStack.PopRealValue();
            var x2 = operandStack.PopRealValue();
            var y1 = operandStack.PopRealValue();
            var x1 = operandStack.PopRealValue();

            GraphicsState graphicState = interpreter.GraphicState;
            var ctm = graphicState.CurrentTransformationMatrix;

            var bezier = new GraphicCubicBezierSegment();
            graphicState.CurrentGeometry.Segments.Add(bezier);

            bezier.ControlPoint1 = MatrixUtilities.TransformPoint(x1, y1, ctm);
            bezier.ControlPoint2 = MatrixUtilities.TransformPoint(x2, y2, ctm);
            bezier.EndPoint = MatrixUtilities.TransformPoint(x3, y3, ctm);

            graphicState.CurrentPoint = new Point(x3, y3);
        }
    }
}
