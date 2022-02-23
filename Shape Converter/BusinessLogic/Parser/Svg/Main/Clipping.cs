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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Main
{
    internal class Clipping
    {
        private GeometryParser geometryParser;
        private GeometryTextParser textParser;
        private SvgParser svgParser;
        private CssStyleCascade cssStyleCascade;
        private Dictionary<string, XElement> globalDefinitions;

        /// <summary>
        /// Constructor
        /// </summary>
        public Clipping(CssStyleCascade cssStyleCascade,
                        Dictionary<string, XElement> globalDefinitions,
                        GeometryParser geometryParser,
                        GeometryTextParser textParser,
                        SvgParser svgParser)
        {
            this.cssStyleCascade = cssStyleCascade;
            this.globalDefinitions = globalDefinitions;
            this.geometryParser = geometryParser;
            this.textParser = textParser;
            this.svgParser = svgParser;
        }

        /// <summary>
        /// Tests whether a clip path is set
        /// </summary>
        public bool IsClipPathSet()
        {
            var clipPath = cssStyleCascade.GetPropertyFromTop("clip-path");

            return !string.IsNullOrEmpty(clipPath);
        }
                                  
        /// <summary>
        /// Set the clipping of the group
        /// </summary>
        public void SetClipPath(GraphicGroup group, 
                                Matrix currentTransformationMatrix)
        {
            var clipPath = cssStyleCascade.GetPropertyFromTop("clip-path");

            if (string.IsNullOrEmpty(clipPath))
            {
                return;
            }

            if (!clipPath.StartsWith("url(", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int endUri = clipPath.IndexOf(")", StringComparison.OrdinalIgnoreCase);
            var uri = clipPath.Substring(4, endUri - 4);
            uri = uri.Trim();
            var id = uri.Substring(1);

            if (!globalDefinitions.ContainsKey(id))
            {
                return;
            }

            var clipElem = globalDefinitions[id];

            // right now we support only a single path for the clip geometry
            var shapeElement = clipElem.Elements().First();

            if (shapeElement == null)
            {
                return;
            }

            GraphicPathGeometry clipGeometry;

            switch (shapeElement.Name.LocalName)
            {
                case "text":
                    clipGeometry = textParser.ParseTextGeometry(shapeElement, currentTransformationMatrix);
                    break;

                case "use":
                    var visual = svgParser.ParseUseElement(shapeElement, currentTransformationMatrix);

                    //for the moment handle single element only
                    if (!(visual is GraphicPath path))
                    {
                        return;
                    }

                    clipGeometry = path.Geometry;
                    break;

                default:
                    clipGeometry = geometryParser.Parse(shapeElement, currentTransformationMatrix);
                    break;
            }

            if (clipGeometry == null)
            {
                return;
            }

            clipGeometry.FillRule = GraphicFillRule.NoneZero;

            var clipRule = cssStyleCascade.GetProperty("clip-rule");

            if (!string.IsNullOrEmpty(clipRule))
            {
                switch (clipRule)
                {
                    case "evenodd":
                        clipGeometry.FillRule = GraphicFillRule.EvenOdd;
                        break;

                    case "nonzero":
                        clipGeometry.FillRule = GraphicFillRule.NoneZero;
                        break;
                }
            }

            group.Clip = clipGeometry;
        }
    }
}
