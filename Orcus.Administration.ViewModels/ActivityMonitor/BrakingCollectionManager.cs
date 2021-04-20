using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Orcus.Administration.FileExplorer.Utilities;

namespace Orcus.Administration.ViewModels.ActivityMonitor
{
    public class BrakingCollectionManager<T>
    {
        private readonly Queue<ItemInfo<T>> _lastItems;
        private readonly List<ItemInfo<T>> _overdueItems;
        private readonly DispatcherTimer _updateTimer;
        private bool _isInEmergencyMode;

        public BrakingCollectionManager(FastObservableCollection<T> list)
        {
            List = list;
            _lastItems = new Queue<ItemInfo<T>>();
            _overdueItems = new List<ItemInfo<T>>();
            _updateTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(400)};
            _updateTimer.Tick += UpdateTimerOnTick;
        }

        public FastObservableCollection<T> List { get; }

        private void UpdateTimerOnTick(object sender, EventArgs eventArgs)
        {
            List.AddItems(_overdueItems.Select(x => x.Item).ToList());
            _overdueItems.Clear();

            if (!ShouldBeInEmergencyMode())
            {
                _isInEmergencyMode = false;
                _updateTimer.Stop();
            }
        }

        private bool ShouldBeInEmergencyMode()
        {
            var packetSpeed = _lastItems.Sum(x => (DateTime.UtcNow - x.Timestamp).TotalMilliseconds)/_lastItems.Count;
            return _lastItems.Count == 3 && packetSpeed < 100; //if they were all added in the last 100 ms
        }

        public void AddItem(T item)
        {
            var currentItemInfo = new ItemInfo<T>(item, DateTime.UtcNow);
            _lastItems.Enqueue(currentItemInfo);
            if (_lastItems.Count > 3)
                _lastItems.Dequeue();

            var shouldBeInEmergencyMode = ShouldBeInEmergencyMode();

            if (_isInEmergencyMode)
                /* {
                 if (!shouldBeInEmergencyMode)
                 {
                     //disable emergency mode
                     _isInEmergencyMode = false;
                     _updateTimer.Stop();
                     var items = _overdueItems.Select(x => x.Item).ToList();
                     items.Add(item);
                     List.AddItems(items);
                     _overdueItems.Clear();
                 }*/
                /* else if ((_overdueItems[0].Timestamp - DateTime.UtcNow).TotalMilliseconds > 400)
             {
                 //add the overdue items every 400 ms
                 var items = _overdueItems.Select(x => x.Item).ToList();
                 items.Add(item);
                 List.AddItems(items);
                 _overdueItems.Clear();
             }//Handled by the timer
             else*/
            {
                _overdueItems.Add(currentItemInfo);
            }
            else
            {
                if (shouldBeInEmergencyMode)
                {
                    _isInEmergencyMode = true;
                    _updateTimer.Start();
                    _overdueItems.Add(currentItemInfo);
                }
                else
                {
                    List.Add(item);
                }
            }
        }

        private struct ItemInfo<T2>
        {
            public ItemInfo(T2 item, DateTime timestamp)
            {
                Item = item;
                Timestamp = timestamp;
            }

            public DateTime Timestamp { get; }
            public T2 Item { get; }
        }
    }
}