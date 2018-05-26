/////////////////////////////////////////////////////////////////////////////////////////////
// MockBuild.cs -   Mock version of Build Server                                           //
//    --> Accept XML string, parse it and get the appropriate files to request to repo     //
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
 * This package will contain Builder Functionality like parsing and also resemble of the build process. 
 * 
 * Public Interface:
 * ================= 
 * void runTests() : Parse BuildRequest and send each testdriver with tested files to build
 * void startBuild() : Build testDriver and tested files togeather and make single dll out of them.
 * void createXML(List<string> testDrivers,string path,int port) : Create an XML using the testdrive and save it in specified location
 * 
 * Build Process:
 * --------------
 * Required Files: MessegePassingCommService.cs, IMessegePassingCommService.cs, XMLHandler.cs,logger.cs

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/




using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FedredationServer
{
    public class MockBuild
    {
        XMLHandler buildRequestXML = new XMLHandler();
        List<string> testDrivers = new List<string>();
        string workDir;
        Logger log;

        /*----< initialize Log file location in Logger class  file and Load BuildRequest.xml>----*/
        public MockBuild(string XMLString, string workDir, string logFile)
        {
            Console.WriteLine("Log file:" + workDir + "/" + logFile);
            this.workDir = workDir;
            log = new Logger(workDir + "/" + logFile);
            buildRequestXML.loadXml(XMLString);
        }

        /*----<Parse BuildRequest and send each testdriver with tested files to build >----*/
        public List<Tuple<string, bool>> runTests()
        {
            List<Tuple<string, bool>> files = buildRequestXML.parseForFiles();
            return files;

        }

        /*----<Build testDriver and tested files togeather and make single dll out of them. >----*/
        public void startBuild(List<string> files, string path)
        {
            Thread.Sleep(2000);
            Console.WriteLine("Start the building process");
            Builder bui = new Builder(log.getFileName());
            bui.BuildFileUsingCSC(files, path);
            log.logWithConsole("Build Success");
        }

        //Create an XML using the testdrive and save it in specified location
        public void createXML(List<string> testDrivers,string path,int port)
        {
            XMLHandler x = new XMLHandler();
            x.author = "Dwivedi Nilesh";
            List<string> temp = new List<string>();
            x.testDriver = testDrivers;
            for (int i = 0; i < testDrivers.Count; i++) 
            {
                x.testFiles.Add(temp);
            }
            x.makeTestRequest();
            string xmlpath = Path.Combine(path, "testRequest-" + port + ".xml");
            Console.WriteLine("XML::" + xmlpath);
            x.saveXml(xmlpath); 
        }
    }


#if(TEST_MOCKBUILD)
    public class TestMockBuild
    {
        /*----< Create instance of MockRepo and Mockbuild and start the build process. For this BuildStorage should Contain buildRequest with appropriate files>---------------------------*/
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Creating XML File");
                XMLHandler tr = new XMLHandler();
                tr.author = "Dwivedi Nilesh";
                tr.testDriver.Add("testDriver15.cs");

                List<string> temp = new List<string>();
                temp.Add("testfile1.cs");
                temp.Add("testfile2.cs");
                temp.Add("testfile3.cs");
                tr.testFiles.Add(temp);
                tr.makeRequest();

                MockBuild mb = new MockBuild(tr.doc.ToString(), ".../../../RepoStorage", "testBuild.txt");
                List<Tuple<string, bool>> L = mb.runTests();
                foreach (Tuple<string, bool> x in L)
                {
                    Console.WriteLine("File in XML:" + x.Item1);
                }

                string[] tempList = { "demo.cs" };

                mb.startBuild(new List<string>(temp), "../../../RepoStorage/Test");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occured:" + e.Message);
            }
        }
    }
#endif
}


