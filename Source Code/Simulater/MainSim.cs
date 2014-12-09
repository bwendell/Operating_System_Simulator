using Simulater;
using Simulater.Interfaces;
using Simulater.Schedulers;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Simulater.Queues;
using System.Diagnostics;


namespace Simulater
{
    public class MainSim
    {
        // This is a thread safe dictionary that hold processes while they are in running state (in use)
        public static ConcurrentDictionary<String, Process> _waitingDict = new ConcurrentDictionary<string, Process>();

        /// <summary>
        /// This is the main part of the simulater. It initializes the meta data and config file and will run what was selected withtin the config file. 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Stopwatch mainTimer = new Stopwatch();

            mainTimer.Start();
            Console.WriteLine("SYSTEM - Start");


            //start timing operations
            Stopwatch stopWatchConfig = new Stopwatch();
            stopWatchConfig.Start();

            //check the command line arguments
            if (args.Length != 2)
            {
                //incorrect number of parameters
                System.Console.WriteLine("Incorrect number of parameters\nUsage: exeName <path to config file> <path to metadata>");
                return;
            }

            //create new config object using the first argument and read metadata using the second
            ConfigObj configObj = new ConfigObj();
            MetaData metaData = new MetaData();
            string configPathStr = args[0]; //pathStr should look something like this: @"C:\Users\<username>\Desktop\OS\config.txt";
            string metaDataPathStr = args[1];

            System.Console.WriteLine(configPathStr + " " + metaDataPathStr);

            //make the process queue
            IQueue<Process> normalProcQueue = new RegQueue();
            IQueue<Process> priorityQueue = new PriorityQueue();

            IQueue<Process> procQueue;

            string schedulingType = "";
            int timeQuantum = -1;

            //read config file to configObj
            if (!configObj.readFromFile(configPathStr))
            {
                //unable to parse config file
                System.Console.WriteLine("Invalid files!");
                return;
            }

            stopWatchConfig.Stop();


            //setup scheduling from config obj
            if (configObj.table.ContainsKey("Processor Scheduling"))
            {
                schedulingType = configObj.table["Processor Scheduling"].ToString();

                if (schedulingType.CompareTo("RR") != 0)
                {
                    //not round robin; not using time quantum
                    timeQuantum = 50;
                }
                else
                {
                    if (configObj.table.ContainsKey("Quantum"))
                    {
                        timeQuantum = (int)configObj.table["Quantum"];
                    }
                    else
                    {
                        Console.WriteLine("NO quantum key");
                    }
                }
            }

            //choose queue type depending on scheduling algorithm
            if (schedulingType == "SJF")
            {
                 procQueue = priorityQueue;
            }
            else
            {
                procQueue = normalProcQueue;
            }


            //time how long it takes to read in metadata
            Stopwatch stopWatchMeta = new Stopwatch();
            stopWatchMeta.Start();

            //use our new queue for our reading of the metadata
            if (!metaData.readFromFile(metaDataPathStr, configObj, ref procQueue))
            {
                //unable to parse config file
                System.Console.WriteLine("Invalid files!");
                Console.Read();
                return;
            }

            /*
            foreach (Process p in procQueue)
            {
                System.Console.WriteLine("Process: PID {0} totalTime: {1}", p.pcb.PID, p.pcb.totalTimeRemaining);
                foreach (SimTask t in p.tasks)
                {
                    System.Console.WriteLine("Task: type \"{0}\" cyclesLeft: {1}, timePerCycles: {2}", t.type, t.cyclesLeft, t.timePerCycle);
                }
                

            }*/

            string outputFilePath = "";
            if (configObj.table.ContainsKey("File Path"))
            {
                outputFilePath = configObj.table["File Path"].ToString();
            }

            

            //set up logging using setting from the config object
            // gets the type of tracing and creeates the proper logger
            string tracetype = configObj.table["Log"].ToString();
            string logfile = null;
            switch(tracetype)
            {
                case "Log to Both":
                    logfile = outputFilePath + "Trace.txt";
                    break;
                case "Log to File":
                    logfile = outputFilePath + "Trace.txt";
                    break;
                case "Log to Console":
                    break;
            }

            Logger trace = new Logger(logfile);
            trace.log(string.Format("System finished reading config ({0}ms)", stopWatchConfig.ElapsedMilliseconds), tracetype);
            trace.log(string.Format("System finished reading metaData ({0}ms)", stopWatchMeta.ElapsedMilliseconds), tracetype);

            
            //set up cycle time
            int processCycleTime = 0; //time to wait between cycles
            if (configObj.table.ContainsKey("Processor cycle time"))
            {
                processCycleTime = (int)configObj.table["Processor cycle time"];
            }
            
            //make a scheduler depending on the type of scheduling and run (start) simulation
            switch(schedulingType)
            {
                case "RR":
                    IScheduler roundRobin = new RR(procQueue, timeQuantum, processCycleTime, trace, tracetype, _waitingDict);
                    roundRobin.run();
                    break;

                case "FIFO":
                    IScheduler fifo = new FIFO(procQueue, timeQuantum, processCycleTime, trace, tracetype, _waitingDict);
                    fifo.run();
                    break;

                case "SJF":
                    IScheduler sjf = new SJF(priorityQueue, timeQuantum, processCycleTime, trace, tracetype, _waitingDict);
                    sjf.run();
                    break;
            }

            //pauses program so it doesnt shutdown immediatly
            Console.WriteLine("*************************************");
            Console.WriteLine("*************************************");
            mainTimer.Stop();
            long tinyTime = mainTimer.ElapsedMilliseconds;
            trace.log(String.Format("Total System Runtime- ({0})ms", tinyTime),tracetype);
            trace.log("*********PROGRAM FINISHED************", tracetype);
            Console.WriteLine("Programe Ofiicially Done");
            Console.Read();
        }
        
    }
}

/// <summary>
/// This is the listener for the event Or in our case the INterrupt. It essentially waits for the event to be raised (IO finished ) and then will execute IOStopped and print the resulting statement. 
/// This is the interrup ssyetm
/// </summary>

public class Listener
{
    private Logger _log;
    private string _logType;
    private IO i;
    public Listener(Logger log, string logtype, IO io)
    {
        _log = log;
        _logType = logtype;
        i = io;
    }
    /// <summary>
    /// This alllows us to hook up to the event
    /// </summary>
    public void Subscribe()
    {
        i.pHandler += new IO.ProcessHandler(IOStopped);
    }
    /// <summary>
    /// This will run whenerver the evcent is raised (i.e. interrup thrown(IO stopped))
    /// </summary>
    /// <param name="i">This is the base class for io </param>
    /// <param name="info">this ius the object args that allos the function to see what thread/io raised the evnt</param>
    public void IOStopped(IO i, IOInfo info)
    {
        lock (_log)
        {
            _log.log(String.Format("PID {0} - finished {1} ({2}ms)", info._pid, info._name, info._totalTime), _logType);
        }
    }

}






