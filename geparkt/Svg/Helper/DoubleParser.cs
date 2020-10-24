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
using System.Text.RegularExpressions;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Helper
{
    /// <summary>
    /// A DoubleLengthPercentAuto desciption
    /// </summary>
    public struct DoubleLengthPercentAuto
    {
        public bool IsAuto { get; set; }

        public bool IsPercentage { get; set; }

        public double Value { get; set; }
    }

    /// <summary>
    /// Parse a double value with units attached
    /// </summary>
    internal static class DoubleParser
    {
        private static Regex unitSplit = new Regex(@"^(?<value>[+-]?\d*\.?\d*)(?<unit>\S*)$");

        /// <summary>
        /// Parse a value as length or percentage.
        /// </summary>
        public static bool ParseLengthPercent(string strVal, out double retVal)
        {
            bool isPercentage = false;
            retVal = 0;

            var match = unitSplit.Match(strVal);

            if (match.Success)
            {
                var unit = match.Groups["unit"].Value;
                strVal = match.Groups["value"].Value;
                retVal = double.Parse(strVal, CultureInfo.InvariantCulture);

                switch (unit)
                {
                    case "cm":
                        retVal *= 96.0 / 2.54;
                        break;

                    case "mm":
                        retVal *= 96.0 / 2.54 / 10.0;
                        break;

                    case "Q":
                        retVal *= 96.0 / 2.54 / 40.0;
                        break;

                    case "in":
                        retVal *= 96.0;
                        break;

                    case "pc":
                        retVal *= 96.0 / 6.0;
                        break;

                    case "pt":
                        retVal *= 96.0 / 72.0;
                        break;

                    case "":
                    case "px":
                        break;

                    case "%":
                        isPercentage = true;
                        retVal /= 100.0;
                        break;

                    default:
                        throw new ArgumentException("unit not supported");
                }
            }

            return isPercentage;
        }

        /// <summary>
        /// Parse a value as length.
        /// </summary>
        public static double ParseLength(string strVal)
        {
            if (ParseLengthPercent(strVal, out double retVal))
            {
                throw new ArgumentException("unit not supported");
            }

            return retVal;
        }

        /// <summary>
        /// Parse a value as number or percent.
        /// </summary>
        public static bool ParseNumberPercent(string strVal, out double retVal)
        {
            bool isPercentage = false;
            retVal = 0;

            var match = unitSplit.Match(strVal);

            if (match.Success)
            {
                var unit = match.Groups["unit"].Value;
                strVal = match.Groups["value"].Value;
                retVal = double.Parse(strVal, CultureInfo.InvariantCulture);

                switch (unit)
                {
                    case "%":
                        isPercentage = true;
                        retVal /= 100.0;
                        break;

                    case "":
                        break;

                    default:
                        throw new ArgumentException("unit not supported");
                }
            }

            return isPercentage;
        }

        /// <summary>
        /// Parse a value as number.
        /// </summary>
        public static double ParseNumber(string strVal)
        {
            return double.Parse(strVal, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get a LengthPercentAuto attribute
        /// </summary>
        public static DoubleLengthPercentAuto GetLengthPercentAuto(string strVal)
        {
            DoubleLengthPercentAuto radius = new DoubleLengthPercentAuto();

            if (strVal != null)
            {
                if (strVal == "auto")
                {
                    radius.IsAuto = true;
                }
                else
                {
                    radius.IsPercentage = DoubleParser.ParseLengthPercent(strVal, out double retVal);
                    radius.Value = retVal;
                }
            }
            else
            {
                radius.IsAuto = true;
            }

            return radius;
        }
    }
}
