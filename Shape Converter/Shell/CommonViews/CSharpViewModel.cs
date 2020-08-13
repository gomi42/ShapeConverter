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
using ShapeConverter.BusinessLogic.Generators.GeometryCSharpSourceGenerator;
using ShapeConverter.Shell.MVVM;

namespace ShapeConverter.Shell.CommonViews
{
    /// <summary>
    /// One entry of the parameter selection ComboBox
    /// </summary>
    public class CSharpParameterItem
    {
        public string Label { get; set; }

        public NormalizeGeometrySourceAspect NormalizeAspect { get; set; }
    }

    /// <summary>
    /// The CSharpViewModel
    /// </summary>
    public class CSharpViewModel : ViewModelBase
    {
        private GraphicVisual selectedVisual;
        private string filename = string.Empty;
        private string sourceCode;
        private CSharpParameterItem selectedParameterItem;
        private GeometryTypeItem selectedGeometryType;
        private bool includeOffset;
        private bool resetView;

        /// <summary>
        /// Constructor
        /// </summary>
        public CSharpViewModel()
        {
            var cSharpParameterItems = new List<CSharpParameterItem>();
            cSharpParameterItems.Add(new CSharpParameterItem { Label = "Height", NormalizeAspect = NormalizeGeometrySourceAspect.Height });
            cSharpParameterItems.Add(new CSharpParameterItem { Label = "Width", NormalizeAspect = NormalizeGeometrySourceAspect.Width });
            cSharpParameterItems.Add(new CSharpParameterItem { Label = "Width and Height", NormalizeAspect = NormalizeGeometrySourceAspect.Individual });
            ParameterItems = cSharpParameterItems;
            SelectedParameterItem = ParameterItems[0];

            var geometryTypes = new List<GeometryTypeItem>();
            geometryTypes.Add(new GeometryTypeItem { Label = "Stream", GeometryGeneratorType = GeometryGeneratorType.Stream });
            geometryTypes.Add(new GeometryTypeItem { Label = "PathGeometry", GeometryGeneratorType = GeometryGeneratorType.PathGeometry });
            GeometryTypes = geometryTypes;
            SelectedGeometryType = GeometryTypes[0];

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

        public List<CSharpParameterItem> ParameterItems { get; set; }

        public CSharpParameterItem SelectedParameterItem
        {
            get
            {
                return selectedParameterItem;
            }

            set
            {
                selectedParameterItem = value;
                NotifyPropertyChanged();
                UpdateSourceCode();
            }
        }

        public List<GeometryTypeItem> GeometryTypes { get; set; }

        public GeometryTypeItem SelectedGeometryType
        {
            get
            {
                return selectedGeometryType;
            }

            set
            {
                selectedGeometryType = value;
                NotifyPropertyChanged();
                UpdateSourceCode();
            }
        }

        public bool IncludeOffset
        {
            get
            {
                return includeOffset; 
            }

            set
            {
                includeOffset = value; 
                NotifyPropertyChanged();
                UpdateSourceCode();
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

        public DelegateCommand CopyToClipboard { get; set; }

        public void Reset()
        {
            ResetView = true;
        }

        public void SetFilename(string filename)
        {
            this.filename = filename;
        }

        public void SetNewGraphicVisual(GraphicVisual visual)
        {
            selectedVisual = visual;

            if (visual == null)
            {
                SourceCode = null;
            }

            UpdateSourceCode();
        }

        /// <summary>
        /// Update the geometry source code
        /// </summary>
        private void UpdateSourceCode()
        {
            if (selectedVisual == null)
            {
                return;
            }

            SourceCode = GeometryCSharpSourceGenerator.GenerateSource(selectedVisual,
                                                                            SelectedGeometryType.GeometryGeneratorType,
                                                                            SelectedParameterItem.NormalizeAspect,
                                                                            IncludeOffset,
                                                                            filename);
        }

        /// <summary>
        /// Copy geometr C# code to the clipboard
        /// </summary>
        private void OnCopySourceCodeToClipboard()
        {
            Clipboard.SetText(SourceCode);
        }
    }
}
