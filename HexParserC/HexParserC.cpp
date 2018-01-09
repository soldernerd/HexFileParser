// HexParserC.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <iostream>
#include <fstream>
#include <string>

#include "hex.h"


using namespace std;




int main()
{
	const int arraysize = 1000000;
	char data[arraysize];
	int i = 0;

	//Run some tests
	//test_hexCharToUint8();
	//test_hexCharsToUint8();
	//test_hexCharsToUint16();

	string infile = "C:\\Users\\luke\\OneDrive\\Visual Studio 2017\\Projects\\HexParser\\sample.hex";
	cout << infile << "\n";
	
	//Copy file to char array
	ifstream myfile(infile);
	if (myfile.is_open())
	{
		while (!myfile.eof() && i<arraysize)
		{
			myfile.get(data[i]);
			i++;
		}
	}

	//Parse entire file for errors
	printf("Checking file for errors\n");
	FileCheckResult_t result;
	checkFile(data, &result);
	if (result.error == RecordErrorNoError)
	{
		printf("No errors, %d entries\n", result.number_of_entries);
	}
	else
	{
		printf("Error at entry %d\n", result.number_of_entries);
		switch (result.error)
		{
			case RecordErrorStartCode:
				printf("RecordErrorStartCode\n");
				break;
			case RecordErrorChecksum:
				printf("RecordErrorChecksum\n");
				break;
			case RecordErrorNoNextRecord:
				printf("RecordErrorNoNextRecord\n");
				break;
			case RecordErrorDataTooLong:
				printf("RecordErrorDataTooLong\n");
				break;
			default:
				printf("Error code 0x%x\n", result.error);
		}
	}
	
	printf("done\n");
	return 0;
    
}

