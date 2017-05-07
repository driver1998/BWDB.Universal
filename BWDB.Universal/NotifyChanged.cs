using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWDB.Universal
{

    public class NotifyChanged<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        T item;
        public T Item
        {
            get => item;
            set
            {
                if (!Equals(item, value))
                {
                    item = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
                }
            }
        }
    }
}
