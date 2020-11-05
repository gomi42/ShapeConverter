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

using System.Linq;
using System.Windows;
using Ntreev.Library.Psd;
using Ntreev.Library.Psd.Readers.LayerResources;
using ShapeConverter.Helper;
using ShapeConverter.Parser.Psd.psd_parser.Readers.ImageResources;

namespace ShapeConverter.Parser.Psd
{
    /// <summary>
    /// The PSD parser
    /// </summary>
    internal class PsdParser : IFileParser
    {
        private ColorSpaceManager colorManager;

        /// <summary>
        /// Parse the given file
        /// </summary>
        GraphicVisual IFileParser.Parse(string filename)
        {
            colorManager = new ColorSpaceManager();
            var group = new GraphicGroup();

            using (PsdDocument document = PsdDocument.Create(filename))
            {
                Size size = new Size(document.Width, document.Height);

                PrepareIccProfile(document);
                ParseLayers(document, size, group);
            }

            CommonHelper.CleanUpTempDir();
            return group;
        }

        /// <summary>
        /// Prepares the icc profile.
        /// </summary>
        void PrepareIccProfile(PsdDocument document)
        {
            var profileResource = document.ImageResources.FirstOrDefault(x => x is IccProfileResource) as IccProfileResource;

            if (profileResource != null)
            {
                colorManager.SetProfile(profileResource.ProfileData);
            }
        }

        /// <summary>
        /// Parse all layers recursively
        /// </summary>
        void ParseLayers(IPsdLayer layer, Size size, GraphicGroup group)
        {
            if (!layer.IsVisible)
            {
                return;
            }

            if (layer.Childreen.Length > 0)
            {
                foreach (var child in layer.Childreen)
                {
                    ParseLayers(child, size, group);
                }
            }
            else
            {
                var graphicPath = ParseLayer(layer, size);

                if (graphicPath != null)
                {
                    group.Children.Add(graphicPath);
                }
            }
        }

        /// <summary>
        /// Parse one layer
        /// </summary>
        private GraphicPath ParseLayer(IPsdLayer layer, Size size)
        {
            GraphicPath graphicPath = ParsePath(layer, size);

            if (graphicPath == null || graphicPath.Geometry.Segments.Count == 0)
            {
                return null;
            }

            ParseColors(layer, graphicPath);

            return graphicPath;
        }

        /// <summary>
        /// Get the path
        /// </summary>
        private GraphicPath ParsePath(IPsdLayer layer, Size size)
        {
            var graphicPath = new GraphicPath();

            foreach (var resource in layer.Resources)
            {
                switch (resource)
                {
                    // vector mask equals paths
                    // vsmsResource inherits from smsk: we don't need to add the extra 'case'
                    case vmskResource vmsk:
                    {
                        ReadBezier(vmsk.Beziers, graphicPath, size);
                        break;
                    }
                }
            }

            if (graphicPath.Geometry.Segments.Count == 0)
            {
                graphicPath = null;
            }

            return graphicPath;
        }

        /// <summary>
        /// Get the colors
        /// </summary>
        private void ParseColors(IPsdLayer layer, GraphicPath graphicPath)
        {
            var bounds = graphicPath.Geometry.Bounds;
            var aspectRatio = bounds.Height / bounds.Width;

            foreach (var resource in layer.Resources)
            {
                switch (resource)
                {
                    // backwards compatibility solid color
                    case vscgResource vscg:
                    {
                        var (color, colorPrecision) = colorManager.GetBrush(vscg.Color, aspectRatio, false);
                        graphicPath.FillBrush = color;
                        graphicPath.ColorPrecision = colorPrecision;
                        break;
                    }

                    // solid color
                    case SoCoResource soCo:
                    {
                        var (color, colorPrecision) = colorManager.GetBrush(soCo.Color, aspectRatio, false);
                        graphicPath.FillBrush = color;
                        graphicPath.ColorPrecision = colorPrecision;
                        break;
                    }

                    // pattern fill
                    case PtFlResource ptFl:
                    {
                        var (color, colorPrecision) = colorManager.GetBrush(ptFl.Color, aspectRatio, false);
                        graphicPath.FillBrush = color;
                        graphicPath.ColorPrecision = colorPrecision;
                        break;
                    }

                    // gradient fill
                    case GdFlResource gdFl:
                    {
                        var (color, colorPrecision) = colorManager.GetBrush(gdFl.Color, aspectRatio, false);
                        graphicPath.FillBrush = color;
                        graphicPath.ColorPrecision = colorPrecision;
                        break;
                    }

                    // stroke infos
                    case vstkResource vstk:
                    {
                        var (color, colorPrecision) = colorManager.GetBrush(vstk.Color, aspectRatio, true);
                        graphicPath.StrokeBrush = color;
                        graphicPath.StrokeThickness = vstk.Width;
                        graphicPath.ColorPrecision = colorPrecision;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Convert a path made of PSD bezier records to GraphicPath objects
        /// </summary>
        private void ReadBezier(BezierDescriptor[] psdBeziers, GraphicPath graphicPath, Size size)
        {
            bool closePath = false;

            Point firstPoint = new Point();
            Point lastControlPoint = new Point();
            Point firstControlPoint = new Point();
            bool isFirstKnot = false;

            foreach (var psdBezier in psdBeziers)
            {
                switch (psdBezier)
                {
                    case BezierSubpathRecord bezierSubpathRecord:
                    {
                        if (closePath)
                        {
                            var bezier = new GraphicCubicBezierSegment();
                            graphicPath.Geometry.Segments.Add(bezier);

                            bezier.ControlPoint1 = lastControlPoint;
                            bezier.ControlPoint2 = firstControlPoint;
                            bezier.EndPoint = firstPoint;
                        }

                        isFirstKnot = true;
                        closePath = bezierSubpathRecord.IsClosed;
                        break;
                    }

                    case BezierKnot bezierKnot:
                    {
                        if (isFirstKnot)
                        {
                            isFirstKnot = false;

                            firstPoint = new Point(bezierKnot.Point2X * size.Width, bezierKnot.Point2Y * size.Height);
                            var move = new GraphicMoveSegment { StartPoint = firstPoint };
                            move.IsClosed = closePath;

                            graphicPath.Geometry.Segments.Add(move);

                            lastControlPoint = new Point(bezierKnot.Point3X * size.Width, bezierKnot.Point3Y * size.Height);
                            firstControlPoint = new Point(bezierKnot.Point1X * size.Width, bezierKnot.Point1Y * size.Height);
                        }
                        else
                        {
                            var bezier = new GraphicCubicBezierSegment();
                            graphicPath.Geometry.Segments.Add(bezier);

                            bezier.ControlPoint1 = lastControlPoint;
                            bezier.ControlPoint2 = new Point(bezierKnot.Point1X * size.Width, bezierKnot.Point1Y * size.Height);
                            bezier.EndPoint = new Point(bezierKnot.Point2X * size.Width, bezierKnot.Point2Y * size.Height);

                            lastControlPoint = new Point(bezierKnot.Point3X * size.Width, bezierKnot.Point3Y * size.Height);
                        }
                        break;
                    }

                    default:
                        break;
                }
            }

            if (closePath)
            {
                var bezier = new GraphicCubicBezierSegment();
                graphicPath.Geometry.Segments.Add(bezier);

                bezier.ControlPoint1 = lastControlPoint;
                bezier.ControlPoint2 = firstControlPoint;
                bezier.EndPoint = firstPoint;
            }
        }
    }
}
