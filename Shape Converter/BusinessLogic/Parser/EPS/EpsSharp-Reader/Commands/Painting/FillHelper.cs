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
using ShapeConverter;

namespace EpsSharp.Eps.Commands.Painting
{
    internal static class FillHelper
    {
        /// <summary>
        /// Build up a path from the current geometry and the current fill
        /// </summary>
        public static void Fill(EpsInterpreter interpreter, GraphicFillRule fillRule)
        {
            GraphicsState graphicState = interpreter.GraphicState;

            graphicState.CurrentGeometry.FillRule = fillRule;
            var path = new GraphicPath();
            path.Geometry = graphicState.CurrentGeometry;

            path.FillBrush = graphicState.FillBrush.GetBrush(graphicState.CurrentGeometry.Bounds);
            path.ColorPrecision = graphicState.FillBrush.ColorPrecision;

            interpreter.GraphicGroup.Childreen.Add(path);
            interpreter.ResetCurrentGeometry();
        }
    }
}
