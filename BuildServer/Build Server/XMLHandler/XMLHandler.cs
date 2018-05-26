/////////////////////////////////////////////////////////////////////////////////////////////
// XMLHandler.cs -   Handle the functionalities related to XML                             //
//    --> Create and Build XML file                                                        //
//    --> Parse the XML file                                                               //
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
 * 
 * 
 * Module Operations:
 * -------------------
 * This package will Handle all the functionalities related to XML. 
 * 
 * 
 * 
 * Public Interface:
 * ================= 
 * void makeRequest() : Request to create XML according to the componets
 * void makeTestRequest() : build XML document that represents a test request
 * List<string> parseList(string propertyName) : parse document for property list
 * void loadXml(string xmlString) : load TestRequest from XML file 
 * void saveXml(string path) : save TestRequest to XML file
 * List<Tuple<string, bool>> parseTest(XElement e, string element, bool b) : parse document for property list of in a pericular test
 * List<Tuple<string, bool>> parseForFiles() : It will parse the test cases and return the testdriver and tested files
 * void show() : Show the content of parseForFiles of XMl
 * 
 * 
 * Build Process:
 * --------------
 * Required Files: Loggger.cs

 * Maintenance History:
    - Version 2.0 Oct 2017 
 * --------------------*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FedredationServer
{
    public class XMLHandler
    {

        public string author { get; set; } = "";
        public string dateTime { get; set; } = "";

        public string testDriver1 { get; set; } = "";
        public List<string> testDriver { get; set; } = new List<string>();
        public List<string> testedFiles { get; set; } = new List<string>();

        public List<List<string>> testFiles { get; set; } = new List<List<string>>();

        public XDocument doc { get; set; } = new XDocument();



        /*----< build XML document that represents a build request >----*/
        public void makeRequest()
        {
            XElement testRequestElem = new XElement("testRequest");
            doc.Add(testRequestElem);

            XElement authorElem = new XElement("author");
            authorElem.Add(author);
            testRequestElem.Add(authorElem);

            XElement dateTimeElem = new XElement("dateTime");
            dateTimeElem.Add(DateTime.Now.ToString());
            testRequestElem.Add(dateTimeElem);

            for (int i = 0; i < testDriver.Count; i++)
            {
                XElement testElem = new XElement("test");
                testRequestElem.Add(testElem);
                XElement driverElem = new XElement("testDriver");
                driverElem.Add(testDriver[i]);
                testElem.Add(driverElem);

                for (int j = 0; j < testFiles[i].Count; j++)
                {
                    XElement testedElem = new XElement("tested");
                    testedElem.Add(testFiles[i][j]);
                    testElem.Add(testedElem);
                }
            }
        }

        /*----< build XML document that represents a test request >----*/
        public void makeTestRequest()
        {
            XElement testRequestElem = new XElement("testRequest");
            doc.Add(testRequestElem);

            XElement authorElem = new XElement("author");
            authorElem.Add(author);
            testRequestElem.Add(authorElem);

            XElement dateTimeElem = new XElement("dateTime");
            dateTimeElem.Add(DateTime.Now.ToString());
            testRequestElem.Add(dateTimeElem);

            for (int i = 0; i < testDriver.Count; i++)
            {  
                XElement driverElem = new XElement("tested");
                driverElem.Add(testDriver[i]);
                testRequestElem.Add(driverElem);
            }
        }

        /*----< parse document for property list >-------------*/
        public List<string> parseList(string propertyName)
        {
            List<string> values = new List<string>();

            IEnumerable<XElement> parseElems = doc.Descendants(propertyName);


            if (parseElems.Count() > 0)
            {
                switch (propertyName)
                {
                    case "tested":
                        foreach (XElement elem in parseElems)
                        {
                            values.Add(elem.Value);
                        }
                        testedFiles = values;
                        break;
                    case "testDriver":
                        foreach (XElement elem in parseElems)
                        {
                            values.Add(elem.Value);
                        }
                        testDriver = values;
                        break;
                    default:
                        break;
                }
            }
            return values;
        }

        /*----< load TestRequest from XML file >-----------------------*/
        public void loadXml(string xmlString)
        {
            //doc = XDocument.Load(path);
            doc = XDocument.Parse(xmlString);
            
        }

        /*----< save TestRequest to XML file >-------------------------*/
        public void saveXml(string path)
        {
            doc.Save(path);
        }

        /*----< parse document for property list of in a pericular test>---------------------*/
        public List<Tuple<string, bool>> parseTest(XElement e, string element, bool b)
        {
            List<Tuple<string, bool>> testedFiles = new List<Tuple<string, bool>>();

            foreach (XElement file in e.Descendants(element))
            {
                testedFiles.Add(new Tuple<string, bool>(file.Value, b));

            }
            return testedFiles;
        }

        /*----< It will parse the test cases and return the testdriver and tested files >---------------------*/
        public List<Tuple<string, bool>> parseForFiles()
        {
            List<Tuple<string, bool>> testedFiles = new List<Tuple<string, bool>>();

            IEnumerable<XElement> testElements = doc.Root.Descendants("test");

            foreach (XElement x in testElements)
            {
                testedFiles.AddRange(parseTest(x, "testDriver", true));
                testedFiles.AddRange(parseTest(x, "tested", false));
            }
            return testedFiles;
        }

        //Show the content of parseForFiles of XMl
        public void show()
        {
            List<Tuple<string, bool>> files = parseForFiles();
            foreach (Tuple<string, bool> file in files)
            {
                Console.WriteLine("File Name in XML:" + file.Item1);
            }
        }

    }

#if (TEST_XML)

    class Test_TR
    {
        /*----< Create one XML file and Print that >-------------*/
        static void Main(string[] args)
        {
            try
            {
                XMLHandler tr = new XMLHandler();
                tr.author = "Dwivedi Nilesh";
                tr.testDriver.Add("testDriver15.cs");
                List<string> temp = new List<string>();
                temp.Add("testfile1.cs");
                temp.Add("testfile2.cs");
                temp.Add("testfile3.cs");
                tr.testFiles.Add(temp);
                tr.makeRequest();
                tr.saveXml("../../../RepoStorage/TestRequest123.xml");
                XMLHandler tr1 = new XMLHandler();
                tr1.loadXml(File.ReadAllText("../../../RepoStorage/TestRequest123.xml"));
                tr1.show();
            }
            catch (Exception e)
            {
                Console.WriteLine("Excetion occured:" + e.Message);
            }
            
        }
    }
    
#endif
}
