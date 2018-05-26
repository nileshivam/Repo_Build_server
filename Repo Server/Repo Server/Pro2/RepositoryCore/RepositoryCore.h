//////////////////////////////////////////////////////////////////////
// RepositoryCore.h - provides means to check-in, version, browse,  //
//        and check-out source code packages.                       //            
// ver 1.0                                                          //
//Dwivedi Nilesh, CSE687 - Object Oriented Design, Spring 2018      //
// Source: Jim Fawcett                                              //
//////////////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
* This package provides one class:
* RepositoryCore -it holds the database
* The package provides function for maintaining database:

* Required Files:
* ---------------
* DbCore.h
* PayLoad.h

* Maintenance History:
* --------------------
* ver 1.0 : 11 Mar 2018
* - first release

Public Interface :
* ------------------
* NoSqlDb::DbCore<NoSqlDb::PayLoad> db() const { return db_; } - returns the db
* NoSqlDb::DbCore<NoSqlDb::PayLoad>& db() { return db_; } - returns refernece of db
* void db(const NoSqlDb::DbCore<NoSqlDb::PayLoad>& db) { db_ = db; } - sets the db

* std::string& path() { return path_; } - returns the path
* std::string path() const { return path_; } - returns the refernece of path

* void showFileRecord() - Show File Records with Payload Values
* bool testRepoCore(RepoCore::RepositoryCore&) - Test functionality and Create initial DB

*/
#ifndef INC_REPO

#define INC_REPO

#include "../../CppNoSqlDb/DbCore/DbCore.h"
#include "../../CppNoSqlDb/PayLoad/PayLoad.h"


namespace RepoCore
{
	class RepositoryCore
	{

	public:
		//----<Constructors for RepositoryCore that set Path value>----------
		RepositoryCore() { path_ = "..\\Repository"; }
		RepositoryCore(std::string path) { path_ = path; }


		NoSqlDb::DbCore<NoSqlDb::PayLoad> db() const { return db_; }
		NoSqlDb::DbCore<NoSqlDb::PayLoad>& db() { return db_; }
		void db(NoSqlDb::DbCore<NoSqlDb::PayLoad>& db) { db_ = db; }

		
		std::string path() const { return path_; }
		std::string& path() { return path_; }

		void showFileRecord();

	private:

		NoSqlDb::DbCore<NoSqlDb::PayLoad> db_;
		std::string path_;
	};
}


namespace TestRepoCore
{
	bool testRepoCore(RepoCore::RepositoryCore&);
}


#endif