/////////////////////////////////////////////////////////////////////
// Version.h - Implements versioning on file                       //            
// ver 1.0                                                         //
// Dwivedi Nilesh, CSE687 - Object Oriented Design, Spring 2018    //
// Source: Jim Fawcett                                             //
/////////////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
* This package provides one class:
* Version - it maintains vesions on a file
* The package provides function for maintaining version on a file:

* Required Files:
* ---------------
* RepositoryCore.h RepoSitoryCore.cpp

* Maintenance History:
* --------------------
* ver 1.0 : 11 Mar 2018
* - first release

Public Interface :
* ------------------
* Version(RepositoryCore& repo) : repo_ {repo} - constructor of the class
* int getVersion(std::string); - returns the latest version of the file
* bool testVersion(RepositoryCore&) - Function to test Functionality of Version 
*/

#ifndef INC_VERSION

#define INC_VERSION

#include<string>
#include "../RepositoryCore/RepositoryCore.h"

namespace RepoCore
{
	class Version
	{
	public:

		//----<Constructor of Version that set Repo value>----------
		Version(RepositoryCore& repo) : repo_( repo ) {}

		int getLatestVersion(std::string);

	private:
		RepositoryCore& repo_;
	};
}


namespace TestRepoCore
{
	bool testVersion(RepoCore::RepositoryCore& r);
}

#endif