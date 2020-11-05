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
using EpsSharp.Eps.Shading;
using ShapeConverter;

namespace EpsSharp.Eps.Commands.GraphicState
{
    /// <summary>
    /// ShFill
    /// </summary>
    internal class ShFillOperand : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var shadingDict = interpreter.OperandStack.PopDictionary();

            GraphicsState graphicState = interpreter.GraphicState;

            var graphicPath = new GraphicPath();
            var shadingDescriptor = ShadingActivator.CreateShading(interpreter, shadingDict);
            graphicPath.Geometry = graphicState.ClippingPath;
            graphicPath.FillBrush = shadingDescriptor.GetBrush(graphicState.CurrentTransformationMatrix,
                                                                               graphicState.ClippingPath.Bounds);
            graphicPath.ColorPrecision = shadingDescriptor.ColorPrecision;

            interpreter.GraphicGroup.Children.Add(graphicPath);
        }
    }
}
