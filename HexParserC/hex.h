
#include <stdint.h>

typedef enum RecordType
{
	RecordTypeData,
	RecordTypeEndOfFile,
	RecordTypeExtendedSegmentAddress,
	RecordTypeStartSegmentAddress,
	RecordTypeExtendedLinearAddress,
	RecordTypeStartLinearAddress
} RecordType_t;

typedef enum RecordError
{
	RecordErrorStartCode = 0xFFFFFFFF,
	RecordErrorChecksum = 0xFFFFFFFE,
	RecordErrorNoNextRecord = 0xFFFFFFFD,
	RecordErrorDataTooLong = 0xFFFFFFFC,
	RecordErrorNoError = 0xFFFFFFF0
} RecordError_t;

typedef struct FileCheckResult
{
	uint16_t number_of_entries;
	RecordError_t error;
} FileCheckResult_t;

typedef struct HexFileEntry
{
	uint16_t dataLength;
	uint16_t address;
	RecordType_t recordType;
	uint8_t data[16];
	uint8_t checksum;
	uint8_t checksumCheck;
} HexFileEntry_t;

//Test functions
void test_hexCharToUint8(void);
void test_hexCharsToUint8(void);
void test_hexCharsToUint16(void);

//Productive functions
uint32_t parseHexFileEntry(char *data, uint32_t offset, HexFileEntry_t *hexEntry);
void checkFile(char *data, FileCheckResult_t *checkResult);