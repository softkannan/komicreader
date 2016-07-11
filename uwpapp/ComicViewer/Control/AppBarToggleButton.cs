using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace ComicViewer
{
    public class AppBarToggleButton : ToggleButton
    {
        /// <summary>
        /// IsCheckedInternal Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsCheckedInternalProperty =
            DependencyProperty.Register("IsCheckedInternal", typeof(bool), typeof(AppBarToggleButton),
                new PropertyMetadata(false, OnIsCheckedInternalChanged));

        /// <summary>
        /// Gets or sets the IsCheckedInternal property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsCheckedInternal
        {
            get { return (bool)GetValue(IsCheckedInternalProperty); }
            set { SetValue(IsCheckedInternalProperty, value); }
        }

        /// <summary>
        /// Handles changes to the IsCheckedInternal property.
        /// </summary>
        private static void OnIsCheckedInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AppBarToggleButton target = (AppBarToggleButton)d;
            bool oldIsCheckedInternal = (bool)e.OldValue;
            bool newIsCheckedInternal = target.IsCheckedInternal;
            target.OnIsCheckedInternalChanged(oldIsCheckedInternal, newIsCheckedInternal);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsCheckedInternal property.
        /// </summary>
        protected virtual void OnIsCheckedInternalChanged(bool oldIsCheckedInternal, bool newIsCheckedInternal)
        {
            if (IsChecked != null)
            {
                VisualStateManager.GoToState(this, IsChecked.Value ? "Checked" : "Unchecked", false);
            }
        }

        public AppBarToggleButton()
        {
            BindingOperations.SetBinding(this, IsCheckedInternalProperty, new Binding() { Path = new PropertyPath("IsChecked"), Source = this });
        }
    }
}
