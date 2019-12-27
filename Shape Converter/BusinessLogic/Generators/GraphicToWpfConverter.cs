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

namespace ShapeConverter.BusinessLogic.Generators
{
    internal static class Converter
    {
        /// <summary>
        /// Converts a GraphicBrushMappingMode to the WPF type
        /// </summary>
        public static BrushMappingMode ConvertToWpf(GraphicBrushMappingMode graphicBrushMappingMode)
        {
            return graphicBrushMappingMode == GraphicBrushMappingMode.RelativeToBoundingBox ? BrushMappingMode.RelativeToBoundingBox : BrushMappingMode.Absolute;
        }

        /// <summary>
        /// Converts a GraphicLineCap to the WPF type
        /// </summary>
        public static PenLineCap ConvertToWPF(GraphicLineCap graphicLineCap)
        {
            PenLineCap lineCap = PenLineCap.Flat;

            switch (graphicLineCap)
            {
                case GraphicLineCap.Flat:
                    lineCap = PenLineCap.Flat;
                    break;

                case GraphicLineCap.Round:
                    lineCap = PenLineCap.Round;
                    break;

                case GraphicLineCap.Square:
                    lineCap = PenLineCap.Square;
                    break;
            }

            return lineCap;
        }

        /// <summary>
        /// Converts a GraphicLineJoin to the WPF type
        /// </summary>
        public static PenLineJoin ConvertToWpf(GraphicLineJoin graphicLineJoin)
        {
            PenLineJoin lineJoin = PenLineJoin.Miter;

            switch (graphicLineJoin)
            {
                case GraphicLineJoin.Miter:
                    lineJoin = PenLineJoin.Miter;
                    break;

                case GraphicLineJoin.Bevel:
                    lineJoin = PenLineJoin.Bevel;
                    break;

                case GraphicLineJoin.Round:
                    lineJoin = PenLineJoin.Round;
                    break;
            }

            return lineJoin;
        }
    }
}
