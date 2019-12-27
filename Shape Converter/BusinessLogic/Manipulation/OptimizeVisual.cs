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
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Generators
{
    /// <summary>
    /// The graphic visual optimizer
    /// </summary>
    internal static class OptimizeVisual
    {
        /// <summary>
        /// Remove all unneeded groups (neutral groups with only one child or 
        /// with clipping set that doesn't clip anything)
        /// Attention: Modifies the original visual
        /// </summary>
        public static GraphicVisual Optimize(GraphicVisual visual)
        {
            if (visual == null)
            {
                return null;
            }

            RemoveClipping(visual);
            return Optimize(visual, 0);
        }

        /// <summary>
        /// Remove all unneeded groups with clipping set that doesn't clip anything
        /// In sum this is a very, very expensive operation. But it helps to
        /// create cleaner and faster code without unnecessary clipping
        /// </summary>
        private static Geometry RemoveClipping(GraphicVisual visual)
        {
            Geometry geometry = null;

            switch (visual)
            {
                case GraphicGroup group:
                {
                    var childreenGeometry = new PathGeometry();
                    geometry = childreenGeometry;

                    foreach (var childVisual in group.Childreen)
                    {
                        var childgeometry = RemoveClipping(childVisual);
                        childreenGeometry.AddGeometry(childgeometry);
                    }

                    if (group.Clip != null)
                    {
                        var groupClipGeometry = GeometryBinaryGenerator.GenerateGeometry(group.Clip);
                        var intersection = groupClipGeometry.FillContainsWithDetail(childreenGeometry, 0.0001, ToleranceType.Absolute);

                        if (intersection == IntersectionDetail.FullyContains)
                        {
                            group.Clip = null;
                        }
                        else
                        if (intersection == IntersectionDetail.Intersects)
                        {
                            var pen1 = new Pen(Brushes.Black, 2);
                            groupClipGeometry = groupClipGeometry.GetWidenedPathGeometry(pen1);
                            groupClipGeometry = groupClipGeometry.GetOutlinedPathGeometry();

                            var pen2 = new Pen(Brushes.Black, 1);
                            childreenGeometry = childreenGeometry.GetWidenedPathGeometry(pen2);
                            childreenGeometry = childreenGeometry.GetOutlinedPathGeometry();

                            intersection = groupClipGeometry.FillContainsWithDetail(childreenGeometry, 0.0001, ToleranceType.Absolute);

                            if (intersection == IntersectionDetail.FullyContains)
                            {
                                group.Clip = null;
                            }
                        }
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    geometry = GeometryBinaryGenerator.GenerateGeometry(graphicPath.Geometry);
                    break;
                }
            }

            return geometry;
        }

        /// <summary>
        /// Remove all unneeded groups recursively
        /// </summary>
        private static GraphicVisual Optimize(GraphicVisual visual, int level)
        {
            GraphicVisual result = visual;

            if (visual is GraphicGroup group)
            {
                int i = 0;

                while (i < group.Childreen.Count)
                {
                    var childVisual = group.Childreen[i];
                    var newVisual = Optimize(childVisual, level + 1);

                    if (newVisual != null)
                    {
                        if (newVisual is GraphicGroup newGroup && newGroup.Clip == null && DoubleUtilities.IsEqual(newGroup.Opacity, 1.0))
                        {
                            foreach (var child in newGroup.Childreen)
                            {
                                group.Childreen.Insert(i, child);
                                i++;
                            }

                            group.Childreen.RemoveAt(i);
                        }
                        else
                        {
                            group.Childreen[i] = newVisual;
                            i++;
                        }
                    }
                    else
                    {
                        group.Childreen.RemoveAt(i);
                    }
                }

                if (group.Childreen.Count == 0)
                {
                    result = null;
                }
                else
                if (group.Childreen.Count == 1 && group.Clip == null && DoubleUtilities.IsEqual(group.Opacity, 1.0))
                {
                    result = group.Childreen[0];
                }
            }

            return result;
        }
    }
}
