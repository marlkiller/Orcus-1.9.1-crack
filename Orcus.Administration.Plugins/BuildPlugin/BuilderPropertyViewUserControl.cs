using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.Plugins.Properties;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;

namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     A base class for a <see cref="IBuilderPropertyView" /> which inherits from  <see cref="UserControl" />
    /// </summary>
    /// <typeparam name="T">The builder property</typeparam>
    public abstract class BuilderPropertyViewUserControl<T> : UserControl, IBuilderPropertyView, INotifyPropertyChanged
        where T : IBuilderProperty
    {
        protected BuilderPropertyViewUserControl()
        {
            DataContextChanged += OnDataContextChanged;
        }

        /// <summary>
        ///     The current builder property. This can be null if the DataContext wasn't set yet. Please put initialization code in
        ///     <see cref="OnCurrentBuilderPropertyChanged" /> instead of the constructor
        /// </summary>
        [CanBeNull]
        public T CurrentBuilderProperty { get; private set; }

        /// <summary>
        ///     The location in the client builder
        /// </summary>
        public abstract BuilderPropertyPosition PropertyPosition { get; }

        /// <summary>
        ///     Strings which represent this builder property
        /// </summary>
        public abstract string[] Tags { get; }

        /// <summary>
        ///     The type of the <see cref="IBuilderProperty" /> (<see cref="T" />)
        /// </summary>
        public Type BuilderProperty { get; } = typeof (T);

        /// <summary>
        ///     Validate the user inputs
        /// </summary>
        public InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            IBuilderProperty builderProperty)
        {
            return ValidateInput(currentBuilderProperties, (T) builderProperty);
        }

        /// <summary>
        ///     Event of <see cref="INotifyPropertyChanged" />
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Validate the user inputs
        /// </summary>
        /// <param name="currentBuilderProperties">All builder properties</param>
        /// <param name="currentBuilderProperty">The builder property to validate</param>
        /// <returns>The validation result based on the current values of the <see cref="BuilderProperty" /></returns>
        public abstract InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            T currentBuilderProperty);

        private void OnDataContextChanged(object sender,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            CurrentBuilderProperty = (T) dependencyPropertyChangedEventArgs.NewValue;
            OnCurrentBuilderPropertyChanged(CurrentBuilderProperty);
        }

        /// <summary>
        ///     The builder property changed
        /// </summary>
        /// <param name="newValue">The new builder property</param>
        protected virtual void OnCurrentBuilderPropertyChanged(T newValue)
        {
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}