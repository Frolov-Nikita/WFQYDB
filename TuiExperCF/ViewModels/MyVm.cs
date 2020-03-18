using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace TuiExperCF.ViewModels
{

    public class BabyShark
    {
        public override string ToString() => "tu-ru-ru-ru";
    }

    public class MyVm : INotifyPropertyChanged
    {
        private string myProperty = "P";
        private BabyShark babyShark = new BabyShark();

        public string MyProperty
        {
            get => myProperty;
            set
            {
                myProperty = value;
                NotifyPropertyChanged();
            }
        }

        public BabyShark BabyShark { 
            get => babyShark;
            set
            {
                babyShark = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
