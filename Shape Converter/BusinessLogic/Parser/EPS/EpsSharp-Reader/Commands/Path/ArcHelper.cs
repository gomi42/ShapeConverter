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
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Helper;
using ShapeConverter.BusinessLogic.ShapeConverter;

namespace EpsSharp.Eps.Commands.Path
{
    /// <summary>
    /// The arc command creator
    /// </summary>
    internal static class ArcHelper
    {
        public static void CreateArc(EpsInterpreter interpreter, bool sweepDirection)
        {
            var operandStack = interpreter.OperandStack;
            GraphicsState graphicState = interpreter.GraphicState;

            var angle2 = operandStack.PopRealValue();
            var angle1 = operandStack.PopRealValue();
            var r = operandStack.PopRealValue();
            var y = operandStack.PopRealValue();
            var x = operandStack.PopRealValue();

            // be aware the segment converter returns coordinates in user space
            var (segments, startPoint, currentPoint) = ArcToPathSegmentConverter.ArcToPathSegments(new Point(x, y), r, angle1, angle2, sweepDirection);

            if (graphicState.CurrentGeometry == null)
            {
                graphicState.CurrentGeometry = new GraphicPathGeometry();

                var point = MatrixUtilities.TransformPoint(startPoint, graphicState.CurrentTransformationMatrix);
                var move = new GraphicMoveSegment { StartPoint = point };
                graphicState.CurrentGeometry.Segments.Add(move);

                graphicState.LastMove = move;
            }
            else
            {
                var point = MatrixUtilities.TransformPoint(startPoint, graphicState.CurrentTransformationMatrix);
                var lineSegment = new GraphicLineSegment { To = point };
                graphicState.CurrentGeometry.Segments.Add(lineSegment);
            }

            var transformVisual = new TransformVisual();
            var transformedSegments = transformVisual.TransformSegments(segments, graphicState.CurrentTransformationMatrix);

            graphicState.CurrentGeometry.Segments.AddRange(transformedSegments);

            graphicState.CurrentPoint = currentPoint;
        }
    }
}
