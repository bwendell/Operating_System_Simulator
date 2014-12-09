using Simulater.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace Simulater
{
    public class MetaData
    {
        string text = "S(start)0; A(start)0; P(run)13; I(keyboard)5; P(run)6; O(monitor)5; P(run)5; I(hard drive)5; P(run)7; A(end)0; A(start)0; P(run)10; I(keyboard)5; P(run)7; O(hard drive)5; P(run)15; A(end)0; A(start)0; P(run)13; I(hard drive)5; P(run)14; O(hard drive)5; P(run)13; I(hard drive)5; P(run)10; A(end)0; S(end)0;";


        public MetaData()
        {
        }


        // reads meta data from file
        public bool readFromFile(string filePath, ConfigObj config, ref IQueue<Process> processes)
        {

            if (File.Exists(filePath))  // file exists
            {
                string readText = File.ReadAllText(filePath);

               //readText = text;

                string[] metaArray = new string[100000];
                Process[] procArray = new Process[15];

                // split meta data into parts
                metaArray = readText.Split(';');

                


                // test output
                int index = 0;
                foreach (string x in metaArray)
                {
                    metaArray[index] = x.Trim(' ');

                    if (x == "")
                    {
                        metaArray[index] = null;
                    }
                    else
                    {
                        if (x[0] == '\r')
                        {
                            metaArray[index] = metaArray[index].Substring(2);
                        }

                        Console.WriteLine(metaArray[index]);
                    }
                   
                        index++;
                }

                int procNum = -1;

                // loop through every element in the array
                foreach (string s in metaArray)
                { 
                    if(s == null)
                    {
                        continue;
                    }
                    // if it's the process start, create a new process
                    if (s.Contains("A(start)"))
                    {
                        procNum++;
                        Process process = new Process();
                        process.tasks = new Queue<SimTask>();
                        process.pcb.PID = procNum;
                        procArray[procNum] = process;
                    }
                    // put the process on the queue after getting all tasks
                    else if (s.Contains("A(end"))
                    {
                        processes.Enqueue(procArray[procNum]);
                        continue;
                    }
                    // ignore start of simulator and end of simulator
                    else if (s.Contains("S(end") || s.Contains("S(start"))
                    {
                        continue;
                    }
                    // put a task to run the process in the task queue
                    if (s.Contains("run"))
                    {
                        SimTask task = new SimTask();
                        string temp = s.Substring(6);
                        int tempInt = 0;

                        // get number of cycles to run
                        if (int.TryParse(temp, out tempInt))
                        {
                            task.cyclesLeft = tempInt;
                            task.initCycles = tempInt;
                        }
                        else
                        {
                            System.Console.WriteLine("Problem reading config file.");
                            return false;
                        }
                        string cycleTime = config.table["Processor cycle time"].ToString();
                        int cycleIntTime = -1;

                        // get time per cycle
                        if (int.TryParse(cycleTime, out cycleIntTime))
                        {
                            task.timePerCycle = cycleIntTime;
                        }

                        task.type = "process";

                        procArray[procNum].pcb.totalTimeRemaining += (cycleIntTime * tempInt);

                        // put onto task queue of current process
                        procArray[procNum].tasks.Enqueue(task);
                    }
                    // put a task to get input from the keyboard in the task queue
                    else if (s.Contains("keyboard"))
                    {
                        SimTask task = new SimTask();
                        string temp = s.Substring(11);
                        int tempInt = 0;

                        // get number of cycles to run
                        if (int.TryParse(temp, out tempInt))
                        {
                            task.cyclesLeft = tempInt;
                            task.initCycles = tempInt;
                        }
                        else
                        {
                            System.Console.WriteLine("Problem reading config file.");
                            return false;
                        }
                        string keyTime = config.table["Keyboard cycle time"].ToString();
                        int keyIntTime = -1;

                        // get time per cycle
                        if (int.TryParse(keyTime, out keyIntTime))
                        {
                            task.timePerCycle = keyIntTime;
                        }

                        task.type = "keyboard";

                        procArray[procNum].pcb.totalTimeRemaining += (keyIntTime * tempInt);

                        // put onto task queue of current process
                        procArray[procNum].tasks.Enqueue(task);
                    }
                    // put a task to output to the monitor in the task queue
                    else if (s.Contains("monitor"))
                    {
                        SimTask task = new SimTask();
                        string temp = s.Substring(10);
                        int tempInt = 0;

                        // get number of cycles to run
                        if (int.TryParse(temp, out tempInt))
                        {
                            task.cyclesLeft = tempInt;
                            task.initCycles = tempInt;
                        }
                        else
                        {
                            System.Console.WriteLine("Problem reading config file.");
                            return false;
                        }
                        string tmpTime = config.table["Monitor display time"].ToString();
                        int tmpIntTime = -1;

                        // get time per cycle
                        if (int.TryParse(tmpTime, out tmpIntTime))
                        {
                            task.timePerCycle = tmpIntTime;
                        }

                        task.type = "monitor";

                        procArray[procNum].pcb.totalTimeRemaining += (tmpIntTime * tempInt);

                        // put onto task queue of current process
                        procArray[procNum].tasks.Enqueue(task);
                    }
                    // put a task to get input from the hard drive in the task queue
                    else if (s.Contains("hard drive"))
                    {
                        SimTask task = new SimTask();
                        string temp = s.Substring(13);
                        int tempInt = 0;

                        // get number of cycles to run
                        if (int.TryParse(temp, out tempInt))
                        {
                            task.cyclesLeft = tempInt;
                            task.initCycles = tempInt;
                        }
                        else
                        {
                            System.Console.WriteLine("Problem reading config file.");
                            return false;
                        }
                        string keyTime = config.table["Hard drive cycle time"].ToString();
                        int keyIntTime = -1;

                        // get time per cycle
                        if (int.TryParse(keyTime, out keyIntTime))
                        {
                            task.timePerCycle = keyIntTime;
                        }

                        task.type = "hard drive";

                        procArray[procNum].pcb.totalTimeRemaining += (keyIntTime * tempInt);

                        // put onto task queue of current process
                        procArray[procNum].tasks.Enqueue(task);
                    }

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
