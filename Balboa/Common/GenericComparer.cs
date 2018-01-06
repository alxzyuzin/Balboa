/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Универсальный класс для сравнения объектов любого типа по заданному полю
 *
 --------------------------------------------------------------------------*/ 

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Balboa.Common
{
    public class GenericComparer<T> : IComparer<T>
    {
        public enum SortOrder { Ascending, Descending };

        private string      _sortColumn;
        private SortOrder   _sortingOrder;
  
        public GenericComparer(string sortcolumn, SortOrder sortingorder)
        {
            _sortColumn = sortcolumn;
            _sortingOrder = sortingorder;
        }
  

        #region public property 
        /// <summary> 
        /// Column Name(public property of the class) to be sorted. 
        /// </summary> 
        public string SortColumn
        {
            get { return _sortColumn; }
        }

        /// <summary> 
        /// Sorting order. 
        /// </summary> 
        public SortOrder SortingOrder
        {
            get { return _sortingOrder; }
        }
        #endregion

        #region public methods 
        /// <summary> 
                /// Compare interface implementation 
                /// </summary> 
                /// <param name="x">custom Object</param> 
                /// <param name="y">custom Object</param> 
                /// <returns>int</returns> 
        public int Compare(T x, T y)
        {
            
            PropertyInfo propertyInfo = typeof(T).GetRuntimeProperty(_sortColumn);
            IComparable obj1 = (IComparable)propertyInfo.GetValue(x, null);
            IComparable obj2 = (IComparable)propertyInfo.GetValue(y, null);
            if (_sortingOrder == SortOrder.Ascending)
            {
                return (obj1.CompareTo(obj2));
            }
            else
            {
                return (obj2.CompareTo(obj1));
            }
        }
        #endregion
    }
} 
  
