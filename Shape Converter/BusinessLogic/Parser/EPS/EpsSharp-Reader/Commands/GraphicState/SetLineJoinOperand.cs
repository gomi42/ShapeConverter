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
    /// SetLineJoin
    /// </summary>
    internal class SetLineJoinOperand : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var lineJoin = interpreter.OperandStack.PopInteger();

            GraphicsState graphicState = interpreter.GraphicState;
            graphicState.LineJoin = GetLineJoin(lineJoin.Value);
        }

        /// <summary>
        /// Get the line join method
        /// </summary>
        /// <returns></returns>
        private GraphicLineJoin GetLineJoin(int lineJoinCode)
        {
            GraphicLineJoin lineJoin = GraphicLineJoin.Miter;

            switch (lineJoinCode)
            {
                case 0:
                    lineJoin = GraphicLineJoin.Miter;
                    break;

                case 1:
                    lineJoin = GraphicLineJoin.Round;
                    break;

                case 2:
                    lineJoin = GraphicLineJoin.Bevel;
                    break;
            }

            return lineJoin;
        }
    }
}
