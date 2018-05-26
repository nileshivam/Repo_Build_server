//////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - GUI for Project3HelpWPF                             //
// ver 2.0                                                                  //
// Author : Dwivedi Nilesh , CSE687 - Object Oriented Design, Spring 2018   //
// Source: Jim Fawcett,													    //
//////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package provides a WPF-based GUI for Project3HelpWPF demo.  It's 
 * responsibilities are to:
 * - Provide a display of directory contents of a remote ServerPrototype.
 * - It provides a subdirectory list and a filelist for the selected directory.
 * - You can navigate into subdirectories by double-clicking on subdirectory
 *   or the parent directory, indicated by the name "..".
 *   
 * Required Files:
 * ---------------
 * Mainwindow.xaml, MainWindow.xaml.cs
 * Translater.dll
 * 
 * Maintenance History:
 * --------------------
 * ver 2.0 : 22 Apr 2018
 * - added tabbed display
 * - moved remote file view to RemoteNavControl
 * - migrated some methods from MainWindow to RemoteNavControl
 * - added local file view
 * - added NoSqlDb with very small demo as server starts up
 * ver 1.0 : 30 Mar 2018
 * - first release
 * - Several early prototypes were discussed in class. Those are all superceded
 *   by this package.
 */

// Translater has to be statically linked with CommLibWrapper
// - loader can't find Translater.dll dependent CommLibWrapper.dll
// - that can be fixed with a load failure event handler
using System;
using System.Collections.Generic;
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
using System.Threading;
using System.IO;
using MsgPassingCommunication;

namespace WpfApp1
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      Console.Title = "Project4 - Dwivedi Nilesh";
    }

    private Stack<string> pathStack_ = new Stack<string>();
    internal Translater translater;
    internal CsEndPoint endPoint_;
    private Thread rcvThrd = null;
    private Dictionary<string, Action<CsMessage>> dispatcher_ 
      = new Dictionary<string, Action<CsMessage>>();
    internal string saveFilesPath;
    internal string sendFilesPath;
    
    //----< process incoming messages on child thread >----------------
        
    private void processMessages()
    {
      ThreadStart thrdProc = () => {
        while (true)
        {
          CsMessage msg = translater.getMessage();
              Console.WriteLine("\nPrinting Messege\n");
          foreach(var item in msg.attributes)
          {
                Console.WriteLine(item.Key + " --> " + item.Value);
          }
          try
          {
            string msgId = msg.value("command");
            Console.Write("\n  client getting message \"{0}\"", msgId);
            if (dispatcher_.ContainsKey(msgId))
              dispatcher_[msgId].Invoke(msg);
          }
          catch(Exception ex)
          {
            Console.Write("\n  {0}", ex.Message);
            msg.show();
          }
        }
      };
      rcvThrd = new Thread(thrdProc);
      rcvThrd.IsBackground = true;
      rcvThrd.Start();
    }
    //----< add client processing for message with key >---------------

    private void addClientProc(string key, Action<CsMessage> clientProc)
    {
      dispatcher_[key] = clientProc;
    }
    ////----< load getDirs processing into dispatcher dictionary >-------

    private void DispatcherLoadGetDirs()
    {
      Action<CsMessage> getDirs = (CsMessage rcvMsg) =>
      {
        Action clrDirs = () =>
        {
          //NavLocal.clearDirs();
          NavRemote.clearDirs();
        };
        Dispatcher.Invoke(clrDirs, new Object[] { });
        var enumer = rcvMsg.attributes.GetEnumerator();
        while (enumer.MoveNext())
        {
          string key = enumer.Current.Key;
          if (key.Contains("dir"))
          {
            Action<string> doDir = (string dir) =>
            {
              NavRemote.addDir(dir);
            };
            Dispatcher.Invoke(doDir, new Object[] { enumer.Current.Value });
          }
        }
        Action insertUp = () =>
        {
          NavRemote.insertParent();
        };
        Dispatcher.Invoke(insertUp, new Object[] { });
      };
      addClientProc("getDirs", getDirs);
    }
    //----< load getFiles processing into dispatcher dictionary >------

    private void DispatcherLoadGetFiles()
    {
      Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
      {
        Action clrFiles = () =>
        {
          NavRemote.clearFiles();
        };
        Dispatcher.Invoke(clrFiles, new Object[] { });
        var enumer = rcvMsg.attributes.GetEnumerator();
        while (enumer.MoveNext())
        {
          string key = enumer.Current.Key;
          if (key.Contains("file"))
          {
            Action<string> doFile = (string file) =>
            {
              NavRemote.addFile(file);
            };
            Dispatcher.Invoke(doFile, new Object[] { enumer.Current.Value });
          }
        }
      };
      addClientProc("getFiles", getFiles);
    }
    //----< load getFiles processing into dispatcher dictionary >------

    private void DispatcherLoadSendFile()
    {
      Action<CsMessage> sendFile = (CsMessage rcvMsg) =>
      {
        Console.Write("\n  processing incoming file");
        string fileName = "";
        var enumer = rcvMsg.attributes.GetEnumerator();
        while (enumer.MoveNext())
        {
          string key = enumer.Current.Key;
          if (key.Contains("sendingFile"))
          {
            fileName = enumer.Current.Value;
            break;
          }
        }
        if (fileName.Length > 0)
        {
          Action<string> act = (string fileNm) => { showFile(fileNm); };
          Dispatcher.Invoke(act, new object[] { fileName });
        }
      };
      addClientProc("sendFile", sendFile);
    }


        //----< load check in processing into dispatcher dictionary >------
        private void DispatcherLoadGetCheckinResponse()
        {
            Action<CsMessage> getCheckinResponse = (CsMessage rcvMsg) =>
            {
                Action<string> doWrite = (string path) =>
                {
                    Console.WriteLine("\n ---------- CheckIn Performed\n You can see the Output from LocalStorge to Storage folder---------- ");
                };
                Dispatcher.Invoke(doWrite, new Object[] { rcvMsg.value("path") });
            };
            addClientProc("getCheckinResponse", getCheckinResponse);
        }


        //----< load query processing into dispatcher dictionary >------
        private void DispatcherLoadQueryResponse()
        {

            Action<CsMessage> getQueryResponse = (CsMessage rcvMsg) =>
            {
                Action doConnect = () =>
                {
                    Console.WriteLine("\n ---------- Query Performed\nYou can see the OutPut in Query Tab ---------- ");
                    listBoxResultFile.Items.Insert(0, "Result of QUERY");
                    var enumer = rcvMsg.attributes.GetEnumerator();
                    while (enumer.MoveNext())
                    {
                        string key = enumer.Current.Key;
                        if (key.Contains("file"))
                        {
                            Action<string> doFile = (string file) =>
                            {
                                listBoxResultFile.Items.Insert(1, file);
                            };
                            Dispatcher.Invoke(doFile, new Object[] { enumer.Current.Value });
                        }
                    }
                };
                Dispatcher.Invoke(doConnect, new Object[] { });
            };
            addClientProc("getQueryResponse", getQueryResponse);
            
        }


        //----< load Connect processing into dispatcher dictionary >------

        private void DispatcherLoadConnect()
        {

            Action<CsMessage> connect = (CsMessage rcvMsg) =>
            {
                Action doConnect = () =>
                {
                    Console.WriteLine("\n ---------- Connection Successful to Server\nClient is listening on Port 8082 \n Server is listening on 8080 port\n---------- ");
                    connectOutOutput.Text += "\n\nConnection Successful to Server\nClient is listening on Port 8082 \n Server is listening on 8080 port\n";
                };
                Dispatcher.Invoke(doConnect, new Object[] { });
            };
            addClientProc("connect", connect);
        }


        //----< load Meta Data processing into dispatcher dictionary >------
        private void DispatcherLoadGetMetaData()
        {
            Action<CsMessage> getMetaDataResponse = (CsMessage rcvMsg) =>
            {
                Action doShow = () =>
                {
                    Console.WriteLine("\n ---------- Got the Metadata from server.\n You can check Remote Tab\n---------- ");
                    NavRemote.mName.Content = rcvMsg.value("mName");
                    NavRemote.mDes.Content = rcvMsg.value("mDes");
                    NavRemote.mCat.Content = rcvMsg.value("mCat");
                    NavRemote.mChildren.Content = rcvMsg.value("mChildren");
                    NavRemote.mStatus.Content = rcvMsg.value("mStatus");
                    NavRemote.mDate.Content = rcvMsg.value("mDate");
                };
                Dispatcher.Invoke(doShow, new Object[] { });
            };
            addClientProc("getMetaDataResponse", getMetaDataResponse);
        }

        //----< load CheckOut processing into dispatcher dictionary >------
        private void DispatcherLoadGetCheckOutResponse()
        {
            Action<CsMessage> getCheckOutResponse = (CsMessage rcvMsg) =>
            {
                Action doWrite = () =>
                {
                    Console.WriteLine("\n ---------- Check Out Performed\nYou can see the OutPut as File From Storage to LocalCO folder ---------- ");

                    string str = "../../../../LocalCO/" + rcvMsg.value("path");
                    Directory.CreateDirectory(str);
                    if (File.Exists(str + "/" + rcvMsg.value("name")))
                        File.Delete(str + "/" + rcvMsg.value("name"));
                    File.Copy("../../../../LocalCO/" + rcvMsg.value("fileName"), str + "/" + rcvMsg.value("name"));
                    File.Delete("../../../../LocalCO/" + rcvMsg.value("fileName"));
                };
                Dispatcher.Invoke(doWrite, new Object[] {});
            };
            addClientProc("getCheckOutResponse", getCheckOutResponse);
        }


        //----< load close Status processing into dispatcher dictionary >------
        private void DispatcherLoadCloseStatus()
        {
            Action<CsMessage> getCloseStatusResponse = (CsMessage rcvMsg) =>
            {
                Action doShow = () =>
                {
                    Console.WriteLine("\n ---------- Closing Status of File\nYou can see the OutPut in Remote tab by clicking on file ---------- ");
                };
                Dispatcher.Invoke(doShow, new Object[] { });
            };
            addClientProc("getCloseStatusResponse", getCloseStatusResponse);
        }



        //----< load all dispatcher processing >---------------------------

        private void loadDispatcher()
    {
            DispatcherLoadGetDirs();
            DispatcherLoadGetFiles();
            DispatcherLoadSendFile();

            DispatcherLoadGetCheckinResponse();
            DispatcherLoadQueryResponse();
            DispatcherLoadGetMetaData();
            DispatcherLoadGetCheckOutResponse();
            DispatcherLoadCloseStatus();
            DispatcherLoadConnect();
    }
    
    //----< start Comm, fill window display with dirs and files >------

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      // start Comm
      endPoint_ = new CsEndPoint();
      endPoint_.machineAddress = "localhost";
      endPoint_.port = 8082;
      NavRemote.navEndPoint_ = endPoint_;

      translater = new Translater();
      translater.listen(endPoint_);

      // start processing messages
      processMessages();

      // load dispatcher
      loadDispatcher();

      CsEndPoint serverEndPoint = new CsEndPoint();
      serverEndPoint.machineAddress = "localhost";
      serverEndPoint.port = 8080;
      pathStack_.Push("../Storage");

      NavRemote.PathTextBlock.Text = "Storage";
      NavRemote.pathStack_.Push("../Storage");

      NavLocal.PathTextBlock.Text = "LocalStorage";
      NavLocal.pathStack_.Push("");
      NavLocal.localStorageRoot_ = "../../../../LocalStorage";

        
      saveFilesPath = translater.setSaveFilePath("../../../../LocalCO");
      sendFilesPath = translater.setSendFilePath("../../../../LocalStorage");
            
      NavLocal.refreshDisplay();
      NavRemote.refreshDisplay();
            
      testGUI();
    }
    //----< strip off name of first part of path >---------------------

    public string removeFirstDir(string path)
    {
      string modifiedPath = path;
      int pos = path.IndexOf("/");
      modifiedPath = path.Substring(pos + 1, path.Length - pos - 1);
      return modifiedPath;
    }
    //----< show file text >-------------------------------------------

    private void showFile(string fileName)
    {
      Paragraph paragraph = new Paragraph();
      string fileSpec = saveFilesPath + "\\" + fileName;
      string fileText = File.ReadAllText(fileSpec);
      paragraph.Inlines.Add(new Run(fileText));
      CodePopupWindow popUp = new CodePopupWindow();
      popUp.codeView.Blocks.Clear();
      popUp.codeView.Blocks.Add(paragraph);
      popUp.Show();
    }


    //----< Fire Event on query button click>------
    private void Query_Button(object sender,RoutedEventArgs r)
    {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;

            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "query");
            msg.add("qName", qName.Text);
            msg.add("qDes", qDes.Text);
            msg.add("qCat", qCat.Text);
            msg.add("qChildren", qChildren.Text);
            msg.add("qVersion",qVersion.Text);
            msg.add("qParent", qParent.Text);
            translater.postMessage(msg);
     }

        //----< respond to mouse click on connect button >----------------
        private void connectButtonClick(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;

            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "connectPort");
            translater.postMessage(msg);
        }


        //----< first test not completed >---------------------------------

        void testGUI()
        {
            Console.WriteLine("\n-------------------------");
            Console.WriteLine("Testing Started\n Clear Storage and LocalCO Folders");
            Console.WriteLine("-------------------------");
            Console.WriteLine("\n(1)  Connection to Server");
            connectButtonClick(new object(), new RoutedEventArgs());
            NavLocal.DirList_MouseDoubleClick(new object(), new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
            Console.WriteLine("\n(2)  CheckIng File A.cpp");
            NavLocal.FileList.SelectedIndex = 0;
            NavLocal.cDes.Text = "This is CPP FILE";
            NavLocal.cChildren.Text = "A::A.h";
            NavLocal.cCat.Text = "First;Second";
            NavLocal.fileSelect(new object(), new RoutedEventArgs());
            NavLocal.CheckIn_Click(new object(), new RoutedEventArgs());
            Console.WriteLine("\n(3)  CheckIng File A.h");
            NavLocal.FileList.SelectedIndex = 1;
            NavLocal.cDes.Text = "This is Header FILE";
            NavLocal.cChildren.Text = "A::A.cpp";
            NavLocal.cCat.Text = "Second;Third";
            NavLocal.fileSelect(new object(), new RoutedEventArgs());
            NavLocal.CheckIn_Click(new object(), new RoutedEventArgs());
            NavRemote.refreshDisplay();
            NavRemote.DirList_MouseDoubleClick(new object(), new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
            NavRemote.FileList.SelectedIndex = 1;
            NavRemote.FileList_MouseDoubleClick(new object(), new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
            Console.WriteLine("\n(4)  Closing Staus of file A.h");
            NavRemote.closeStatus_Click(new object(), new RoutedEventArgs());
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("\n(5)  CheckIng Again File A.h, It will stored as A.h.2 ");
            NavLocal.refreshDisplay();
            NavLocal.FileList.SelectedIndex = 1;
            NavLocal.cDes.Text = "This is Header FILE";
            NavLocal.cChildren.Text = "A::A.cpp";
            NavLocal.cCat.Text = "Second;Third";
            NavLocal.fileSelect(new object(), new RoutedEventArgs());
            NavLocal.CheckIn_Click(new object(), new RoutedEventArgs());
            NavRemote.refreshDisplay();
            Console.WriteLine("\n(6)  CheckOut file A::A.cpp, This is also checkout its dependent File A::A.h\n You can find it in LocalCO ");
            NavRemote.CheckOut_Click(new object(), new RoutedEventArgs());
            Console.WriteLine("\n(7)  Querying on Database using Name --> /.h/ , Desciption --> /Header / , Category ----> /Second/ , Children ----> /A::A.cpp/  ");
            Query_Button(new object(), new RoutedEventArgs());

        }

        
    }
}
