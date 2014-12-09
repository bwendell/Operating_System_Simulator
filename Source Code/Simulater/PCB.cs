using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulater
{
    /// <summary>
    ///  the PCB class holds all data relating to a process
    /// </summary>
    public class PCB
    {

        public PCB()
        {
            totalTimeRemaining = 0; //used for SJF
            PID = -1; //PIDs are ints 1,2,...,n
        }

        public float totalTimeRemaining { get; set; }
        public int PID { get; set; }
        public DateTime lastTaskStartTime;
    }
}
