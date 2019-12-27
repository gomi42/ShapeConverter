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
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content.Objects;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;
using ShapeConverter.BusinessLogic.Parser.Pdf.Pattern;

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// The special pattern color space
    /// </summary>
    internal class PatternColorSpace : IColorSpace
    {
        private PatternManager patternManager;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Placeholder;

        /// <summary>
        /// Constructor
        /// </summary>
        public PatternColorSpace(PatternManager patternManager)
        {
            this.patternManager = patternManager;
        }

        /// <summary>
        /// init
        /// </summary>
        public void Init(PdfArray colorSpaceArray)
        {
        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfValuesPerColor()
        {
            return 0;
        }

        /// <summary>
        /// GetColor
        /// </summary>
        public Color GetColor(List<double> values, double alpha)
        {
            byte color = 0x90;
            return Color.FromArgb((byte)(alpha * 255), color, color, color);
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(CSequence operands, Matrix matrix, double alpha)
        {
            var name = (CName)operands[0];
            var pattern = patternManager.GetPattern(name.Name);

            return new PatternBrushDescriptor(pattern, matrix, alpha);
        }
    }

    /// <summary>
    /// The pattern brush creator
    /// </summary>
    internal class PatternBrushDescriptor : IBrushDescriptor
    {
        private IPattern pattern;
        private Matrix matrix;
        private double alpha;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => pattern.ColorPrecision;

        /// <summary>
        /// Constructor
        /// </summary>
        public PatternBrushDescriptor(IPattern pattern,  Matrix matrix, double alpha)
        {
            this.pattern = pattern;
            this.matrix = matrix;
            this.alpha = alpha;
        }

        /// <summary>
        /// Get a graphic brush
        /// </summary>
        public GraphicBrush GetBrush(PdfRect boundingBox, List<FunctionStop> softMask)
        {
            return pattern.GetBrush(matrix, boundingBox, alpha, softMask);
        }
    }
}
