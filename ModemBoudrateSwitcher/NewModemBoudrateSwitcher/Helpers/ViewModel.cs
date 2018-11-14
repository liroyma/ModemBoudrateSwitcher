using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NewModemBoudrateSwitcher.Helpers
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        protected virtual void SetProperty<T>(ref T member, T val, bool trackChange = false, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(member, val)) return;

            member = val;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public string GetCurrentMethod()
        {
            return "";
        }
    }
}
