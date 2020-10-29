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
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
    /// <summary>
    /// The Css style cascade extension
    /// </summary>
    internal static class CssStyleCascadeExtension
    {
        /// <summary>
        /// Get an attribute as double from the cascade
        /// </summary>
        public static double GetNumber(this CssStyleCascade cssStyleCascade, string name, double defaultValue)
        {
            double retVal = defaultValue;

            var strVal = cssStyleCascade.GetProperty(name);

            if (!string.IsNullOrEmpty(strVal))
            {
                retVal = DoubleParser.GetNumber(strVal);
            }

            return retVal;
        }

        /// <summary>
        /// Get an attribute as double from the top of cascade only
        /// </summary>
        public static double GetNumberPercentFromTop(this CssStyleCascade cssStyleCascade, string name, double defaultValue)
        {
            double retVal = defaultValue;

            var strVal = cssStyleCascade.GetPropertyFromTop(name);

            if (!string.IsNullOrEmpty(strVal))
            {
                (_, retVal) = DoubleParser.GetNumberPercent(strVal);
            }

            return retVal;
        }

        /// <summary>
        /// Get the transformation matrix from top of the cascade
        /// </summary>
        /// <returns></returns>
        public static Matrix GetTransformMatrixFromTop(this CssStyleCascade cssStyleCascade)
        {
            var transform = cssStyleCascade.GetPropertyFromTop("transform");

            if (!string.IsNullOrEmpty(transform))
            {
                return TransformMatrixParser.GetTransformMatrix(transform);
            }

            return Matrix.Identity;
        }
    }
}
