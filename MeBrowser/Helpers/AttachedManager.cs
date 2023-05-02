using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MeBrowser.Helpers
{
    public class AttachedManager
    {
        //Attached Property
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(AttachedManager),
                new PropertyMetadata(new CornerRadius(0)));

        public static readonly DependencyProperty DoubleValueProperty =
            DependencyProperty.RegisterAttached(
                "DoubleValue",
                typeof(double),
                typeof(AttachedManager),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty TagProperty =
            DependencyProperty.RegisterAttached(
                "Tag",
                typeof(object),
                typeof(AttachedManager),
                new PropertyMetadata(null));

        public static readonly DependencyProperty StringValueProperty =
            DependencyProperty.RegisterAttached(
                "StringValue",
                typeof(string),
                typeof(AttachedManager),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FlagProperty =
            DependencyProperty.RegisterAttached(
                "Flag",
                typeof(bool),
                typeof(AttachedManager),
                new PropertyMetadata(false));
        public static CornerRadius GetCornerRadius(DependencyObject target)
        {
            return (CornerRadius)target.GetValue(CornerRadiusProperty);
        }
        public static void SetCornerRadius(DependencyObject target, CornerRadius value)
        {
            target.SetValue(CornerRadiusProperty, value);
        }
        public static double GetDoubleValue(DependencyObject target)
        {
            return (double)target.GetValue(DoubleValueProperty);
        }
        public static void SetDoubleValue(DependencyObject target, double value)
        {
            target.SetValue(DoubleValueProperty, value);
        }
        public static object GetTag(DependencyObject target)
        {
            return target.GetValue(TagProperty);
        }
        public static void SetTag(DependencyObject target, object value)
        {
            target.SetValue(TagProperty, value);
        }
        public static string GetStringValue(DependencyObject target)
        {
            return (string)target.GetValue(StringValueProperty);
        }
        public static void SetStringValue(DependencyObject target, string value)
        {
            target.SetValue(StringValueProperty, value);
        }
        public static bool GetFlag(DependencyObject target)
        {
            return (bool)target.GetValue(FlagProperty);
        }
        public static void SetFlag(DependencyObject target, bool value)
        {
            target.SetValue(FlagProperty, value);
        }

        //Attached Event
        public static readonly RoutedEvent KeyPressEvent =
            EventManager.RegisterRoutedEvent("KeyPress", RoutingStrategy.Bubble, typeof(KeyPressEventHandler), typeof(AttachedManager));
        public static void AddKeyPressHandler(DependencyObject d, KeyPressEventHandler handler)
        {
            if (d is not UIElement uielement) return;
            uielement.AddHandler(KeyPressEvent, handler);
        }
        public static void RemoveKeyPressHandler(DependencyObject d, KeyPressEventHandler handler)
        {
            if (d is not UIElement uielement) return;
            uielement.RemoveHandler(KeyPressEvent, handler);
        }
    }
    public delegate void KeyPressEventHandler(object sender, KeyPressEventArgs e);

    public class KeyPressEventArgs : RoutedEventArgs
    {
        public KeyPressEventArgs(Key key, RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
            Key = key;
        }
        public Key Key { get; }
    }
}
