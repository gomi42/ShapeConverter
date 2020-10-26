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
    /// The percent base value selector
    /// </summary>
    internal enum PercentBaseSelector
    {
        ViewBoxWidth,
        ViewBoxHeight,
        ViewBoxDiagonal,
        None
    }

    /// <summary>
    /// A DoubleLengthPercentAuto desciption
    /// </summary>
    public struct DoubleLengthPercentAuto
    {
        public bool IsAuto { get; set; }

        public double Value { get; set; }
    }

    /// <summary>
    /// Parse a double value with units attached
    /// </summary>
    internal class DoubleParser
    {
        private CssStyleCascade cssStyleCascade;
        private static Regex doubleUnit = new Regex(@"^(?<value>(?:[+-]?(?:(?:\d*\.\d+)|(?:\d+\.?)))(?:[Ee][+-]?\d+)?)(?<unit>\S*)$");

        /// <summary>
        /// Constructor
        /// </summary>
        public DoubleParser(CssStyleCascade cssStyleCascade)
        {
            this.cssStyleCascade = cssStyleCascade;
        }

        /// <summary>
        /// Parse a value as length or percentage.
        /// </summary>
        public double ParseLengthPercent(string strVal, PercentBaseSelector percentBaseSelector)
        {
            var (_, retVal) = ParseLengthPercentInternal(strVal, percentBaseSelector);

            return retVal;
        }

        /// <summary>
        /// Parse a value as length.
        /// </summary>
        public double ParseLength(string strVal)
        {
            var (percent, retVal) = ParseLengthPercentInternal(strVal, PercentBaseSelector.None);

            if (percent)
            {
                throw new ArgumentException("unit not supported");
            }

            return retVal;
        }

        /// <summary>
        /// Parse a value as number or percent.
        /// </summary>
        public static (bool, double) ParseNumberPercent(string strVal)
        {
            bool isPercentage = false;
            double retVal = 0;

            strVal = strVal.Trim();
            var match = doubleUnit.Match(strVal);

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

            return (isPercentage, retVal);
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
        public DoubleLengthPercentAuto GetLengthPercentAuto(string strVal, PercentBaseSelector percentBaseSelector)
        {
            DoubleLengthPercentAuto retVal = new DoubleLengthPercentAuto();

            if (strVal != null)
            {
                if (strVal == "auto")
                {
                    retVal.IsAuto = true;
                }
                else
                {
                    (_, retVal.Value) = ParseLengthPercentInternal(strVal, percentBaseSelector);
                }
            }
            else
            {
                retVal.IsAuto = true;
            }

            return retVal;
        }

        /// <summary>
        /// Parse a value as length or percentage.
        /// </summary>
        private (bool, double) ParseLengthPercentInternal(string strVal, PercentBaseSelector percentBaseSelector)
        {
            bool isPercentage = false;
            double retVal = 0;

            strVal = strVal.Trim();
            var match = doubleUnit.Match(strVal);

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

                    case "vw":
                        var width = cssStyleCascade.GetCurrentViewBox().ViewBox.Width;
                        retVal = retVal / 100.0 * width;
                        break;

                    case "vh":
                        var height = cssStyleCascade.GetCurrentViewBox().ViewBox.Height;
                        retVal = retVal / 100.0 * height;
                        break;

                    case "vvmin":
                    {
                        var viewBox = cssStyleCascade.GetCurrentViewBox().ViewBox;

                        if (viewBox.Width < viewBox.Height)
                        {
                            retVal = retVal / 100.0 * viewBox.Width;
                        }
                        else
                        {
                            retVal = retVal / 100.0 * viewBox.Height;
                        }
                        break;
                    }

                    case "vvmax":
                    {
                        var viewBox = cssStyleCascade.GetCurrentViewBox().ViewBox;

                        if (viewBox.Width > viewBox.Height)
                        {
                            retVal = retVal / 100.0 * viewBox.Width;
                        }
                        else
                        {
                            retVal = retVal / 100.0 * viewBox.Height;
                        }
                        break;
                    }

                    case "%":
                    {
                        isPercentage = true;
                        retVal /= 100.0;

                        var viewBox = cssStyleCascade.GetCurrentViewBox().ViewBox;

                        switch (percentBaseSelector)
                        {
                            case PercentBaseSelector.ViewBoxWidth:
                                retVal = viewBox.Width * retVal;
                                break;

                            case PercentBaseSelector.ViewBoxHeight:
                                retVal = viewBox.Height * retVal;
                                break;

                            case PercentBaseSelector.ViewBoxDiagonal:
                                retVal = Math.Sqrt(viewBox.Width * viewBox.Width + viewBox.Height * viewBox.Height) * retVal;
                                break;
                        }
                        break;
                    }

                    default:
                        throw new ArgumentException("unit not supported");
                }
            }

            return (isPercentage, retVal);
        }
    }
}
