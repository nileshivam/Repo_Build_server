/////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - GUI which represent the client interaction //
// ver 1.0                                                         //
// Language:    C#, Visual Studio 2017                             //
// Platform:    Dell Inspiron 15, Windows 10                       //
// Application: Build Server                                       //
//                                                                 //
// Author Name : Dwivedi Nilesh                                    //
// Source: Dr. Jim Fawcett                                         //
// CSE681: Software Modeling and Analysis, Fall 2017               //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * -------------------
 * This package will contain functionality for the GUI handler
 * It will handler each event of GUI to interaction with client
 * 
 *  Interface information:
 *    void initializeFilesList() - this method initializes first tab second list box
 *    initializeTestDriverList() - initializes second tab list box
 *    void sendButton_Click(object sender, RoutedEventArgs e) - decides action to be performed when sender button is clicked
 *    AddTestButton_Click(object sender, RoutedEventArgs e) -decides action to be performed when create test request button is clicked
 *    Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) - decides action to be performed before closing window
 *    AddTestButton_Click(object sender, RoutedEventArgs e) - decides action to be performed when add test button is clicked 
 *    testDriverListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) - decides action to be performed when anything in test files list box in first tab is double clicked
 *    void Refresh_Click(object sender, RoutedEventArgs e) : Refresh the fileslist of build log
 *    void Test_Refresh_Click(object sender, RoutedEventArgs e) : Refresh the fileslist of test log
 *    void logListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) : function for viewing the build logs
*     void testLogListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) : function for viewing the test logs
*     void initializeLogListBox() : initialize list box in Build tab
*     void initializeTestLogListBox() : initialize list box in test tab
*     void requestFileClick(object sender, RoutedEventArgs e) : function for loading the file list when client request for
 *    
 *    
 *
 * Build Process:
 * --------------
 * Required Files: MessegePassingCommService.cs, IMessegePassingCommService.cs, XMLHandler.cs

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/



using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using MessagePassingComm;
using FedredationServer;

namespace PluggableRepoClient
{
    using FileSpec = String;  // c:/.../category/filename
    using FileRef = String;   // category/filename
    using FileName = String;  // filename may have version number at end

    public partial class MainWindow : Window
    {

        List<Window1> popups = new List<Window1>();
        List<List<string>> files=new List<List<string> >();
        List<string> testDriver = new List<string>();
        Comm c;
        

        //Intialize the file listbox
        void initializeFilesList()
        {
        
            List<string> files = getFiles("../../../RepoStorage/");
            foreach (string file in files)
            {
                if (file.Contains(".cs") && !file.Contains("Driver"))
                    filesListBox.Items.Add(file);
            }
        
            statusLabel.Text = "Action: Show file contains by double clicking on fileName";
        }

        //initializes test Driver list box
        void initializeTestDriverList()
        {

            List<string> files = getFiles("../../../RepoStorage/");
            foreach (string file in files)
            {
                if (file.Contains("Driver") && file.Contains(".cs"))
                    testDriverListBox.Items.Add(file);
            }

        }

        //Get the files in specific path
        public List<FileName> getFiles(String path, bool showPath = false)
        {

            FileSpec[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName("../../../RepoStorage/"));

            for (int i = 0; i < files.Length; ++i)
            {

                if (showPath)
                    files[i] = System.IO.Path.GetFullPath(files[i]);  // now an absolute FileSpec
                else
                    files[i] = System.IO.Path.GetFileName(files[i]);  // now a FileName
            }
            return files.ToList<FileName>();
        }

        //It find the files with pattern in specified path
        public List<FileName> filesWithPattern(String path, String pattern)
        {

            FileSpec[] files = Directory.GetFiles(path, pattern);

            for (int i = 0; i < files.Length; ++i)
            {
                files[i] = System.IO.Path.GetFileName(files[i]);  // now a FileName
            }
            return files.ToList<FileName>();
        }

        //Main window to intialize components
        public MainWindow()
        {
            c = new Comm("http://localhost", 8079);
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            initializeTestLogListBox();
            initializeLogListBox();
        }
        

        //Decide the action to be performed when send button is clicked
        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            statusLabel.Text = "Status: Sending selected file to MockBuilder";
        }
        

        //decides the action to be performed when generate test request button is clicked
        private void CreateTestRequestButton_Click(object sender, RoutedEventArgs e)
        {
            XMLHandler testRequestXML = new XMLHandler();
            testRequestXML.author = "Nilesh Dwivedi";
            testRequestXML.testDriver = testDriver;
            testRequestXML.testFiles = files;
            testRequestXML.makeRequest();
            Random rng = new Random();
            int value = rng.Next(1000);
            string text = value.ToString("000");
            string filename = "../../../RepoStorage/BuildRequest"+text+".xml";
            testRequestXML.saveXml(filename);
            files.Clear();
            testDriver.Clear();
            TestRequest.IsEnabled = false;
            statusLabel.Text = "Status: Build Request Created with name " + filename;
        }



        //decides the action to be performed when add test button is clicked 
        private void AddTestButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)testDriverListBox.SelectedItem == null || filesListBox.SelectedItems.Count == 0)
                statusLabel.Text = "Status: Please select Test Driver and Code to test";
            else
            {
                testDriver.Add((string)testDriverListBox.SelectedItem);
                List<string> temp = new List<string>();
                foreach (string selectedItem in filesListBox.SelectedItems)
                {
                    temp.Add(selectedItem);
                }
                files.Add(temp);
                statusLabel.Text = "Status: Test has added";
                TestRequest.IsEnabled = true;
                testDriverListBox.UnselectAll();
                filesListBox.UnselectAll();
            }
        }

        //decides what to be done when show code button is clicked 
        private void showCodeButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);
        }

        //decides action to be peformed when anything in test files list box in first tab is double clicked 
        private void filesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);

            codePopup.codeView.Blocks.Clear();
            string fileName = (string)filesListBox.SelectedItem;

            codePopup.codeLabel.Text = "Source code is: " + fileName;
      
            showFile(fileName, codePopup);
            return;
        }

        //decides action to be performed when anything in test driver list box in first tab is double clicked 
        private void testDriverListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);

            codePopup.codeView.Blocks.Clear();
            string fileName = (string)testDriverListBox.SelectedItem;

            codePopup.codeLabel.Text = "Source code: " + fileName;

            showFile(fileName, codePopup);
            return;
        }

        //Shows file in other window from popup
        private void showFile(string fileName, Window1 popUp)
        {
            string path = System.IO.Path.Combine("../../../RepoStorage/", fileName);
            Paragraph paragraph = new Paragraph();
            string fileText = "";
            try
            {
                fileText = System.IO.File.ReadAllText(path);
            }
            finally
            {
                paragraph.Inlines.Add(new Run(fileText));
            }
            popUp.codeView.Blocks.Clear();
            popUp.codeView.Blocks.Add(paragraph);
        }


        //decides action to be peformed if window close command
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var popup in popups)
                popup.Close();
        }

        //decides action to be peformed when stop builder button is clicked by user
        private void StopBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.quit);
            csndMsg.command = "quit";
            csndMsg.author = "Jim Fawcett";
            csndMsg.to = "http://localhost:8080/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";
            csndMsg.body = "Close";
            c.postMessage(csndMsg);
        }
        
        //decides action to be perfomed when start builder button is clicked 
        private void StartBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            if(noOfProcessesTextBox.Text == "")
                statusLabel.Text = "Status: Please enter value less than 10 and greater than 0 in text box";
            else
            { 
            if (Convert.ToInt32(noOfProcessesTextBox.Text) > 10 || Convert.ToInt32(noOfProcessesTextBox.Text) <= 0)
                statusLabel.Text = "Status: Please enter value less than 10 in text box" ;
            else
            { 
                if (createMother(noOfProcessesTextBox.Text))
                {
                    Console.Write(" - succeeded");
                }
                else
                {
                    Console.Write(" - failed");
                }


                if (createTH())
                {
                    Console.Write(" - succeeded");
                }
                else
                {
                    Console.Write(" - failed");
                }
            }
            }
        }

        //It will create and execute repository
        bool createRepo()
        {
            Process proc = new Process();
            string fileName = "..\\..\\..\\Project3V1\\bin\\debug\\Project3V1.exe";
            string absFileSpec = System.IO.Path.GetFullPath(fileName);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            
            try
            {
                Process.Start(fileName);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }


        //creates mother builder process
        bool createMother(string numberOfProcesses)
        {
            
            createRepo();
            string fileName = "..\\..\\..\\MotherBuilder\\bin\\debug\\MotherBuilder.exe";
            string absFileSpec = System.IO.Path.GetFullPath(fileName);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            string commandline = numberOfProcesses;
            try
            {
                Process.Start(fileName, commandline);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }

        //creates Test Harness process
        bool createTH()
        {
            string fileName = "..\\..\\..\\MockTestHarness\\bin\\debug\\MockTestHarness.exe";
            string absFileSpec = System.IO.Path.GetFullPath(fileName);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            string commandline = "";
            try
            {
                Process.Start(fileName, commandline);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }


        //Refresh the fileslist of build log
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            logListBox.Items.Clear();
            String pattern = "BuildLog*";
            List<string> files = filesWithPattern("../../../ClientFileStore/", pattern);
            foreach (string file in files)
            {
                logListBox.Items.Add(file);
            }

        }


        //Refresh the fileslist of test log
        private void Test_Refresh_Click(object sender, RoutedEventArgs e)
        {
            testLogListBox.Items.Clear();
            String pattern = "TestLog*";
            List<string> files = filesWithPattern("../../../ClientFileStore/", pattern);
            foreach (string file in files)
            {
                testLogListBox.Items.Add(file);
            }
        }

        //function for viewing the build logs
        private void logListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);

            codePopup.codeView.Blocks.Clear();
            string fileName = (string)logListBox.SelectedItem;

            codePopup.codeLabel.Text = "Source code: " + fileName;

            showFile(fileName, codePopup);
            return;
        }

        //function for viewing the test logs
        private void testLogListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);

            codePopup.codeView.Blocks.Clear();
            string fileName = (string)testLogListBox.SelectedItem;

            codePopup.codeLabel.Text = "Source code: " + fileName;

            showFile(fileName, codePopup);
            return;
        }

        
        //initialize list box in Build tab
        private void initializeLogListBox()
        {
            logListBox.Items.Clear();
            String pattern = "*BuildLog*";
            List<string> files = filesWithPattern("../../../RepoStorage/", pattern);
            foreach (string file in files)
            {
                logListBox.Items.Add(file);
            }
            statusLabel.Text = "Hit refresh to view recent build logs";
        }

        //initialize list box in test tab
        private void initializeTestLogListBox()
        {
            testLogListBox.Items.Clear();
            String pattern = "THLog*";
            List<string> files = filesWithPattern("../../../RepoStorage/", pattern);
            foreach (string file in files)
            {
                testLogListBox.Items.Add(file);
            }
            statusLabel.Text = "Hit refresh to view recent test logs";
        }

        //function for loading the file list when client request for
        private void requestFileClick(object sender, RoutedEventArgs e)
        {
            initializeFilesList();
            initializeTestDriverList();
        }



    }
}
