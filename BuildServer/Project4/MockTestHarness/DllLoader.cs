//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  DllLoder.cs -   Demonstrate Robust loading and dynamic invocation of                                                //
//                  Dynamic Link Libraries found in specified location                                                  //
//                  Tests now return bool for pass or fail                                                              //
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
 * This package will Load the dll files and perform required actions 
 * 
 * Public Interface:
 * ================= 
 * bool checkTestDriver(Type t) : heck the type implements SampleTest.ITest interface or not. If it does then type T will be TestDriver class.
 * void loadDlls(List<string> dllFiles) : Send to load the perticular dll file in list of DLL
 * void loadDll(string fileName) : Load a perticular dll file an then run test on that
 * bool runSimulatedTest(Type t, Assembly asm) : Create instance of Type t and get the ouput of functio test to determine result of test
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
using System.Threading.Tasks;

namespace FedredationServer
{
    public class DllLoader
    {

        Logger log;

        //SEt the initial value of Logger
        public DllLoader(string fileName)
        {
            log = new Logger(fileName);
        }

        /*----<Check the type implements SampleTest.ITest interface or not. If it does then type T will be TestDriver class.  >----*/
        bool checkTestDriver(Type t)
        {
            bool b = t.GetInterface("SampleTest.ITest", true) != null;
            return b;
        }

        /*----<Send to load the perticular dll file in list of DLL >----*/
        public void loadDlls(List<string> dllFiles)
        {
            foreach (string dllFile in dllFiles)
            {
                loadDll("../../../THStorage/" + dllFile);
            }
        }

        /*----<Load a perticular dll file an then run test on that>----*/
        public void loadDll(string fileName)
        {
            log.logWithConsole("Starting to load dll file with name:" + fileName);

            Assembly assem = Assembly.Load(File.ReadAllBytes(fileName));
            Type[] types = assem.GetTypes();

            bool b = false;

            foreach (Type t in types)
            {
                if (checkTestDriver(t))
                {
                    b = true;
                    runSimulatedTest(t, assem);
                }
            }

            if(!b)
            {
                throw new Exception("Could not find class for testDriver");
            }
        }


        /*----< Create instance of Type t and get the ouput of functio test to determine result of test>----*/
        bool runSimulatedTest(Type t, Assembly asm)
        {
            try
            {
                log.logWithConsole(" attempting to create instance of {0}"+ t.ToString());
                object obj = asm.CreateInstance(t.ToString());
                
                bool status = false;
                MethodInfo method = t.GetMethod("test");
                if (method != null)
                    status = (bool)method.Invoke(obj, new object[0]);
                else
                    throw new Exception("Could not find the method TEST in test driver");
                Func<bool, string> act = (bool pass) =>
                {
                    if (pass)
                        return "Passed";
                    return "Failed";
                };
                log.logWithConsole("The Result of this test:" + act(status));
            }
            catch (Exception ex)
            {
                if(ex is TargetInvocationException)
                {
                    throw new Exception("Test Failed with message:" + ex.Message+" \nException in the target file is:"+ex.InnerException);
                }
                throw new Exception("Test Failed with message:" + ex.Message);
            }
            return true;
        }
    }

#if (TEST_DLLLOADER)
    public class TestDllLoader
    {
        /*----< For test It will Load the dll file in THStorage/demo.dll>---------------------------*/
        public static void Main(string[] args)
        {
            try
            {
                List<string> testList = new List<string>();
                DllLoader d = new DllLoader("../../../RepoStorage/Test/TestFile.txt");
                d.loadDll("../../../RepoStorage/Test/demo.dll");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occured:" + e.Message);
            }
            
        }
    }
#endif
}
