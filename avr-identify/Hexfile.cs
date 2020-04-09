using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace avr_identify
{
    /// <summary>
    /// Read a file in the Intel Hex file format.
    /// Since the same data can be encoded differently (varying number of bytes per line)
    /// we decode it and return byte arrays, each prepended with the sections starting address.
    /// 
    /// </summary>
    public class Hexfile
    {
        private StreamReader _sr;

        public Hexfile(string filename)
        {
            _sr = new StreamReader(filename);
        }

        /*
Intel Hex Data Layout
Each line in an Intel Hex file has the same basic layout, like this:

:BBAAAATT[DDDDDDDD]CC

where
: is start of line marker
BB is number of data bytes on line
AAAA is address in bytes
TT is type discussed below but 00 means data
DD is data bytes, number depends on BB value
CC is checksum (2s-complement of number of bytes+address+data)

(from https://www.kanda.com/blog/microcontrollers/intel-hex-files-explained/)
        */

        public Section Read()
        {
            Section section;

            if (_sr.EndOfStream) return null;

            var line = _sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) return null;

            if (line.Substring(0, 1) != ":")
                throw new ApplicationException("Bad format: " + line);

            var len = Hex2Int(line.Substring(1, 2));
            var address = Hex2Int(line.Substring(3, 4));
            var type = Hex2Int(line.Substring(7, 2));

            if (type == 00) // Data
            {
                section = new Section();
                section.address = address;
                section.data = Hex2Bytearray(line.Substring(9, len * 2));

                // Optionally, check CRC
            }
            else if (type == 01) // End of file
            {
                return null;
            }
            else
            {
                // TODO: Handle type 2 and 4
                throw new ApplicationException($"Unhandled row type {type}: " + line);
            }

            // We have a valid section, see if we can add more data:

            while (!_sr.EndOfStream)
            {
                var loc = _sr.BaseStream.Position; // To be able to roll back a line

                line = _sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;

                if (line.Substring(0, 1) != ":")
                    throw new ApplicationException("Bad format: " + line);

                len = Hex2Int(line.Substring(1, 2));
                address = Hex2Int(line.Substring(3, 4));
                type = Hex2Int(line.Substring(7, 2));

                if (type != 00)
                {
                    _sr.BaseStream.Seek(loc, SeekOrigin.Begin);
                    break;
                }

                if (address != section.address + section.data.Length) // adjacent to previous line
                {
                    _sr.BaseStream.Seek(loc, SeekOrigin.Begin);
                    break;
                }

                // All OK, take the data
                var data = Hex2Bytearray(line.Substring(9, len * 2));
                section.data = section.data.Concat(data).ToArray();
            }

            return section;
        }

        private int Hex2Int(string hex)
        {
            int i = Convert.ToInt32(hex, 16);
            return i;
        }

        private byte Hex2Byte(string hex)
        {
            byte b = Convert.ToByte(hex, 16);
            return b;
        }

        private byte[] Hex2Bytearray(string hex)
        {
            // Assert hex.length % 2 == 0
            var len = hex.Length / 2;
            var data = new byte[len];
            for (int i = 0; i < hex.Length; i += 2)
            {
                data[i / 2] = Hex2Byte(hex.Substring(i, 2));
            }

            return data;
        }
        public class Section
        {
            public long address;
            public byte[] data;
        }
    }
}
