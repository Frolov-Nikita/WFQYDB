using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace WFQYDB
{
    public class LimitedObservableCollection<T> : ObservableCollection<T>
    {
        public int Limit { get; set; } = 10;

        protected override void InsertItem(int index, T item)
        {
            while (Count >= Limit)
            { 
                base.RemoveItem(0);
                index--;
            }

            //CheckReentrancy();
            base.InsertItem(index, item);

            //OnPropertyChanged(nameof(Count));
            //OnPropertyChanged("Item[]");
            //OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        //private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        //    OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        //private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index) =>        
        //    OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        
    }
}
