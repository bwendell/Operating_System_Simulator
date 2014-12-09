using Simulater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulater
{
    /// <summary>
    /// This is the Process class this contains its own PCB and a queue of tasks that need to be completed
    /// </summary>
    public class Process
    {

        public Process()
        {
            pcb = new PCB();
            tasks = new Queue<SimTask>();
        }

        public PCB pcb { get; private set; }
        public Queue<SimTask> tasks { get; set; }

        /// <summary>
        /// Run function for the process will find out what task needs to be run and will run it. 
        /// </summary>
        /// <param name="log">Logger object</param>
        /// <param name="logType">and the kllog type we are using.</param>
        /// <returns></returns>
        public bool run(Logger log, string logType)
            //only call this function if you intend to run a processing task. Threads are started in main
        {
            SimTask topTask = tasks.Peek(); //get the next task
            pcb.totalTimeRemaining -= tasks.Peek().timePerCycle;
            if (topTask.run(log, logType, pcb.PID)) //run task and pop if it returns finished==yes
            { 
                tasks.Dequeue();
            }
            if (tasks.Count == 0) //task queue is empty. Process is done
            {
                //use lock to log it thread safe
                lock (log)
                {
                    log.log(string.Format(@"PID {0} - Process finished", pcb.PID), logType);
                }
                return true;
            }
            else
            {
                return false;
            }
            
        }
        
    }
}
