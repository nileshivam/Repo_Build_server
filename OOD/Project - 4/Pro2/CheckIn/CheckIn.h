/////////////////////////////////////////////////////////////////////
// CheckIn.h - Implements checkin of a file                      //            
// ver 1.0                                                         //
// Dwivedi Nilesh, CSE687 - Object Oriented Design, Spring 2018      //
// Source: Jim Fawcett                                             //
/////////////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
* This package provides one class:
* Checkin -it implements checkin on a file
* The package provides function for performing checkin on a file:

* Required Files:
* ---------------
* RepositoryCore.h RepositoryCore.cpp
* DateTime.h DateTime.cpp
* FileSystem.h FileSystem.cpp
* StringUtilities.h
* version.h version.cpp

* Maintenance History:
* --------------------
* ver 1.0 : 11 Mar 2018
* - first release

Public Interface :
* ------------------
* CheckIn(RepositoryCore& repo) : repo_{repo} - constructor for checkin class
* void checkInOnFile(std::string fileName, std::string path, NoSqlDb::DbElement<NoSqlDb::PayLoad> dbElem) - CheckIn a file from specified Directory
* void closeStatus(std::string fileName) - starting point for calling closeStatusRec
* void closeStatusRec(std::string fileName, std::unordered_set<std::string>& visited) - Close Status recursively by closing its childrens status
* std::string getKeyByFileName(std::string fileName) - Get value of Key by appending latest version to fileName
* void createChildRelation(std::string parent, std::string child) - create dependency relation between parent and child
* void changeChildren(std::string key, NoSqlDb::DbElement<NoSqlDb::PayLoad>& dbElem) - convert the user entered children to key names
* getStatus(std::string) - get the status of the file
* bool testCheckIn(RepoCore::RepositoryCore& r) - function to test functionalities of CheckIn
*
*/
#ifndef INC_CHECKIN

#define INC_CHECKIN

#include<string>
#include "../RepositoryCore/RepositoryCore.h"
#include "../../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../../CppNoSqlDb/Utilities/StringUtilities/StringUtilities.h"
#include "../Version/version.h"
#include<unordered_set>

namespace RepoCore
{
	class CheckIn
	{

	public:
		//----< Constructor for Checkin which set Repo >----------
		CheckIn(RepositoryCore& repo) : repo_( repo ) {}

		std::string checkInOnFile(std::string path, std::string fileName, NoSqlDb::DbElement<NoSqlDb::PayLoad> dbElem);
		void closeStatus(std::string fileName);
		void closeStatusRec(std::string fileName, std::unordered_set<std::string>& visited);
		int getStatus(std::string fileName);
		std::string getKeyByFileName(std::string fileName);
		void changeChildren(std::string key, NoSqlDb::DbElement<NoSqlDb::PayLoad>& dbElem);
		void createChildRelation(std::string parent, std::string child);

	private:
		RepositoryCore& repo_;
	};
}




namespace TestRepoCore
{
	bool testCheckIn(RepoCore::RepositoryCore& r);
}

#endif