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
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShapeConverter.BusinessLogic.Generators;

namespace ShapeConverter.BusinessLogic.Exporter
{
    internal static class IcoExporter
    {
        /// <summary>
        /// Export to an .ICO format
        /// </summary>
        public static void ExportIco(GraphicVisual visual, string filename)
        {
            try
            {
                int[] resolutions = { 16, 32, 64, 128, 256 };
                FileStream file = new FileStream(filename, FileMode.Create);
                BinaryWriter binWriter = new BinaryWriter(file);

                List<long> offsets = new List<long>();
                // icon dir

                // must be 0
                Int16 int16 = 0;
                binWriter.Write(int16);

                // type
                int16 = 1;
                binWriter.Write(int16);

                // # of images
                int16 = (Int16)resolutions.Length;
                binWriter.Write(int16);

                // ICONDIRENTRY 

                foreach (var resolution in resolutions)
                {
                    // width
                    byte int8 = (byte)(resolution == 256 ? 0 : resolution);
                    binWriter.Write(int8);

                    // height
                    binWriter.Write(int8);

                    // # colors in palette
                    int8 = 0;
                    binWriter.Write(int8);

                    // reserved => 0
                    binWriter.Write(int8);

                    // # color planes
                    int16 = 1;
                    binWriter.Write(int16);

                    // # bit per pixel
                    int16 = 0;
                    binWriter.Write(int16);

                    offsets.Add(file.Position);
                    UInt32 uint32 = 0;

                    // size of the image
                    binWriter.Write(uint32);

                    // offset of the image in file
                    binWriter.Write(uint32);
                }

                int resIndex = 0;

                foreach (var resolution in resolutions)
                {
                    var startPosition = file.Position;

                    var normalizer = new NormalizeVisual();
                    var normalizedVisual = normalizer.Normalize(visual, NormalizeAspect.Both, resolution);

                    double height = normalizer.AspectRatio * resolution;
                    var drawingBrush = DrawingBrushBinaryGenerator.Generate(normalizedVisual);

                    DrawingVisual viz = new DrawingVisual();

                    using (DrawingContext dc = viz.RenderOpen())
                    {
                        dc.DrawRectangle(drawingBrush, null, new Rect(0, 0, resolution, resolution));
                    }

                    RenderTargetBitmap bmp =
                        new RenderTargetBitmap((int)resolution, (int)resolution, // Size
                            96, 96, // DPI           
                            PixelFormats.Pbgra32);

                    bmp.Render(viz);
                    BitmapEncoder encoder = null;
                    encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));

                    encoder.Save(file);
                    var currentPosition = file.Position;

                    var size = currentPosition - startPosition;
                    file.Position = offsets[resIndex];

                    // patch size
                    var uint32 = (UInt32)size;
                    binWriter.Write(uint32);

                    // patch offset of the image
                    uint32 = (UInt32)startPosition;
                    binWriter.Write(uint32);

                    // set position back to the end
                    file.Position = currentPosition;
                    resIndex++;
                }

                file.Close();
                file.Dispose();
            }
            catch
            {
            }
        }
    }
}
