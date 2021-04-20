using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Orcus.Commands.WindowsCustomizer
{
    public class WindowsPropertyInfo<T> : IWindowsPropertyInfo
    {
        private readonly PropertyInfo _propertyInfo;

        public WindowsPropertyInfo(Expression<Func<T>> picker)
        {
            Picker = picker;
            _propertyInfo = (PropertyInfo) ((MemberExpression) Picker.Body).Member;
        }

        public Expression<Func<T>> Picker { get; }

        public T Value
        {
            get { return (T) _propertyInfo.GetValue(null, null); }
            set { _propertyInfo.SetValue(null, value, null); }
        }

        public string Name => _propertyInfo.Name;
        public Type DataType => typeof (T);

        object IWindowsPropertyInfo.Value
        {
            get { return Value; }
            set { Value = (T) value; }
        }
    }

    public interface IWindowsPropertyInfo
    {
        string Name { get; }
        object Value { get; set; }
        Type DataType { get; }
    }
}