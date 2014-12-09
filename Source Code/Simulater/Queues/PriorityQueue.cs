using Simulater.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulater.Schedulers
{
    /// <summary>
    /// Just your standard Priority Queue here. Holds Processes and gives priority based on pcb.totalTimeRemaining
    /// (lower totalTimeRemaining will go to the front of the queue)
    /// Inherits from IQueue
    /// </summary>
    public class PriorityQueue : IQueue<Process>
    {

        Process[] processes;
        private int cCount; 
        public int Count 
        {
            get
            {
                return cCount;
            }
            set
            {
                cCount = value;
            }
        }
    

        public PriorityQueue()
        {
            processes = new Process[90];
            Count = 0;
        }

        public void sort()
        {
            bool isSorted = false;
            while (!isSorted)
            {
                isSorted = true;
                for (int i = 0; i < Count - 1; i++)
                {
                    if (processes[i].pcb.totalTimeRemaining > processes[i + 1].pcb.totalTimeRemaining)
                    {
                        isSorted = false;
                        Process temp = processes[i];
                        processes[i] = processes[i + 1];
                        processes[i + 1] = temp;
                    }
                }
            }
        }

        public Process Dequeue()
        {
            Process temp = processes[0];
            for (int i = 0; i < Count - 1; i++)
            {
                processes[i] = processes[i + 1];
            }
            Count--;
            return temp;
        }

        public void Enqueue(Process proc)
        {
            
            if (Count == 90)
            {
                Console.WriteLine("You can't use that many processes!!!");
                Exception exc = new Exception();
                throw exc;
            }
            processes[Count] = proc;
            Count++;
            sort();
        }


        public Process Peek()
        {
            return processes[0];
        }

        public IEnumerator<Process> GetEnumerator()
        {
            List<Process> list = new List<Process>();

            foreach(Process p in processes)
            {
                list.Add(p);
            }

            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            List<Process> list = new List<Process>();

            foreach (Process p in processes)
            {
                list.Add(p);
            }

            return list.GetEnumerator();
        }
    }
}
