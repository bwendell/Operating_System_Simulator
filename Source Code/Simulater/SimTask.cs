using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Simulater.Interfaces;

namespace Simulater
{
    /// <summary>
    /// Each Process has tasks that needs to be simulated
    /// This class runs those tasks, anmd will simulate the times for thewm, they also provide usefull information to the main simulater(Scheduler and PCB)
    /// </summary>
    public class SimTask
    {
        public int cyclesLeft { set; get; }
        public int timePerCycle; //put this in while reading meta data
        //public PCB* parentPCB; //want reference to parent's PCB
        public string type; // process/monitorIO/etc..
        public int initCycles; //initial number of cycles. if cyclesLeft is the same as this when run(..) is called, then we've just started the task
        public Stopwatch stopWatch; //times the task

        /// <summary>
        /// Run tasks function it will run tasks and time it using the STopwatch object
        /// </summary>
        /// <param name="log">log object</param>
        /// <param name="logType">type of log</param>
        /// <param name="pid">pid of the parent process</param>
        /// <returns></returns>
        public bool run(Logger log, string logType, int pid)
        {
            if (type == @"process") {
                //main thread will sleep between cpu cycles, so we don't need to sleep here
                if (cyclesLeft == initCycles)
                {
                    stopWatch = new Stopwatch();
                    stopWatch.Start(); //start the task clock
                }
                cyclesLeft--;
                if (cyclesLeft == 0) { //no more cycles left, this task if finished.
                    //stop stopwatch
                    stopWatch.Stop();
                    //get elapsed time
                    long timeTaken = stopWatch.ElapsedMilliseconds;

                    //use lock and log out message
                    lock (log)
                    {
                        log.log(string.Format(@"PID {0} - finished processing ({1}ms)", pid, timeTaken), logType);
                    }
                    return true;
                } else {
                    return false;
                }
            } else {
                //shouldn't have called run. I/O tasks are initiated from main
                return false;
            }
        }
    }
}
