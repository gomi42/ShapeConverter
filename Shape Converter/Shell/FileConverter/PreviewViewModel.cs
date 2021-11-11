//
// Author:
//   Michael Göricke
//
// Copyright (c) 2020
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
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.Shell.MVVM;


namespace ShapeConverter.Shell.FileConverter
{
    /// <summary>
    /// The PreviewViewModel
    /// </summary>
    public class PreviewViewModel :ViewModelBase
    {
        private Brush preview;
        private string colorPrecisionMessage;

        /// <summary>
        /// The preview of the shape as a brush
        /// </summary>
        public Brush Preview
        {
            get => preview;
            set => SetProperty(ref preview, value);
        }

        /// <summary>
        /// Show color precision warning message
        /// </summary>
        public string ColorPrecisionMessage
        {
            get => colorPrecisionMessage;
            set => SetProperty(ref colorPrecisionMessage, value);
        }

        /// <summary>
        /// Set a new visual
        /// </summary>
        public void SetNewGraphicVisual(GraphicVisual visual, GraphicColorPrecision? colorPrecision = null)
        {
            if (visual == null)
            {
                ColorPrecisionMessage = null;
                Preview = null;
                return;
            }

            ColorPrecisionMessage = CommonViews.Helper.GetColorPrecisionText(colorPrecision);
            Preview = DrawingBrushBinaryGenerator.Generate(visual);
        }
    }
}
