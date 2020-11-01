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
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;

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
    /// Parse a double value with units attached.
    /// All methods throw an exception in case of a syntax error which is the intended behavior
    /// </summary>
    internal class DoubleParser
    {
        private CssStyleCascade cssStyleCascade;
        private static Regex doubleUnit = new Regex(@"^(?<value>\S+?)(?<unit>[%a-z]*)$", RegexOptions.Compiled);
        private static Regex listDelimiter = new Regex(@"([ ]+,?[ ]*)|(,[ ]*)", RegexOptions.Compiled);

        /// <summary>
        /// Constructor
        /// </summary>
        public DoubleParser(CssStyleCascade cssStyleCascade)
        {
            this.cssStyleCascade = cssStyleCascade;
        }

        public double GetLengthPercent(string strVal, PercentBaseSelector percentBaseSelector)
        {
            var (dblVal, unit) = SplitNumberUnit(strVal);

            if (ApplyLength(ref dblVal, unit))
            {
                return dblVal;
            }

            if (ApplyPercent(ref dblVal, unit, percentBaseSelector))
            {
                return dblVal;
            }

            throw new ArgumentException("unknown unit");
        }

        /// <summary>
        /// Parse a value as length.
        /// </summary>
        public double GetLength(string strVal)
        {
            var (dblVal, unit) = SplitNumberUnit(strVal);

            if (ApplyLength(ref dblVal, unit))
            {
                return dblVal;
            }

            throw new ArgumentException("unknown unit");
        }

        /// <summary>
        /// Parse a value as number or percent.
        /// </summary>
        public static (bool, double) GetNumberPercent(string strVal)
        {
            bool isPercentage = false;

            var (retVal, unit) = SplitNumberUnit(strVal);

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

            return (isPercentage, retVal);
        }

        /// <summary>
        /// Parse a value as number.
        /// </summary>
        public static double GetNumber(string strVal)
        {
            return double.Parse(strVal, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get a LengthPercentAuto
        /// </summary>
        public DoubleLengthPercentAuto GetLengthPercentAuto(string strVal, PercentBaseSelector percentBaseSelector)
        {
            DoubleLengthPercentAuto retVal = new DoubleLengthPercentAuto();

            if (strVal == null || strVal == "auto")
            {
                retVal.IsAuto = true;
            }
            else
            {
                retVal.Value = GetLengthPercent(strVal, percentBaseSelector);
            }

            return retVal;
        }

        /// <summary>
        /// Get a list of numbers
        /// </summary>
        public List<double> GetNumberList(string strVal)
        {
            strVal = strVal.Trim();
            var splits = listDelimiter.Split(strVal);
            var dbls = new List<double>();

            for (int i = 0; i < splits.Length; i += 2)
            {
                var dbl = GetNumber(splits[i]);
                dbls.Add(dbl);
            }

            return dbls;
        }

        /// <summary>
        /// Get a list of points
        /// </summary>
        public List<Point> GetPointList(string strVal)
        {
            strVal = strVal.Trim();
            var splits = listDelimiter.Split(strVal);
            var points = new List<Point>();

            for (int i = 0; i < splits.Length; i += 4)
            {
                var x = GetNumber(splits[i]);
                var y = GetNumber(splits[i + 2]);
                points.Add(new Point(x, y));
            }

            return points;
        }

        /// <summary>
        /// Get a list of length or percent
        /// </summary>
        public List<double> GetLengthPercentList(string strVal, PercentBaseSelector percentBaseSelector)
        {
            strVal = strVal.Trim();
            var splits = listDelimiter.Split(strVal);
            var dbls = new List<double>();

            for (int i = 0; i < splits.Length; i += 2)
            {
                var dbl = GetLengthPercent(splits[i], percentBaseSelector);
                dbls.Add(dbl);
            }

            return dbls;
        }

        /// <summary>
        /// Split the given string into a double and a unit
        /// </summary>
        private static (double, string) SplitNumberUnit(string strVal)
        {
            strVal = strVal.Trim();
            var match = doubleUnit.Match(strVal);

            if (!match.Success)
            {
                throw new ArgumentException("syntax error");
            }

            var unit = match.Groups["unit"].Value;
            strVal = match.Groups["value"].Value;
            var retVal = double.Parse(strVal, CultureInfo.InvariantCulture);

            return (retVal, unit);
        }

        /// <summary>
        /// Apply percent unit conversion to the given value
        /// </summary>
        private bool ApplyPercent(ref double dblVal, string unit, PercentBaseSelector percentBaseSelector)
        {
            if (unit != "%")
            {
                return false;
            }

            dblVal /= 100.0;
            var viewBox = cssStyleCascade.GetCurrentViewBox().ViewBox;

            switch (percentBaseSelector)
            {
                case PercentBaseSelector.ViewBoxWidth:
                    dblVal = viewBox.Width * dblVal;
                    break;

                case PercentBaseSelector.ViewBoxHeight:
                    dblVal = viewBox.Height * dblVal;
                    break;

                case PercentBaseSelector.ViewBoxDiagonal:
                    dblVal = Math.Sqrt(viewBox.Width * viewBox.Width + viewBox.Height * viewBox.Height) * dblVal;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Apply length unit conversion to the given value
        /// </summary>
        private bool ApplyLength(ref double dblVal, string unit)
        {
            bool unitProcessed = true;

            switch (unit)
            {
                case "em":
                    dblVal *= GetFontSize();
                    break;

                case "cm":
                    dblVal *= 96.0 / 2.54;
                    break;

                case "mm":
                    dblVal *= 96.0 / 2.54 / 10.0;
                    break;

                case "Q":
                    dblVal *= 96.0 / 2.54 / 40.0;
                    break;

                case "in":
                    dblVal *= 96.0;
                    break;

                case "pc":
                    dblVal *= 96.0 / 6.0;
                    break;

                case "pt":
                    dblVal *= 96.0 / 72.0;
                    break;

                case "":
                case "px":
                    break;

                case "vw":
                    var width = cssStyleCascade.GetCurrentViewBox().ViewBox.Width;
                    dblVal = dblVal / 100.0 * width;
                    break;

                case "vh":
                    var height = cssStyleCascade.GetCurrentViewBox().ViewBox.Height;
                    dblVal = dblVal / 100.0 * height;
                    break;

                case "vvmin":
                {
                    var viewBox = cssStyleCascade.GetCurrentViewBox().ViewBox;

                    if (viewBox.Width < viewBox.Height)
                    {
                        dblVal = dblVal / 100.0 * viewBox.Width;
                    }
                    else
                    {
                        dblVal = dblVal / 100.0 * viewBox.Height;
                    }
                    break;
                }

                case "vvmax":
                {
                    var viewBox = cssStyleCascade.GetCurrentViewBox().ViewBox;

                    if (viewBox.Width > viewBox.Height)
                    {
                        dblVal = dblVal / 100.0 * viewBox.Width;
                    }
                    else
                    {
                        dblVal = dblVal / 100.0 * viewBox.Height;
                    }
                    break;
                }

                default:
                    unitProcessed = false;
                    break;
            }

            return unitProcessed;
        }

        /// <summary>
        /// Get the font size
        /// </summary>
        private double GetFontSize()
        {
            var fontSizeStr = cssStyleCascade.GetProperty("font-size");
            double fontSize = 16.0;

            if (!string.IsNullOrEmpty(fontSizeStr))
            {
                fontSize = GetLengthPercent(fontSizeStr, PercentBaseSelector.ViewBoxDiagonal);
            }

            return fontSize;
        }
    }
}
