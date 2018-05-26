//////////////////////////////////////////////////////////////////////
// CheckOut.cpp - Implements CheckOut Functionalities               //
// ver 1.0													      	//
// Source: Jim Jawcett												//
// Dwivedi Nilesh, CSE687 - Object Oriented Design, Spring 2018     //
//////////////////////////////////////////////////////////////////////


#include "CheckOut.h"

using namespace RepoCore;

//----< CheckOut File from repository path to specified directory >----------
void CheckOut::checkOutForFile(std::string localPath, std::string fileName)
{
	std::vector<std::string> parts = Utilities::split(fileName, ':');
	std::string nameSpace = parts[0];
	std::string fileNamer = parts[2];

	if (!FileSystem::Directory::exists(localPath + "\\" + nameSpace))
	{
		FileSystem::Directory::create(localPath + "\\" + nameSpace);
	}
	Version v(repo_);
	int latest = v.getLatestVersion(fileName);
	
	std::string src = repo_.path() + "\\" + nameSpace + "\\" + fileNamer + "." + std::to_string(latest);
	std::string dest = localPath + "\\" + nameSpace + "\\" + fileNamer;

	bool t1 = FileSystem::File::copy(src, dest, false);
}

//----< perform test on functionalities of CheckOut >----------
bool TestRepoCore::testCheckOut(RepositoryCore& r)
{
	Utilities::title("Testing for CheckOut");
	Utilities::putline();
	CheckOut c(r);
	std::cout << "Checkout file A::A.h to ..\\LocalCO" << std::endl;
	c.checkOutForFile("..\\LocalCO", "A::A.h");

	std::cout << "Checkout file A::A.cpp to ..\\LocalCo" << std::endl;
	c.checkOutForFile("..\\LocalCO", "A::A.cpp");

	return true;
}


#ifdef TEST_CHECKOUT

//----< Entry Point of CheckOut >----------
int main()
{
	RepositoryCore r;
	TestRepoCore::testRepoCore(r);
	TestRepoCore::testCheckOut(r);
}

#endif // TEST_CHECKOUT
