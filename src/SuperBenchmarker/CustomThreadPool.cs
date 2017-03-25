using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    class CustomThreadPool
    {
        private int _size;
        private IWorkItemFactory _workItemFactory;
        private CancellationTokenSource _cancellationTokenSource;
        private List<Thread> _threadPool = new List<Thread>();
        private int _countOfOperations;
        private int _total;

        public class WorkItemFinishedEventArgs : EventArgs
        {
            public WorkItemFinishedEventArgs(WorkResult result)
            {
                Result = result;
            }

            public WorkResult Result { get; private set; }
        }

        public event EventHandler<WorkItemFinishedEventArgs> WorkItemFinished; 

        public CustomThreadPool(IWorkItemFactory workItemFactory, int size = 100)
        {
            _workItemFactory = workItemFactory;
            _size = size;

            for (int i = 0; i < _size; i++)
            {
                var thread = new Thread(() => LoopAsync(_cancellationTokenSource.Token).Wait());
                _threadPool.Add(thread);
            }
        }

        protected void OnWorkItemFinished(WorkItemFinishedEventArgs eventArgs)
        {
            if (WorkItemFinished != null)
                WorkItemFinished(this, eventArgs);
        }


        public void Start(int countOfOperations, CancellationTokenSource tokenSource)
        {
            _countOfOperations = countOfOperations;
            _cancellationTokenSource = tokenSource;

            _threadPool.ForEach((a) => a.Start()); 
        }

        private async Task LoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var workItem = _workItemFactory.GetWorkItem();
                    var result = await workItem;
                    if(result.NoWork)
                        break;
                    stopwatch.Stop();
                    OnWorkItemFinished(new WorkItemFinishedEventArgs(result));
                }
                catch (Exception e) // THIS PATH REALLY SHOULD NEVER HAPPEN !!
                {
                    stopwatch.Stop();
                    Console.WriteLine(e);
                    OnWorkItemFinished(new WorkItemFinishedEventArgs(new WorkResult()
                    {
                        Status = 999,
                        Index = -1,
                        Parameters = new Dictionary<string, object>(),
                        Ticks = stopwatch.ElapsedTicks
                    }));
                }

                var total = Interlocked.Increment(ref _total);
                if (total >= _countOfOperations)
                    _cancellationTokenSource.Cancel();
            }
        }


        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
