// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
namespace Microsoft.Xaml.Behaviors.Input
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using Microsoft.Xaml.Behaviors;

    public enum KeyTriggerFiredOn
    {
        KeyDown,
        KeyUp
    }

    /// <summary>
    /// A Trigger that is triggered by a keyboard event.  If the target Key and Modifiers are detected, it fires.
    /// </summary>
    public class KeyTrigger : EventTriggerBase<UIElement>
    {
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(Key), typeof(KeyTrigger));

        public static readonly DependencyProperty ModifiersProperty = DependencyProperty.Register("Modifiers", typeof(ModifierKeys), typeof(KeyTrigger));

        public static readonly DependencyProperty ActiveOnFocusProperty = DependencyProperty.Register("ActiveOnFocus", typeof(bool), typeof(KeyTrigger));

        public static readonly DependencyProperty FiredOnProperty = DependencyProperty.Register("FiredOn", typeof(KeyTriggerFiredOn), typeof(KeyTrigger));

        private UIElement targetElement;

        /// <summary>
        /// The key that must be pressed for the trigger to fire.
        /// </summary>
        public Key Key
        {
            get { return (Key)this.GetValue(KeyTrigger.KeyProperty); }
            set { this.SetValue(KeyTrigger.KeyProperty, value); }
        }

        /// <summary>
        /// The modifiers that must be active for the trigger to fire (the default is no modifiers pressed).
        /// </summary>
        public ModifierKeys Modifiers
        {
            get { return (ModifierKeys)this.GetValue(KeyTrigger.ModifiersProperty); }
            set { this.SetValue(KeyTrigger.ModifiersProperty, value); }
        }

        /// <summary>
        /// If true, the Trigger only listens to its trigger Source object, which means that element must have focus for the trigger to fire.
        /// If false, the Trigger listens at the root, so any unhandled KeyDown/Up messages will be caught.
        /// </summary>
        public bool ActiveOnFocus
        {
            get { return (bool)this.GetValue(KeyTrigger.ActiveOnFocusProperty); }
            set { this.SetValue(KeyTrigger.ActiveOnFocusProperty, value); }
        }

        /// <summary>
        /// Determines whether or not to listen to the KeyDown or KeyUp event.
        /// </summary>
        public KeyTriggerFiredOn FiredOn
        {
            get { return (KeyTriggerFiredOn)this.GetValue(KeyTrigger.FiredOnProperty); }
            set { this.SetValue(KeyTrigger.FiredOnProperty, value); }
        }

        protected override string GetEventName()
        {
            return "Loaded";
        }

        private void OnKeyPress(object sender, KeyEventArgs e)
        {
            bool isKeyMatch = e.Key == this.Key;
            // Handle the scenario where the Alt key is pressed (reported as Key.System), allowing the main key to be detected correctly.
            if (e.Key == Key.System)
            {
                // Check if the actual key being held (e.SystemKey) matches the developer-defined key.
                isKeyMatch = e.SystemKey == this.Key;
            }

            // Get the actual modifiers considering special keys like LeftCtrl, RightCtrl, etc.
            ModifierKeys actualModifiers = GetActualModifiers();

            // Check if the registered modifiers exactly match the modifiers required by the trigger.
            bool isModifiersMatch = actualModifiers == this.Modifiers;

            if (isKeyMatch && isModifiersMatch)
            {
                this.InvokeActions(e);
            }
        }

        private static ModifierKeys GetActualModifiers()
        {
            // Explicitly check each modifier key to ensure accurate current state is captured
            ModifierKeys actualModifiers = ModifierKeys.None;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                actualModifiers |= ModifierKeys.Control;
            }
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                actualModifiers |= ModifierKeys.Shift;
            }
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt) || Keyboard.IsKeyDown(Key.System))
            {
                actualModifiers |= ModifierKeys.Alt;
            }

            return actualModifiers;
        }

        protected override void OnEvent(EventArgs eventArgs)
        {
            // Listen to keyboard events.
            if (this.ActiveOnFocus)
            {
                this.targetElement = this.Source;
            }
            else
            {
                this.targetElement = KeyTrigger.GetRoot(this.Source);
            }

            if (this.FiredOn == KeyTriggerFiredOn.KeyDown)
            {
                this.targetElement.KeyDown += this.OnKeyPress;
            }
            else
            {
                this.targetElement.KeyUp += this.OnKeyPress;
            }

            // Unregister the Loaded event of the Source object to prevent the KeyUp or KeyDown events from being registered multiple times.
            // this is especially important when the KeyTrigger is used in a TabControl/TabItem.
            UnregisterLoaded(Source as FrameworkElement);
        }

        protected override void OnDetaching()
        {
            if (this.targetElement != null)
            {
                if (this.FiredOn == KeyTriggerFiredOn.KeyDown)
                {
                    this.targetElement.KeyDown -= this.OnKeyPress;
                }
                else
                {
                    this.targetElement.KeyUp -= this.OnKeyPress;
                }
            }

            base.OnDetaching();
        }

        private static UIElement GetRoot(DependencyObject current)
        {
            UIElement last = null;

            while (current != null)
            {
                last = current as UIElement;
                current = VisualTreeHelper.GetParent(current);
            }

            return last;
        }
    }
}
