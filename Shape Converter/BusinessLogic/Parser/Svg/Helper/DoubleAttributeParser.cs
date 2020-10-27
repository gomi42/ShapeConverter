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

using System.Xml.Linq;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Helper
{
    /// <summary>
    /// Extension methods for the DoubleParser
    /// </summary>
    internal static class DoubleAttributeParser
    {
        /// <summary>
        /// Converts the specified attribute to a double value
        /// </summary>
        public static double GetLength(this DoubleParser doubleParser, XElement element, string name, double defaultValue)
        {
            double retVal = defaultValue;
            XAttribute xAttr = element.Attribute(name);

            if (xAttr != null)
            {
                retVal = doubleParser.GetLength(xAttr.Value);
            }

            return retVal;
        }

        /// <summary>
        /// Converts the specified attribute to a double value
        /// </summary>
        public static (bool, double) GetNumberPercent(this DoubleParser doubleParser, XElement element, string name, double defaultValue)
        {
            bool isPercentage = false;
            double retVal = defaultValue;
            XAttribute xAttr = element.Attribute(name);

            if (xAttr != null)
            {
                (isPercentage, retVal) = DoubleParser.GetNumberPercent(xAttr.Value);
            }

            return (isPercentage, retVal);
        }

        /// <summary>
        /// Get a LengthPercent
        /// </summary>
        public static double GetLengthPercent(this DoubleParser doubleParser, XElement element, string attrName, double defaultValue, PercentBaseSelector percentBaseSelector)
        {
            double retVal = defaultValue;
            XAttribute xAttr = element.Attribute(attrName);

            if (xAttr != null)
            {
                retVal = doubleParser.GetLengthPercent(xAttr.Value, percentBaseSelector);
            }

            return retVal;
        }

        /// <summary>
        /// Get a LengthPercentAuto attribute
        /// </summary>
        public static DoubleLengthPercentAuto GetLengthPercentAuto(this DoubleParser doubleParser, XElement element, string attrName, PercentBaseSelector percentBaseSelector)
        {
            XAttribute xAttr = element.Attribute(attrName);
            return doubleParser.GetLengthPercentAuto(xAttr?.Value, percentBaseSelector);
        }
    }
}
