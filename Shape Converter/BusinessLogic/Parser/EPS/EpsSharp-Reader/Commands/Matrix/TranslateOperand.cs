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

using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Commands.MatrixCmd
{
    /// <summary>
    /// Translate
    /// </summary>
    internal class TranslateOperand : CommandOperand
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

                matrix.Translate(transX, transY);

                var array = new ArrayOperand();
                array.Values.Add(new OperandProxy(new RealOperand(matrix.M11)));
                array.Values.Add(new OperandProxy(new RealOperand(matrix.M12)));
                array.Values.Add(new OperandProxy(new RealOperand(matrix.M21)));
                array.Values.Add(new OperandProxy(new RealOperand(matrix.M22)));
                array.Values.Add(new OperandProxy(new RealOperand(matrix.OffsetX)));
                array.Values.Add(new OperandProxy(new RealOperand(matrix.OffsetY)));

                operandStack.Push(array);
            }
            else
            {
                var transY = OperandHelper.GetRealValue(op);
                var transX = operandStack.PopRealValue();

                GraphicsState graphicState = interpreter.GraphicState;

                var ctm = graphicState.TransformationMatrix;
                ctm.Translate(transX, transY);
                graphicState.TransformationMatrix = ctm;
            }
        }
    }
}
