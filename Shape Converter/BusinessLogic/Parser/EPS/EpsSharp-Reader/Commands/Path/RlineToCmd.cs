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
    /// RlineTo
    /// </summary>
    internal class RlineToCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var dy = operandStack.PopRealValue();
            var dx = operandStack.PopRealValue();

            GraphicsState graphicState = interpreter.GraphicState;
            var ctm = graphicState.CurrentTransformationMatrix;

            var currentPoint = graphicState.CurrentPoint;
            var x = currentPoint.X + dx;
            var y = currentPoint.Y + dy;

            var point = MatrixUtilities.TransformPoint(x, y, ctm);

            var lineTo = new GraphicLineSegment { To = point };
            graphicState.CurrentGeometry.Segments.Add(lineTo);

            graphicState.CurrentPoint = new Point(x, y);
        }
    }
}
