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
using System.Globalization;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Helper
{
    /// <summary>
    /// Parse a double that might have a unit attached.
    /// We only support px unit, all other units make the SVG invalid
    /// </summary>
    internal static class DoubleWithUnitParser
    {
        /// <summary>
        /// Parse a double that might have a unit attached.
        /// We only support "px" and "%", all other units make the SVG invalid
        /// </summary>
        public static double Parse(string strVal)
        {
            double retVal;

            var hasUnit = strVal.EndsWith("px", StringComparison.OrdinalIgnoreCase);

            if (hasUnit)
            {
                strVal = strVal.Substring(0, strVal.Length - 2);
                retVal = double.Parse(strVal, CultureInfo.InvariantCulture);
            }
            else
            {
                var hasPercent = strVal.EndsWith("%", StringComparison.OrdinalIgnoreCase);

                if (hasPercent)
                {
                    strVal = strVal.Substring(0, strVal.Length - 1);
                    retVal = double.Parse(strVal, CultureInfo.InvariantCulture);
                    retVal /= 100.0;
                }
                else
                {
                    retVal = double.Parse(strVal, CultureInfo.InvariantCulture);
                }
            }

            return retVal;
        }
    }
}
