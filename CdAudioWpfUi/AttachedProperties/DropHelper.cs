/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

using System.Windows;
using System.Windows.Input;

using CdAudioLib.Abstraction;

using DragDropEffects = System.Windows.DragDropEffects;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;

namespace CdAudioWpfUi.AttachedProperties
{
    /// <summary>
    /// DropHelper
    /// </summary>
    public class DropHelper
    {
        public static readonly DependencyProperty ExternalDropCommandProperty =
            DependencyProperty.RegisterAttached(
                "ExternalDropCommand",
                typeof(ICommand),
                typeof(DropHelper),
                new PropertyMetadata(null, OnDropCommandChanged));

        public static ICommand GetExternalDropCommand(DependencyObject obj)
            => (ICommand)obj.GetValue(ExternalDropCommandProperty);

        public static void SetExternalDropCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(ExternalDropCommandProperty, value);

        public static readonly DependencyProperty InternalDropCommandProperty =
            DependencyProperty.RegisterAttached(
            "InternalDropCommand",
            typeof(ICommand),
            typeof(DropHelper),
            new PropertyMetadata(null, OnDropCommandChanged));

        public static ICommand GetInternalDropCommand(DependencyObject obj)
            => (ICommand)obj.GetValue(InternalDropCommandProperty);

        public static void SetInternalDropCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(InternalDropCommandProperty, value);

        private static void OnDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement)
            {
                if (e.OldValue == null && e.NewValue != null)
                {
                    uiElement.Drop += OnDrop;
                }
                else if (e.OldValue != null && e.NewValue == null)
                {
                    uiElement.Drop -= OnDrop;
                }
            }
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                if (d == null) return;
                ICommand command;


                if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var index = (int)e.Data.GetData("SourceIndex");
                    command = GetInternalDropCommand(d);
                    if (command != null && command.CanExecute(index))
                    {
                        DragDropMode mode = e.Effects switch
                        {
                            DragDropEffects.Move => DragDropMode.Move,
                            DragDropEffects.Copy => DragDropMode.Copy,
                            DragDropEffects.Link => DragDropMode.Swap,
                            _ => DragDropMode.Swap
                        };
                        command.Execute((index, mode));
                        e.Handled = true;
                    }
                }
                else
                {
                    var data = e.Data.GetData(DataFormats.FileDrop);
                    if (!(data is string[] filePaths)) return;
                    command = GetExternalDropCommand(d);
                    if (command != null && command.CanExecute(filePaths))
                    {
                        command.Execute(filePaths);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
