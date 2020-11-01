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

using System.Collections.Generic;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Main
{
    /// <summary>
    /// the parent/child list
    /// </summary>
    internal class ParentChildPriorityList
    {
        List<double> parentValues;
        List<double> childValues;
        int parentIndex;
        int childIndex;

        public List<double> ParentValues
        {
            get
            {
                return parentValues;
            }

            set
            {
                parentValues = value;
                parentIndex = 0;
            }
        }

        public List<double> ChildValues
        {
            get
            {
                return childValues;
            }

            set
            {
                childValues = value;
                childIndex = 0;
            }
        }

        /// <summary>
        /// Switch to next index
        /// </summary>
        public void Next()
        {
            if (childValues != null && childIndex < childValues.Count)
            {
                childIndex++;
            }

            if (parentValues != null && parentIndex < parentValues.Count)
            {
                parentIndex++;
            }
        }

        /// <summary>
        /// Return the current value, child values have priority
        /// </summary>
        public bool GetCurrent(out double val)
        {
            val = 0;

            if (childValues != null && childIndex < childValues.Count)
            {
                val = childValues[childIndex];

                return true;
            }

            if (parentValues != null && parentIndex < parentValues.Count)
            {
                val = parentValues[parentIndex];

                return true;
            }

            return false;
        }

        /// <summary>
        /// Return the current value or the last value of the list, child values have priority
        /// </summary>
        public double GetCurrentOrLast()
        {
            double val = 0;

            if (childValues != null)
            {
                if (childIndex < childValues.Count)
                {
                    val = childValues[childIndex];
                }
                else
                {
                    val = childValues[childValues.Count - 1];
                }
            }
            else
            if (parentValues != null)
            {
                if (parentIndex < parentValues.Count)
                {
                    val = parentValues[parentIndex];
                }
                else
                {
                    val = parentValues[parentValues.Count - 1];
                }
            }

            return val;
        }
    }

    /// <summary>
    /// Position definition of one dimension
    /// </summary>
    internal class OneDimensionPositions
    {
        ParentChildPriorityList AbsoluteValue;
        ParentChildPriorityList RelativeValue;
        double current;

        public OneDimensionPositions()
        {
            AbsoluteValue = new ParentChildPriorityList();
            RelativeValue = new ParentChildPriorityList();
            Current = 0;
        }

        /// <summary>
        /// The current value
        /// </summary>
        public double Current
        {
            get
            {
                double val;

                if (!AbsoluteValue.GetCurrent(out val))
                {
                    val = current;
                }

                if (RelativeValue.GetCurrent(out double relVal))
                {
                    val += relVal;
                }

                return val;
            }

            set
            {
                current = value;
            }
        }

        /// <summary>
        /// Set the new absolute and relative position lists of the parent
        /// </summary>
        public void SetParentValues(List<double> absoluteValues, List<double> relativeValues)
        {
            AbsoluteValue.ParentValues = absoluteValues;
            RelativeValue.ParentValues = relativeValues;
        }

        /// <summary>
        /// Set the new absolute and relative position lists of the child
        /// </summary>
        public void SetChildValues(List<double> absoluteValues, List<double> relativeValues)
        {
            AbsoluteValue.ChildValues = absoluteValues;
            RelativeValue.ChildValues = relativeValues;
        }

        /// <summary>
        /// Switch to next index
        /// </summary>
        public void Next()
        {
            AbsoluteValue.Next();
            RelativeValue.Next();
        }
    }

    /// <summary>
    /// Position description of characters
    /// </summary>
    internal class CharacterPositions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CharacterPositions()
        {
            X = new OneDimensionPositions();
            Y = new OneDimensionPositions();
        }

        public OneDimensionPositions X { get; set; }
        public OneDimensionPositions Y { get; set; }

        /// <summary>
        /// Switch to next index
        /// </summary>
        public void Next()
        {
            X.Next();
            Y.Next();
        }
    }
}
