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

namespace ShapeConverter.BusinessLogic.Helper
{
    internal static class DoubleUtilities
    {
        /// <summary>
        /// Test whether 2 doubles are equal
        /// </summary>
        public static bool IsEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < 1e-15;
        }

        /// <summary>
        /// Determines whether the specified d1 is zero.
        /// </summary>
        public static bool IsZero(double d1)
        {
            return Math.Abs(d1) < 1e-15;
        }

        /// <summary>
        /// Format a double
        /// </summary>
        public static string FormatString(double d)
        {
            var strValue = d.ToString("0.#####", CultureInfo.InvariantCulture);

            return strValue;
        }

        /// <summary>
        /// Interpolate the given x
        /// </summary>
        public static double Interpolate(double x,
                                         double xMin,
                                         double xMax,
                                         double yMin,
                                         double yMax)
        {
            var y = (x - xMin) * (yMax - yMin) / (xMax - xMin) + yMin;

            return y;
        }
    }
}
