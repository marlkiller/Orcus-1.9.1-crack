using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.FileExplorer.Models;

namespace Orcus.Administration.FileExplorer.Controls
{
    /// <summary>
    ///     Uses ISuggestSource and HierarchyHelper to suggest automatically.
    /// </summary>
    public class SuggestBox : SuggestBoxBase
    {
        public static readonly DependencyProperty HierarchyHelperProperty =
            DependencyProperty.Register("HierarchyHelper", typeof (IHierarchyHelper),
                typeof (SuggestBox), new UIPropertyMetadata(new PathHierarchyHelper("Parent", "Value", "SubEntries")));

        public static readonly DependencyProperty SuggestSourcesProperty = DependencyProperty.Register(
            "SuggestSources", typeof (IEnumerable<ISuggestSource>), typeof (SuggestBox), new PropertyMetadata(
                new List<ISuggestSource>(new[] {new AutoSuggestSource()})));

        public static readonly DependencyProperty RootItemProperty = DependencyProperty.Register("RootItem",
            typeof (object), typeof (SuggestBox), new PropertyMetadata(null));

        public IHierarchyHelper HierarchyHelper
        {
            get { return (IHierarchyHelper) GetValue(HierarchyHelperProperty); }
            set { SetValue(HierarchyHelperProperty, value); }
        }

        public IEnumerable<ISuggestSource> SuggestSources
        {
            get { return (IEnumerable<ISuggestSource>) GetValue(SuggestSourcesProperty); }
            set { SetValue(SuggestSourcesProperty, value); }
        }

        /// <summary>
        ///     Assigned by Breadcrumb
        /// </summary>
        public object RootItem
        {
            get { return GetValue(RootItemProperty); }
            set { SetValue(RootItemProperty, value); }
        }

        protected override void updateSource()
        {
            var txtBindingExpr = GetBindingExpression(TextProperty);
            if (txtBindingExpr == null)
                return;

            bool valid = true;
            if (HierarchyHelper != null)
                valid = HierarchyHelper.GetItem(RootItem, Text) != null;

            if (valid)
            {
                txtBindingExpr.UpdateSource();
                RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
            }
            else
                Validation.MarkInvalid(txtBindingExpr,
                    new ValidationError(new PathExistsValidationRule(), txtBindingExpr,
                        "Path not exists.", null));
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            var suggestSources = SuggestSources;
            var hierarchyHelper = HierarchyHelper;
            string text = Text;
            object data = RootItem;
            IsHintVisible = String.IsNullOrEmpty(text);

            if (IsEnabled && suggestSources != null)
                Task.Run(async () =>
                {
                    var tasks = (from s in suggestSources
                        select
                            s.SuggestAsync(data, text, hierarchyHelper)).ToList();
                    await Task.WhenAll(tasks);
                    return tasks.SelectMany(tsk => tsk.Result).Distinct().ToList();
                }).ContinueWith(
                    pTask =>
                    {
                        if (!pTask.IsFaulted)
                            SetValue(SuggestionsProperty, pTask.Result);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected new static void OnSuggestionsChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            SuggestBox sbox = sender as SuggestBox;
            if (args.OldValue != args.NewValue)
                sbox.popupIfSuggest();
        }
    }
}