using System;
using System.ComponentModel;

namespace Utilities
{
    public interface IValueHolderReadOnly<T> : INotifyPropertyChanged
    {
        T Value { get; }
    }

    public interface IValueHolder<T> : IValueHolderReadOnly<T>
    {
        new T Value { get; set; }
    }

    public class ValueHolder<T> : IValueHolder<T>
    {
        private T _value;

        public T Value
        {
            get 
	        { 
		        return _value;
	        }
            set 
	        {
                if ((_value == null && value != null) || (_value != null && !_value.Equals(value)))
                {
                    _value = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
	        }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
