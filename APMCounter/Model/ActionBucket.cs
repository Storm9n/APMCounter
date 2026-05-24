using APMCounter.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace APMCounter.Model
{
    internal class ActionBucket: IObservable<int>
    {
        private readonly List<IObserver<int>> _observers;
        private readonly int[] apmBucket;
        private readonly Timer timer;
        private int _bucketIndex;
        // Private static instance of the class
        private static readonly Lazy<ActionBucket> instance = new Lazy<ActionBucket>(() => new ActionBucket());


        private ActionBucket()
        {
            _observers = new List<IObserver<int>>();
            apmBucket = new int[Consts.SECONDSFORAPM];
            DateTimeOffset now = DateTimeOffset.Now;
            int millisecondsOffSecond = (int)Math.Round((now.AddSeconds(1) - now).TotalMilliseconds);
            timer = new Timer(SetBucketIndex, null, millisecondsOffSecond, 1000);
        }

        public static ActionBucket Instance => instance.Value;

        public void Increase()
        {
            apmBucket[_bucketIndex] += 1;
            NotifyObservers(apmBucket[_bucketIndex]);
        }

        public void Decrease()
        {
            apmBucket[_bucketIndex] -= 1;
            NotifyObservers(apmBucket[_bucketIndex]);
        }

        public void RemoveAtSecond(int second)
        {
            apmBucket[second % Consts.SECONDSFORAPM] = 0;
            NotifyObservers(0);
        }

        public int RetrieveAtSecond(int second)
        {
            return apmBucket[second % Consts.SECONDSFORAPM];
        }

        public int CalculateApm()
        {
            return apmBucket.Aggregate(0, (result, apmAtSecond) => result + apmAtSecond);
        }

        private void SetBucketIndex(object state)
        {
            _bucketIndex = DateTimeOffset.Now.Second % Consts.SECONDSFORAPM;
            RemoveAtSecond(_bucketIndex);
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            if (!_observers.Contains(observer))
               _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<int>> _observers;
            private IObserver<int> _observer;

            public Unsubscriber(List<IObserver<int>> observers, IObserver<int> observer)
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

        public void NotifyObservers(int action)
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
