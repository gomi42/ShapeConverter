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

using System.Windows;
using System.Windows.Media;

namespace ShapeConverter.BusinessLogic.Generators
{
    internal static class PreviewBrushGenerator
    {
        /// <summary>
        /// Create a WPF Brush used as preview.
        /// We want to show any shape even more or less invisible shapes. A shape is invisible when:
        /// - there is no fill color or the fill color is almost white
        /// - there is no stroke color or the stroke color is almost white
        /// - the stroke is too thin
        /// To judge the stroke thickness we normalize the shape to the size of the preview. This also
        /// normalizes the stroke thickness.
        /// </summary>
        public static Brush GeneratePreview(GraphicPath graphicPath, Size previewSize)
        {
            var normalizer = new NormalizeVisual();
            var normalizedPath = (GraphicPath)normalizer.Normalize(graphicPath, previewSize);

            if (!IsShapeVisible(normalizedPath))
            {
                byte gray = 0xD0;
                normalizedPath.StrokeBrush = new GraphicSolidColorBrush { Color = Color.FromRgb(gray, gray, gray) };
                normalizedPath.StrokeThickness = 1;
            }

            var brush = DrawingBrushBinaryGenerator.Generate(normalizedPath);

            return brush;
        }

        /// <summary>
        /// Set the fill and stroke colors
        /// </summary>
        private static bool IsShapeVisible(GraphicPath graphicPath)
        {
            bool isFillVisible = false;
            bool isStrokeVisible = false;

            if (graphicPath.FillBrush != null)
            {
                isFillVisible = IsBrushVisible(graphicPath.FillBrush);
            }

            if (graphicPath.StrokeBrush != null)
            {
                if (graphicPath.StrokeThickness > 0.1)
                {
                    isStrokeVisible = IsBrushVisible(graphicPath.StrokeBrush);
                }
            }

            return isFillVisible || isStrokeVisible;
        }

        /// <summary>
        /// Create a brush from the specified graphic brush
        /// </summary>
        private static bool IsBrushVisible(GraphicBrush graphicBrush)
        {
            bool isVisible = true;

            switch (graphicBrush)
            {
                case GraphicSolidColorBrush graphicSolidColor:
                {
                    if (!IsColorVisible(graphicSolidColor.Color))
                    {
                        isVisible = false;
                    }

                    break;
                }

                case GraphicLinearGradientBrush graphicLinearGradientBrush:
                {
                    foreach (var stop in graphicLinearGradientBrush.GradientStops)
                    {
                        if (!IsColorVisible(stop.Color))
                        {
                            isVisible = false;
                            break;
                        }
                    }

                    break;
                }

                case GraphicRadialGradientBrush graphicRadialGradientBrush:
                {
                    foreach (var stop in graphicRadialGradientBrush.GradientStops)
                    {
                        if (!IsColorVisible(stop.Color))
                        {
                            isVisible = false;
                            break;
                        }
                    }

                    break;
                }
            }

            return isVisible;
        }

        /// <summary>
        /// Determines whether the specified color is visible on a white background
        /// </summary>
        private static bool IsColorVisible(Color color)
        {
            const byte AlphaVisibilityBoundary = 30;
            const double LuminosityVisibilityBoundary = 0.95;

            if (color.A < AlphaVisibilityBoundary)
            {
                return false;
            }

            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = (r > g && r > b) ? r : (g > b) ? g : b;
            double min = (r < g && r < b) ? r : (g < b) ? g : b;

            double luminosity = (max + min) / 2.0f;

            return luminosity < LuminosityVisibilityBoundary;
        }
    }
}
