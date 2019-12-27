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
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShapeConverter.BusinessLogic.Generators;

namespace ShapeConverter.BusinessLogic.Exporter
{
    internal static class ImageExporter
    {
        public static void ExportImage(GraphicVisual visual, int width, bool addMargin, string filename)
        {
            // https://stackoverflow.com/questions/9080231/how-to-save-geometry-as-image

            var fileExtension = Path.GetExtension(filename).ToLower();

            var hasTransparentBackground = fileExtension == ".png";

            try
            {
                var normalizer = new NormalizeVisual();
                var paths = normalizer.Normalize(visual, NormalizeAspect.Width, width);

                double height = normalizer.AspectRatio * width;
                var drawingBrush = DrawingBrushBinaryGenerator.Generate(paths);

                double margin = 0;

                if (addMargin)
                {
                    margin = (width > height ? width : height) / 20.0;
                }

                double imageWidth = width + 2 * margin;
                double imageHeight = Math.Ceiling(height + 2 * margin);

                DrawingVisual viz = new DrawingVisual();

                using (DrawingContext dc = viz.RenderOpen())
                {
                    if (!hasTransparentBackground)
                    {
                        dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, imageWidth, imageHeight));
                    }

                    dc.DrawRectangle(drawingBrush, null, new Rect(margin, margin, width, height));
                }

                RenderTargetBitmap bmp =
                    new RenderTargetBitmap((int)imageWidth, (int)imageHeight, // Size
                        96, 96, // DPI           
                        PixelFormats.Pbgra32);

                bmp.Render(viz);
                BitmapEncoder encoder = null;

                switch (fileExtension)
                {
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;

                    case ".jpg":
                        encoder = new JpegBitmapEncoder();
                        break;

                    case ".tiff":
                        encoder = new TiffBitmapEncoder();
                        break;

                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;

                    case ".gif":
                        encoder = new GifBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(bmp));

                using (FileStream file = new FileStream(filename, FileMode.Create))
                {
                    encoder.Save(file);
                }
            }
            catch
            {
            }
        }
    }
}
