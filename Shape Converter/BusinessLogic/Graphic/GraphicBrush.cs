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
using System.Windows;
using System.Windows.Media;

namespace ShapeConverter
{
    /// <summary>
    /// Base class for a color definition
    /// </summary>
    public abstract class GraphicBrush
    {
    }

    /// <summary>
    /// A solid color definition
    /// </summary>
    /// <seealso cref="ShapeConverter.GraphicBrush" />
    public class GraphicSolidColorBrush : GraphicBrush
    {
        public Color Color { get; set; }
    }

    /// <summary>
    /// A gradient stop
    /// </summary>
    public class GraphicGradientStop
    {
        public Color Color { get; set; }
        public double Position { get; set; }
    }

    /// <summary>
    /// Base class for gradient brushes
    /// </summary>
    public abstract class GraphicGradientBrush : GraphicBrush
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public GraphicBrushMappingMode MappingMode { get; set; } = GraphicBrushMappingMode.RelativeToBoundingBox;
        public List<GraphicGradientStop> GradientStops { get; set; }
    }

    /// <summary>
    /// A linear gradient color definition
    /// </summary>
    /// <seealso cref="ShapeConverter.GraphicBrush" />
    public class GraphicLinearGradientBrush : GraphicGradientBrush
    {
    }

    /// <summary>
    /// A radial gradient color definition
    /// </summary>
    /// <seealso cref="ShapeConverter.GraphicBrush" />
    public class GraphicRadialGradientBrush : GraphicGradientBrush
    {
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }
    }
}
