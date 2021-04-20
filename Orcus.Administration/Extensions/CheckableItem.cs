using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Extensions
{
    public class CheckableItem<T> : PropertyChangedBase
    {
        private bool _isChecked;
        private T _item;

        public CheckableItem(T item, bool isChecked)
        {
            Item = item;
            IsChecked = isChecked;
        }

        public CheckableItem(T item) : this(item, false)
        {
        }

        public CheckableItem()
        {
        }

        public T Item
        {
            get { return _item; }
            set { SetProperty(value, ref _item); }
        }


        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(value, ref _isChecked); }
        }
    }
}