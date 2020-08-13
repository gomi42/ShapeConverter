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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.Shell.MVVM;
using ShapeConverter.Shell.CommonViews;


namespace ShapeConverter.Shell.FileConverter
{
    public class PreviewViewModel :ViewModelBase
    {
        private Brush preview;
        private string colorPrecisionMessage;

        public Brush Preview
        {
            get 
            {
                return preview; 
            }

            set
            {
                preview = value;
                NotifyPropertyChanged();
            }
        }

        public string ColorPrecisionMessage
        {
            get
            {
                return colorPrecisionMessage;
            }

            set
            {
                colorPrecisionMessage = value;
                NotifyPropertyChanged();
            }
        }

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
