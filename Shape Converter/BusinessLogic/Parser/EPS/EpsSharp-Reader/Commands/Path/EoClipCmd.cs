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

namespace EpsSharp.Eps.Commands.Path
{
    /// <summary>
    /// EoClip
    /// The current path should intersect with the existing clipping.
    /// We ignore that for a moment, its an edge case in the context
    /// the ShapeConverter is used.
    /// </summary>
    internal class EoClipCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            ClipHelper.SetClip(interpreter, interpreter.GraphicState.CurrentGeometry);
        }
    }
}
