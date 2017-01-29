using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MifareApp_2._0.Model
{
    class Conversions
    {
        public static int GetByteCount(string hexString)
        {
            int numHexChars = 0;
            char c;

            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                {
                    numHexChars++;
                }
            }

            if (numHexChars % 2 != 0)
            {
                numHexChars--;
            }

            return (numHexChars / 2);
        }

        public static byte[] GetBytes(string hexString, out int discarded)
        {
            discarded = 0;
            string newString = "";
            char c;

            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                {
                    newString += c;
                }
                else
                {
                    discarded++;
                }
            }

            if (newString.Length % 2 != 0)
            {
                discarded++;
                newString = newString.Substring(0, newString.Length - 1);
            }

            int byteLength = newString.Length / 2;
            byte[] bytes = new byte[byteLength];
            string hex;
            int j = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                hex = new String(new Char[] { newString[j], newString[j + 1] });
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }

            return bytes;
        }
        public static string ToString(byte[] bytes)
        {
            string hexString = "";

            for (int i = 0; i < bytes.Length; i++)
            {
                hexString += bytes[i].ToString("X2");
            }

            return hexString;
        }
        public static bool InHexFormat(string hexString)
        {
            bool hexFormat = true;

            foreach (char digit in hexString)
            {
                if (!IsHexDigit(digit))
                {
                    hexFormat = false;
                    break;
                }
            }

            return hexFormat;
        }

        public static bool IsHexDigit(Char c)
        {
            int numChar;
            int numA = Convert.ToInt32('A');
            int num1 = Convert.ToInt32('0');
            c = Char.ToUpper(c);
            numChar = Convert.ToInt32(c);

            if (numChar >= numA && numChar < (numA + 6))
            {
                return true;
            }
            if (numChar >= num1 && numChar < (num1 + 10))
            {
                return true;
            }

            return false;
        }

        private static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
            {
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            }
                
            byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);

            return newByte;
        }

        public static byte[] toHexByteArrayFromDecimalArray(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);

            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            string stringHexArray = hex.ToString();
            byte[] hexArray = toHexByteArrayFromString(stringHexArray);

            return hexArray;
        }

        public static byte[] toHexByteArrayFromString(string hexString)
        {
            return Enumerable.Range(0, hexString.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[] getArrayBufferToWrite(byte blockNumber, byte[] blockNewContent)
        {
            byte[] array = { 0xFF, 0xD6, 0x00, blockNumber, 0x10 };
            var resultArray = new byte[array.Length + blockNewContent.Length];
            array.CopyTo(resultArray, 0);
            blockNewContent.CopyTo(resultArray, array.Length);

            return resultArray;
        }

        public static string ExtractIDNumber(string wholeBlockContent)
        {
            string idValue = "";
            
            for (int i = 3; i <= 11; i += 2)
            {
                idValue += wholeBlockContent[i];
            }

            return idValue;
        }
    }
}