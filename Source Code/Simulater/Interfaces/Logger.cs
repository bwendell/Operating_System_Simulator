using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulater.Interfaces
{
    /// <summary>
    /// Logger class object
    /// </summary>
    public class Logger
    {
        public string _file { get; set; }
        public StreamWriter writer;
        /// <summary>
        /// Constructor object, also opens file for reading if available
        /// </summary>
        /// <param name="file">file to write to</param>
        public Logger(string file = null)
        {
            _file = file;
            if (_file != null)
            {
                if (!File.Exists(_file))
                {
                    writer = File.CreateText(_file);
                }
                else
                {
                    writer = new StreamWriter(_file);
                }

                writer.Close();
            }
        }

        /// <summary>
        /// basic log function, will log based on type
        /// </summary>
        /// <param name="message">the log message to be added</param>
        /// <param name="type">log type (from config file) defaulted to Console loggin</param>
        /// <param name="fileName">filename if you wish to overload and output to a different file </param>
        public void log(string message, string type = "Log to Console", string fileName =null)
        {
            switch(type)
            {
                case "Log to Console":
                    console(message);
                    break;
                case "Log to Both":
                    both(message);
                    break;
                case "Log to File":
                    logfile(message);
                    break;
            }
        }
        private void console(string message)
        {
            Console.WriteLine(message);
        }

        private void both (string message)
        {
            console(message);
            logfile(message);
        }
        private void logfile(string message)
        {
            if (_file == null)
            {
                throw new Exception("File does not exist");
            }

           using (StreamWriter write = File.AppendText(_file))
           {
               write.WriteLine(message);
           }
           
        }      
    }
}
