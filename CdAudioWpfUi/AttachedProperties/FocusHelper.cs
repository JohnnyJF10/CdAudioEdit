

using System.Windows;

namespace CdAudioWpfUi.AttachedProperties
{
    public static class FocusHelper
    {
        public static readonly DependencyProperty MonitorFocusEventsProperty =
            DependencyProperty.RegisterAttached(
                "MonitorFocusEvents",
                typeof(bool),
                typeof(FocusHelper),
                new PropertyMetadata(false, OnMonitorFocusEventsChanged));

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused",
                typeof(bool),
                typeof(FocusHelper),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static bool GetMonitorFocusEvents(DependencyObject obj)
        {
            return (bool)obj.GetValue(MonitorFocusEventsProperty);
        }

        public static void SetMonitorFocusEvents(DependencyObject obj, bool value)
        {
            obj.SetValue(MonitorFocusEventsProperty, value);
        }

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        private static void OnMonitorFocusEventsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.GotFocus += Element_GotFocus;
                    element.LostFocus += Element_LostFocus;

                    SetIsFocused(element, element.IsFocused);
                }
                else
                {
                    element.GotFocus -= Element_GotFocus;
                    element.LostFocus -= Element_LostFocus;
                }
            }
        }

        private static void Element_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject dependencyObject)
            {
                SetIsFocused(dependencyObject, true);
            }
        }

        private static void Element_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject dependencyObject)
            {
                SetIsFocused(dependencyObject, false);
            }
        }
    }
}
