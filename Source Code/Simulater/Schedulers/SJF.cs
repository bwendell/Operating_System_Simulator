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
    /// This is the scheduler simulater for Shortest Job First.
    /// It inherits IScheduler
    /// </summary>
    public class SJF : IScheduler
    {
        //class members
        public IQueue<Process> procQueue;
        public int timeQuantum;
        public int processCycleTime;
        public Logger trace;
        public string tracetype;
        public ConcurrentDictionary<String, Process> _waitingDict;

        public SJF(IQueue<Process> q, int quant, int cycle, Logger t, string type, ConcurrentDictionary<String, Process> pp)
        {
            procQueue = q;
            timeQuantum = quant;
            processCycleTime = cycle;
            tracetype = type;
            trace = t;
            _waitingDict = pp;
        }

        /// <summary>
        /// Basic Run function. WIll start the siimulater with SJF Scheduling enabled
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
                        //This is the same as FIFO, except that the queue used is a priority queue (based on totaltimeremaining)
                        for (int numSteps = 0; numSteps < timeQuantum || numSteps == -1; )
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
                            { //otherwise kick off a thread for IO (Its an IO now)
                                //newthread/put in waiting queue/etc
                                //get info needed 
                                int timePerCycle = procQueue.Peek().tasks.Peek().timePerCycle;
                                int numCycles = procQueue.Peek().tasks.Peek().cyclesLeft;
                                int pid = procQueue.Peek().pcb.PID;
                                string type = procQueue.Peek().tasks.Peek().type;
                                string parent = procQueue.Peek().pcb.PID.ToString();

                                //create IO object
                                IO anIO = new IO(pid, type, parent, timePerCycle, numCycles, _waitingDict, ref procQueue);

                                //use lock for thread safe tracing (loggin)
                                lock (trace)
                                {
                                    trace.log(string.Format("PID {0} - Starting {1} I/O", pid, type), tracetype);
                                }

                                //dequeue the task
                                procQueue.Peek().tasks.Dequeue();

                                //reset numsteps
                                numSteps = timeQuantum;

                                //Create listener(interrupt) for this unique thread IO object event
                                Listener ioListener = new Listener(trace, tracetype, anIO);

                                //thread out the listener so main thread wil have no effect on performance and execution time
                                //start thread
                                ThreadStart threadlist = new ThreadStart(ioListener.Subscribe);
                                Thread listen = new Thread(threadlist);
                                listen.Name = "Listener:" + type;
                                listen.Start();

                                //create new IO thread
                                ThreadStart IOThread = new ThreadStart(anIO.Start);
                                Thread ioT = new Thread(IOThread);
                                ioT.Name = type + " IO Thread";

                                //add the process to the waiting dict. since it has IO running now
                                _waitingDict.TryAdd(string.Format("{0}{1}", type, pid), procQueue.Dequeue());

                                //start that thread
                                ioT.Start();


                            }

                            //proccess finished.
                            if (procDidFinish)
                            {
                                numSteps = timeQuantum; //will cause the for loop to fall through
                            }

                            Thread.Sleep(processCycleTime);
                            // Thread.Sleep(10);
                        }

                        //if the proccess is nmot finished
                        if (!procDidFinish)
                        {
                            //if the Queue is not empty
                            if (procQueue.Count != 0)
                            {
                                //dequeue and enqueue
                                procQueue.Enqueue(procQueue.Dequeue());

                                //loggging statement (Swapping that process))
                                lock (trace)
                                {
                                    trace.log(string.Format("SYSTEM - swapping processes"), tracetype);
                                }
                            }
                        }
                        //if process happens to finish dequeue it
                        else
                        {
                            procQueue.Dequeue();
                        }

                        if(procQueue.Count == 0)
                        {
                            Thread.Sleep(5);
                        }
                    }              
        }
    }
}