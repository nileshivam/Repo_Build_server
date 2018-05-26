//////////////////////////////////////////////////////////////////////////////////////////////////////////
// Repository.cs -   This package contain all the functionality of Respository and communicate with     //
// other modules using WCF.                                                                             //
// version: 2.0                                                                                         //
// Language:    C#, Visual Studio 2017                                                                  //
// Platform:    Dell Inspiron 15, Windows 10                                                            //
// Application: Remote Build Server Prototypes                                                          //
//                                                                                                      //
// Source: Dr. Jim Fawcett                                                                              //
// Author Name : Dwivedi Nilesh                                                                         //
// CSE681: Software Modeling and Analysis, Fall 2017                                                    //
//////////////////////////////////////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * -------------------
 * This package will act as a Repositiry and receive messeges by other modules and process accordingly. 
 * 
 * Public Interface:
 * ================= 
 * void sendFile(string to,string file) : Send file from repostorage to childBuilder to location "to".
 * void sendRequest() : Send Build Request to the Mother Builder
 * void getMessege() : get the imcoming messege from other modules and process it
 * List<String> filesWithPattern(String path, String pattern) : Search file with specified pattern in a path
 * 
 * Build Process:
 * --------------
 * Required Files: MessegePassingCommService.cs, IMessegePassingCommService.cs

 * Maintenance History:
    - Version 2.0 Oct 2017 
 * --------------------*/


using MessagePassingComm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FedredationServer;

namespace Project3Help
{
    public class Repository
    {
        Comm c;
        int port;
        Logger log;

        //Intialize port and Comm object
        Repository()
        {
            log = new Logger("../../../RepoStorage/RepoLog.txt");
            port = 8081;
            c = new Comm("http://localhost", port);
        
        }


        //Send file from repostorage to childBuilder
        void sendFile(string to, string file)
        {
            bool transferSuccess = c.postFile("../../../RepoStorage", to, file);

        }

        //Send Build Request to the Mother Builder
        void sendRequest(string fileName)
        {
            string testRequest = File.ReadAllText("../../../RepoStorage/" + fileName);
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "request";
            csndMsg.author = "Jim Fawcett";
            csndMsg.to = "http://localhost:8080/IPluggableComm";
            csndMsg.from = "http://localhost:8081/IPluggableComm";
            csndMsg.body = testRequest;
            c.postMessage(csndMsg);
        }

        //get the imcoming messege from other modules and process it
        public void getMessege()
        {
            while (true)
            {
                CommMessage csndMsg1 = c.getMessage();
                if (csndMsg1.type.ToString() == "request")
                {
                    csndMsg1.show();
                    string[] parts = csndMsg1.body.Split(',');
                    sendFile(parts[1], parts[0]);
                }
            }
        }

        //Search file with specified pattern in a path
        public List<String> filesWithPattern(String path, String pattern)
        {
            String[] files = Directory.GetFiles(path, pattern);

            for (int i = 0; i < files.Length; ++i)
            {
                files[i] = System.IO.Path.GetFileName(files[i]);  // now a FileName
            }
            return files.ToList<String>();
        }

        //Main method of Repo class that will run on the execution of exe of file of class
        static void Main(string[] args)
        {
            try
            {
                Repository repo = new Repository();
                List<string> fileNames = repo.filesWithPattern("../../../RepoStorage", "BuildRequest*");
                foreach(string file in fileNames)
                {
                    Console.WriteLine("FileName:" + file);
                    repo.sendRequest(file);
                }
                repo.getMessege();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occured:" + e.Message);
            }

        }
    }
}