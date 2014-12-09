using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Simulater
{
    /// <summary>
    /// Class for the configuration file read in
    /// </summary>
    public class ConfigObj
    {
        public ConfigObj()
        {
            table = new Hashtable();
        }
        public Hashtable table { get; private set; }
        /// <summary>
        /// Reads in config file of specific format to this (ConfigObj) object's Hashtable (table)
        /// Assumes that each line in the config file is of the format   <property_name>: <value>
        /// excluding lines 0 and 12 with a total of 13 lines
        /// additionally, the properties must be in the expected order
        /// limitations: format is very strict. A more flexible format would be an upgrade
        /// </summary>
        /// <param name="filePath">file for the config object</param>
        /// <returns></returns>
        public bool readFromFile(string filePath) {

            bool success = false;
            if (success = File.Exists(filePath))
            {
                string readText = File.ReadAllText(filePath);
                char[] delimiterChars = { '\r', '\r', '\r', '\r', '\r', '\r', '\r', '\r', '\r', '\r', '\r', '\r', '\r', '\r' }; //where to split the lines of the config file

                System.Console.WriteLine("Original text: '{0}'", filePath);

                string[] words = readText.Split(delimiterChars);
                System.Console.WriteLine("{0} words in text:", words.Length);

                for (int i = 0; i < words.Length; ++i )
                {
                    string s = words[i];
                    System.Console.WriteLine(s);
                    int index = -1; //index of colon
                    index = s.IndexOf(@":");
                    int index2 = s.LastIndexOf(@" "); //check that there is a space after the colon
                    if (index == -1 || index2 < index)
                    {
                        //error reading config file
                        success = false;
                        return success;
                    }
                    else
                    {
                        //return string by removing characters from s with starting index 0 and count <index of semicolon>+1(for the space)
                        //should leave only the <value>
                        string value = s.Remove(0, index+2);
                        //check if value is a numeric value
                        int floatValue = 0;
                        bool isNumeric = int.TryParse(value, out floatValue);
                        
                        string key = s.Remove(index);
                        key = key.Remove(0, 1); //remove newline

                        //handle (<TIME_UNIT>)
                        //get first and last index of parentheses and check if the string between in recognizeable
                        int parenIndex = key.IndexOf(@"(");
                        int parenIndex2 = key.LastIndexOf(@")");
                        if (parenIndex > 0 && parenIndex2 > 0)
                        {
                            string timeUnit = key.Remove(0, parenIndex+1);
                            timeUnit = timeUnit.Remove(timeUnit.Length-1);

                            //key has time unit. remove it from the key
                            key = key.Remove(parenIndex - 1, parenIndex2+2 - parenIndex); //parenIndex-1 to get the preceding space

                            if (timeUnit.CompareTo(@"msec") == 0)
                            {
                                //milisec
                                if (!isNumeric)
                                {
                                    success = false;
                                    return success;
                                }
                                else
                                {
                                    //(internal) time values are given in miliSeconds
                                    floatValue *= 1;
                                }
                            }
                            else if (timeUnit.CompareTo(@"sec") == 0)
                            {
                                //sec
                                if (!isNumeric)
                                {
                                    success = false;
                                    return success;
                                }
                                else
                                {
                                    //(internal) time values are given in miliSeconds
                                    floatValue *= 1000;
                                }
                            }
                            else if (timeUnit.CompareTo(@"cycles") == 0)
                            {
                                //this is not a time unit, ie: "Quantum (cycles)"
                                //do nothing
                            }
                            else
                            {
                                if (isNumeric)
                                {
                                    //unrecognized time unit!
                                    success = false;
                                    return success;
                                }
                            }
                        }
                        

                        //use numeric value (if nessesary) for value
                        if (isNumeric)
                        {
                            System.Console.WriteLine("Key: \"{0}\" Value: \"{1}\" - numeric", key, value);
                            table.Add(key, floatValue);
                        }
                        else
                        {
                            System.Console.WriteLine("Key:: \"{0}\" Value: \"{1}\"", key, value);
                            table.Add(key, value);
                        }
                        
                    }
                }

            }
            success = true;
            return success;
        }
    }
}
