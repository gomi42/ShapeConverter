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

using System.Globalization;
using System.Text;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Exporter
{
    /// <summary>
    /// Export to several formats
    /// </summary>
    public static class SvgExporter
    {
        /// <summary>
        /// Export to an image format
        /// </summary>
        public static void ExportSvg(GraphicVisual visual, int width, string filename)
        {
            XNamespace ns = "http://www.w3.org/2000/svg";
            XElement root = new XElement(ns + "svg");
            root.Add(new XAttribute("Version", "1.1"));

            var normalizer = new NormalizeVisual();
            var normalizedVisual = normalizer.Normalize(visual, NormalizeAspect.Width, width);

            double height = normalizer.AspectRatio * width;
            root.Add(new XAttribute("viewBox", $"0 0 {width} {height}"));

            XElement definitions = new XElement(ns + "defs");
            int definitionsCount = 0;
            var element = Generate(normalizedVisual, ns, definitions, ref definitionsCount);

            if (definitions.HasElements)
            {
                root.Add(definitions);
            }

            root.Add(element);

            using (var writer =
                XmlWriter.Create(filename, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }))
            {
                root.Save(writer);
            }
        }

        /// <summary>
        /// Generate a visual recursively to xml
        /// </summary>
        private static XElement Generate(GraphicVisual visual, XNamespace ns, XElement definitions, ref int definitionsCount)
        {
            XElement element = null;

            switch (visual)
            {
                case GraphicGroup group:
                {
                    element = new XElement(ns + "g");

                    if (!DoubleUtilities.IsEqual(group.Opacity, 1))
                    {
                        element.Add(new XAttribute("opacity", DoubleUtilities.FormatString(group.Opacity)));
                    }

                    if (group.Clip != null)
                    {
                        var clipElement = new XElement(ns + "clipPath");
                        definitions.Add(clipElement);

                        definitionsCount++;
                        string defId = $"clip{definitionsCount}";

                        element.Add(new XAttribute("clip-path", $"url(#{defId})"));
                        clipElement.Add(new XAttribute("id", defId));

                        var pathElement = new XElement(ns + "path");
                        clipElement.Add(pathElement);

                        var pathStr = StreamSourceGenerator.GenerateStreamGeometry(group.Clip, false);
                        pathElement.Add(new XAttribute("d", pathStr));

                    }

                    foreach (var childVisual in group.Childreen)
                    {
                        var path = Generate(childVisual, ns, definitions, ref definitionsCount);
                        element.Add(path);
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    element = GeneratePath(graphicPath, ns, definitions, ref definitionsCount);

                    break;
                }
            }

            return element;
        }

        /// <summary>
        /// Generate a shape
        /// </summary>
        private static XElement GeneratePath(GraphicPath graphicPath, XNamespace ns, XElement definitions, ref int definitionsCount)
        {
            var pathElement = new XElement(ns + "path");

            var pathStr = StreamSourceGenerator.GenerateStreamGeometry(graphicPath.Geometry, false);
            pathElement.Add(new XAttribute("d", pathStr));

            SetColors(graphicPath, pathElement, ns, definitions, ref definitionsCount);

            return pathElement;
        }

        /// <summary>
        /// Set the colors
        /// </summary>
        private static void SetColors(GraphicPath graphicPath, XElement pathElement, XNamespace ns, XElement definitions, ref int definitionsCount)
        {
            if (graphicPath.FillBrush != null)
            {
                GenerateBrush(graphicPath.FillBrush, pathElement, ns, "fill", definitions, ref definitionsCount);
            }
            else
            {
                pathElement.Add(new XAttribute("fill", "none"));
            }

            if (graphicPath.StrokeBrush != null)
            {
                GenerateBrush(graphicPath.StrokeBrush, pathElement, ns, "stroke", definitions, ref definitionsCount);

                pathElement.Add(new XAttribute("stroke-width", DoubleUtilities.FormatString(graphicPath.StrokeThickness)));

                if (graphicPath.StrokeLineCap != GraphicLineCap.Flat)
                {
                    pathElement.Add(new XAttribute("stroke-linecap", ConvertToSvg(graphicPath.StrokeLineCap)));
                }

                if (graphicPath.StrokeLineJoin != GraphicLineJoin.Miter)
                {
                    pathElement.Add(new XAttribute("stroke-linejoin", ConvertToSvg(graphicPath.StrokeLineJoin)));
                }

                if (!DoubleUtilities.IsEqual(graphicPath.StrokeMiterLimit, 4))
                {
                    pathElement.Add(new XAttribute("stroke-miterlimit", DoubleUtilities.FormatString(graphicPath.StrokeMiterLimit)));
                }

                if (graphicPath.StrokeDashes != null)
                {
                    var result = new StringBuilder();

                    for (int i = 0; i < graphicPath.StrokeDashes.Count; i++)
                    {
                        if (i != 0)
                        {
                            result.Append(" ");
                        }

                        result.Append(DoubleUtilities.FormatString(graphicPath.StrokeDashes[i] * graphicPath.StrokeThickness));
                    }

                    pathElement.Add(new XAttribute("stroke-dasharray", result.ToString()));

                    if (!DoubleUtilities.IsZero(graphicPath.StrokeDashOffset))
                    {
                       pathElement.Add(new XAttribute("stroke-dashoffset", DoubleUtilities.FormatString(graphicPath.StrokeDashOffset * graphicPath.StrokeThickness)));
                    }
                }
            }
        }

        /// <summary>
        /// Converts a GraphicLineCap to the SVG value
        /// </summary>
        public static string ConvertToSvg(GraphicLineCap graphicLineCap)
        {
            string result = null;

            switch (graphicLineCap)
            {
                case GraphicLineCap.Flat:
                    result = "butt";
                    break;

                case GraphicLineCap.Round:
                    result = "round";
                    break;

                case GraphicLineCap.Square:
                    result = "round";
                    break;
            }

            return result;
        }

        /// <summary>
        /// Converts a GraphicLineJoin to the SVG value
        /// </summary>
        private static string ConvertToSvg(GraphicLineJoin graphicLineJoin)
        {
            string result = null;

            switch (graphicLineJoin)
            {
                case GraphicLineJoin.Miter:
                    result = "miter-clip";
                    break;

                case GraphicLineJoin.Bevel:
                    result = "bevel";
                    break;

                case GraphicLineJoin.Round:
                    result = "round";
                    break;
            }

            return result;
        }

        /// <summary>
        /// Generate a brush
        /// </summary>
        private static void GenerateBrush(GraphicBrush graphicBrush, XElement pathElement, XNamespace ns, string property, XElement definitions, ref int definitionsCount)
        {
            switch (graphicBrush)
            {
                case GraphicSolidColorBrush graphicSolidColor:
                {
                    var color = graphicSolidColor.Color;
                    pathElement.Add(new XAttribute(property, FormatColor(color)));

                    var alpha = color.A;

                    if (alpha != 255)
                    {
                        pathElement.Add(new XAttribute($"{property}-opacity", DoubleUtilities.FormatString(alpha / 255.0)));
                    }
                    break;
                }

                case GraphicLinearGradientBrush linearGradientBrush:
                {
                    var gradientElement = new XElement(ns + "linearGradient");
                    definitions.Add(gradientElement);

                    definitionsCount++;
                    string defId = $"lingrad{definitionsCount}";

                    gradientElement.Add(new XAttribute("id", defId));
                    pathElement.Add(new XAttribute(property, $"url(#{defId})"));

                    gradientElement.Add(new XAttribute("x1", DoubleUtilities.FormatString(linearGradientBrush.StartPoint.X)));
                    gradientElement.Add(new XAttribute("y1", DoubleUtilities.FormatString(linearGradientBrush.StartPoint.Y)));

                    gradientElement.Add(new XAttribute("x2", DoubleUtilities.FormatString(linearGradientBrush.EndPoint.X)));
                    gradientElement.Add(new XAttribute("y2", DoubleUtilities.FormatString(linearGradientBrush.EndPoint.Y)));

                    foreach (var stop in linearGradientBrush.GradientStops)
                    {
                        var stopElement = new XElement(ns + "stop");
                        stopElement.Add(new XAttribute("offset", DoubleUtilities.FormatString(stop.Position)));
                        stopElement.Add(new XAttribute("stop-color", FormatColor(stop.Color)));

                        var alpha = stop.Color.A;

                        if (alpha != 255)
                        {
                            stopElement.Add(new XAttribute("stop-opacity", DoubleUtilities.FormatString(alpha / 255.0)));
                        }

                        gradientElement.Add(stopElement);
                    }

                    break;
                }

                case GraphicRadialGradientBrush radialGradientBrush:
                {
                    var gradientElement = new XElement(ns + "radialGradient");
                    definitions.Add(gradientElement);

                    definitionsCount++;
                    string defId = $"radgrad{definitionsCount}";

                    gradientElement.Add(new XAttribute("id", defId));
                    pathElement.Add(new XAttribute(property, $"url(#{defId})"));

                    gradientElement.Add(new XAttribute("fx", DoubleUtilities.FormatString(radialGradientBrush.StartPoint.X)));
                    gradientElement.Add(new XAttribute("fy", DoubleUtilities.FormatString(radialGradientBrush.StartPoint.Y)));

                    gradientElement.Add(new XAttribute("cx", DoubleUtilities.FormatString(radialGradientBrush.EndPoint.X)));
                    gradientElement.Add(new XAttribute("cy", DoubleUtilities.FormatString(radialGradientBrush.EndPoint.Y)));

                    gradientElement.Add(new XAttribute("r", DoubleUtilities.FormatString((radialGradientBrush.RadiusX + radialGradientBrush.RadiusY) / 2)));

                    foreach (var stop in radialGradientBrush.GradientStops)
                    {
                        var stopElement = new XElement(ns + "stop");
                        stopElement.Add(new XAttribute("offset", DoubleUtilities.FormatString(stop.Position)));
                        stopElement.Add(new XAttribute("stop-color", FormatColor(stop.Color)));

                        var alpha = stop.Color.A;

                        if (alpha != 255)
                        {
                            stopElement.Add(new XAttribute("stop-opacity", DoubleUtilities.FormatString(alpha / 255.0)));
                        }

                        gradientElement.Add(stopElement);
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Format a color
        /// </summary>
        private static string FormatColor(Color color)
        {
            return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }
    }
}
