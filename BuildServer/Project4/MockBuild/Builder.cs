/////////////////////////////////////////////////////////////////////////////////////////////
// Builder.cs - Build C# files and combine them into one dll file using CSC                //
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
 * This package will Build the list of c# files into one dll
 * 
 * Public Interface:
 * ================= 
 * bool checkFileExist(string file) : Check If specified file Exist Throw Exception If not
 * void checkforFiles(List<string> files,string path) : Check If specified file Exist Throw Exception If not
 * public bool BuildFileUsingCSC(List<string> files,string workDir) : Build files using CSC with testDriver and testfiles
 * 
 * Build Process:
 * --------------
 * Required Files: Loggger.cs

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FedredationServer
{
    public class Builder
    {
        Logger log;

        //Set intitial value of logger
        public Builder(string fileName)
        {
            log = new Logger(fileName);
        }

        /*----< Check If specified file Exist Throw Exception If not>---------------------------*/
        bool checkFileExist(string file)
        {
            if (!File.Exists(file))
            {
                throw new Exception("File not Available:" + file);
            }
            return true;
        }

        /*----< Check for List of files to be Exist or not>---------------------------*/
        void checkforFiles(List<string> files,string path)
        {
            foreach(string file in files)
            {
                string filePath= Path.Combine(path, file);
                checkFileExist(filePath);
            }
        }
       
        /*----< Build files using CSC with testDriver and testfiles>---------------------------*/
        public bool BuildFileUsingCSC(List<string> files,string workDir= "../../../BuildStorage")
        {
            Console.WriteLine("Start building");
            foreach(string temp in files)
            {
                Console.WriteLine("File:" + temp);
            }

            checkforFiles(files,workDir);
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            string command = "/Ccsc /target:library ";
 
            foreach (string file in files)
            {
                command += " " + file;
            }
            p.StartInfo.Arguments = command;

            p.StartInfo.WorkingDirectory = workDir;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.WaitForExit();

            string errors = p.StandardError.ReadToEnd();
            string output = p.StandardOutput.ReadToEnd();
            
            if (p.ExitCode == 0)
            {
                log.logWithConsole("Successfully Builded files without Error");
                log.logWithConsole("Build Successful for these files:");
                log.logWithConsole("Output of Build is:" + output);
                log.logWithConsole("Error of Build is:" + errors);
                return true;
            }
            else
            {
                log.logWithConsole("Output of Build is:" + output);
                throw new Exception("Build UnSuccessful for these files because of Compile time error");
            }
        }
    }

#if (TEST_BUILDER)
    public class TestBuilder
    {
        /*----< Set Logger and Buildfile TestFile1.csc in the BuildStorage Directory>---------------------------*/
        public static void Main(string[] args)
        {
            try
            {
                Logger.setVar("../../../RepoStorage/Test/testLogger.txt");
                string[] temp = { "demo.cs" };
                Builder buiObj=new Builder("../../../RepoStorage/Test/testLogger.txt");
                buiObj.BuildFileUsingCSC(new List<string>(temp), "../../../RepoStorage/Test");
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception Occured:" + e.Message);
            }
            
        }
    }
#endif
}
