using Simulater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulater.Queues
{
    /// <summary>
    /// Basic wrapper for Built in Queue
    /// Inherits IQueue interface
    /// Needed to provide common interface for queues that our processes and io can use
    /// this is just a wrapper for the normal queue
    /// </summary>
    public class RegQueue: IQueue<Process>
    {
        private Queue<Process> _mainQueue;

        private int cCount;
        public int Count
        {
            get
            {
                return _mainQueue.Count;
            }
            set
            {
                cCount = value;
            }
        }

        public RegQueue()
        {
            _mainQueue = new Queue<Process>();
        }
        public Process Dequeue()
        {
            return _mainQueue.Dequeue();
        }

        public void Enqueue(Process obj)
        {
            _mainQueue.Enqueue(obj);
        }

        public Process Peek()
        {
            return _mainQueue.Peek();
        }

        public IEnumerator<Process> GetEnumerator()
        {
            return _mainQueue.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _mainQueue.GetEnumerator();
        }
    }
}
