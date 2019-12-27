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
using System.Windows.Media;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// Alpha
    /// </summary>
    internal class GraphicAlpha
    {
        public double Current => Object * Layer;

        public double Object { get; set; } = 1.0;

        public double Layer { get; set; } = 1.0;

        public GraphicAlpha Clone()
        {
            return new GraphicAlpha { Object = Object, Layer = Layer };
        }
    }

    /// <summary>
    /// The graphic state
    /// </summary>
    internal class GraphicsState
    {
        /// <summary>
        /// The mirror matrix to transform from PDF coordinates to user (screen) coordinates
        /// </summary>
        public Matrix Mirror { get; set; }

        /// <summary>
        /// The current clipping path
        /// </summary>
        public GraphicPathGeometry ClippingPath { get; set; }

        /// <summary>
        /// The current transformation matrix in logical user coordinates
        /// </summary>
        public Matrix TransformationMatrix { get; set; }

        /// <summary>
        /// The current transformation matrix in final screen coordinates
        /// </summary>
        public Matrix CurrentTransformationMatrix => TransformationMatrix * Mirror;

        /// <summary>
        /// The alpha for the fill color
        /// </summary>
        public GraphicAlpha FillAlpha { get; set; } = new GraphicAlpha();

        /// <summary>
        /// The fill color
        /// </summary>
        public IBrushDescriptor FillBrush { get; set; }

        /// <summary>
        /// The alpha for the stroke color
        /// </summary>
        public GraphicAlpha StrokeAlpha { get; set; } = new GraphicAlpha();

        /// <summary>
        /// The stroke colore
        /// </summary>
        public IBrushDescriptor StrokeBrush { get; set; }

        /// <summary>
        /// The line width
        /// </summary>
        public double LineWidth { get; set; } = 1.0;

        /// <summary>
        /// The line cap definition
        /// </summary>
        public GraphicLineCap LineCap { get; set; } = GraphicLineCap.Flat;

        /// <summary>
        /// The line join method
        /// </summary>
        public GraphicLineJoin LineJoin { get; set; } = GraphicLineJoin.Miter;

        /// <summary>
        /// The miter limit
        /// </summary>
        public double MiterLimit { get; set; } = 10.0;

        /// <summary>
        /// The dash pattern
        /// </summary>
        public List<double> Dashes { get; set; }

        /// <summary>
        /// The dash offset
        /// </summary>
        public double DashOffset { get; set; }

        /// <summary>
        /// The current color space for fill operations
        /// </summary>
        public IColorSpace ColorSpace { get; set; }

        /// <summary>
        /// The current color space for stroke operations
        /// </summary>
        public IColorSpace StrokeColorSpace { get; set; }

        /// <summary>
        /// Return true if the color was accurately computed
        /// otherwise the color is estimated
        /// </summary>
        public GraphicColorPrecision ColorPrecision { get; set; } = GraphicColorPrecision.Precise;

        /// <summary>
        /// Transparency description
        /// </summary>
        public List<FunctionStop> SoftMask { get; set; } = new List<FunctionStop>();

        /// <summary>
        /// Clone the state
        /// </summary>
        /// <returns></returns>
        public GraphicsState Clone()
        {
            var clone = new GraphicsState();

            clone.Mirror = Mirror;
            clone.ClippingPath = ClippingPath;
            clone.TransformationMatrix = TransformationMatrix;
            clone.FillAlpha = FillAlpha.Clone();
            clone.FillBrush = FillBrush;
            clone.StrokeAlpha = StrokeAlpha.Clone();
            clone.StrokeBrush = StrokeBrush;
            clone.LineWidth = LineWidth;
            clone.LineJoin= LineJoin;
            clone.LineCap = LineCap;
            clone.MiterLimit = MiterLimit;
            clone.Dashes = Dashes;
            clone.DashOffset = DashOffset;
            clone.ColorSpace = ColorSpace;
            clone.StrokeColorSpace = StrokeColorSpace;
            clone.ColorPrecision = ColorPrecision;
            clone.SoftMask = SoftMask;

            return clone;
        }
    }
}
