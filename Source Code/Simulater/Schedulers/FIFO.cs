using Simulater.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simulater.Schedulers
{
    /// <summary>
    /// This is the scheduler for First in first out
    /// Inherits IScheduler
    /// </summary>
    public class FIFO :IScheduler
    {
        //data members
        public IQueue<Process> procQueue;
        public int timeQuantum;
        public int processCycleTime;
        public Logger trace;
        public string tracetype;
        public ConcurrentDictionary<String, Process> _waitingDict;

        public FIFO(IQueue<Process> q, int quant, int cycle, Logger t, string type, ConcurrentDictionary<String, Process> pp)
        {
            procQueue = q;
            timeQuantum = quant;
            processCycleTime = cycle;
            tracetype = type;
            trace = t;
            _waitingDict = pp;
        }

        /// <summary>
        /// Basic run function for the scheduler
        /// </summary>
        public void run()
        {
            //will run as long as the procQueue (ready queue) has processes OR the waitingDic (dictionary of waiting processes) has processes the loop will continue
            while ((procQueue.Count != 0) || (!_waitingDict.IsEmpty))
            {
                //Process nextProc = procQueue.Peek();
                bool procDidFinish = false;

                //Since numSteps is not incremented in the for loop, the for loop will never fall through except in the following cases:
                //if a process finishes, or starts an I/O thread (which it much wait for), then the loop will fall through and the next process is put in
                for (int numSteps = 0; numSteps < timeQuantum || numSteps == -1;)
                {
                    if (procQueue.Count == 0)
                    {
                        break;
                    }
                    procDidFinish = false;
                    string typeOfNextTask = procQueue.Peek().tasks.Peek().type;

                    if (typeOfNextTask.CompareTo(@"process") == 0)
                    { //if the next task for this process is to process data, let it
                        procDidFinish = procQueue.Peek().run(trace, tracetype);
                    }
                    else
                    { //otherwise kick off a thread for IO 
                        //newthread/put in waiting queue/etc
                        int timePerCycle = procQueue.Peek().tasks.Peek().timePerCycle;
                        int numCycles = procQueue.Peek().tasks.Peek().cyclesLeft;
                        int pid = procQueue.Peek().pcb.PID;
                        string type = procQueue.Peek().tasks.Peek().type;
                        string parent = procQueue.Peek().pcb.PID.ToString();

                        //create an IO instance
                        IO anIO = new IO(pid, type, parent, timePerCycle, numCycles, _waitingDict, ref procQueue);

                        //use lock for thread safe logging
                        lock (trace)
                        {
                            trace.log(string.Format("PID {0} - Starting {1} I/O", pid, type), tracetype);
                        }

                        //dequeue process and reset numsteps
                        procQueue.Peek().tasks.Dequeue();
                        numSteps = timeQuantum;

                        //create listener thread for unique io and start it
                        Listener ioListener = new Listener(trace, tracetype, anIO);

                        ThreadStart threadlist = new ThreadStart(ioListener.Subscribe);
                        Thread listen = new Thread(threadlist);
                        listen.Name = "Listener:" + type;
                        listen.Start();

                        //create io thread and start it
                        ThreadStart IOThread = new ThreadStart(anIO.Start);
                        Thread ioT = new Thread(IOThread);
                        ioT.Name = type + " IO Thread";

                        //add the process to thew waiting dictionary
                        _waitingDict.TryAdd(string.Format("{0}{1}", type, pid), procQueue.Dequeue());


                        ioT.Start();


                    }

                    //process fiunished
                    if (procDidFinish)
                    {
                        numSteps = timeQuantum; //will cause the for loop to fall through
                    }

                    Thread.Sleep(processCycleTime);
                    // Thread.Sleep(10);
                }

                //if the process did not finish swap processes
                if (!procDidFinish)
                {
                    if (procQueue.Count != 0)
                    {
                        procQueue.Enqueue(procQueue.Dequeue());
                        lock (trace)
                        {
                            trace.log(string.Format("SYSTEM - swapping processes"), tracetype);
                        }
                    }
                }
                else
                {
                    procQueue.Dequeue();
                }
                if (procQueue.Count == 0)
                {
                    Thread.Sleep(5);
                }

            }
        }
    }
}
