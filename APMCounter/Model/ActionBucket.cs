using APMCounter.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace APMCounter.Model
{
    internal class ActionBucket: IObservable<Action>
    {
        private readonly List<IObserver<Action>> _observers;
        private readonly List<Action>[] apmBucket;
        private readonly Timer timer;
        private int _bucketIndex;
        // Private static instance of the class
        private static readonly Lazy<ActionBucket> instance = new Lazy<ActionBucket>(() => new ActionBucket());


        private ActionBucket()
        {
            _observers = new List<IObserver<Action>>();
            apmBucket = new List<Action>[Consts.SECONDSFORAPM];
            foreach (int i in Enumerable.Range(0, Consts.SECONDSFORAPM)) 
            { 
                apmBucket[i] = new List<Action>(); 
            }
            DateTimeOffset now = DateTimeOffset.Now;
            int millisecondsOffSecond = (int)Math.Round((now.AddSeconds(1) - now).TotalMilliseconds);
            timer = new Timer(SetBucketIndex, null, millisecondsOffSecond, 1000);
        }

        public static ActionBucket Instance => instance.Value;

        public void Insert(Action action)
        {
            apmBucket[_bucketIndex].Add(action);
            NotifyObservers(action);
        }

        public void Remove(Action action)
        {
            apmBucket[_bucketIndex].Remove(action);
            NotifyObservers(action);
        }

        public void RemoveAtSecond()
        {
            apmBucket[_bucketIndex].Clear();
            NotifyObservers(null);
        }

        public List<Action> RetrieveAtSecond(int second)
        {
            return apmBucket[second % Consts.SECONDSFORAPM];
        }

        public int CalculateApm()
        {
            return apmBucket.Aggregate(0, (result, actionList) => result + actionList.Count);
        }

        private void SetBucketIndex(object state)
        {
            _bucketIndex = DateTimeOffset.Now.Second % Consts.SECONDSFORAPM;
            RemoveAtSecond();
        }

        public IDisposable Subscribe(IObserver<Action> observer)
        {
            if (!_observers.Contains(observer))
               _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<Action>> _observers;
            private IObserver<Action> _observer;

            public Unsubscriber(List<IObserver<Action>> observers, IObserver<Action> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }

        public void NotifyObservers(Action action)
        {
            foreach (var observer in _observers)
            {
                if (observer != null)
                {
                    observer.OnNext(action);
                }
                else
                    observer.OnCompleted();
            }
        }

        public void EndTransmission()
        {
            foreach (var observer in _observers)
                observer.OnCompleted();

            _observers.Clear();
        }

        ~ActionBucket()
        {
            EndTransmission();
            timer.Dispose();
        }
    }
}
