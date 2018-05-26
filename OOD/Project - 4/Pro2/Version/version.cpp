//////////////////////////////////////////////////////////////////////
// Version.cpp - Implements Version Functionalities                 //
// ver 1.0													      	//
// Source: Jim Jawcett												//
// Dwivedi Nilesh, CSE687 - Object Oriented Design, Spring 2018     //
//////////////////////////////////////////////////////////////////////


#include<string>
#include "version.h"

#include "../../FileSystem-Windows/FileSystemDemo/FileSystem.h"

using namespace RepoCore;

//----<Return the Latest Version of File>----------
int Version::getLatestVersion(std::string fileName)
{
	int ver = 0;
	while (repo_.db().contains(fileName + "." + std::to_string(ver + 1)))
	{
		ver++;
	}
	return ver;
}

//----<Function to test Functionality of Version>----------
bool TestRepoCore::testVersion(RepositoryCore& r)
{
	RepoCore::Version v(r);
	std::cout << "Getting the latest version of C::C.h ---> " << v.getLatestVersion("C::C.h") << std::endl;

	return true;
}


#ifdef TEST_VERSION

//----<Entry Point of Version>----------
int main()
{
	RepositoryCore r;
	TestRepoCore::testVersion(r);
}

#endif