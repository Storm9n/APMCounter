using APMCounter.Model;
using System;
using System.ComponentModel;
using Action = APMCounter.Model.Action;

namespace APMCounter.ViewModel
{
    internal class ApmViewModel: INotifyPropertyChanged, IObserver<Action>
    {
        public ActionBucket bucket;
        private int apm;

        private IDisposable unsubscriber;

        public int Apm
        {
            get
            { 
                return apm; 
            }
            set
            { 
                apm = value;
                OnPropertyChanged(nameof(Apm));
            }
        }
 
        public ApmViewModel()
        {
            bucket = ActionBucket.Instance;
            bucket.Subscribe(this);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        public virtual void Subscribe(IObservable<Action> provider)
        {
            if (provider != null)
                unsubscriber = provider.Subscribe(this);
        }

        public void OnNext(Action value)
        {
            Apm = bucket.CalculateApm();
        }

        public void OnError(Exception error)
        {
            return;
        }

        public void OnCompleted()
        {
            Apm = bucket.CalculateApm();
        }

        public virtual IDisposable GetUnsubscriber()
        {
            return unsubscriber;
        }

        public virtual void Unsubscribe(IDisposable unsubscriber)
        {
            unsubscriber?.Dispose();
        }

        ~ApmViewModel()
        {
            Unsubscribe(GetUnsubscriber());
        }
    }
}
