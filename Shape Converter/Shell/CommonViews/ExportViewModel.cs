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

using ShapeConverter.Shell.Exporter;
using ShapeConverter.Shell.MVVM;

namespace ShapeConverter.Shell.CommonViews
{
    public class ExportViewModel : ViewModelBase
    {
        private GraphicVisual selectedVisual;
        private bool addMargin;
        private string exportWidth;
        private bool showValidationError;
        private string errorMessage;

        public ExportViewModel()
        {
            Export = new DelegateCommand(OnExport, OnExportCanExecute);
            ExportWidth = "1000";
        }

        public string ExportWidth
        {
            get 
            {
                return exportWidth; 
            }

            set
            {
                exportWidth = value; 
                NotifyPropertyChanged();
                ValidateWidth();
            }
        }

        public bool AddMargin
        {
            get
            {
                return addMargin; 
            }

            set
            {
                addMargin = value;
                NotifyPropertyChanged();
            }
        }

        public DelegateCommand Export { get; set; }

        public bool ShowValidationError
        {
            get
            {
                return showValidationError;
            }

            set
            {
                showValidationError = value;
                NotifyPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }

            set
            {
                errorMessage = value;
                NotifyPropertyChanged();
            }
        }

        public void SetNewGraphicVisual(GraphicVisual visual)
        {
            selectedVisual = visual;
            Export.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Validate the width entry
        /// </summary>
        private void ValidateWidth()
        {
            if (int.TryParse(ExportWidth, out _))
            {
                ShowValidationError = false;
            }
            else
            {
                ShowValidationError = true;
            }

            Export.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Export the shapes
        /// </summary>
        private void OnExport()
        {
            int.TryParse(ExportWidth, out int width);
            VisualExporter.Export(selectedVisual, width, AddMargin, out string message);

            if (string.IsNullOrEmpty(message))
            {
                ErrorMessage = null;
            }
            else
            {
                ErrorMessage = message;
            }
        }

        private bool OnExportCanExecute()
        {
            return !ShowValidationError && selectedVisual != null;
        }
    }
}
