using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sorzus.Wpf.Toolkit
{
    /// <summary>
    ///     A base class which implements <see cref="INotifyPropertyChanged" /> and provides some easy to use methods
    /// </summary>
    /// <example>
    /// <code>
    ///     private string _testProperty;
    ///     public string TestProperty
    ///     {
    ///         get { return _testProperty; }
    ///         set { SetProperty(value, ref _testProperty); }
    ///     }
    /// </code>
    /// </example>
    [Serializable]
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Set the given the given field with the value if they differentiate and raise the property changed event
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="value">The new value</param>
        /// <param name="field">The backing field</param>
        /// <param name="property">The expression of the property</param>
        /// <returns>Return true if the values differentiate and property changed was raised</returns>
        protected virtual bool SetProperty<T>(T value, ref T field, Expression<Func<object>> property)
        {
            return SetProperty(value, ref field, GetPropertyName(property));
        }

        /// <summary>
        ///     Set the given the given field with the value if they differentiate and raise the property changed event
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="value">The new value</param>
        /// <param name="field">The backing field</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>Return true if the values differentiate and property changed was raised</returns>
        protected virtual bool SetProperty<T>(T value, ref T field, [CallerMemberName] string propertyName = null)
        {
            if (field == null || !field.Equals(value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Raise the <see cref="PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the changed property</param>
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Raise the <see cref="PropertyChanged" /> event
        /// </summary>
        /// <param name="property">The expression of the property</param>
        public void OnPropertyChanged(Expression<Func<object>> property)
        {
            OnPropertyChanged(GetPropertyName(property));
        }

        /// <summary>
        ///     Get the name of a property by it's expression
        /// </summary>
        /// <param name="property">The expression of the property</param>
        /// <returns>The name of the property from the expression</returns>
        protected string GetPropertyName(Expression<Func<object>> property)
        {
            var lambda = property as LambdaExpression;
            MemberExpression memberExpression;

            var unaryExpression = lambda.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = (MemberExpression) lambda.Body;
            }

            var propertyInfo = memberExpression?.Member as PropertyInfo;
            return propertyInfo?.Name ?? string.Empty;
        }
    }
}