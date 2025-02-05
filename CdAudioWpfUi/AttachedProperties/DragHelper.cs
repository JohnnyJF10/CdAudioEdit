/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

using Point = System.Windows.Point;
using ListView = Wpf.Ui.Controls.ListView;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using DragDropEffects = System.Windows.DragDropEffects;
using ListViewItem = Wpf.Ui.Controls.ListViewItem;
using DragEventArgs = System.Windows.DragEventArgs;
using DataObject = System.Windows.DataObject;

namespace CdAudioWpfUi.AttachedProperties
{
    public static class DragHelper
    {
        private static Point _dragStartPoint;

        public static readonly DependencyProperty CanDragProperty = DependencyProperty.RegisterAttached(
            "CanDrag",
            typeof(bool),
            typeof(DragHelper),
            new PropertyMetadata(false, OnDragableChanged));

        public static void SetCanDrag(UIElement element, bool value)
        {
            element.SetValue(CanDragProperty, value);
        }

        public static bool GetCanDrag(UIElement element)
        {
            return (bool)element.GetValue(CanDragProperty);
        }

        private static void OnDragableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView listView)
            {
                if (e.OldValue != null)
                {
                    listView.PreviewMouseLeftButtonDown -= ListView_PreviewMouseLeftButtonDown;
                    listView.PreviewMouseMove -= ListView_PreviewMouseMove;
                    listView.DragOver -= ListView_DragOver;
                }

                if (e.NewValue != null)
                {
                    listView.PreviewMouseLeftButtonDown += ListView_PreviewMouseLeftButtonDown;
                    listView.PreviewMouseMove += ListView_PreviewMouseMove;
                    listView.DragOver += ListView_DragOver;
                }
            }
        }

        private static void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            {
                var listView = sender as ListView;
                if (listView != null)
                {
                    _dragStartPoint = e.GetPosition(listView);
                }
            }
        }

        private static void ListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var listView = sender as ListView;
            if (listView != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(listView);

                if (Math.Abs(currentPosition.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPosition.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var item = GetListViewItemUnderMouse(listView, currentPosition);
                    if (item == null) return;
                    int? index = listView?.ItemContainerGenerator.IndexFromContainer(item);
                    if (index != null && index != -1)
                    {
                        DragDropEffects effects = Keyboard.Modifiers switch
                        {
                            ModifierKeys.Control => DragDropEffects.Copy,
                            ModifierKeys.Alt => DragDropEffects.Move,
                            _ => DragDropEffects.Link
                        };
                        DragDrop.DoDragDrop(listView, new DataObject("SourceIndex", index), effects);
                    }
                }
            }
        }

        private static void ListView_DragOver(object sender, DragEventArgs e)
        {
            if (sender is ListView lvw_items)
            {
                Point position = e.GetPosition(lvw_items);
                HitTestResult hitTestResult = VisualTreeHelper.HitTest(lvw_items, position);
                if (hitTestResult?.VisualHit != null)
                {
                    ListViewItem? listViewItem = FindAncestor<ListViewItem>(hitTestResult.VisualHit);
                    if (listViewItem != null)
                    {
                        listViewItem.IsSelected = true;
                    }
                }

                var scroller = FindAncestor<System.Windows.Controls.ScrollViewer>(lvw_items);
                if (scroller != null && scroller is System.Windows.Controls.ScrollViewer)
                {
                    Point positionS = e.GetPosition(scroller);
                    if (positionS.Y >= scroller.ActualHeight - 50)
                    {
                        scroller?.ScrollToVerticalOffset(scroller.VerticalOffset + 1);
                    }
                    else if (positionS.Y <= 50)
                    {
                        scroller?.ScrollToVerticalOffset(scroller.VerticalOffset - 1);
                    }
                }
            }
        }

        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null && !(current is T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }

        private static ListViewItem? GetListViewItemUnderMouse(ListView listView, Point position)
        {
            var hitTestResult = VisualTreeHelper.HitTest(listView, position);
            var dependencyObject = hitTestResult?.VisualHit;

            while (dependencyObject != null && !(dependencyObject is ListViewItem))
            {
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            return dependencyObject as ListViewItem;
        }

    }

}
