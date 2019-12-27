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
using System.Windows;
using System.Windows.Media;

namespace ShapeConverter.BusinessLogic.Helper
{
    internal static class MatrixUtilities
    {
        /// <summary>
        /// Normalize a single point
        /// </summary>
        public static Point TransformPoint(Point point, Matrix transformMatrix)
        {
            return point * transformMatrix;
        }

        /// <summary>
        /// Normalize a single point
        /// </summary>
        public static Point TransformPoint(double x, double y, Matrix transformMatrix)
        {
            return new Point(x, y) * transformMatrix;
        }

        /// <summary>
        /// Normalize a size
        /// </summary>
        public static (double, double) TransformSize(double x, double y, Matrix transformMatrix)
        {
            var transx = x * transformMatrix.M11 + y * transformMatrix.M21;
            var transy = y * transformMatrix.M22 + x * transformMatrix.M12;

            return (Math.Abs(transx), Math.Abs(transy));
        }

        /// <summary>
        /// Transform a single value based on the overall scale of the matrix
        /// </summary>
        public static double TransformScale(double val, Matrix transformMatrix)
        {
            // One of the important goals of the ShapeConverter is not to have the need for transformations at runtime.
            // That means all transformations need to be incorporated into each single coordinate. That in turn
            // means that shapes with unbalanced scaling (x and y scale are not the same) cannot be displayed correctly.
            // This is a known limitation of the approach and is acceptable because it is not expected that small 
            // logos and icons are that complex. Not having transformation at runtime takes precedence.
            // If the scaling is unbalanced it's clear at this point we cannot make it right. But here we need
            // a single scaling factor (e.g. for the thickness of a pen). We try our best and determine the x AND y
            // scale factor by getting the length of a normalized horizontal and vertical vector. In case of a
            // balanced scaling both length values are the same - otherwise we take the bigger one. That doesn't
            // matter much the display isn't anway 100% correct.

            // size of the vector (1,0)
            var scaleX = Math.Sqrt(transformMatrix.M11 * transformMatrix.M11 + transformMatrix.M12 * transformMatrix.M12);

            // size of the vector (0,1)
            var scaleY = Math.Sqrt(transformMatrix.M22 * transformMatrix.M22 + transformMatrix.M21 * transformMatrix.M21);

            return val * Math.Max(scaleX, scaleY);
        }
    }
}
