
using Awd.MVVM;

namespace Awd
{
    class WaitViewModel : ViewModelBase
    {
        string message;
        public string Message
        {
            get { return message; }
            set
            {
                if (message != value)
                {
                    message = value;
                    RaisePropertyChanged("Message");
                }
            }
        }
    }
}