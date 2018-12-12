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
    public enum SortOrder {[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Asc")] Asc, [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Desc")] Desc };
    public class GenericComparer<T> : IComparer<T>
    {
        

        private string      _sortColumn;
        private SortOrder   _sortOrder;
  
        public GenericComparer(string sortColumn, SortOrder sortOrder)
        {
            _sortColumn = sortColumn;
            _sortOrder = sortOrder;
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
            get { return _sortOrder; }
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
            //To do
            // Проверить что obj1 и obj2 не null 
            // иначе говоря в типе Т присутствует поле _sortColumn 

            PropertyInfo propertyInfo = typeof(T).GetRuntimeProperty(_sortColumn);
            IComparable obj1 = (IComparable)propertyInfo.GetValue(x, null);
            IComparable obj2 = (IComparable)propertyInfo.GetValue(y, null);
            if (_sortOrder == SortOrder.Asc)
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
  
