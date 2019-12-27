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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ShapeConverter
{
    /// <summary>
    /// The UniformTabPanel positions the TabItems on a pixel boundary in 
    /// order to avoid unsharp shapes
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Panel" />
    public class UniformTabPanel : Panel
    {
        /// <summary>
        /// Measures the override.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            if (InternalChildren.Count == 0)
            {
                return (new Size(3000, 42));
            }

            int numVisibleChildreen = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    numVisibleChildreen++;
                }
            }

            int width = (int)(constraint.Width / numVisibleChildreen);
            Size childConstraint = new Size(width, constraint.Height);
            double height = 42;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    child.Measure(childConstraint);
                    height = child.DesiredSize.Height;
                }
            }

            return new Size(constraint.Width, height);
        }

        /// <summary>
        /// Arranges the override.
        /// </summary>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            List<UIElement> visibleChildreen = new List<UIElement>();

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    visibleChildreen.Add(child);
                }
            }

            if (visibleChildreen.Count == 0)
            {
                return (arrangeSize);
            }

            int width = (int)(arrangeSize.Width / visibleChildreen.Count);
            int delta = (int)(arrangeSize.Width - width * visibleChildreen.Count);

            double x = 0;

            for (int i = 0; i < visibleChildreen.Count; i++)
            {
                UIElement child = visibleChildreen[i];

                int width2 = width;

                if (delta > 0)
                {
                    width2++;
                    delta--;
                }

                child.Arrange(new Rect(x, 0, width2, arrangeSize.Height));
                x += width2;
            }

            return (arrangeSize);
        }
    }
}
