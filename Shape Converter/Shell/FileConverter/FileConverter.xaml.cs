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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Generators.GeometrySourceGenerator;
using ShapeConverter.Parser;
using ShapeConverter.Shell.Exporter;
using ShapeConverter.Shell.FileConverter;

namespace ShapeConverter
{
    /// <summary>
    /// Interaction logic for IllustratorConverter.xaml
    /// At the moment we don't follow the MVVM paradigm that is more or less standard for
    /// WPF applications. The main focus of the ShapeConverter are the algorithms. And it
    /// turned out we don't need much UI logic to present the results. 
    /// </summary>
    public partial class FileConverter : UserControl
    {
        private FileParser fileParser;
        private List<PreviewShapeViewModel> previewIcons = new List<PreviewShapeViewModel>();
        private GraphicVisual graphicVisual;
        private GraphicVisual selectedVisual;
        private string filename;
        private bool filenameSetFromCode;
        private bool selectionChangedFromCode;
        private bool enableAllItems;

        /// <summary>
        /// Constructor
        /// </summary>
        public FileConverter()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            AllowDrop = true;

            fileParser = new FileParser();
            fileParser.Init();
        }

        /// <summary>
        /// Test whether drag and drop is allowed
        /// </summary>
        protected override void OnDragOver(DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 1)
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Accept dropping a file
        /// </summary>
        protected override void OnDrop(DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length != 1)
            {
                return;
            }

            filenameSetFromCode = true;
            FilenameTextBox.Text = files[0];
            ShowPaths(files[0]);
            filenameSetFromCode = false;
        }

        /// <summary>
        /// Open the file selection dialogbox
        /// </summary>
        private void OnFileSelect(object sender, RoutedEventArgs e)
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
                filenameSetFromCode = true;
                FilenameTextBox.Text = openDialog.FileName;
                ShowPaths(openDialog.FileName);
                filenameSetFromCode = false;
            }
        }

        /// <summary>
        /// Handle changed manual entry of the filename
        /// </summary>
        private void OnFilenameChanged(object sender, TextChangedEventArgs e)
        {
            if (!filenameSetFromCode)
            {
                ShowPaths(FilenameTextBox.Text);
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
                ErrorIndicator.Visibility = Visibility.Collapsed;
                ExportButton.IsEnabled = true;
                //ExportIcoButton.IsEnabled = true;
            }
            else
            {
                graphicVisual = null;

                ErrorIndicator.Visibility = Visibility.Visible;
                ExportButton.IsEnabled = false;
                //ExportIcoButton.IsEnabled = false;
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
                previewIcons = new List<PreviewShapeViewModel>();
                int index = 0;

                PrepSelectionList(graphicVisual, previewIcons, ref index);
                PathSelectionBox.ItemsSource = previewIcons;

                if (previewIcons.Count > 0)
                {
                    PathSelectionBox.ScrollIntoView(previewIcons[0]);
                    StreamCode.ScrollToHome();
                    DrawingBrushCode.ScrollToHome();
                    GeometryCode.ScrollToHome();
                }
            }
            else
            {
                PathSelectionBox.ItemsSource = null;
                SetColorWarning("");
            }

            SelectAll();
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
            if (selectionChangedFromCode || PathSelectionBox == null || StreamCode == null || DrawingBrushCode == null || Preview == null)
            {
                return;
            }

            // get the selected paths
            selectedVisual = BuildSelectedDrawing(graphicVisual);

            // handle the color warning indicator
            if (selectedVisual == null)
            {
                SetColorWarning("");

                Preview.Fill = null;
                StreamCode.Text = string.Empty;
                DrawingBrushCode.Text = string.Empty;
                GeometryCode.Text = string.Empty;

                return;
            }

            GraphicColorPrecision colorPrecision = GetColorPrecision(selectedVisual);

            switch (colorPrecision)
            {
                case GraphicColorPrecision.Precise:
                    SetColorWarning("");
                    break;

                case GraphicColorPrecision.Estimated:
                    SetColorWarning("Colors are estimated");
                    break;

                case GraphicColorPrecision.Placeholder:
                    SetColorWarning("Colors are placeholders");
                    break;
            }

            // on request normalize the drawin
            GraphicVisual visual = selectedVisual;

            if (NormalizeCheckBox.IsChecked == true)
            {
                var normalizer = new NormalizeVisual();
                visual = normalizer.Normalize(selectedVisual, NormalizeAspect.Both, 100);
            }

            // update the preview
            var drawingBrush = DrawingBrushBinaryGenerator.Generate(visual);
            Preview.Fill = drawingBrush;

            // update the stream source code
            UpdateStreamSourceCode();

            // update the drawing brush source code
            var drawingBrushSource = DrawingBrushSourceGenerator.Generate(visual);
            DrawingBrushCode.Text = drawingBrushSource;

            // update the geometry source code
            UpdateGeometrySourceCode();

            ExportMessage.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// The loaded handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TypeComboBox.ItemContainerGenerator.StatusChanged += OnTypeStatusChanged;
        }

        /// <summary>
        /// The status of type items changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTypeStatusChanged(object sender, EventArgs e)
        {
            SetTypeItemStatus();
        }

        /// <summary>
        /// Try to set the status of the type items
        /// </summary>
        private void SetTypeItemStatus()
        {
            if (enableAllItems)
            {
                var generator = TypeComboBox.ItemContainerGenerator;
                var item = (ComboBoxItem)generator.ContainerFromIndex(2);

                if (item == null)
                {
                    return;
                }

                item.IsEnabled = true;
                item = (ComboBoxItem)generator.ContainerFromIndex(3);

                if (item == null)
                {
                    return;
                }

                item.IsEnabled = true;
            }
            else
            {
                var generator = TypeComboBox.ItemContainerGenerator;
                var item = (ComboBoxItem)generator.ContainerFromIndex(2);

                if (item == null)
                {
                    return;
                }

                item.IsEnabled = false;
                item = (ComboBoxItem)generator.ContainerFromIndex(3);

                if (item == null)
                {
                    return;
                }

                item.IsEnabled = false;
            }
        }

        /// <summary>
        /// Update the Stream
        /// </summary>
        private void UpdateStreamSourceCode()
        {
            if (TypeComboBox == null || StreamCode == null)
            {
                return;
            }

            GraphicVisual visual = selectedVisual;

            if (NormalizeCheckBox.IsChecked == true)
            {
                var normalizer = new NormalizeVisual();
                visual = normalizer.Normalize(selectedVisual, NormalizeAspect.Both, 100);
            }
            string xamlStream;
            GraphicPath graphicPath = visual as GraphicPath;

            if (graphicPath != null)
            {
                enableAllItems = true;
                SetTypeItemStatus();

                StreamCode.TextWrapping = TextWrapping.NoWrap;
            }
            else
            {
                enableAllItems = false;
                SetTypeItemStatus();

                if (TypeComboBox.SelectedIndex >= 2)
                {
                    TypeComboBox.SelectedIndex = 1;
                }

                StreamCode.TextWrapping = TextWrapping.NoWrap;
            }

            if (graphicPath != null)
            {
                if (TypeComboBox.SelectedIndex == 0)
                {
                    var streams = StreamSourceGenerator.GenerateStreamGeometries(visual);
                    xamlStream = string.Join("\n", streams);
                }
                else
                if (TypeComboBox.SelectedIndex == 2)
                {
                    xamlStream = StreamSourceGenerator.GeneratePathGeometry(graphicPath);
                }
                else
                if (TypeComboBox.SelectedIndex == 3)
                {
                    xamlStream = StreamSourceGenerator.GenerateGeometry(graphicPath);
                }
                else
                {
                    xamlStream = StreamSourceGenerator.GeneratePath(visual);
                }
            }
            else
            if (TypeComboBox.SelectedIndex == 0)
            {
                var streams = StreamSourceGenerator.GenerateStreamGeometries(visual);
                xamlStream = string.Join("\n", streams);
            }
            else
            {
                xamlStream = StreamSourceGenerator.GeneratePath(visual);
            }

            StreamCode.Text = xamlStream;
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
        /// Set the color warning
        /// </summary>
        private void SetColorWarning(string warning)
        {
            if (string.IsNullOrEmpty(warning))
            {
                ColorWarning1.Visibility = Visibility.Collapsed;
                ColorWarning2.Visibility = Visibility.Collapsed;
                ColorWarning3.Visibility = Visibility.Collapsed;
            }
            else
            {
                ColorWarning1.Visibility = Visibility.Visible;
                ColorWarning2.Visibility = Visibility.Visible;
                ColorWarning3.Visibility = Visibility.Visible;

                ColorWarning1.Content = warning;
                ColorWarning2.Content = warning;
                ColorWarning3.Content = warning;
            }
        }

        /// <summary>
        /// Update the geometry source code
        /// </summary>
        private void UpdateGeometrySourceCode()
        {
            if (selectedVisual == null || ParameterComboBox == null)
            {
                return;
            }

            GeometrySourceGenerator geometrySourceGenerator;

            if (CreationComboBox.SelectedIndex == 0)
            {
                geometrySourceGenerator = new StreamGeometrySourceGenerator();
            }
            else
            {
                geometrySourceGenerator = new PathGeometrySourceGenerator();
            }

            geometrySourceGenerator.Filename = filename;
            geometrySourceGenerator.IncludeOffset = AddLeftTopCheckBox.IsChecked == true;

            if (ParameterComboBox.SelectedIndex == 0)
            {
                geometrySourceGenerator.NormalizeAspect = NormalizeGeometrySourceAspect.Height;
            }
            else
            if (ParameterComboBox.SelectedIndex == 1)
            {
                geometrySourceGenerator.NormalizeAspect = NormalizeGeometrySourceAspect.Width;
            }
            else
                geometrySourceGenerator.NormalizeAspect = NormalizeGeometrySourceAspect.Individual;

            GeometryCode.Text = geometrySourceGenerator.GenerateSource(selectedVisual);
        }

        /// <summary>
        /// Checkbox handler
        /// </summary>
        private void OnNormalizeCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            UpdateAll();
        }

        /// <summary>
        /// Checkbox handler
        /// </summary>
        private void OnNormalizeCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            UpdateAll();
        }

        /// <summary>
        /// Checkbox handler
        /// </summary>
        private void OnUpdateGeometrySourceCode(object sender, RoutedEventArgs e)
        {
            UpdateGeometrySourceCode();
        }

        /// <summary>
        /// Type ComboBox selection change handler
        /// </summary>
        private void ParameterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGeometrySourceCode();
        }

        /// <summary>
        /// Type ComboBox selection change handler
        /// </summary>
        private void TypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStreamSourceCode();
        }

        /// <summary>
        /// Selects all click.
        /// </summary>
        private void OnSelectAll(object sender, RoutedEventArgs e)
        {
            var selectAll = previewIcons.Any(x => x.IsSelected == false);
            SelectAll(selectAll);
        }

        /// <summary>
        /// Select all items in the listbox
        /// </summary>
        private void SelectAll()
        {
            SelectAll(true);
        }

        /// <summary>
        /// Select all items in the listbox
        /// </summary>
        private void SelectAll(bool selectAll)
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
            UpdateAll();
        }

        /// <summary>
        /// Validate the width entry
        /// </summary>
        private void ExportWidthChanged(object sender, TextChangedEventArgs e)
        {
            if (ExportButton == null)
            {
                return;
            }

            if (int.TryParse(WidthTextBox.Text, out int width))
            {
                ExportButton.IsEnabled = true;
                WidthEntryErrorIndicator.Visibility = Visibility.Collapsed;
            }
            else
            {
                ExportButton.IsEnabled = false;
                WidthEntryErrorIndicator.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Export the shapes
        /// </summary>
        private void ExportClick(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(WidthTextBox.Text, out int width))
            {
                return;
            }

            VisualExporter.Export(selectedVisual, width, MarginCheckBox.IsChecked == true, out string message);

            if (string.IsNullOrEmpty(message))
            {
                ExportMessage.Visibility = Visibility.Collapsed;
            }
            else
            {
                ExportMessage.Visibility = Visibility.Visible;
                ExportMessage.Content = message;
            }
        }

        /// <summary>
        /// Turn on/off the checker board in the background
        /// </summary>
        private void OnCheckerBoardToggleButtonChanged(object sender, RoutedEventArgs e)
        {
            CheckerBoardBackground.Visibility = CheckerBoardToggleButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Copy stream code to the clipboard
        /// </summary>
        private void OnCopyStreamToClipBoard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(StreamCode.Text);
        }

        /// <summary>
        /// Copy drawing brush code to the clipboard
        /// </summary>
        private void OnCopyDrawingBrushToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(DrawingBrushCode.Text);
        }

        /// <summary>
        /// Copy geometr C# code to the clipboard
        /// </summary>
        private void OnCopyGeometryToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GeometryCode.Text);
        }
    }
}
