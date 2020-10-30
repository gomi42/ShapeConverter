//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019-2020
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
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Parser.Svg.Main;

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
    /// <summary>
    /// The ShapeParser parses a single shape into a GraphicVisual
    /// </summary>
    internal class ShapeParser
    {
        private GeometryParser geometryParser;
        private Clipping clipping;
        private BrushParser brushParser;
        private CssStyleCascade cssStyleCascade;

        /// <summary>
        /// Constructor
        /// </summary>
        public ShapeParser(CssStyleCascade cssStyleCascade,
                           BrushParser brushParser,
                           GeometryParser geometryParser,
                           Clipping clipping)
        {
            this.cssStyleCascade = cssStyleCascade;
            this.brushParser = brushParser;
            this.geometryParser = geometryParser;
            this.clipping = clipping;
        }

        /// <summary>
        /// Parse a single SVG shape
        /// </summary>
        public GraphicVisual Parse(XElement shape,
                                   Matrix currentTransformationMatrix)
        {
            GraphicVisual graphicVisual = null;

            cssStyleCascade.PushStyles(shape);

            var transformMatrix = cssStyleCascade.GetTransformMatrixFromTop();
            currentTransformationMatrix = transformMatrix * currentTransformationMatrix;

            var geometry = geometryParser.Parse(shape, currentTransformationMatrix);

            if (geometry != null)
            {
                var graphicPath = new GraphicPath();
                graphicPath.Geometry = geometry;
                graphicVisual = graphicPath;

                brushParser.SetFillAndStroke(shape, graphicPath, currentTransformationMatrix);

                if (clipping.IsClipPathSet())
                {
                    // shapes don't support clipping, create a group around it
                    var group = new GraphicGroup();
                    graphicVisual = group;
                    group.Children.Add(graphicPath);

                    clipping.SetClipPath(group, currentTransformationMatrix);
                }
            }

            cssStyleCascade.Pop();

            return graphicVisual;
        }
    }
}
