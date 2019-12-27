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

using System.IO;
using ShapeConverter.BusinessLogic.Exporter;

namespace ShapeConverter.Shell.Exporter
{
    /// <summary>
    /// Export to several formats
    /// </summary>
    public static class VisualExporter
    {
        /// <summary>
        /// Export to an image format
        /// </summary>
        public static void Export(GraphicVisual visual, int width, bool addMargin, out string message)
        {
            message = string.Empty;
            var saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.Filter = "ICO File (*.ico)|*.ico|SVG File (*.svg)|*.svg|PNG File (*.png)|*.png|JPG File (*.jpg)|*.jpg|TIFF File (*.tiff)|*.tiff|BMP File (*.bmp)|*.bmp|GIF File (*.gif)|*.gif|EPS File (*.eps)|*.eps";

            var result = saveDialog.ShowDialog();

            if (result == false)
            {
                return;
            }

            var fileExtension = Path.GetExtension(saveDialog.FileName).ToLower();

            switch (fileExtension)
            {
                case ".svg":
                    SvgExporter.ExportSvg(visual, width, saveDialog.FileName);
                    break;

                case ".ico":
                    IcoExporter.ExportIco(visual, saveDialog.FileName);
                    break;

                case ".eps":
                {
                    var exporter = new EpsExporter();
                    exporter.Export(visual, width, saveDialog.FileName, out message);
                    break;
                }

                default:
                    ImageExporter.ExportImage(visual, width, addMargin, saveDialog.FileName);
                    break;
            }
        }
    }
}
