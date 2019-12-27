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
using ShapeConverter;

namespace EpsSharp.Eps.Commands.GraphicState
{
    /// <summary>
    /// SetLineCap
    /// </summary>
    internal class SetLineCapOperand : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var lineCap = interpreter.OperandStack.PopInteger();

            GraphicsState graphicState = interpreter.GraphicState;
            graphicState.LineCap = GetLineCap(lineCap.Value);
        }

        /// <summary>
        /// Gets the line cap
        /// </summary>
        /// <returns></returns>
        private GraphicLineCap GetLineCap(int lineCapCode)
        {
            GraphicLineCap lineCap = GraphicLineCap.Flat;

            switch (lineCapCode)
            {
                case 0:
                    lineCap = GraphicLineCap.Flat;
                    break;

                case 1:
                    lineCap = GraphicLineCap.Round;
                    break;

                case 2:
                    lineCap = GraphicLineCap.Square;
                    break;
            }

            return lineCap;
        }
    }
}
