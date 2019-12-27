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

namespace EpsSharp.Eps.Commands.MatrixCmd
{
    /// <summary>
    /// Itransform
    /// </summary>
    internal class ItransformOperand : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var op = operandStack.Pop();

            if (op is ArrayOperand arrayOperand)
            {
                var matrix = OperandHelper.GetMatrix(arrayOperand);
                var transY = operandStack.PopRealValue();
                var transX = operandStack.PopRealValue();

                matrix.Invert();
                var vector = matrix.Transform(new Vector(transX, transY));

                operandStack.Push(new RealOperand(vector.X));
                operandStack.Push(new RealOperand(vector.Y));
            }
            else
            {
                var transY = OperandHelper.GetRealValue(op);
                var transX = operandStack.PopRealValue();

                var ctm = interpreter.GraphicState.TransformationMatrix;
                var matrix = new Matrix(ctm.M11, ctm.M12, ctm.M21, ctm.M22, ctm.OffsetX, ctm.OffsetY);
                matrix.Invert();
                var vector = matrix.Transform(new Point(transX, transY));

                operandStack.Push(new RealOperand(vector.X));
                operandStack.Push(new RealOperand(vector.Y));
            }
        }
    }
}
