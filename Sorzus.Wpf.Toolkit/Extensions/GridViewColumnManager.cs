using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Sorzus.Wpf.Toolkit.Extensions
{
    public class GridViewColumnManager
    {
        private readonly ListView _listView;
        private ColumnData _columnData;
        private readonly Dictionary<string, GridViewColumn> _removedColumns;

        public static readonly DependencyProperty ColumnNameProperty = DependencyProperty.RegisterAttached(
            "ColumnName", typeof (string), typeof (GridViewColumnManager), new PropertyMetadata(default(string)));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var listView = dependencyObject as ListView;
            if (listView == null)
                throw new ArgumentException(nameof(listView));

            new GridViewColumnManager(listView);
        }

        public static readonly DependencyProperty ColumnDataProperty = DependencyProperty.RegisterAttached(
            "ColumnData", typeof (ColumnData), typeof (GridViewColumnManager), new PropertyMetadata(default(ColumnData), PropertyChangedCallback));

        public static void SetColumnData(DependencyObject element, ColumnData value)
        {
            element.SetValue(ColumnDataProperty, value);
        }

        public static ColumnData GetColumnData(DependencyObject element)
        {
            return (ColumnData) element.GetValue(ColumnDataProperty);
        }

        public static void SetColumnName(DependencyObject element, string value)
        {
            element.SetValue(ColumnNameProperty, value);
        }

        public static string GetColumnName(DependencyObject element)
        {
            return (string) element.GetValue(ColumnNameProperty);
        }

        public GridViewColumnManager(ListView listView)
        {
            _listView = listView;
            _removedColumns = new Dictionary<string, GridViewColumn>();
            Load();
        }

        private void Load()
        {
            _columnData = GetColumnData(_listView);
            _columnData.ColumnAdded += ColumnDataOnColumnAdded;
            _columnData.ColumnRemoved += ColumnDataOnColumnRemoved;
            ReorderColumns();
            CheckColumnVisibility();

            var view = (GridView)_listView.View;
            view.Columns.CollectionChanged += ColumnsOnCollectionChanged;
        }

        private void ColumnDataOnColumnRemoved(object sender, string s)
        {
            var view = (GridView)_listView.View;
            var column = view.Columns.First(x => GetColumnName(x) == s);
            view.Columns.Remove(column);
            _removedColumns.Add(s, column);

            _listView.UpdateLayout();
        }

        private void ColumnDataOnColumnAdded(object sender, string s)
        {
            var view = (GridView) _listView.View;
            var relativeIndex = 0;
            for (int i = 0; i < _columnData.Order.Count; i++)
            {
                var columnName = _columnData.Order[i];
                if (!_columnData.Visible.Contains(columnName))
                    continue;

                if (columnName == s)
                {
                    view.Columns.Insert(relativeIndex, _removedColumns[s]);
                    _removedColumns.Remove(s);
                    break;
                }
                relativeIndex++;
            }

            _listView.UpdateLayout();
        }

        private void ColumnsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Move)
            {

                var newCollection = ((GridView) _listView.View).Columns.Select(GetColumnName).ToList();
                foreach (var column in _columnData.Order.Where(x => !newCollection.Contains(x)))
                    newCollection.Insert(_columnData.Order.IndexOf(column), column);
                _columnData.Order = newCollection;

                /*
                                var movedItem =
                     GetColumnName(((GridView) _listView.View).Columns[notifyCollectionChangedEventArgs.NewStartingIndex]);
                  int currentRelativeIndex = 0;
                 for (int i = 0; i < _columnData.Order.Count; i++)
                 {
                     var columnName = _columnData.Order[i];
                     if (!_columnData.Visible.Contains(columnName))
                         continue;

                     if (currentRelativeIndex == notifyCollectionChangedEventArgs.NewStartingIndex)
                     {
                         if (currentRelativeIndex < _columnData.Order.Count - 1 && notifyCollectionChangedEventArgs.NewStartingIndex > 0)
                             currentRelativeIndex ++; //the item was moved to the right side

                         _columnData.Order.Move(_columnData.Order.IndexOf(movedItem), currentRelativeIndex);
                         break;
                     }

                     currentRelativeIndex++;
                 }*/
            }
        }

        private void ReorderColumns()
        {
            var view = (GridView) _listView.View;
            var dictionary = new Dictionary<string, GridViewColumn>();
            foreach (var column in view.Columns)
            {
                if (column.ReadLocalValue(ColumnNameProperty) == null)
                    continue;

                var name = GetColumnName(column);
                dictionary.Add(name, column);
            }

            for (int i = 0; i < _columnData.Order.Count; i++)
            {
                GridViewColumn gridViewColumn;
                if (!dictionary.TryGetValue(_columnData.Order[i], out gridViewColumn))
                    continue;

                var index = view.Columns.IndexOf(gridViewColumn);
                if (index != i)
                    view.Columns.Move(index, i);
            }
        }

        private void CheckColumnVisibility()
        {
            var view = (GridView) _listView.View;
            var dictionary = new Dictionary<string, GridViewColumn>();

            //Check all visible columns
            for (int i = view.Columns.Count - 1; i >= 0; i--)
            {
                var column = view.Columns[i];
                if (column.ReadLocalValue(ColumnNameProperty) == null)
                    continue;

                var name = GetColumnName(column);
                if (!_columnData.Visible.Contains(name))
                {
                    //Remove column which should not be visible
                    _removedColumns.Add(name, column);
                    view.Columns.Remove(column);
                    continue;
                }

                dictionary.Add(name, column);
            }

            //Add missing columns
            foreach (var visibleItem in _columnData.Visible)
            {
                if (dictionary.ContainsKey(visibleItem))
                    continue;

                view.Columns.Insert(_columnData.Visible.IndexOf(visibleItem), _removedColumns[visibleItem]);
                _removedColumns.Remove(visibleItem);
            }
        }
    }

    public class ColumnData
    {
        public event EventHandler<string> ColumnAdded;
        public event EventHandler<string> ColumnRemoved;

        public ColumnData()
        {
            Order = new List<string>();
            Visible = new ObservableCollection<string>();
        }

        public List<string> Order { get; set; }
        public ObservableCollection<string> Visible { get; set; }

        public void RemoveColumn(string columnName)
        {
            if (Visible.Contains(columnName))
            {
                Visible.Remove(columnName);
                ColumnRemoved?.Invoke(this, columnName);
            }
        }

        public void AddColumn(string columnName)
        {
            if (!Visible.Contains(columnName))
            {
                Visible.Add(columnName);
                ColumnAdded?.Invoke(this, columnName);
            }
        }
    }
}