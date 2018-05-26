//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  MockTestHarness.cs - Mock version of Test Harness                                                                   //
//      --> Receive TestRequest from ChildBuilder and Files from repo                                                   //                                 //
//      --> Load the dll files by parsing the testRequest.xml sent and Get the output of test function of test driver   //
//      --> Send Log to repo                                                                                            //
//      --> Communicate with other modules using WCF                                                                    //
// version: 2.0                                                                                                         //
// Language:    C#, Visual Studio 2017                                                                                  //
// Platform:    Dell Inspiron 15, Windows 10                                                                            //
// Application: Remote Build Server Prototypes                                                                          //
//                                                                                                                      //
// Source: Dr. Jim Fawcett                                                                                              //
// Author Name : Dwivedi Nilesh                                                                                         //
// CSE681: Software Modeling and Analysis, Fall 2017                                                                    //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * -------------------
 * This package will act as a Test Harness and perform tests on the appropriate dll files 
 * 
 * Public Interface:
 * ================= 
 * void requestFile(string file) : Request file from the Repository
 * void sendFile(string to, string file) : Send file to another location from THStorage
 * void getMessege() : Receive the messege from other components and perform required actions
 * 
 * Build Process:
 * --------------
 * Required Files: MessegePassingCommService.cs, IMessegePassingCommService.cs,Logger.cs

 * Maintenance History:
    - Version 2.0 Nov 2017 
 * --------------------*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessagePassingComm;

namespace FedredationServer
{
    public class MockTestHarness
    {

        Comm c;
        int port;
        string from, path;
        Logger log;

        /*----< initialize Log file location in Logger class  file>---------------------------*/
        public MockTestHarness()
        {
            log = new Logger("../../../THStorage/THLog.txt");
            port = 8078;
            c = new Comm("http://localhost", port);
            from = "http://localhost:" + port + "/IPluggableComm";
            path = "../../../THStorage";
        }

        //Request file from the Repository
        void requestFile(string file)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "request";
            csndMsg.author = "Jim Fawcett";
            csndMsg.to = "http://localhost:8081/IPluggableComm";
            csndMsg.from = from;
            csndMsg.body = file + "," + path;
            c.postMessage(csndMsg);
        }

        //Send file to another location from THStorage
        void sendFile(string to, string file)
        {
            while (true)
            {
                if (File.Exists(path + "/" + file))
                {
                    break;
                }
            }

            bool transferSuccess = c.postFile(path, to, file);

        }

        //Receive the messege from other components and perform required actions
        public void getMessege()
        {
            while (true)
            {
                CommMessage csndMsg1 = c.getMessage();
                Console.WriteLine("Messege received");
                csndMsg1.show();
                if (csndMsg1.type.ToString() == "request")
                {
                    csndMsg1.show();
                    string testRequest = csndMsg1.body;
                    Console.WriteLine("Body is::" + testRequest);
                    XMLHandler x = new XMLHandler();
                    string testRequestContent = File.ReadAllText("../../../THStorage/" + testRequest);
                    x.loadXml(testRequestContent);
                    List<string> testDrivers = x.parseList("tested");
                    foreach(string temp in testDrivers)
                    {
                        Console.WriteLine("TEMP:" + temp);
                        requestFile(temp);
                    }
                    Thread.Sleep(2000);
                    DllLoader d = new DllLoader(log.getFileName());
                    d.loadDlls(testDrivers);
                    sendFile("../../../RepoStorage", "THLog.txt");
                }
            }
        }

        public static void Main(string[] args)
        {
            try
            {
                MockTestHarness th = new MockTestHarness();
                th.getMessege();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occured:" + e.Message);
            }

        }
    }




}
