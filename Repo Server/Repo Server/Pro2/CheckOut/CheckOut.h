/////////////////////////////////////////////////////////////////////
// Checkout.h - Implements checkout of a file                      //            
// ver 1.0                                                         //
// Dwivedi Nilesh, CSE687 - Object Oriented Design, Spring 2018      //
// Source: Jim Fawcett                                             //
/////////////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
* This package provides one class:
* Ckeckout -it implements checkout on a file
* The package provides function for performing checkout on a file:

* Required Files:
* ---------------
* RepositoryCore.h RepositoryCore.cpp
* FileSystem.h FileSystem.cpp
* Version.h Version.cpp
* StringUtilities.h

* Maintenance History:
* --------------------
* ver 1.0 : 11 Mar 2018
* - first release

Public Interface :
* ------------------
* void Checkout::checkoutFile(std::string localPath, std::string fileName) - CheckOut File from repository path to specified directory
* bool testCheckOut(RepoCore::RepositoryCore& r) - perform test on functionalities of CheckOut
*/

#ifndef INC_CHECKOUT
#define INC_CHECKOUT
#include<string>
#include "../RepositoryCore/RepositoryCore.h"
#include "../Version/version.h"


#include "../../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../../CppNoSqlDb/Utilities/StringUtilities/StringUtilities.h"

namespace RepoCore
{
	class CheckOut
	{
	public:

		CheckOut(RepositoryCore& repo) : repo_{ repo } {}
		void checkOutForFile(std::string localPath, std::string fileName);

	private:
		RepositoryCore& repo_;
	};
}



namespace TestRepoCore
{
	bool testCheckOut(RepoCore::RepositoryCore& r);
}

#endif