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

using System.Collections.Generic;
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;
using ShapeConverter.BusinessLogic.Helper;

namespace EpsSharp.Eps.Commands.GraphicState
{
    /// <summary>
    /// SetDash
    /// </summary>
    internal class SetDashOperand : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var offset = operandStack.PopRealValue();
            var array = operandStack.PopArray();

            SetDashPattern(interpreter.GraphicState, offset, array);
        }

        /// <summary>
        /// Sets the dash pattern
        /// </summary>
        private void SetDashPattern(GraphicsState graphicState, double dashPhase, ArrayOperand dashArray)
        {
            if (dashArray.Values.Count == 0)
            {
                graphicState.Dashes = null;
                graphicState.DashOffset = 0;
                return;
            }

            var dashes = new List<double>();

            foreach (var val in dashArray.Values)
            {
                var dbl = MatrixUtilities.TransformScale(OperandHelper.GetRealValue(val.Operand), graphicState.CurrentTransformationMatrix);
                dashes.Add(dbl);
            }

            graphicState.Dashes = dashes;
            graphicState.DashOffset = dashPhase;
        }
    }
}
