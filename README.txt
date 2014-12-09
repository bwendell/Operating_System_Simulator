README:


NOTE:
* Do not specify file name, only file path for example
   * C:\Users\gman109\Desktop\
* The Simulator is FINISHED only when IT SAYS
   * *********PROGRAM FINISHED************
* The Three Schedulers available are:
   * FIFO
   * SJF
   * RR
* Please confirm your path in config file exists
* Start program by example below
   * C:\Users\userExample> Simulater config.txt metadata.txt
   * make sure config.txt o and metadata.txt either exist in the same folder as the executable or you provide the full path for each
* We have included our example config files and metadata within the directory
* Included is two folders
   * one which contains just the exe and necesssary sample files
   * two, which is the source code


Quick Overview: 
        Our simulator will Start up by taking in the configuration file and meta data file and filling out the proper structures. Next it looks to see what scheduling we are using.It will then run that scheduler.


Interrupt System: 
        In our simulator we are utilizing c#/.nets built in event system for interrupts. Here is a link if you would like more information on events.http://msdn.microsoft.com/en-us/library/aa645739%28v=vs.71%29.aspx. Within the IO class there is an event delegate. Whenever that delegate is called there is a listener in another class (which is threaded out to avoid execution problems) which will hear the event and execute its function, no matter what line of code is running in the other threads. This is a real time interrupt system that will interrupt the simulation when it is called.


Locks:
        In many parts of the simulator when we log you will see we use a Lock. This is similar to a mutex lock. We were running into file access exceptions ( a logger in another thread was trying to access a file at the same time as another thread). These locks will lock the log object to whatever thread is using it. Any thread that tries to use the object is put into a ready queue waiting to access that object.


Queues: 
        We had to create an common interface for queues. Since we had a normal queue and a priority queue, we needed to make sure that our methods that needed these queues didn't care what the queue is just that it has the same methods it needs (hence the need for the interface). Each queue inherits our IQueue interface, and implements its own version of a queue. 








Main/Simulation loop:
        Main simply reads in the config file, then chooses a queue type and time quantum based on the config object. Next, it reads in the metadata to the queue (or priority queue) and begins the main loop depending on the scheduling type.
        The simulation loop runs until both the process queue and the waiting dictionary are empty. Each iteration it chooses the next process and goes into a for loop, which will run either until it has iterated the correct number of times (time quantum) or it breaks due to the process in the for loop being moved to the waiting dictionary or completing. In FIFO and SJF scheduling, the iterator in the for loop does not increment, causing the time quantum never to be reached. Inside the loop, the type of the next task in the current process’s task queue is checked. If the type is “process” (run) then the number of cycles for that task will simply be decremented. Otherwise, the process’s next task is an I/O operation and the simulator loop starts a thread to run it, moving the process to the waiting dictionary. If a process finishes on run, the loop will fall through, putting in the next process, and if the process finishes after an I/O operation is completed, the thread finish handler will simply take it out of the dictionary and not return into the ready queue.


Example metadata:


S(start)0; A(start)0; I(hard drive)12; O(hard drive)7; I(keyboard)15;
O(hard drive)11; P(run)10; P(run)9;I(hard drive)14;P(run)11; O(monitor)13;
P(run)10; O(monitor)6; P(run)15; A(end)0; S(end)0.


S(start)0; A(start)0; I(hard drive)10; P(run)13; O(monitor)15;
I(keyboard)9; O(monitor)5; I(hard drive)6; O(monitor)12; P(run)9; I(keyboard)10;
O(hard drive)7; P(run)10; I(keyboard)7; A(end)0; S(end)0.


S(start)0; A(start)0; P(run)13; I(keyboard)5; P(run)6; O(monitor)5;
P(run)5; I(hard drive)5; P(run)7; A(end)0; A(start)0; P(run)10; I(keyboard)5; P(run)7;
O(hard drive)5; P(run)15; A(end)0; A(start)0; P(run)13; I(hard drive)5; P(run)14;
O(hard drive)5; P(run)13; I(hard drive)5; P(run)10; A(end); S(end)0.


S(start)0; A(start)0; P(run)13; I(keyboard)5; P(run)6; O(monitor)5;
P(run)5; I(hard drive)5; P(run)7; A(end)0; A(start)0; P(run)10; I(keyboard)5; P(run)7;
O(hard drive)5; P(run)15; A(end)0; A(start)0; P(run)13; I(hard drive)5; P(run)14;
O(hard drive)5; P(run)13; I(hard drive)5; P(run)10; A(end); A(start)0; P(run)13; I(keyboard)5;
P(run)6; O(monitor)5; P(run)5; I(hard drive)5; P(run)7; A(end)0; A(start)0; P(run)10; I(keyboard)5;
P(run)7; O(hard drive)5; P(run)15; A(end)0; S(end)0.








Example Config File:


Version: 1.0
Quantum (cycles): 3
Processor Scheduling: FIFO
File Path: C:\Users\gman109\Desktop\
Processor cycle time (msec): 10
Monitor display time (msec): 25
Hard drive cycle time (msec): 50
Printer cycle time (msec): 500
Keyboard cycle time (msec): 1000
Memory type: Fixed
Log: Log to Both