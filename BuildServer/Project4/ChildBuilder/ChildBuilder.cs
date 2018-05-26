/////////////////////////////////////////////////////////////////////////////////////////////
// ChildBuilder.cs -   This package contain all the functionality of child Process         //
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
 * This package will act as a child Process which is created by mother builder and receive messeges by other modules and process accordingly. 
 * 
 * Public Interface:
 * ================= 
 * void requestFile(string file) : send Request file messege to Repository
 * bool sendFile(string to,string file) : Function for sending file from childBuilder location to another location
 * void sendToTH() : Send the test request to test Harness
 * void sendReady() : Send ready messege to Mother Builder which indicate that child is ready
 * void getMessege() : Get incoming messege from other modules and perform required actions
 * 
 * 
 * Build Process:
 * --------------
 * Required Files: MessegePassingCommService.cs, IMessegePassingCommService.cs, Logger.cs

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/

using MessagePassingComm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FedredationServer;
using System.Diagnostics;
using System.IO;

namespace ChildBuilder
{
    class ChildBuilder
    {
        Comm c;
        int port;
        string path, from;
        Logger log;


        //Set the initial values of childBuilder
        ChildBuilder(int port)
        {
            Console.WriteLine("Child Created with Port:"+port);
            this.port = port;
            c = new Comm("http://localhost", port);
            path = "../../../" + port + "storage";
            from = "http://localhost:" + port + "/IPluggableComm";
            System.IO.Directory.CreateDirectory(path);
            log = new Logger(path + "/" + port + "BuildLog.txt");
            sendReady();
        }


        //send Request file messege to Repository
        void requestFile(string file)
        {
            Console.WriteLine("Reuesting File from Repository" + file);
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "request";
            csndMsg.author = "Jim Fawcett";
            csndMsg.to = "http://localhost:8081/IPluggableComm";
            csndMsg.from = from;
            csndMsg.body = file + "," + path;
            c.postMessage(csndMsg);
            Console.WriteLine("Successfully Received file" + file);
        }

        //Function for sending file from childBuilder location to another location
        bool sendFile(string to,string file)
        {
            while (true)
                if (File.Exists(path + "/" + file)) 
                    break;

            bool transferSuccess = c.postFile(path,to, file);
            log.logWithConsole("Sent file:" + file);
            return transferSuccess;
        }

        //Send the test request to test Harness
        void sendToTH()
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "request";
            csndMsg.author = "Jim Fawcett";
            csndMsg.to = "http://localhost:8078/IPluggableComm";
            csndMsg.from = from;
            csndMsg.body = "testRequest-" + port + ".xml";
            c.postMessage(csndMsg);
        }

        //Send ready messege to Mother Builder which indicate that child is ready
        void sendReady()
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.ready);
            csndMsg.command = "show";
            csndMsg.author = "Jim Fawcett";
            csndMsg.to = "http://localhost:8080/IPluggableComm";
            csndMsg.from = from;
            csndMsg.body = port.ToString();
            c.postMessage(csndMsg);
        }

        //Get incoming messege from other modules and perform required actions
        void getMessege()
        {
            while (true)
            {
                CommMessage csndMsg = c.getMessage();
                csndMsg.show();
                if (csndMsg.type.ToString() == "quit")
                    Process.GetCurrentProcess().Kill();
                if (csndMsg.type.ToString() == "buildRequest")
                {
                    List<string> fileNames = new List<string>();
                    MockBuild mb = new MockBuild(csndMsg.body, path, port + "BuildLog.txt");
                    List<Tuple<string, bool>> files = mb.runTests();
                    List<string> testDrivers = new List<string>();
                    foreach (Tuple<string, bool> file in files)
                    {
                        if(file.Item2)
                        {
                            string temp = file.Item1.Split('.')[0];
                            temp += ".dll";
                            testDrivers.Add(temp);
                            if (fileNames.Count != 0)
                                mb.startBuild(fileNames, path);
                            fileNames.Clear();
                        }
                        requestFile(file.Item1);
                        fileNames.Add(file.Item1);
                    }
                    mb.startBuild(fileNames, path);
                    mb.createXML(testDrivers, path, port);
                    Thread.Sleep(2000);
                    sendFile("../../../THStorage", "testRequest-" + port + ".xml");
                    foreach (string temp in testDrivers)
                        sendFile("../../../RepoStorage", temp);
                    Thread.Sleep(2000);
                    sendFile("../../../RepoStorage", port + "BuildLog.txt");
                    sendToTH();
                    log.logWithConsole("Sent Messege to TH");
                    sendReady();
                }
            }
        }


        //Main method in ChildBuilder Class but as For No receiver it will not work
        static void Main(string[] args)
        {
            try
            {
                ChildBuilder cb = new ChildBuilder(Int32.Parse(args[0]));
                cb.getMessege();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occured:" + e.Message);
            }
        }
    }
}