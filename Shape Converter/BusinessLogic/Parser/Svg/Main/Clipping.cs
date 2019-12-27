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
    internal static class Clipping
    {
        /// <summary>
        /// Tests whether a clip path is set
        /// </summary>
        public static bool IsClipPathSet(CssStyleCascade cssStyleCascade)
        {
            var clipPath = cssStyleCascade.GetPropertyFromTop("clip-path");

            return !string.IsNullOrEmpty(clipPath);
        }
                                  
        /// <summary>
        /// Set the clipping of the group
        /// </summary>
        public static void SetClipPath(GraphicGroup group, 
                                       Matrix currentTransformationMatrix, 
                                       CssStyleCascade cssStyleCascade,
                                       Dictionary<string, XElement> globalDefinitions)
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

            // richt now we support only a single path for the clip geometry
            var shapeElement = clipElem.Elements().First();

            if (shapeElement == null)
            {
                return;
            }

            var clipGeometry = GeometryParser.Parse(shapeElement, currentTransformationMatrix);
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
