using Simulater.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simulater
{
    /// <summary>
    /// This is the IO class
    /// </summary>
    public class IO
    {
        // These are trhe main data members for an io 
        public string _name { get; set; }
        public int _time { get; set; }
        public int _pid { get; set; }
        public int _scalar { get; set; }
        public string _parent { get; set; }
        public IQueue<Process> _procQueue;

        /// <summary>
        /// these three method signatures and so on are what is required for the event(interrupt to work)
        /// </summary>
        public event ProcessHandler pHandler;
        public delegate void ProcessHandler(IO i, IOInfo info);
        public ConcurrentDictionary<String, Process> _waitingDict;

        public IO(int pid, string type, string parent, int time, int scalar, ConcurrentDictionary<string,Process> dict, ref IQueue<Process> procQueue)
        {
            _name = type;
            _pid = pid;
            _parent = parent;
            _time = time;
            _scalar = scalar;
            _waitingDict = dict;
            _procQueue = procQueue;
        }

        /// <summary>
        /// This is essentially what the thread is. It will create a IOinfo class and put the proper information in it.
        /// Then it will start a stop watch
        /// It will sleep the thread a required amount of time and then stop the stopwatch
        /// Lastly it will raise the event (interrupt) that it is done.
        /// </summary>
        public void Start()
        {
            //create io info and start staopwatch
            IOInfo ioInfo = new IOInfo(_pid, _name);
            int time = _time * _scalar;

            //sleep thread and stop stopwatch when done
            Thread.Sleep(time);
            ioInfo.Stop();

            //raise event
            pHandler(this, ioInfo);

            try
            {
                lock (_waitingDict)
                {
                    lock (_procQueue)
                    {
                        Process nowReadyProcess; //this process is now ready to go back into the process queue

                        _waitingDict.TryGetValue(string.Format("{0}{1}", _name, _pid), out nowReadyProcess);

                        // put process back onto queue if there are more tasks
                        if (nowReadyProcess.tasks.Count != 0)
                        {
                            _procQueue.Enqueue(nowReadyProcess);
                        }
                        else
                        {
                            //process has no more tasks to execute. don't put it back into the ready queue
                        }

                        _waitingDict.TryRemove(string.Format("{0}{1}", _name, _pid), out nowReadyProcess);
                    }
                }
                
            }
            catch(Exception e)
            {
                Console.WriteLine(String.Format("Something is screwy exception e {0}", e.Message));
            }
        }

    }

    /// <summary>
    /// This class exists so the listener can get information from what IO sent raised the event
    /// </summary>
    public class IOInfo
    {
        public string _name;
        public long _totalTime;
        public int _pid;
        public Stopwatch timer;

        public IOInfo( int pid, string name)
        {
            _pid = pid;
            _name = name;
            timer = new Stopwatch();
            timer.Start();
            
        }

        // function to stop the timer and get elapsed time
        public void Stop()
        {
            timer.Stop();
            _totalTime = timer.ElapsedMilliseconds;
        }

    }
}
