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
    /// <summary>
    /// Stroke
    /// </summary>
    internal class StrokeCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            GraphicsState graphicState = interpreter.GraphicState;

            var path = new GraphicPath();
            path.Geometry = graphicState.CurrentGeometry;

            path.StrokeBrush = graphicState.FillBrush.GetBrush(graphicState.CurrentGeometry.Bounds);
            path.StrokeThickness = graphicState.LineWidth;
            path.StrokeDashes = graphicState.Dashes;
            path.StrokeDashOffset = graphicState.DashOffset;
            path.StrokeLineCap = graphicState.LineCap;
            path.StrokeLineJoin = graphicState.LineJoin;
            path.StrokeMiterLimit = graphicState.MiterLimit;

            path.ColorPrecision = graphicState.FillBrush.ColorPrecision;

            interpreter.GraphicGroup.Childreen.Add(path);
            interpreter.ResetCurrentGeometry();
        }
    }
}
