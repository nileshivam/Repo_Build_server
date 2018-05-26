#pragma once
//////////////////////////////////////////////////////////////////////////////
// ServerPrototype.h - Console App that processes incoming messages			//
// ver 1.3																	//
// Author : Dwivedi Nilesh , CSE687 - Object Oriented Design, Spring 2018   //
// Source: Jim Fawcett,														//
//////////////////////////////////////////////////////////////////////////////
/*
*  Package Operations:
* ---------------------
*  Package contains one class, Server, that contains a Message-Passing Communication
*  facility. It processes each message by invoking an installed callable object
*  defined by the message's command key.
*  - This is implemented with a message dispatcher (unodered_map<Msg.Id,ServerProc>
*    where ServerProcs are defined for each type of processing required by the server.
*
*  Message handling runs on a child thread, so the Server main thread is free to do
*  any necessary background processing (none, so far).
*
*  Required Files:
* -----------------
*  ServerPrototype.h, ServerPrototype.cpp
*  Comm.h, Comm.cpp, IComm.h
*  Message.h, Message.cpp (static library)
*  Process.h, Process.cpp (static library)
*  FileSystem.h, FileSystem.cpp
*  CheckIn.h CheckIn.cpp
*  CheckOut.h CheckOut.cpp
*  Version.h Version.cpp
*  Repository.h Repository.cpp
*  DbCore.h
*  Utilities.h

Public Interface :

* ------------------

* void Server::checkIn(Msg& msg) - Does Preprocessing for CheckIn
* void Server::checkOut(Msg& msg) -  Does Preprocessing for CheckOut
* void Server::getMetaData(Msg& msg) - Does Preprocessing for GetMeta Data query
* void Server::query(Msg& msg) - Does Preprocessing for Query in specified Element
* void Server::closeStatus(Msg& msg) - Does Preprocessing for Closing status of file
* void Server::processMessages() - start processing messages on child thread

*

*
*  Maintenance History:
* ----------------------
*  ver 1.3 : 1 May
*  -  Updaqted Process Messege to prosessing of checkIn and CheckOut
*  ver 1.2 : 22 Apr 2018
*  - added NoSqlDb to server members
*  - added simple demo of db in Server startup
*  ver 1.1 : 09 Apr 2018
*  - added ServerProcs for
*    - sending files for popup display
*    - executing remote analysis
*  ver 1.0 : 03/27/2018
*  - first release
*/
#include <vector>
#include <string>
#include <unordered_map>
#include <functional>
#include <thread>
#include<regex>
#include "../CppCommWithFileXfer/Message/Message.h"
#include "../CppCommWithFileXfer/MsgPassingComm/Comm.h"
#include "../CppNoSqlDb/DbCore/DbCore.h"
#include "../CppNoSqlDb/PayLoad/PayLoad.h"
#include "../Pro2/RepositoryCore/RepositoryCore.h"
#include "../Pro2/CheckIn/CheckIn.h"
#include "../Pro2/CheckOut/CheckOut.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../CppNoSqlDb/Query/Query.h"
#include <windows.h>
#include <tchar.h>

namespace Repository
{
  using File = std::string;
  using Files = std::vector<File>;
  using Dir = std::string;
  using Dirs = std::vector<Dir>;
  using SearchPath = std::string;
  using Key = std::string;
  using Msg = MsgPassingCommunication::Message;
  using ServerProc = std::function<Msg(Msg)>;
  using MsgDispatcher = std::unordered_map<Key,ServerProc>;
  
  const SearchPath storageRoot = "../Storage";  // root for all server file storage
  const MsgPassingCommunication::EndPoint serverEndPoint("localhost", 8080);  // listening endpoint

  class Server
  {
  public:
    Server(MsgPassingCommunication::EndPoint ep, const std::string& name);
    void start();
    void stop();
    void addMsgProc(Key key, ServerProc proc);
    bool hasMessageKey(const Key& key);
    void processMessages();
    void postMessage(MsgPassingCommunication::Message msg);
    MsgPassingCommunication::Message getMessage();
    static Dirs getDirs(const SearchPath& path = storageRoot);
    static Files getFiles(const SearchPath& path = storageRoot);
    MsgPassingCommunication::Context* getContext();
    std::string setSendFilePath(const std::string& relPath);
    std::string setSaveFilePath(const std::string& relPath);
    std::string getSendFilePath();
    std::string getSaveFilePath();
    void initializeDb();

	void checkIn(Msg& msg);
	void checkOut(Msg& msg);
	void getMetaData(Msg& msg);
	void query(Msg& msg);
	void closeStatus(Msg& msg);
	


  private:
	  std::vector<std::string> getParent(std::vector<std::string>);
    MsgPassingCommunication::Comm comm_;
    MsgDispatcher dispatcher_;
    std::thread msgProcThrd_;
	RepoCore::RepositoryCore r;
    //NoSqlDb::DbCore<NoSqlDb::PayLoad> db_;

	
  };

  //----< return the string made from vector >--------
  inline std::string makeStringFromVector(std::vector<std::string> vec)
  {
	  std::string ansRe;
	  for (auto it : vec)
		  ansRe += it + ";";
	  return ansRe;
  }


  //----< return reference to MsgPassingCommunication context >--------

  inline MsgPassingCommunication::Context* Server::getContext()
  {
    return comm_.getContext();
  }
  //----< initialize server endpoint and give server a name >----------

  inline Server::Server(MsgPassingCommunication::EndPoint ep, const std::string& name)
    : comm_(ep, name) , r("../Storage") {
	 
    initializeDb();
  }

  inline std::string Server::setSendFilePath(const std::string& relPath)
  {
    comm_.setSendFilePath(relPath);
  }
  inline std::string Server::setSaveFilePath(const std::string& relPath)
  {
    comm_.setSaveFilePath(relPath);
  }
  inline std::string Server::getSendFilePath()
  {
    comm_.getSendFilePath();
  }
  inline std::string Server::getSaveFilePath()
  {
    comm_.getSaveFilePath();
  }
  //----< start server's instance of Comm >----------------------------

  inline void Server::start()
  {
    comm_.start();
  }
  //----< stop Comm instance >-----------------------------------------

  inline void Server::stop()
  {
    if(msgProcThrd_.joinable())
      msgProcThrd_.join();
    comm_.stop();
  }

  //----< Does Preprocessing for CheckIn >-----------------------------------------
  void Server::checkIn(Msg& msg)
  {
	  std::cout << "\n-------------------------------------------\n" << std::endl;
	  std::cout << "Client Requested for Check In with " << msg.value("sendingFile") << " file\n";
	  std::cout << "\n-------------------------------------------\n" << std::endl;

	  RepoCore::CheckIn c(r);
	  NoSqlDb::DbElement<NoSqlDb::PayLoad> dbElem;
	  dbElem.name(msg.value("cName"));
	  dbElem.descrip(msg.value("cDes"));

	  dbElem.children() = Utilities::split(msg.value("cChildren"), ';');

	  dbElem.payLoad().categories() = Utilities::split(msg.value("cCat"), ';');

	  std::string fileName = c.checkInOnFile("ANYTHING", msg.value("keyFile"), dbElem);

	  FileSystem::File::copy(r.path() + "\\" + msg.value("sendingFile"), r.path() + "\\" + fileName);
	  FileSystem::File::remove(r.path() + "\\" + msg.value("sendingFile"));

	  msg.remove("sendingFile");
	  msg.remove("cName");
	  msg.remove("cDes");
	  msg.remove("cChildren");
	  msg.remove("cCat");

	  NoSqlDb::showDb(r.db());
  }



  //----< Does Preprocessing for CheckOut >-----------------------------------------
  void Server::checkOut(Msg& msg)
  {
	  std::cout << "\n-------------------------------------------\n" << std::endl;
	  std::cout << "Client Requested for checkOut In with " << msg.value("fileName") << " file\n";
	  std::cout << "You can see file from LocalStorage folder to Storage Folder" << std::endl;
	  std::cout << "\n-------------------------------------------\n" << std::endl;

	  std::string key = msg.value("path") + "::" + msg.value("fileName");
	  NoSqlDb::DbElement<NoSqlDb::PayLoad> dbElem = r.db().operator[](key);

	  for (auto child : dbElem.children())
	  {
		  std::vector<std::string> v = Utilities::split(child, ':');

		  MsgPassingCommunication::Context* cpCtx = getContext();
		  std::string csendFilePath = "../Storage/" + v[0];
		  cpCtx->sendFilePath = csendFilePath;

		  RepoCore::Version vObj(r);
		  int let = vObj.getLatestVersion(child);
		  Msg cMsg = msg;

		  cMsg.attribute("path", v[0]);
		  cMsg.attribute("name", v[2]);
		  cMsg.attribute("sendingFile", v[2] + "." + std::to_string(let));
		  cMsg.attribute("fileName", v[2] + "." + std::to_string(let));

		  Msg creply = dispatcher_[msg.command()](cMsg);
		  postMessage(creply);
		  msg.show();
		  creply.show();
	  }

	  MsgPassingCommunication::Context* pCtx = getContext();
	  std::string sendFilePath = "../Storage/" + msg.value("path");
	  pCtx->sendFilePath = sendFilePath;

	  msg.attribute("name", dbElem.name());
	  msg.attribute("sendingFile", msg.value("fileName"));

  }



  //----< Does Preprocessing for GetMeta Data query >-----------------------------------------
  void Server::getMetaData(Msg& msg)
  {

	  std::cout << "\n-------------------------------------------\n" << std::endl;
	  std::cout << "Client Requested for MetaData In with " << msg.value("mKey") << " file\n";
	  std::cout << "You can see file from Storage folder to LocalCO Folder" << std::endl;
	  std::cout << "\n-------------------------------------------\n" << std::endl;

	  std::string key = msg.value("mKey");
	  NoSqlDb::DbElement<NoSqlDb::PayLoad> dbElem = r.db().operator[](key);
	  msg.attribute("mName", dbElem.name());
	  msg.attribute("mDes", dbElem.descrip());
	  msg.attribute("mChildren", makeStringFromVector(dbElem.children()));
	  msg.attribute("mCat", makeStringFromVector(dbElem.payLoad().categories()));
	  msg.attribute("mDate", dbElem.dateTime());
	  std::string st;
	  if (dbElem.payLoad().status() == 1)
		  st = "Open";
	  else
		  st = "Close";
	  msg.attribute("mStatus", st);
  }

  std::vector<std::string> Server::getParent(std::vector<std::string> keys)
  {
	  std::vector<std::string> newKeys;
	  for (auto it : keys)
	  {
		  bool done = true;
		  for (auto entry : r.db().keys())
		  {
			  for (auto child : r.db().operator[](entry).children())
			  {
				  std::regex e(child);
				  if (std::regex_search(it, e))
				  {
					  done = false;
					  break;
				  }
			  }
			  if (!done)
				  break;
		  }
		  if (done)
			  newKeys.push_back(it);
	  
	  }
	  return newKeys;
  }


  //----< Does Preprocessing for Query in specified Element >-----------------------------------------
  void Server::query(Msg& msg)
  {
	  std::cout << "\n-------------------------------------------\n" << std::endl;
	  std::cout << "Client Requested for Query\n";
	  std::cout << "\n-------------------------------------------\n" << std::endl;
	  NoSqlDb::Conditions<NoSqlDb::PayLoad> conds;
	  NoSqlDb::Query<NoSqlDb::PayLoad> q1(r.db());
	  conds.name(msg.value("qName"));
	  conds.description(msg.value("qDes"));
	  conds.children(Utilities::split(msg.value("qChildren"), ';'));
	  std::string category = msg.value("qCat");
	  if (category != "")
	  {
		  std::vector<std::string> categories = Utilities::split(category, ';');
		  for (auto cat : categories)
		  {
			  auto hasCategory = [&cat](NoSqlDb::DbElement<NoSqlDb::PayLoad>& elem)
			  {
				  return (elem.payLoad()).hasCategory(cat);
			  };
			  q1.select(hasCategory);
		  }
	  }
	  q1.select(conds);
	  std::string ver = msg.value("qVersion");
	  if (ver != "")
	  {
		  std::regex e(ver);
		  std::vector<std::string> newKeys;
		  for (auto it : q1.keys())
			  if (std::regex_search(it, e))
				  newKeys.push_back(it);
		  q1.from(newKeys);
	  }
	  if (msg.value("qParent") == "YES")
		  q1.from(getParent(q1.keys()));
	  std::vector<std::string> files = q1.keys();
	  for (unsigned int i = 0;i<files.size();i++)
		  msg.attribute("file" + std::to_string(i + 1), files[i]);
  }


  //----< Does Preprocessing for Closing status of file >-----------------------------------------
  void Server::closeStatus(Msg& msg)
  {
	  std::cout << "\n-------------------------------------------\n" << std::endl;
	  std::cout << "Client Requested for Closing Status of file " << msg.value("fileName") << std::endl;
	  std::cout << "\n-------------------------------------------\n" << std::endl;


	  std::string key = msg.value("path") + "::" + msg.value("fileName");
	  NoSqlDb::DbElement<NoSqlDb::PayLoad> dbElem = r.db().operator[](key);
	  RepoCore::CheckIn c(r);
	  std::string key1 = msg.value("path") + "::" + msg.value("fileName");
	  NoSqlDb::DbElement<NoSqlDb::PayLoad> dbElem1 = r.db().operator[](key1);
	  c.closeStatus(msg.value("path") + "::" + dbElem1.name());
  }

  //----< pass message to Comm for sending >---------------------------

  inline void Server::postMessage(MsgPassingCommunication::Message msg)
  {
    comm_.postMessage(msg);
  }
  //----< get message from Comm >--------------------------------------

  inline MsgPassingCommunication::Message Server::getMessage()
  {
    Msg msg = comm_.getMessage();
    return msg;
  }
  //----< add ServerProc callable object to server's dispatcher >------

  inline void Server::addMsgProc(Key key, ServerProc proc)
  {
    dispatcher_[key] = proc;
  }
  //----< does server have specified key? >----------------------------

  inline bool Server::hasMessageKey(const Key& key)
  {
    for (auto item : dispatcher_)
    {
      if (item.first == key)
        return true;
    }
    return false;
  }
  //----< start processing messages on child thread >------------------

  inline void Server::processMessages()
  {
    auto proc = [&]()
    {
      if (dispatcher_.size() == 0)
      {
        std::cout << "\n  no server procs to call";
        return;
      }
      while (true)
      {
        Msg msg = getMessage();
        std::cout << "\n  received message: " << msg.command() << " from " << msg.from().toString();
        if (msg.containsKey("verbose"))
        {
          std::cout << "\n";
          msg.show();
        }
        if (msg.command() == "serverQuit")
          break;
		if (msg.command() == "checkIn")
			checkIn(msg);
		else if (msg.command() == "query")
			query(msg);
		else if (msg.command() == "getMetaData")
			getMetaData(msg);
		else if (msg.command() == "checkOut")
			checkOut(msg);
		else if (msg.command() == "closeStatus")
			closeStatus(msg);
        Msg reply;
        reply.to(msg.from());
        reply.from(msg.to());
        if (hasMessageKey(msg.command()))
          reply = dispatcher_[msg.command()](msg);
        else
          reply.command("error - unknown command");
        if (msg.to().port != msg.from().port)  // avoid infinite message loop
        {
          postMessage(reply);
          msg.show();
          reply.show();
        }
      }
      std::cout << "\n  server message processing thread is shutting down";
    };
    std::thread t(proc);
    std::cout << "\n  starting server thread to process messages";
    msgProcThrd_ = std::move(t);
  }


  inline void Server::initializeDb()
  {
    NoSqlDb::DbElement<NoSqlDb::PayLoad> elem;
    elem.name("B::B.cpp");
    elem.descrip("Source File of B");
	elem.payLoad().categories().push_back("Third");
	elem.payLoad().categories().push_back("Fourth");
	elem.children().push_back("A::A.cpp");
	elem.children().push_back("A::A.h");
    r.db().operator[]("B::B.cpp.1") = elem;
    
	elem.name("B::B.h");
	elem.descrip("Header File of B");
	elem.payLoad().categories().clear();
	elem.payLoad().categories().push_back("Fourth");
	elem.payLoad().categories().push_back("Fifth");
	elem.children().clear();
	r.db().operator[]("B::B.h.1") = elem;

	NoSqlDb::showDb(r.db());
    std::cout << "\n";
  }
}