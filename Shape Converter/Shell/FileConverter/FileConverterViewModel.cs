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
using System.Linq;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.Parser;
using ShapeConverter.Shell.CommonViews;
using ShapeConverter.Shell.MVVM;

namespace ShapeConverter.Shell.FileConverter
{
    public class FileConverterViewModel : ViewModelBase
    {
        private FileParser fileParser;
        private List<PreviewShapeViewModel> previewIcons = new List<PreviewShapeViewModel>();
        private GraphicVisual graphicVisual;
        private GraphicVisual selectedVisual;
        private string filename;
        private bool showError;
        private bool selectionChangedFromCode;

        public FileConverterViewModel()
        {
            PreviewViewModel = new PreviewViewModel();
            ResourceViewModel = new ResourceViewModel();
            XamlViewModel = new XamlViewModel();
            CSharpViewModel = new CSharpViewModel();
            ExportViewModel = new ExportViewModel();

            SelectFile = new DelegateCommand(OnSelectFile);
            SelectAll = new DelegateCommand(OnSelectAll);

            fileParser = new FileParser();
            fileParser.Init();
        }

        public PreviewViewModel PreviewViewModel { get; set; }

        public ResourceViewModel ResourceViewModel { get; set; }

        public XamlViewModel XamlViewModel { get; set; }

        public CSharpViewModel CSharpViewModel { get; set; }

        public ExportViewModel ExportViewModel { get; set; }

        public DelegateCommand SelectFile { get; set; }

        public string Filename
        {
            get
            {
                return filename;
            }

            set
            {
                filename = value;
                NotifyPropertyChanged();
                ShowPaths(filename);
            }
        }

        public List<PreviewShapeViewModel> PreviewIcons
        {
            get
            {
                return previewIcons;
            }

            set
            {
                previewIcons = value;
                NotifyPropertyChanged();
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

        public DelegateCommand SelectAll { get; set; }

        /// <summary>
        /// Open the file selection dialogbox
        /// </summary>
        private void OnSelectFile()
        {
            var exts = fileParser.GetSupportedFileExtensions();

            var filterFormat = exts.Select(x => $"*{x}");
            string descriptions = string.Join("; ", filterFormat);

            var openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = $"Vector Files ({descriptions})|{descriptions}|all files (*.*)|*.*";
            openDialog.Multiselect = false;

            var result = openDialog.ShowDialog();

            if (result == true)
            {
                Filename = openDialog.FileName;
            }
        }

        /// <summary>
        /// Add graphic paths to the graphic path selection listbox
        /// </summary>
        void ShowPaths(string filename)
        {
            this.filename = filename;

            graphicVisual = fileParser.Parse(filename);

            if (graphicVisual != null)
            {
                ShowError = false;
                CSharpViewModel.SetFilename(filename);
            }
            else
            {
                ShowError = true;
            }

            PrepSelectionList();
        }

        /// <summary>
        /// Prepare all data for the graphic path selection listbox
        /// </summary>
        void PrepSelectionList()
        {
            if (graphicVisual != null)
            {
                var previewIcons = new List<PreviewShapeViewModel>();
                int index = 0;

                PrepSelectionList(graphicVisual, previewIcons, ref index);
                PreviewIcons = previewIcons;

                if (previewIcons.Count > 0)
                {
                    //PathSelectionBox.ScrollIntoView(previewIcons[0]);
                    //StreamCode.ScrollToHome();
                    //DrawingBrushCode.ScrollToHome();
                    //GeometryCode.ScrollToHome();
                }

                SelectAllPreviewIcons(true);
            }
            else
            {
                PreviewIcons = null;
                PreviewViewModel.SetNewGraphicVisual(null);
                ResourceViewModel.SetNewGraphicVisual(null);
                XamlViewModel.SetNewGraphicVisual(null);
                CSharpViewModel.SetNewGraphicVisual(null);
                ExportViewModel.SetNewGraphicVisual(null);
            }
        }

        /// <summary>
        /// Get all graphic elements recursively
        /// </summary>
        void PrepSelectionList(GraphicVisual visual, List<PreviewShapeViewModel> pathVms, ref int index)
        {
            switch (visual)
            {
                case GraphicGroup group:
                {
                    foreach (var childVisual in group.Childreen)
                    {
                        PrepSelectionList(childVisual, pathVms, ref index);
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    var pathViewModel = new PreviewShapeViewModel(OnPreviewSelectionChanged);
                    pathViewModel.OriginalShape = graphicPath;
                    pathVms.Add(pathViewModel);

                    break;
                }
            }
        }

        /// <summary>
        /// Get all selected paths
        /// </summary>
        /// <returns>list with all selected paths</returns>
        private List<GraphicPath> GetSelectedPaths()
        {
            var paths = new List<GraphicPath>();

            foreach (var previewIcon in previewIcons)
            {
                if (previewIcon.IsSelected)
                {
                    paths.Add(previewIcon.OriginalShape);
                }
            }

            return paths;
        }

        /// <summary>
        /// Update all views with the new selection
        /// </summary>
        private void UpdateAll()
        {
            GraphicColorPrecision? colorPrecision = null;

            // get the selected paths
            selectedVisual = BuildSelectedDrawing(graphicVisual);

            if (selectedVisual != null)
            {
                colorPrecision = GetColorPrecision(selectedVisual);
            }

            PreviewViewModel.SetNewGraphicVisual(selectedVisual, colorPrecision);
            ResourceViewModel.SetNewGraphicVisual(selectedVisual, colorPrecision);
            XamlViewModel.SetNewGraphicVisual(selectedVisual, colorPrecision);
            CSharpViewModel.SetNewGraphicVisual(selectedVisual);
            ExportViewModel.SetNewGraphicVisual(selectedVisual);
        }

        /// <summary>
        /// Builds the selected visual.
        /// </summary>
        private GraphicVisual BuildSelectedDrawing(GraphicVisual visual)
        {
            var selectedShapes = GetSelectedPaths();
            GraphicVisual selectedVisual = null;

            if (visual != null)
            {
                selectedVisual = BuildSelectedGeometry(visual, selectedShapes);
                selectedVisual = OptimizeVisual.Optimize(selectedVisual);
            }

            return selectedVisual;
        }

        /// <summary>
        /// Builds the selected drawing recursively
        /// </summary>
        private GraphicVisual BuildSelectedGeometry(GraphicVisual visual, List<GraphicPath> selectedPaths)
        {
            GraphicVisual graphicVisual = null;

            switch (visual)
            {
                case GraphicGroup group:
                {
                    var graphicGroup = new GraphicGroup();
                    graphicVisual = graphicGroup;
                    graphicGroup.Opacity = group.Opacity;
                    graphicGroup.Clip = group.Clip;

                    foreach (var childVisual in group.Childreen)
                    {
                        var selectedGeometry = BuildSelectedGeometry(childVisual, selectedPaths);

                        if (selectedGeometry != null)
                        {
                            graphicGroup.Childreen.Add(selectedGeometry);
                        }
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    if (selectedPaths.Contains(graphicPath))
                    {
                        graphicVisual = graphicPath;
                    }

                    break;
                }
            }

            return graphicVisual;
        }

        /// <summary>
        /// Gets the lowest color precision of the specified visual
        /// </summary>
        private GraphicColorPrecision GetColorPrecision(GraphicVisual visualRoot)
        {
            GraphicColorPrecision GetColorPrecision(GraphicVisual geometry, GraphicColorPrecision precision)
            {
                GraphicColorPrecision result = precision;

                switch (geometry)
                {
                    case GraphicGroup group:
                    {
                        foreach (var graphicPath in group.Childreen)
                        {
                            result = GetColorPrecision(graphicPath, precision);
                        }

                        break;
                    }

                    case GraphicPath graphicPath:
                    {
                        if (graphicPath.ColorPrecision > precision)
                        {
                            result = graphicPath.ColorPrecision;
                        }

                        break;
                    }
                }

                return result;
            }

            ////////

            return GetColorPrecision(visualRoot, GraphicColorPrecision.Precise);
        }

        /// <summary>
        /// Selects all click.
        /// </summary>
        private void OnSelectAll()
        {
            var selectAll = previewIcons.Any(x => x.IsSelected == false);
            SelectAllPreviewIcons(selectAll);
        }

        /// <summary>
        /// Select all items in the listbox
        /// </summary>
        private void SelectAllPreviewIcons(bool selectAll)
        {
            selectionChangedFromCode = true;

            foreach (var previewIcon in previewIcons)
            {
                previewIcon.IsSelected = selectAll;
            }

            selectionChangedFromCode = false;
            UpdateAll();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnPreviewSelectionChanged()
        {
            if (!selectionChangedFromCode)
            {
                UpdateAll();
            }
        }
    }
}
