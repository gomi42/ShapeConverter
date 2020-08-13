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

using System.Collections.Generic;
using System.Windows;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.Shell.MVVM;

namespace ShapeConverter.Shell.CommonViews
{
    public enum ResourceGeometryGeneratorType
    {
        Stream,
        Geometry,
        PathGeometry
    }

    public class ResourceGeometryTypeItem
    {
        public string Label { get; set; }

        public ResourceGeometryGeneratorType GeometryGeneratorType { get; set; }
    }

    public class ResourceViewModel : ViewModelBase
    {
        private GraphicVisual selectedVisual;

        private string sourceCode;
        private bool normalize = true;
        private List<ResourceGeometryTypeItem> geometryTypeItems;
        private ResourceGeometryTypeItem selectedGeometryTypeItem;
        private string colorPrecisionMessage;
        private bool resetView;

        public ResourceViewModel()
        {
            var geometryCreationItems = new List<ResourceGeometryTypeItem>();
            geometryCreationItems.Add(new ResourceGeometryTypeItem { Label = "Stream", GeometryGeneratorType = ResourceGeometryGeneratorType.Stream });
            geometryCreationItems.Add(new ResourceGeometryTypeItem { Label = "Geometry", GeometryGeneratorType = ResourceGeometryGeneratorType.Geometry });
            geometryCreationItems.Add(new ResourceGeometryTypeItem { Label = "PathGeometry", GeometryGeneratorType = ResourceGeometryGeneratorType.PathGeometry });
            GeometryTypeItems = geometryCreationItems;
            SelectedGeometryTypeItem = GeometryTypeItems[0];

            CopyToClipboard = new DelegateCommand(OnCopySourceCodeToClipboard);
        }

        public bool ResetView
        {
            get
            {
                return resetView; 
            }

            set
            {
                resetView = value;
                NotifyPropertyChanged();
            }
        }

        public string SourceCode
        {
            get
            {
                return sourceCode;
            }

            set
            {
                sourceCode = value;
                NotifyPropertyChanged();
            }
        }

        public bool Normalize
        {
            get
            {
                return normalize;
            }

            set
            {
                normalize = value;
                NotifyPropertyChanged();
                UpdateSourceCode();
            }
        }

        public List<ResourceGeometryTypeItem> GeometryTypeItems
        {
            get
            {
                return geometryTypeItems;
            }

            set
            {
                geometryTypeItems = value;
                NotifyPropertyChanged();
            }
        }

        public ResourceGeometryTypeItem SelectedGeometryTypeItem
        {
            get
            {
                return selectedGeometryTypeItem;
            }

            set
            {
                selectedGeometryTypeItem = value;
                NotifyPropertyChanged();
                UpdateSourceCode();
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

        public DelegateCommand CopyToClipboard { get; set; }

        public void Reset()
        {
            ResetView = true;
        }

        public void SetNewGraphicVisual(GraphicVisual visual, GraphicColorPrecision? colorPrecision = null)
        {
            selectedVisual = visual;

            if (visual == null)
            {
                ColorPrecisionMessage = null;
                SourceCode = null;
            }

            ColorPrecisionMessage = Helper.GetColorPrecisionText(colorPrecision);
            UpdateSourceCode();
        }

        /// <summary>
        /// Update the resource source code
        /// </summary>
        private void UpdateSourceCode()
        {
            var path = selectedVisual;

            if (path == null)
            {
                SourceCode = string.Empty;
                return;
            }

            if (Normalize)
            {
                var normalizer = new NormalizeVisual();
                path = normalizer.Normalize(selectedVisual, NormalizeAspect.Both, 100);
            }

            switch (SelectedGeometryTypeItem.GeometryGeneratorType)
            {
                case ResourceGeometryGeneratorType.Stream:
                {
                    var streams = StreamSourceGenerator.GenerateStreamGeometries(path);
                    SourceCode = string.Join("\n", streams);
                    break;
                }

                case ResourceGeometryGeneratorType.Geometry:
                {
                    SourceCode = GeometrySourceGenerator.GenerateGeometry(path);
                    break;
                }

                case ResourceGeometryGeneratorType.PathGeometry:
                {
                    SourceCode = PathGeometrySourceGenerator.GeneratePathGeometry(path);
                    break;
                }
            }
        }

        /// <summary>
        /// Copy source code to the clipboard
        /// </summary>
        private void OnCopySourceCodeToClipboard()
        {
            Clipboard.SetText(SourceCode);
        }
    }
}
