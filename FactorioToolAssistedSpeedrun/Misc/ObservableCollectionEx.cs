using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace FactorioToolAssistedSpeedrun.Misc
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        private bool _notificationSuppressed = false;
        private bool _havePendingNotifications = false;

        public bool NotificationSuppressed
        {
            get { return _notificationSuppressed; }
            set
            {
                _notificationSuppressed = value;
                if (_notificationSuppressed == false && _havePendingNotifications)
                {
                    OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                    OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    _havePendingNotifications = false;
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (NotificationSuppressed)
            {
                _havePendingNotifications = true;
                return;
            }
            base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (NotificationSuppressed) return;
            base.OnPropertyChanged(e);
        }
    }
}