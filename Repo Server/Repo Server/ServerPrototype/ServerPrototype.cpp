//////////////////////////////////////////////////////////////////////////////
// ServerPrototype.cpp - Console App that processes incoming messages			//
// ver 1.3																	//
// Author : Dwivedi Nilesh , CSE687 - Object Oriented Design, Spring 2018   //
// Source: Jim Fawcett,														//
//////////////////////////////////////////////////////////////////////////////

#include "ServerPrototype.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../Process/Process/Process.h"
#include <chrono>

namespace MsgPassComm = MsgPassingCommunication;

using namespace Repository;
using namespace FileSystem;
using Msg = MsgPassingCommunication::Message;

//----< return name of every file on path >----------------------------

Files Server::getFiles(const Repository::SearchPath& path)
{
  return Directory::getFiles(path);
}
//----< return name of every subdirectory on path >--------------------

Dirs Server::getDirs(const Repository::SearchPath& path)
{
  return Directory::getDirectories(path);
}

namespace MsgPassingCommunication
{
  // These paths, global to MsgPassingCommunication, are needed by 
  // several of the ServerProcs, below.
  // - should make them const and make copies for ServerProc usage

  std::string sendFilePath;
  std::string saveFilePath;

  //----< show message contents >--------------------------------------

  template<typename T>
  void show(const T& t, const std::string& msg)
  {
    std::cout << "\n  " << msg.c_str();
    for (auto item : t)
    {
      std::cout << "\n    " << item.c_str();
    }
  }
  //----< test ServerProc simply echos message back to sender >--------

  std::function<Msg(Msg)> echo = [](Msg msg) {
    Msg reply = msg;
    reply.to(msg.from());
    reply.from(msg.to());
    return reply;
  };
  //----< getFiles ServerProc returns list of files on path >----------

  std::function<Msg(Msg)> getFiles = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("getFiles");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != ".")
        searchPath = searchPath + "\\" + path;
      Files files = Server::getFiles(searchPath);
      size_t count = 0;
      for (auto item : files)
      {
        std::string countStr = Utilities::Converter<size_t>::toString(++count);
        reply.attribute("file" + countStr, item);
      }
    }
    else
    {
      std::cout << "\n  getFiles message did not define a path attribute";
    }
    return reply;
  };
  //----< getDirs ServerProc returns list of directories on path >-----

  std::function<Msg(Msg)> getDirs = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("getDirs");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != ".")
        searchPath = searchPath + "\\" + path;
      Files dirs = Server::getDirs(searchPath);
      size_t count = 0;
      for (auto item : dirs)
      {
        if (item != ".." && item != ".")
        {
          std::string countStr = Utilities::Converter<size_t>::toString(++count);
          reply.attribute("dir" + countStr, item);
        }
      }
    }
    else
    {
      std::cout << "\n  getDirs message did not define a path attribute";
    }
    return reply;
  };

  //----< sendFile ServerProc sends file to requester >----------------
  /*
  *  - Comm sends bodies of messages with sendingFile attribute >------
  */
  std::function<Msg(Msg)> sendFile = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("sendFile");
    reply.attribute("sendingFile", msg.value("fileName"));
    reply.attribute("fileName", msg.value("fileName"));
    reply.attribute("verbose", "blah blah");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != "." && path != searchPath)
        searchPath = searchPath + "\\" + path;
      if (!FileSystem::Directory::exists(searchPath))
      {
        std::cout << "\n  file source path does not exist";
        return reply;
      }
      std::string filePath = searchPath + "/" + msg.value("fileName");
      std::string fullSrcPath = FileSystem::Path::getFullFileSpec(filePath);
      std::string fullDstPath = sendFilePath;
      if (!FileSystem::Directory::exists(fullDstPath))
      {
        std::cout << "\n  file destination path does not exist";
        return reply;
      }
      fullDstPath += "/" + msg.value("fileName");
      FileSystem::File::copy(fullSrcPath, fullDstPath);
    }
    else
    {
      std::cout << "\n  getDirs message did not define a path attribute";
    }
    return reply;
  };

  //----< analyze code on current server path >--------------------------
  /*
  *  - Creates process to run CodeAnalyzer on specified path
  *  - Won't return until analysis is done and logfile.txt
  *    is copied to sendFiles directory
  */
  std::function<Msg(Msg)> codeAnalyze = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("sendFile");
    reply.attribute("sendingFile", "logfile.txt");
    reply.attribute("fileName", "logfile.txt");
    reply.attribute("verbose", "blah blah");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != "." && path != searchPath)
        searchPath = searchPath + "\\" + path;
      if (!FileSystem::Directory::exists(searchPath))
      {
        std::cout << "\n  file source path does not exist";
        return reply;
      }
      // run Analyzer using Process class

      Process p;
      p.title("test application");
      //std::string appPath = "c:/su/temp/project4sample/debug/CodeAnalyzer.exe";
      std::string appPath = "CodeAnalyzer.exe";
      p.application(appPath);

      //std::string cmdLine = "c:/su/temp/project4Sample/debug/CodeAnalyzer.exe ";
      std::string cmdLine = "CodeAnalyzer.exe ";
      cmdLine += searchPath + " ";
      cmdLine += "*.h *.cpp /m /r /f";
      //std::string cmdLine = "c:/su/temp/project4sample/debug/CodeAnalyzer.exe ../Storage/path *.h *.cpp /m /r /f";
      p.commandLine(cmdLine);

      std::cout << "\n  starting process: \"" << appPath << "\"";
      std::cout << "\n  with this cmdlne: \"" << cmdLine << "\"";

      CBP callback = []() { std::cout << "\n  --- child process exited ---"; };
      p.setCallBackProcessing(callback);

      if (!p.create())
      {
        std::cout << "\n  can't start process";
      }
      p.registerCallback();

      std::string filePath = searchPath + "\\" + /*msg.value("codeAnalysis")*/ "logfile.txt";
      std::string fullSrcPath = FileSystem::Path::getFullFileSpec(filePath);
      std::string fullDstPath = sendFilePath;
      if (!FileSystem::Directory::exists(fullDstPath))
      {
        std::cout << "\n  file destination path does not exist";
        return reply;
      }
      fullDstPath += std::string("\\") + /*msg.value("codeAnalysis")*/ "logfile.txt";
      FileSystem::File::copy(fullSrcPath, fullDstPath);
    }
    else
    {
      std::cout << "\n  getDirs message did not define a path attribute";
    }
    return reply;
  };


  //----< Function Pointer for processing CheckIn >------
  std::function<Msg(Msg)> checkIn = [](Msg msg)
  {
	  Msg reply = msg;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("getCheckinResponse");
	  return reply;
  };


  //----< Function Pointer for processing Check out >------
  std::function<Msg(Msg)> checkOut = [](Msg msg)
  {
	  Msg reply = msg;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("getCheckOutResponse");
	  return reply;
  };

  //----< Function Pointer for processing Closing status >------
  std::function<Msg(Msg)> closeStatus = [](Msg msg)
  {
	  Msg reply = msg;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("getCloseStatusResponse");
	  return reply;
  };



  //----< Function Pointer for getting Meta Data >------
  std::function<Msg(Msg)> getMetaData = [](Msg msg)
  {
	  Msg reply = msg;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("getMetaDataResponse");
	  return reply;
  };

  //----< Function Pointer for getting Meta Data >------
  std::function<Msg(Msg)> query = [](Msg msg)
  {
	  Msg reply = msg;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("getQueryResponse");
	  return reply;
  };


  //----< Function Pointer for getting file content >------
  std::function<Msg(Msg)> getFileContent = [](Msg msg)
  {
	  Msg reply = msg;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.file(msg.value("sendFile"));
	  return reply;
  };


  //----< Function Pointer for connecting server and client >------
  std::function<Msg(Msg)> connectPort = [](Msg msg)
  {
	  Msg reply = msg;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.attribute("command", "connect");
	  return reply;
  };

}


using namespace MsgPassingCommunication;

int main()
{
  SetConsoleTitleA("Project4 Server Console");
  std::cout << "\n  Testing Server Prototype";
  std::cout << "\n ==========================";
  std::cout << "\n";
  saveFilePath = "../Storage";
  sendFilePath = "../Storage";
  Server server(serverEndPoint, "ServerPrototype");
  MsgPassingCommunication::Context* pCtx = server.getContext();
  pCtx->saveFilePath = saveFilePath;
  pCtx->sendFilePath = sendFilePath;
  server.start();
  std::cout << "\n  testing getFiles and getDirs methods";
  std::cout << "\n --------------------------------------";
  Files files = server.getFiles();
  show(files, "Files:");
  Dirs dirs = server.getDirs();
  show(dirs, "Dirs:");
  std::cout << "\n";
  std::cout << "\n  testing message processing";
  std::cout << "\n ----------------------------";
  server.addMsgProc("echo", echo);
  server.addMsgProc("getFiles", getFiles);
  server.addMsgProc("getDirs", getDirs);
  server.addMsgProc("sendFile", sendFile);
  server.addMsgProc("codeAnalyze", codeAnalyze);
  server.addMsgProc("serverQuit", echo);
  server.addMsgProc("checkIn", checkIn);
  server.addMsgProc("checkOut", checkOut);
  server.addMsgProc("getMetaData", getMetaData);
  server.addMsgProc("connectPort", connectPort);
  server.addMsgProc("getFileContent", getFileContent);
  server.addMsgProc("closeStatus", closeStatus);
  server.addMsgProc("query", query);
  server.processMessages();
  Msg msg(serverEndPoint, serverEndPoint);  // send to self
  msg.name("msgToSelf");
  std::cout << "\n  press enter to exit\n";
  std::cin.get();
  std::cout << "\n";
  msg.command("serverQuit");
  server.postMessage(msg);
  server.stop();
  return 0;
}

