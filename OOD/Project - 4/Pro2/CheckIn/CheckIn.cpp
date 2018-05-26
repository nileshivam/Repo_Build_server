//////////////////////////////////////////////////////////////////////
// CheckIn.cpp - Implements CheckIn Functionalities                 //
// ver 1.0													      	//
// Source: Jim Jawcett												//
// Dwivedi Nilesh, CSE687 - Object Oriented Design, Spring 2018     //
//////////////////////////////////////////////////////////////////////


#include "CheckIn.h"
#include<iostream>
#include<string>
#include<unordered_set>

using namespace RepoCore;

//----<convert the user entered children to key names>----------
void CheckIn::changeChildren(std::string key, NoSqlDb::DbElement<NoSqlDb::PayLoad>& dbElem)
{
	std::vector<std::string> children = dbElem.children();
	dbElem.children().clear();
	for (auto child : children)
	{
		createChildRelation(key, child);
	}
}

//----< create dependency relation between parent and child >----------
void CheckIn::createChildRelation(std::string parent, std::string child)
{
	std::string keyParent = getKeyByFileName(parent);
	std::string keyChild = getKeyByFileName(child);
	repo_.db().operator[](keyParent).children().push_back(keyChild);
}

//----< CheckIn a file from specified Directory >----------
std::string CheckIn::checkInOnFile(std::string path, std::string fileName, NoSqlDb::DbElement<NoSqlDb::PayLoad> dbElem)
{
	std::cout << "CheckIn for File " << fileName << " Started" << std::endl;
	std::vector<std::string> parts = Utilities::split(fileName, ':');
	std::string nameSpace = parts[0];
	std::string realFileName = parts[2];
	Version v(repo_);
	std::string repoNSPAth = repo_.path() + "\\" + nameSpace;
	int latest = v.getLatestVersion(fileName);
	int status = (latest == 0)?2: getStatus(fileName);
	if (!FileSystem::Directory::exists(repoNSPAth))
	{
		FileSystem::Directory::create(repoNSPAth);
	}
	
	if (status == 2)
	{
		latest++;
		dbElem.payLoad().path(nameSpace + "\\" + realFileName);
		dbElem.payLoad().vNum(latest);
	}
	if (status == 1)
	{
		return nameSpace + "\\" + realFileName + "." + std::to_string(latest);
	}
		
	std::string key = fileName + "." + std::to_string(latest);
	repo_.db().addRecord(key, dbElem);
	//changeChildren(fileName, repo_.db().operator[](key));
	std::cout << "CheckIn for File " << fileName << " Completed" << std::endl;
	return nameSpace + "\\" + realFileName + "." + std::to_string(latest);;
}

//----< Get value of Key by appending latest version to fileName >----------
std::string CheckIn::getKeyByFileName(std::string fileName)
{
	Version v(repo_);
	int latest = v.getLatestVersion(fileName);
	std::string key = fileName + "." + std::to_string(latest);
	return key;
}

//----< get the status of the file >----------
int CheckIn::getStatus(std::string fileName)
{
	return repo_.db().operator[](getKeyByFileName(fileName)).payLoad().status();
}

//----< Close Status recursively by closing its childrens status >----------
void CheckIn::closeStatusRec(std::string fileName,std::unordered_set<std::string>& visited)
{
	if (visited.find(fileName) != visited.end())
		return;
	
	//std::vector<std::string> children = repo_.db().operator[](fileName).children();
	std::vector<std::string> children;
	visited.insert(fileName);
	for (auto child : children)
		closeStatusRec(child,visited);
	
	repo_.db()[fileName].payLoad().status(2);
	std::cout << "Closed status for: " << fileName << std::endl;
}

//----< starting point for calling closeStatusRec >----------
void CheckIn::closeStatus(std::string fileName)
{
	std::cout << "Closing status for:" << fileName << " And all its children" << std::endl;
	std::unordered_set<std::string> visited;
	closeStatusRec(getKeyByFileName(fileName), visited);
	std::cout << std::endl << std::endl;
}


//----< function to test functionalities of CheckIn >----------
bool TestRepoCore::testCheckIn(RepositoryCore& r)
{
	Utilities::Title("Testing for CheckIn");
	Utilities::putline();
	CheckIn c(r);
	NoSqlDb::DbElement<NoSqlDb::PayLoad> dbElem;
	dbElem.name("Dwivedi Nilesh");
	dbElem.dateTime(DateTime().now());
	dbElem.payLoad().categories().push_back("Test");
	dbElem.payLoad().categories().push_back("Header");
	dbElem.descrip("header file of A");
	c.checkInOnFile("..\\Local", "A::A.h", dbElem);
	dbElem.descrip("Header file of B");
	c.checkInOnFile("..\\Local", "B::B.h", dbElem);
	dbElem.descrip("Header file of C");
	c.checkInOnFile("..\\Local", "C::C.h", dbElem);
	dbElem.payLoad().categories().operator[](1) = "Source";
	dbElem.descrip("Source file of A");
	c.checkInOnFile("..\\Local", "A::A.cpp", dbElem);
	dbElem.descrip("Source file of B");
	c.checkInOnFile("..\\Local", "B::B.cpp", dbElem);
	dbElem.descrip("Source file of C");
	c.checkInOnFile("..\\Local", "C::C.cpp", dbElem);
	c.createChildRelation("B::B.cpp", "B::B.h");
	c.createChildRelation("B::B.h", "B::B.cpp");
	c.createChildRelation("C::C.cpp", "C::C.h");
	c.createChildRelation("C::C.h", "C::C.cpp");
	r.showFileRecord();
	c.closeStatus("C::C.cpp");
	c.closeStatus("A::A.cpp");
	r.showFileRecord();
	std::cout << "Now checking again for C.h" << std::endl;
	dbElem.descrip("Header file of C");
	dbElem.children().clear();
	dbElem.children().push_back("C::C.cpp");
	c.checkInOnFile("..\\Local", "C::C.h", dbElem);
	std::cout << "Now checking again for A.cpp" << std::endl;
	dbElem.descrip("Source file of A");
	dbElem.children().clear();
	dbElem.children().push_back("A::A.h");
	dbElem.children().push_back("B::B.h");
	dbElem.children().push_back("C::C.h");
	c.checkInOnFile("..\\Local", "A::A.cpp", dbElem);
	r.showFileRecord();
	return true;
}


#ifdef TEST_CHECKIN

//----< Entry Point for CheckIn >----------
int main()
{
	RepositoryCore r;

	TestRepoCore::testRepoCore(r);

	TestRepoCore::testCheckIn(r);
}

#endif

