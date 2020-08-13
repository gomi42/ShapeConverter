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

using ShapeConverter.Parser.StreamGeometry;
using ShapeConverter.Shell.CommonViews;
using ShapeConverter.Shell.MVVM;
using System.Windows.Media;

namespace ShapeConverter.Shell.StreamConverter
{
    /// <summary>
    /// One entry in the fill/stroke ComboBox selection
    /// </summary>
    public class FillStrokeItem
    {
        public string Label { get; set; }
        
        public bool Fill { get; set; }
        
        public bool Stroke { get; set; }
    }

    /// <summary>
    /// The StreamConverterViewModel
    /// </summary>
    public class StreamConverterViewModel : ViewModelBase
    {
        private string streamInput;
        private bool showError;

        public StreamConverterViewModel()
        {
            PreviewViewModel = new PreviewViewModel();
            ResourceViewModel = new ResourceViewModel();
            XamlViewModel = new XamlViewModel();
            CSharpViewModel = new CSharpViewModel();
            ExportViewModel = new ExportViewModel();
        }

        public PreviewViewModel PreviewViewModel { get; set; }

        public ResourceViewModel ResourceViewModel { get; set; }

        public XamlViewModel XamlViewModel { get; set; }
        
        public CSharpViewModel CSharpViewModel { get; set; }

        public ExportViewModel ExportViewModel { get; set; }

        public string StreamInput
        {
            get 
            {
                return streamInput; 
            }

            set
            {
                streamInput = value; 
                NotifyPropertyChanged();
                CreatePaths(streamInput);
            }
        }

        public bool ShowError
        {
            get 
            {
                return showError; 
            }

            set
            {
                showError = value; 
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Parse and display the new input stream
        /// </summary>
        private void CreatePaths(string stream)
        {
            GraphicPath path;

            try
            {
                var streamGeometryParser = new StreamGeometryParser();
                var graphicPathGeometry = streamGeometryParser.ParseGeometry(stream);

                path = new GraphicPath();
                path.Geometry = graphicPathGeometry;
                path.FillBrush = new GraphicSolidColorBrush { Color = Color.FromRgb(128, 128, 128) };

                ShowError = false;
            }
            catch
            {
                path = null;
                ShowError = true;
            }

            PreviewViewModel.SetNewGraphicPath(path);
            ResourceViewModel.SetNewGraphicVisual(path);
            XamlViewModel.SetNewGraphicVisual(path);
            CSharpViewModel.SetNewGraphicVisual(path);
            ExportViewModel.SetNewGraphicVisual(path);
        }
    }
}
