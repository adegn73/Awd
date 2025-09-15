using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Awd.MVVM
{ 
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        protected void RaisePropertyChanged([CallerMemberName] string callerMemberName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(callerMemberName));
        }

    }
}