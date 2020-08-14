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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.Shell.MVVM;

namespace ShapeConverter.Shell.FileConverter
{
    /// <summary>
    /// View model for a single graphic path element in the selection listbox
    /// </summary>
    public class PreviewShapeViewModel : ViewModelBase
    {
        private Brush brush;
        private bool isSelected;
        Action selectionChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public PreviewShapeViewModel(Action selectionChanged)
        {
            this.selectionChanged = selectionChanged;
            ChangeSelection = new DelegateCommand(OnSelectionChange);
        }

        /// <summary>
        /// Gets or sets the selected state
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }

            set
            {
                isSelected = value;
                NotifyPropertyChanged();

                selectionChanged();
            }
        }

        /// <summary>
        /// The preview selection changed
        /// </summary>
        public ICommand ChangeSelection { get; }

        /// <summary>
        /// Reference to the original path data
        /// </summary>
        public GraphicPath OriginalShape { get; set; }

        /// <summary>
        /// The brush to show the shape with
        /// </summary>
        public Brush Brush
        {
            get
            {
                if (brush == null)
                {
                    brush = PreviewBrushGenerator.GeneratePreview(OriginalShape, new Size(100, 50));
                }

                return brush;
            }
        }

        /// <summary>
        /// Called when the user toggles the selection state of a preview shape
        /// </summary>
        private void OnSelectionChange()
        {
            IsSelected = !IsSelected;
        }
    }
}
