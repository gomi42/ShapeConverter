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

using System.Windows.Media;

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// The font rendering mode
    /// </summary>
    internal enum FontRenderingMode
    {
        /// <summary>
        /// Fill the font shape
        /// </summary>
        Fill,

        /// <summary>
        /// Stroke the font shape
        /// </summary>
        Stroke,

        /// <summary>
        /// Fill and stroke the font shape
        /// </summary>
        FillAndStroke,

        /// <summary>
        /// A PDF mode that is not handled 
        /// </summary>
        Unknown
    }

    /// <summary>
    /// The font state
    /// </summary>
    internal class FontState
    {
        /// <summary>
        /// The current text transformation matrix
        /// </summary>
        public Matrix TextMatrix { get; set; } = Matrix.Identity;

        /// <summary>
        /// The current text line transformation matrix
        /// </summary>
        public Matrix TextLineMatrix { get; set; } = Matrix.Identity;

        /// <summary>
        /// The name of the current font
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        /// The current font size
        /// </summary>
        public double FontSize { get; set; } = 1;

        /// <summary>
        /// Gets or sets the leading.
        /// </summary>
        public double Leading { get; set; }

        /// <summary>
        /// The current rendering mode
        /// </summary>
        public FontRenderingMode RenderingMode { get; set; } = 0;

        /// <summary>
        /// The current font descriptor
        /// </summary>
        public IFont FontDescriptor { get; set; }
    }
}
