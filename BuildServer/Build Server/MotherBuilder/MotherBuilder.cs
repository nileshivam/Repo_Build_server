////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MotherBuilder.cs -   This package contain all the functionality of Mother Builder and communicate with     //
// other modules using WCF.                                                                                   //
// version: 1.0                                                                                               //
// Language:    C#, Visual Studio 2017                                                                        //
// Platform:    Dell Inspiron 15, Windows 10                                                                  //
// Application: Remote Build Server Prototypes                                                                //
//                                                                                                            //
// Source: Dr. Jim Fawcett                                                                                    //
// Author Name : Dwivedi Nilesh                                                                               //
// CSE681: Software Modeling and Analysis, Fall 2017                                                          //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * -------------------
 * This package will act as a Mother Builder and receive messeges by other modules and process accordingly. 
 * 
 * Public Interface:
 * ================= 
 * void start() : Start the thread which will assign the buildrequest to child process
 * void getMessege() : Receive the messege from other components and perform required actions
 * void sendClose() : Send quit messege to child to kill the chuild process
 * bool createProcess(int port) : Create child process with the argument as port number
 * void createChild(int n_child) : It will create child according to the specified number
 * void killChild() : Kill all the child processes in the process pull
 * 
 * Build Process:
 * --------------
 * Required Files: MessegePassingCommService.cs, IMessegePassingCommService.cs

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/


using MessagePassingComm;
using SWTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project3Help
{
    public class MotherBuilder
    {
        Comm c = new Comm("http://localhost", 8080);
        List<int> pool = new List<int>();
        BlockingQueue<int> ready;
        BlockingQueue<string> buildQ;
        int n_child = 0;
         

        //Intialize reay and building queue
        public MotherBuilder()
        {
            ready = new BlockingQueue<int>();
            buildQ = new BlockingQueue<string>();
        }

        //Start the thread which will assign the buildrequest to child process
        public void start()
        {
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    CommMessage csndMsg3 = new CommMessage(CommMessage.MessageType.buildRequest);
                    csndMsg3.command = "BuildRequest";
                    csndMsg3.author = "Jim Fawcett";
                    csndMsg3.from = "http://localhost:" + "8080" + "/IPluggableComm";
                    csndMsg3.body = buildQ.deQ();
                    int port = ready.deQ();
                    csndMsg3.to = "http://localhost:" + port + "/IPluggableComm";
                    c.postMessage(csndMsg3);
                }

            });
            t.Start();
            getMessege();
        }


        //Receive the messege from other components and perform required actions
        void getMessege()
        {
            while (true)
            {
                CommMessage csndMsg3 = c.rcvr.getMessage();
                csndMsg3.show();
                if (csndMsg3.type.ToString() == "request")
                {
                    buildQ.enQ(csndMsg3.body);
                }
                if (csndMsg3.type.ToString() == "ready")
                {
                    ready.enQ(Int32.Parse(csndMsg3.body));
                }
                if (csndMsg3.type.ToString() == "quit")
                {
                    killChild();
                    Process.GetCurrentProcess().Kill();
                }

            }
        }


        //Send quit messege to child to kill the chuild process
        void sendClose(int port)
        {
            CommMessage csndMsg3 = new CommMessage(CommMessage.MessageType.quit);
            csndMsg3.command = "QUIT";
            csndMsg3.author = "Jim Fawcett";
            csndMsg3.from = "http://localhost:" + "8080" + "/IPluggableComm";
            //csndMsg3.body = buildQ.deQ();
            //int port = ready.deQ();
            csndMsg3.to = "http://localhost:" + port + "/IPluggableComm";
            csndMsg3.show();
            c.postMessage(csndMsg3);
        }

        //Create child process with the argument as port number
        bool createProcess(int portNumber)
        {
            Process p = new Process();
            string fileName = "..\\..\\..\\ChildBuilder\\bin\\debug\\ChildBuilder.exe";
            string absFileSpec = Path.GetFullPath(fileName);
            
            Console.Write("\n  attempting to start {0}", absFileSpec);
            string commandline = portNumber.ToString();
            try
            {
                p.StartInfo.FileName = fileName;
                p.StartInfo.Arguments = commandline;
                p.Start();
                pool.Add(p.Id);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }

            return true;
        }

        //It will create child according to the specified number
        void createChild(int n_child)
        {
            this.n_child = n_child;
            for (int i = 0; i < n_child; i++)
            {
                createProcess(8082 + i);
            }
        }

        //Kill all the child processes in the process pull
        void killChild()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                Process p = Process.GetProcessById(pool[i]);
                p.Kill();
            }
        }

        //Main method of mother builder
        static void Main(string[] args)
        {
            try
            {
                MotherBuilder mb = new MotherBuilder();
                mb.createChild(Int32.Parse(args[0]));
                mb.start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured:" + e.Message);
            }
        }
    }
}
