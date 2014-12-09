using Simulater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Simulater.Schedulers
{
    /// <summary>
    /// This is the scheduler for Round Robin inheriting IScheduler
    /// </summary>
    public class RR : IScheduler
    {
        //class memebrs
        public IQueue<Process> procQueue; 
        public int timeQuantum;
        public int processCycleTime;
        public Logger trace;
        public string tracetype;
        public ConcurrentDictionary<String, Process> _waitingDict;

        public RR(IQueue<Process> q, int quant, int cycle, Logger t, string type, ConcurrentDictionary<String, Process> pp)
        {
            procQueue = q;
            timeQuantum = quant;
            processCycleTime = cycle;
            tracetype = type;
            trace = t;
            _waitingDict = pp;
        }

        /// <summary>
        /// BAsic run function
        /// </summary>
        public void run()
        {
            //will run as long as the procQueue (ready queue) has processes OR the waitingDic (dictionary of waiting processes) has processes the loop will continue
            while ((procQueue.Count != 0) || (!_waitingDict.IsEmpty))
            {
                //Process nextProc = procQueue.Peek();
                bool procDidFinish = false;

                //gives each process <timeQuantum> cycles in the processor before going to the next process.
                //if a process finishes, or starts an I/O thread (which it much wait for), then the loop will fall through and the next process is put in
                for (int numSteps = 0; numSteps < timeQuantum || numSteps == -1; ++numSteps)
                {
                    if (procQueue.Count == 0)
                    {
                        continue;
                    }
                    procDidFinish = false;
                    string typeOfNextTask = procQueue.Peek().tasks.Peek().type;

                    if (typeOfNextTask.CompareTo(@"process") == 0)
                    { //if the next task for this process is to process data, let it
                        procDidFinish = procQueue.Peek().run(trace, tracetype);
                    }
                    else
                    { //otherwise kick off a thread
                        //newthread/put in waiting queue/etc
                        //get the information required
                        int timePerCycle = procQueue.Peek().tasks.Peek().timePerCycle;
                        int numCycles = procQueue.Peek().tasks.Peek().cyclesLeft;
                        int pid = procQueue.Peek().pcb.PID;
                        string type = procQueue.Peek().tasks.Peek().type;
                        string parent = procQueue.Peek().pcb.PID.ToString();

                        //instantiate a new IO object
                        IO anIO = new IO(pid, type, parent, timePerCycle, numCycles, _waitingDict, ref procQueue);

                        //use lock for thread safe logging
                        lock (trace)
                        {
                            trace.log(string.Format("PID {0} - Starting {1} I/O", pid, type), tracetype);
                        }

                        //dequeue the task and reset numsteps
                        procQueue.Peek().tasks.Dequeue();
                        numSteps = timeQuantum;

                        // create listener for the unique io event
                        Listener ioListener = new Listener(trace, tracetype, anIO);

                        //create thread for listiner to make it independent of thge main thread execution
                        //start thread
                        ThreadStart threadlist = new ThreadStart(ioListener.Subscribe);
                        Thread listen = new Thread(threadlist);
                        listen.Name = "Listener:" + type;
                        listen.Start();

                        //create IO thread and start the thread
                        ThreadStart IOThread = new ThreadStart(anIO.Start);
                        Thread ioT = new Thread(IOThread);
                        ioT.Name = type + " IO Thread";

                        //add the process to our waiting dictionary
                        _waitingDict.TryAdd(string.Format("{0}{1}", type, pid), procQueue.Dequeue());


                        ioT.Start();
                    }

                    //process finsihed
                    if (procDidFinish)
                    {
                        numSteps = timeQuantum; //will cause the for loop to fall through
                    }

                    Thread.Sleep(processCycleTime);
                    // Thread.Sleep(10);
                }

                //process noty finished swap processes if not just dequeue
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
