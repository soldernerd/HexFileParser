using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParseHexCS
{
    class Program
    {
        public enum RecordType
        {
            RecordTypeData,
            RecordTypeEndOfFile,
            RecordTypeExtendedSegmentAddress,
            RecordTypeStartSegmentAddress,
            RecordTypeExtendedLinearAddress,
            RecordTypeStartLinearAddress
        }

        public enum RecordError
        {
            RecordErrorStartCode = -1,
            RecordErrorChecksum = -2,
            RecordErrorNoNextRecord = -3,
            RecordErrorDataTooLong = -4
        }

        public unsafe struct HexFileEntry
        {
            public byte dataLength;
            public UInt16 address;
            public RecordType recordType;
            public fixed byte data[16];
            public byte checksum;
            public byte checksumCheck;
        }

        static unsafe void Main(string[] args)
        {

            HexFileEntry entry = new HexFileEntry();

            byte hexCharToByte(byte b)
            {
                //int number = (int)strtol(hexstring, NULL, 16);
                char c = (char)b;
                return byte.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber);
            }

            byte hexCharsToByte(byte b1, byte b2)
            {
                byte retVal = (byte)(hexCharToByte(b1) << 4);
                retVal += (byte)hexCharToByte(b2);
                return retVal;
            }

            UInt16 hexCharsToUint16(byte b1, byte b2, byte b3, byte b4)
            {
                UInt16 retVal = (UInt16)(hexCharToByte(b1) << 12);
                retVal += (UInt16)(hexCharToByte(b2) << 8);
                retVal += (UInt16)(hexCharToByte(b3) << 4);
                retVal += hexCharToByte(b4);
                return retVal;
            }

            void printHexFileEntry(ref HexFileEntry entr)
            {

                Console.WriteLine(String.Format("Data lenght: {0}", entr.dataLength));
                Console.WriteLine(String.Format("Address: {0}", entr.address));
                Console.WriteLine(String.Format("Record type: {0}", entr.recordType));

                Console.Write("Data: ");
                for (int j = 0; j < entr.dataLength; ++j)
                {
                    fixed (HexFileEntry* entry_ptr = &entr)
                    {
                        Console.Write(entry_ptr->data[j]);
                    }
                    Console.Write(' ');
                }
                Console.Write(System.Environment.NewLine);

                Console.WriteLine(String.Format("Checksum: {0}", entr.checksum));
                if (entr.checksumCheck != 0)
                    Console.WriteLine("ERROR: Checksum error");
                else
                    Console.WriteLine("Checksum ok");
            }

            int parseHexFileEntry(byte[] dat, int offset, ref HexFileEntry entr)
            {
                int j;

                /*
                for(int x=offset; x<offset+43; ++x)
                {
                    Console.Write((char)dat[x]);
                }
                Console.Write(System.Environment.NewLine);
                */

                //Check for start code
                if (dat[offset] != (byte)':')
                {
                    Console.WriteLine("ERROR: Line does not start with ':'");
                    return (int)RecordError.RecordErrorStartCode;
                }

                //Get data lenght
                entr.dataLength = hexCharsToByte(dat[offset + 1], dat[offset + 2]);
                if (entr.dataLength > 16)
                {
                    Console.WriteLine("ERROR: Too much data");
                    return (int)RecordError.RecordErrorDataTooLong;
                }

                //Get address
                entr.address = hexCharsToUint16(dat[offset + 3], dat[offset + 4], dat[offset + 5], dat[offset + 6]);

                //Get record type
                entr.recordType = (RecordType)hexCharsToByte(dat[offset + 7], dat[offset + 8]);

                //Get data
                for (j = 0; j < entr.dataLength; ++j)
                {
                    fixed (HexFileEntry* entry_ptr = &entr)
                    {
                        entry_ptr->data[j] = hexCharsToByte(dat[offset + 9 + j + j], dat[offset + 10 + j + j]);
                    }
                }

                //Get checksum
                entr.checksum = hexCharsToByte(dat[offset + 9 + j + j], dat[offset + 10 + j + j]);

                //Calculate checksum
                entr.checksumCheck = entr.dataLength;
                entr.checksumCheck += (byte)(entr.address >> 8);
                entr.checksumCheck += (byte)(entr.address & 0xFF);
                entr.checksumCheck += (byte)entr.recordType;
                for (j = 0; j < entr.dataLength; ++j)
                {
                    fixed (HexFileEntry* entry_ptr = &entr)
                    {
                        entr.checksumCheck += entry_ptr->data[j];
                    }
                }
                entr.checksumCheck += entr.checksum;

                //Throw an error if checksum does not match
                if (entr.checksumCheck != 0)
                {
                    Console.WriteLine("ERROR: Checksum error");
                    return (int)RecordError.RecordErrorChecksum;
                }

                //Find offset of the next entry
                if (entr.recordType != RecordType.RecordTypeEndOfFile)
                {
                    offset += 11 + j + j;
                    if (dat[++offset] == (byte)':')
                    {
                        return offset;
                    }
                    else if (dat[++offset] == (byte)':')
                    {
                        return offset;
                    }
                    else if (dat[++offset] == (byte)':')
                    {
                        return offset;
                    }
                    else
                    {
                        Console.WriteLine("ERROR: No next entry found");
                        return (int)RecordError.RecordErrorNoNextRecord;
                    }
                }
                else
                {
                    //Console.WriteLine("End of file");
                    return 0;
                }
            }

            string fileContent = File.ReadAllText(@"C:\Users\luke\OneDrive\Visual Studio 2017\Projects\HexParser\sample.hex", Encoding.ASCII);
            byte[] bytes = Encoding.ASCII.GetBytes(fileContent);

            Console.WriteLine(bytes.Length);

            int start = 0;
            int last_start = 0;
            int total_data = 0;
            while (start >= 0 && entry.recordType != RecordType.RecordTypeEndOfFile && start < 1000000)
            {
                last_start = start;
                start = parseHexFileEntry(bytes, start, ref entry);

                if (entry.recordType == RecordType.RecordTypeData)
                {
                    total_data += entry.dataLength;
                }

                if (entry.recordType != RecordType.RecordTypeData)
                {
                    Console.WriteLine(System.Environment.NewLine);
                    Console.WriteLine(String.Format("Offset: {0}", last_start));
                    printHexFileEntry(ref entry);
                }
            }

            Console.Write(System.Environment.NewLine);
            Console.WriteLine(String.Format("{0} bytes of data", total_data));
        }
    }
}
