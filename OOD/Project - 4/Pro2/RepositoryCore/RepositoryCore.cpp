//////////////////////////////////////////////////////////////////////
// RepositoryCore.cpp - Implements RepositoryCore Functionalities   //
// ver 1.0													      	//
// Source: Jim Jawcett												//
// Dwivedi Nilesh, CSE687 - Object Oriented Design, Spring 2018     //
//////////////////////////////////////////////////////////////////////

#include "RepositoryCore.h"
#include "../../CppNoSqlDb/Utilities/StringUtilities/StringUtilities.h"
#include<iostream>

using namespace RepoCore;

//----<Show File Records with Payload Values>----------
void RepositoryCore::showFileRecord()
{
	std::cout << std::endl << std::endl;
	NoSqlDb::showDb(db_);
	NoSqlDb::PayLoad::showDb(db_);
	std::cout << std::endl << std::endl;
}

//----<Test functionality and Create initial DB>----------
bool TestRepoCore::testRepoCore(RepositoryCore& r)
{
	Utilities::title("Demonstrating Repository and Creating Intial DB -- REQ#6");
	Utilities::putline();
	NoSqlDb::DbElement<NoSqlDb::PayLoad> dbElem;
	dbElem.name("Dwivedi Nilesh");
	dbElem.dateTime(DateTime().now());
	dbElem.payLoad().categories().push_back("Test");
	dbElem.payLoad().categories().push_back("Header");
	dbElem.descrip("Header file of A");
	r.db().addRecord("A::A.h.1", dbElem);
	dbElem.descrip("Header file of B");
	r.db().addRecord("B::B.h.1", dbElem);
	dbElem.descrip("Header file of C");
	r.db().addRecord("C::C.h.1", dbElem);
	dbElem.payLoad().categories().operator[](1)="Source";
	dbElem.descrip("Source file of A");
	r.db().addRecord("A::A.cpp.1", dbElem);
	dbElem.descrip("Source file of B");
	r.db().addRecord("B::B.cpp.1", dbElem);
	dbElem.descrip("Source file of C");
	r.db().addRecord("C::C.cpp.1", dbElem);
	r.db().operator[]("A::A.h.1").children().push_back("A::A.cpp.1");
	r.db().operator[]("A::A.cpp.1").children().push_back("A::A.h.1");
	r.db().operator[]("A::A.cpp.1").children().push_back("B::B.h.1");
	r.db().operator[]("A::A.cpp.1").children().push_back("C::C.h.1");
	r.db().operator[]("B::B.cpp.1").children().push_back("B::B.h.1");
	r.db().operator[]("C::C.cpp.1").children().push_back("C::C.h.1");
	r.db().operator[]("B::B.h.1").children().push_back("B::B.cpp.1");
	r.db().operator[]("C::C.h.1").children().push_back("C::C.cpp.1");
	std::cout << "Closing Status for B.cpp and B.h" << std::endl;
	r.db().operator[]("B::B.cpp.1").payLoad().status(2);
	r.db().operator[]("B::B.h.1").payLoad().status(2);
	std::cout << "Closing Status for C.cpp and C.h" << std::endl;
	r.db().operator[]("C::C.cpp.1").payLoad().status(2);
	r.db().operator[]("C::C.h.1").payLoad().status(2);
	std::cout << "DB Created" << std::endl << std::endl;
	r.showFileRecord();
	Utilities::putline();
	return true;
}

#ifdef TEST_REPOCORE

//----<Entry Point of RepositoryCore>----------
int main()
{
	RepositoryCore r;

	TestRepoCore::testRepoCore(r);

}

#endif