using System;
using System.ComponentModel;

namespace CleanSlate.Sample
{
    class MainViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string notBlank;
        public string NotBlank
        {
            get { return notBlank; }
            set
            {
                if (StringComparer.InvariantCultureIgnoreCase.Equals(notBlank, value))
                    return;
                notBlank = value;
                OnPropertyChanged("Index");
                OnPropertyChanged("NotBlank");
            }
        }

        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "NotBlank")
                {
                    if (String.IsNullOrEmpty(NotBlank))
                    {
                        return "Field must not be blank.";
                    }
                }

                return "";
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}