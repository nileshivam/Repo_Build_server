/////////////////////////////////////////////////////////////////////////////////////////////
// Logger.cs -   Log each activity of modules to log file.                                 //
//               The class has static variable which can be set by setVar Function.        //
//               Using this statoic variable log function will write in the log            //
// version: 2.0                                                                            //
// Language:    C#, Visual Studio 2017                                                     //
// Platform:    Dell Inspiron 15, Windows 10                                               //
// Application: Remote Build Server Prototypes                                             //
//                                                                                         //
// Source: Dr. Jim Fawcett                                                                 //
// Author Name : Dwivedi Nilesh                                                            //
// CSE681: Software Modeling and Analysis, Fall 2017                                       //
/////////////////////////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * -------------------
 * This package will Log each activity of modules to log file.
 * 
 * Public Interface:
 * ================= 
 * void setVar(string fileName) : Set the fileName Variable which is the file in which the log is to be written
 * void log(string fileName, String lines) : Print lines on console as well as write it to Log File 
 * void logOnly(string fileName,string lines) : Write the argument to the log File
 *  void logWithConsole(string lines): Call the Log function with the Class level fileName variabele 
 * 
 * Build Process:
 * --------------
 * Required Files: 

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace FedredationServer
{
    public class Logger
    {

        string fileName = "";

        //Set FileName to test location
        public Logger()
        {
            fileName = "../../../RepoStorage/Test/testLogger.txt";
        }

        //Get the filename of the logger object
        public string getFileName()
        {
            return fileName;
        }

        //Set fileName to specified File
        public Logger(string fileName)
        {
            this.fileName = fileName;
        }


        /*----< Set the fileName Variable which is the file in which the log is to be written >---------------------------*/
        public void setVar(string fileName)
        {
            this.fileName = fileName;
        }


        /*----< Print lines on console as well as write it to Log File >---------------------------*/
        public void log(string fileName, String lines)
        {
            Console.WriteLine(lines + "\n");
            logOnly(fileName, lines);
        }

        /*----< Write the argument to the log File >---------------------------*/
        public void logOnly(string fileName, string lines)
        {

            System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true);
            lines += "\n";
            string time = DateTime.Now.ToString("dd/mm/yyyy h:mm:ss tt");
            file.WriteLine(time + ": " + lines);
            file.Close();
        }

        /*----< Call the Log function with the Class level fileName variabele   >---------------------------*/
        public void logWithConsole(string lines)
        {
            if (fileName == "")
                fileName = "../../../RepoStorage/Test/testLogger.txt";
            log(fileName, lines);
        }
    }

#if (TEST_LOGGER)
    public class TestLogger

    {
        /*----< Intialize the logger and log some text to test>---------------------------*/
        public static void Main(string[] args)
        {
            Logger log = new Logger(); 
            log.logWithConsole("Logger worked");
        }
    }
#endif
}
