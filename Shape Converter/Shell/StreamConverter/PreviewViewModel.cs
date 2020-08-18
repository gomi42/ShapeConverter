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

using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.Shell.CommonViews;
using ShapeConverter.Shell.MVVM;
using System.Collections.Generic;
using System.Windows.Media;

namespace ShapeConverter.Shell.StreamConverter
{
    /// <summary>
    /// One entry in the fill/stroke ComboBox selection
    /// </summary>
    public class FillStrokeItem
    {
        /// <summary>
        /// the label to display
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// true if the shape is to be shown filled
        /// </summary>
        public bool Fill { get; set; }

        /// <summary>
        /// true if the shape is to be shown stroked
        /// </summary>
        public bool Stroke { get; set; }
    }
    
    /// <summary>
    /// The PreviewViewModel
    /// </summary>
    public class PreviewViewModel : ViewModelBase
    {
        private Geometry preview;
        private List<FillStrokeItem> fillStrokeItems;
        private FillStrokeItem selectedFillStrokeItem;
        private Brush previewFill;
        private Brush previewStroke;
        private double previewStrokeThickness;

        /// <summary>
        /// Constructor
        /// </summary>
        public PreviewViewModel()
        {
            var fillStrokeItems = new List<FillStrokeItem>();
            fillStrokeItems.Add(new FillStrokeItem { Label = "fill", Fill = true, Stroke = false });
            fillStrokeItems.Add(new FillStrokeItem { Label = "stroke", Fill = false, Stroke = true });
            fillStrokeItems.Add(new FillStrokeItem { Label = "filled and stroked", Fill = true, Stroke = true });
            FillStrokeItems = fillStrokeItems;
            SelectedFillStrokeItem = FillStrokeItems[0];
        }

        /// <summary>
        /// The preview of the shape as a Geometry
        /// </summary>
        public Geometry Preview
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

        /// <summary>
        /// List of visualization items of the shape
        /// </summary>
        public List<FillStrokeItem> FillStrokeItems
        {
            get
            {
                return fillStrokeItems;
            }

            set
            {
                fillStrokeItems = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Selected visualization of the shape
        /// </summary>
        public FillStrokeItem SelectedFillStrokeItem
        {
            get
            {
                return selectedFillStrokeItem;
            }

            set
            {
                selectedFillStrokeItem = value;
                NotifyPropertyChanged();
                UpdatePreviewAll();
            }
        }

        /// <summary>
        /// The fill brush of the preview
        /// </summary>
        public Brush PreviewFill
        {
            get
            {
                return previewFill;
            }

            set
            {
                previewFill = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// The stroke brush of the preview
        /// </summary>
        public Brush PreviewStroke
        {
            get
            {
                return previewStroke;
            }

            set
            {
                previewStroke = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// The stroke thickness of the preview
        /// </summary>
        public double PreviewStrokeThickness
        {
            get
            {
                return previewStrokeThickness;
            }

            set
            {
                previewStrokeThickness = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Set a new visual
        /// </summary>
        public void SetNewGraphicPath(GraphicPath path)
        {
            if (path == null)
            {
                Preview = null;
            }

            var geometry = GeometryBinaryGenerator.GenerateGeometry(path.Geometry);
            Preview = geometry;
            UpdatePreviewAll();
        }

        /// <summary>
        /// Update the preview
        /// </summary>
        private void UpdatePreviewAll()
        {
            UpdatePreviewFill();
            UpdatePreviewStroke();
        }

        /// <summary>
        /// Update the fill of the preview
        /// </summary>
        private void UpdatePreviewFill()
        {
            if (SelectedFillStrokeItem.Fill)
            {
                PreviewFill = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
            }
            else
            {
                PreviewFill = null;
            }
        }

        /// <summary>
        /// Update the stroke of the preview
        /// </summary>
        private void UpdatePreviewStroke()
        {
            if (SelectedFillStrokeItem.Stroke)
            {
                PreviewStroke = new SolidColorBrush(Color.FromRgb(0x0, 0x0, 0x0));
                PreviewStrokeThickness = 2;
            }
            else
            {
                PreviewStroke = null;
                PreviewStrokeThickness = 0;
            }
        }
    }
}
