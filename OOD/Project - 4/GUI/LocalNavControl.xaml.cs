﻿//////////////////////////////////////////////////////////////////////////////
// LocalNavControl.xaml.cs - Control GUI for Local Navigation               //
// ver 1.1                                                                  //
// Author : Dwivedi Nilesh , CSE687 - Object Oriented Design, Spring 2018   //
// Source: Jim Fawcett,													    //
//////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package provides a WPF-based control GUI for Project4Demo.  It's 
 * responsibilities are to:
 * - Provide a display of directory contents of the local client.
 * - It provides a subdirectory list and a filelist for the selected directory.
 * - You can navigate into subdirectories by double-clicking on subdirectory
 *   or the parent directory, indicated by the name "..".
 *   
 * Required Files:
 * ---------------
 * LocalNavControl.xaml, LocalNavControl.xaml.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 1 May 2018
 * -- Updated Some callback functions
 * ver 1.0 : 22 Apr 2018
 * - first release
 */
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
using MsgPassingCommunication;

namespace WpfApp1
{
  public partial class LocalNavControl : UserControl
  {
    internal Stack<string> pathStack_ = new Stack<string>();
    internal string localStorageRoot_;

    public LocalNavControl()
    {
      InitializeComponent();
    }


    //----< event fired on checkin button click >-----------------------
    public void CheckIn_Click(object sender,RoutedEventArgs e)
    {
            string fileName = (string)FileList.SelectedItem;
            MainWindow win = (MainWindow)Window.GetWindow(this);

            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;

            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(win.endPoint_));
            msg.add("sendingFile", fileName);
            msg.add("path", pathStack_.Peek());
            msg.add("fileName", fileName);
            msg.add("command", "checkIn");
            msg.add("cName", (string)cName.Content);
            msg.add("cDes", cDes.Text);
            msg.add("cCat", cCat.Text);
            msg.add("cChildren", cChildren.Text);
            string str = (string)cKey.Content;
            msg.add("keyFile", str + "::" + fileName);

            win.sendFilesPath = win.translater.setSendFilePath("../../../../LocalStorage/" + str);
            
            win.translater.postMessage(msg);
            
     }


        //----< event fired on file select >-----------------------
        public void fileSelect(object sender,RoutedEventArgs e)
    {
            string fileName = (string)FileList.SelectedItem;
            string str = pathStack_.Peek();
            if (str.Length > 0)
                str = str.Substring(1);
            cKey.Content = str;
            cName.Content = fileName;
        }


        //----< event fired on Refresh button click >-----------------------
        private void Refresh_Click(object sender, RoutedEventArgs e)
    {
      DirList.Items.Clear();
      string path = localStorageRoot_ + pathStack_.Peek();
      string[] dirs = System.IO.Directory.GetDirectories(path);
      foreach (string dir in dirs)
      {
        if (dir != "." && dir != "..")
        {
          string itemDir = System.IO.Path.GetFileName(dir);
          DirList.Items.Add(itemDir);
        }
      }
      DirList.Items.Insert(0, "..");

      FileList.Items.Clear();
      string[] files = System.IO.Directory.GetFiles(path);
      foreach (string file in files)
      {
        string itemFile = System.IO.Path.GetFileName(file);
        FileList.Items.Add(itemFile);
      }
    }

        //----< Refresh the display >-----------------------
   internal void refreshDisplay()
    {
      Refresh_Click(this, null);
    }
    //----< strip off name of first part of path >---------------------

    public string removeFirstDir(string path)
    {
      string modifiedPath = path;
      int pos = path.IndexOf("/");
      modifiedPath = path.Substring(pos + 1, path.Length - pos - 1);
      return modifiedPath;
    }

    internal void clearDirs()
    {
      DirList.Items.Clear();
    }
    //----< function dispatched by child thread to main thread >-------

    internal void addDir(string dir)
    {
      DirList.Items.Add(dir);
    }
    //----< function dispatched by child thread to main thread >-------

    internal void insertParent()
    {
      DirList.Items.Insert(0, "..");
    }
    //----< function dispatched by child thread to main thread >-------

    internal void clearFiles()
    {
      FileList.Items.Clear();
    }
    //----< function dispatched by child thread to main thread >-------

    internal void addFile(string file)
    {
      FileList.Items.Add(file);
    }
    //----< show file text on MouseDoubleClick >-----------------------

    private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      MainWindow win = (MainWindow)Window.GetWindow(this);

      string fileName = (string)FileList.SelectedItem;
      string srcFile = localStorageRoot_ + "/" + pathStack_.Peek() + "/" + fileName;
      srcFile = System.IO.Path.GetFullPath(srcFile);
      string dstFile = win.sendFilesPath + "/" + fileName;
      System.IO.File.Copy(srcFile, dstFile, true);

      CsEndPoint serverEndPoint = new CsEndPoint();
      serverEndPoint.machineAddress = "localhost";
      serverEndPoint.port = 8080;
      CsMessage msg = new CsMessage();
      msg.add("to", CsEndPoint.toString(serverEndPoint));
      msg.add("from", CsEndPoint.toString(win.endPoint_));
      msg.add("sendingFile", fileName);
      msg.add("path", pathStack_.Peek());
      msg.add("fileName", fileName);
      win.translater.postMessage(msg);
    }
    
        
    //----< respond to mouse double-click on dir name >----------------

    public void DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      MainWindow win = (MainWindow)Window.GetWindow(this);
            
      string selectedDir;
      if (DirList.SelectedIndex == -1)
      {
            selectedDir = "A";
      }
      else
      {
            selectedDir = (string)DirList.SelectedItem;
      }
      string path;
      if (selectedDir == "..")
      {
        if (pathStack_.Count > 1)  // don't pop off "LocalStorage"
          pathStack_.Pop();
        else
          return;
      }
      else
      {
        path = pathStack_.Peek() + "/" + selectedDir;
        pathStack_.Push(path);
      }
      PathTextBlock.Text = "LocalStorage" + pathStack_.Peek();

      refreshDisplay();
    }
  }
}