using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace MeBrowser.Model
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        Dictionary<string, object> _values = new Dictionary<string, object>();
        protected virtual void SetProperty(object value, [CallerMemberName] string propertyName = "")
        {
            _values[propertyName] = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual T GetProperty<T>([CallerMemberName] string propertyName = "")
        {
            if (_values.ContainsKey(propertyName))
                return (T)_values[propertyName];
            else
                return default;

        }
        protected virtual void OnPropertyChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
