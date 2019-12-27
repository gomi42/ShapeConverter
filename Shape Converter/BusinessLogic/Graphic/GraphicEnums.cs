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

namespace ShapeConverter
{
    /// <summary>
    /// The graphic fill rule
    /// </summary>
    public enum GraphicFillRule
    {
        EvenOdd,
        NoneZero
    }

    /// <summary>
    /// The brush mapping mode
    /// </summary>
    public enum GraphicBrushMappingMode
    {
        RelativeToBoundingBox,
        Absolute
    }

    /// <summary>
    /// Describes colorAccuracy of the converted color
    /// </summary>
    public enum GraphicColorPrecision
    {
        /// <summary>
        /// the color is precise
        /// </summary>
        Precise = 0,

        /// <summary>
        /// the color is a good estimation
        /// </summary>
        Estimated = 1,

        /// <summary>
        /// the color is just a placeholder
        /// </summary>
        Placeholder = 2
    }

    /// <summary>
    /// Describes the shape at the beginning and the end of a segment.
    /// </summary>
    public enum GraphicLineCap
    {
        Flat,
        Round,
        Square
    }

    /// <summary>
    /// Describes the shape that joins two segments
    /// </summary>
    public enum GraphicLineJoin
    {
        Miter,
        Bevel,
        Round
    }
}