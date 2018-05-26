#ifndef PAYLOAD_H
#define PAYLOAD_H
///////////////////////////////////////////////////////////////////////
// PayLoad.h - application defined payload                           //
// ver 1.1                                                           //
// Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018         //
///////////////////////////////////////////////////////////////////////
/*
*  Package Operations:
*  -------------------
*  This package provides a single class, PayLoad:
*  - holds a payload string and vector of categories
*  - provides means to set and access those values
*  - provides methods used by Persist<PayLoad>:
*    - Sptr toXmlElement();
*    - static PayLoad fromXmlElement(Sptr elem);
*  - provides a show function to display PayLoad specific information
*  - PayLoad processing is very simple, so this package contains only
*    a header file, making it easy to use in other packages, e.g.,
*    just include the PayLoad.h header.
*
*  Required Files:
*  ---------------
*    PayLoad.h, PayLoad.cpp - application defined package
*    DbCore.h, DbCore.cpp
*
*  Maintenance History:
*  --------------------
*  ver 1.1 : 19 Feb 2018
*  - added inheritance from IPayLoad interface
*  Ver 1.0 : 10 Feb 2018
*  - first release
*
*/

#include <string>
#include <vector>
#include <memory>
#include <iostream>
#include "../DbCore/Definitions.h"
#include "../DbCore/DbCore.h"
#include "IPayLoad.h"

///////////////////////////////////////////////////////////////////////
// PayLoad class provides:
// - a std::string value which, in Project #2, will hold a file path
// - a vector of string categories, which for Project #2, will be 
//   Repository categories
// - methods used by Persist<PayLoad>:
//   - Sptr toXmlElement();
//   - static PayLoad fromXmlElement(Sptr elem);


namespace NoSqlDb
{
	class PayLoad : public IPayLoad<PayLoad>
	{
	public:
		PayLoad()
		{
			isLatest_ = true;
			status_ = 1;
			value_ = "";
		}
		PayLoad(const std::string& val) : value_(val) {}
		static void identify(std::ostream& out = std::cout);
		PayLoad& operator=(const std::string& val)
		{
			value_ = val;
			return *this;
		}
		operator std::string() { return value_; }

		std::string value() const { return value_; }
		std::string& value() { return value_; }
		void value(const std::string& value) { value_ = value; }

		std::string path() const { return path_; }
		std::string& path() { return path_; }
		void path(std::string path) { path_ = path; }

		int vNum() const { return vNum_; };
		int& vNum() { return vNum_; }
		void vNum(int vNum) { vNum_ = vNum; }

		int status() const { return status_; };
		int& status() { return status_; }
		void status(int status) { status_ = status; }

		bool isLatest() const { return isLatest_; }
		bool& isLatest() { return isLatest_; }
		void isLatest(bool isLatest) { isLatest_ = isLatest; }

		std::vector<std::string>& categories() { return categories_; }
		std::vector<std::string> categories() const { return categories_; }

		bool hasCategory(const std::string& cat)
		{
			return std::find(categories().begin(), categories().end(), cat) != categories().end();
		}

		static void showPayLoadHeaders(std::ostream& out = std::cout);
		static void showElementPayLoad(std::string, NoSqlDb::DbElement<PayLoad>& elem, std::ostream& out = std::cout);
		static void showDb(NoSqlDb::DbCore<PayLoad>& db, std::ostream& out = std::cout);

	private:
		std::string value_;
		std::vector<std::string> categories_;
		std::string path_;
		int vNum_;
		bool isLatest_;
		int status_;
	};

	//----< show file name >---------------------------------------------

	inline void PayLoad::identify(std::ostream& out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}

	/////////////////////////////////////////////////////////////////////
	// PayLoad display functions

	inline void PayLoad::showPayLoadHeaders(std::ostream& out)
	{
		out << "\n  ";
		out << std::setw(25) << std::left << "Key";
		out << std::setw(45) << std::left << "Status";
		out << std::setw(25) << std::left << "Categories";
		out << "\n  ";
		out << std::setw(10) << std::left << "--------";
		out << std::setw(40) << std::left << "--------------------------------------";
		out << std::setw(25) << std::left << "-----------------------";
	}


	inline void PayLoad::showElementPayLoad(std::string key, NoSqlDb::DbElement<PayLoad>& elem, std::ostream& out)
	{
		out << "\n  ";
		out << std::setw(25) << std::left << key;
		std::string status;
		if (elem.payLoad().status() == 1)
			status = "Open";
		else
			status = "Close";
		out << std::setw(40) << std::left << status;
		for (auto cat : elem.payLoad().categories())
		{
			out << cat << " ";
		}
	}

	inline void PayLoad::showDb(NoSqlDb::DbCore<PayLoad>& db, std::ostream& out)
	{
		showPayLoadHeaders(out);
		for (auto item : db)
		{
			NoSqlDb::DbElement<PayLoad> temp = item.second;
			PayLoad::showElementPayLoad(item.first, temp, out);
		}
	}
}
#endif
