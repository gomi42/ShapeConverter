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

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ShapeConverter.Shell.MVVM;

namespace ShapeConverter.Shell.CommonViews
{
    /// <summary>
    /// Interaction logic for ResourceView.xaml
    /// </summary>
    public partial class ResourceView : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ResourceView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        /// <summary>
        /// Handle DataContext changed
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var triggerResetViewProp = DataContext.GetType().GetProperty("TriggerResetView", BindingFlags.Public | BindingFlags.Instance);

            if (triggerResetViewProp == null || !typeof(ITrigger).IsAssignableFrom(triggerResetViewProp.PropertyType))
            {
                return;
            }

            var trigger = (ITrigger)triggerResetViewProp.GetValue(DataContext);
            trigger.TriggerFired += OnReset;
        }

        /// <summary>
        /// Handle view reset request
        /// We move up the source code to the beginning
        /// </summary>
        private void OnReset()
        {
            SourceCode.ScrollToHome();
        }
    }
}
