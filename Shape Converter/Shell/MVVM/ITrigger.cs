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

namespace ShapeConverter.Shell.MVVM
{
    /// <summary>
    /// ITrigger
    /// It is amost identical to ICommand. This is used to trigger
    /// the view from the viewmodel without any data
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// Execute the trigger
        /// </summary>
        void Fire();
    }
}
