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
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;
using ShapeConverter;
using ShapeConverter.BusinessLogic.Helper;

namespace EpsSharp.Eps.Commands.Path
{
    /// <summary>
    /// RectClip
    /// The given rectangle should intersect with the existing clipping.
    /// We ignore that for a moment, its an edge case in the context
    /// the ShapeConverter is used.
    /// </summary>
    internal class RectClipCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var topOperand = operandStack.Pop();
            var graphicState = interpreter.GraphicState;
            var ctm = graphicState.CurrentTransformationMatrix;
            var geometry = new GraphicPathGeometry();

            switch (topOperand)
            {
                case IntegerOperand integerOperand:
                case RealOperand realOperand:
                {
                    var height = OperandHelper.GetRealValue(topOperand);
                    var width = operandStack.PopRealValue();
                    var y = operandStack.PopRealValue();
                    var x = operandStack.PopRealValue();

                    AddRectangle(geometry, x, y, width, height, ctm);
                    break;
                }

                case ArrayOperand arrayOperand:
                {
                    for (int i = 0; i < arrayOperand.Values.Count; i += 4)
                    {
                        var x = OperandHelper.GetRealValue(arrayOperand.Values[i].Operand);
                        var y = OperandHelper.GetRealValue(arrayOperand.Values[i + 1].Operand);
                        var height = OperandHelper.GetRealValue(arrayOperand.Values[i + 2].Operand);
                        var width = OperandHelper.GetRealValue(arrayOperand.Values[i + 3].Operand);

                        AddRectangle(geometry, x, y, width, height, ctm);
                    }
                    break;
                }

                case StringOperand stringOperand:
                {
                    var values = HomogeneousNumberArrayDecoder.Decode(stringOperand);

                    for (int i = 0; i < values.Count; i += 4)
                    {
                        var x = values[i];
                        var y = values[i + 1];
                        var height = values[i + 2];
                        var width = values[i + 3];

                        AddRectangle(geometry, x, y, width, height, ctm);
                    }

                    break;
                }
            }

            ClipHelper.SetClip(interpreter, geometry);
            interpreter.ResetCurrentGeometry();
        }

        private void AddRectangle(GraphicPathGeometry geometry, double x, double y, double width, double height, Matrix ctm)
        {
            var point1 = MatrixUtilities.TransformPoint(x, y, ctm);
            var point2 = MatrixUtilities.TransformPoint(x + width, y + height, ctm);

            var move = new GraphicMoveSegment { StartPoint = point1 };
            geometry.Segments.Add(move);

            var lineTo = new GraphicLineSegment { To = new Point(point2.X, point1.Y) };
            geometry.Segments.Add(lineTo);

            lineTo = new GraphicLineSegment { To = new Point(point2.X, point2.Y) };
            geometry.Segments.Add(lineTo);

            lineTo = new GraphicLineSegment { To = new Point(point1.X, point2.Y) };
            geometry.Segments.Add(lineTo);

            move.IsClosed = true;
        }
    }
}
