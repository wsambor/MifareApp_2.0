using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MifareApp_2._0.Model
{
    public class CardReader
    {
        public string Name { get; set; }
        public List<string> readers { get; set; }

        public static readonly byte BLOCK_LENGTH = 0x10;
        public int retVal;
        public uint dwScope;                                                // Scope of the resource manager context
        public IntPtr hContext;                                             // Context Handle value
        public string ReaderList;                                           // List Of Reader
        HID.HiDWinscard.SCARD_READERSTATE ReaderState;                      // Object of SCARD_READERSTATE
        IntPtr hCard;                                                       // Card handle
        IntPtr protocol;                                                    // Protocol used currently
        int valueTimeout;                                                   // The maximum amount of time to wait for an action
        uint readerCount;                                                   // Count for number of readers
        public bool IsAuthenticated;                                        // Boolean variable to check the authentication
        bool releaseFlag;                                                   // Flag to release
        int discarded;                                                      // Stores the number of discarded character
        byte bcla;                                                          // Class Byte
        byte bins;                                                          // Instruction Byte
        byte bp1;                                                           // Parameter Byte P1
        byte bp2;                                                           // Parameter Byte P2
        byte len;                                                           // Lc/Le Byte
        byte[] sendBuffer = new byte[255];                                  // Send Buffer in SCardTransmit
        byte[] receiveBuffer = new byte[255];                               // Receive Buffer in SCardTransmit
        int sendbufferlen;                                                  // Send Buffer length in SCardTransmit
        int receivebufferlen;                                               // Receive Buffer length in SCardTransmit
        byte currentBlock;                                                  // Stores the current block selected

        public CardReader()
        {

        }

        public CardReader(string name)
        {
            Name = name;
        }

        public List<CardReader> getListReaders()
        {
            uint pcchReaders = 0;
            int nullindex = -1;
            char nullchar = (char)0;
            List<CardReader> readers = new List<CardReader>();

            // Establish context.
            dwScope = 2;
            retVal = HID.SCardEstablishContext(dwScope, IntPtr.Zero, IntPtr.Zero, out hContext);
            retVal = HID.SCardListReaders(hContext, null, null, ref pcchReaders);
            byte[] mszReaders = new byte[pcchReaders];

            // Fill readers buffer with second call.
            retVal = HID.SCardListReaders(hContext, null, mszReaders, ref pcchReaders);

            string currBuffer = Encoding.ASCII.GetString(mszReaders);
            ReaderList = currBuffer;

            int length = (int)pcchReaders;

            if (length > 0)
            {
                while (currBuffer[0] != nullchar)
                {
                    nullindex = currBuffer.IndexOf(nullchar);  
                    string reader = currBuffer.Substring(0, nullindex);
                    readers.Add(new CardReader(reader));
                    length = length - (reader.Length + 1);
                    currBuffer = currBuffer.Substring(nullindex + 1, length);
                }
            }

            return readers;
        }

        public bool Connect(out string status)
        {
            status = "";

            retVal = HID.SCardConnect(hContext, 
                                      Name, 
                                      HID.HiDWinscard.SCARD_SHARE_SHARED, 
                                      HID.HiDWinscard.SCARD_PROTOCOL_T1,
                                      ref hCard, 
                                      ref protocol); 

            ReaderState.RdrName = Name;
            ReaderState.RdrCurrState = HID.HiDWinscard.SCARD_STATE_UNAWARE;
            ReaderState.RdrEventState = 0;
            ReaderState.UserData = "Mifare Card";
            valueTimeout = 0;
            readerCount = 1;

            if (retVal == 0)
            {
                status = "> SCardConnect" + "   Successful \n";
                retVal = HID.SCardGetStatusChange(hContext, valueTimeout, ref ReaderState, readerCount);

                return true;
            }
            else
            {
                status = "> SCardConnect" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retVal) + "H\n";

                return false;
            }
        }

        public void Disconnect(out string status)
        {
            status = "";

            retVal = HID.SCardDisconnect(hCard, HID.HiDWinscard.SCARD_UNPOWER_CARD);
            if (retVal == 0)
            {
                status = "> SCardDisconnect" + "   Successful \n";
            }
            else
            {
                status = "> SCardDisConnect" + "   Failed... " + "   Error Code: " + String.Format("{0:x}");

            }
        }

        public void sCardEstablishContext(out string status)
        {
            status = "";

            try
            {
                dwScope = 2;
                if (Name != "" && Name != null)
                {
                    retVal = HID.SCardEstablishContext(dwScope, IntPtr.Zero, IntPtr.Zero, out hContext);
                    if (retVal == 0)
                    {
                        IsAuthenticated = false;
                        status = "> SCardEstablishContext" + " Successful";
                        releaseFlag = true;
                    }
                    else
                    {
                        status = "> SCardEstablishContext" + " Failed... " + " Error Code: " + String.Format("{0:x}", retVal) + "H\n";
                    }
                }
                else
                {
                    status = "Failed... " + String.Format("{0:x}", retVal) + "H\n";
                }
            }
            catch
            {
                // TODO: Fill by some exception
            }
        }

        public void LoadKey(int keynum, string key, out string status)
        {
            HID.HiDWinscard.SCARD_IO_REQUEST sioreq;
            sioreq.dwProtocol = 0x2;
            sioreq.cbPciLength = 8;
            HID.HiDWinscard.SCARD_IO_REQUEST rioreq;
            rioreq.cbPciLength = 8;
            rioreq.dwProtocol = 0x2;

            if (key.Length == 12)
            {
                Byte[] str3 = Conversions.GetBytes(key, out discarded); 
                bcla = 0xFF;
                bins = 0x82;
                bp1 = 0x20;
                bp2 = (byte)keynum;
                len = 0x6;
                sendBuffer[0] = bcla;
                sendBuffer[1] = bins;
                sendBuffer[2] = bp1;
                sendBuffer[3] = bp2;
                sendBuffer[4] = len;
                for (int k = 0; k <= str3.Length - 1; k++)
                {
                    sendBuffer[k + 5] = str3[k];
                }
                sendbufferlen = 0xB;
                receivebufferlen = 255;
                retVal = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
                if (retVal == 0)
                {
                    if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                    {
                        status = "> LOAD KEY ( No. " + keynum + " )   Successful \n";
                    }
                    else
                    {
                        status = "> Load Key" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
                    }
                }
                else
                {
                    status = "> Load Key" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retVal) + "H\n";
                }

            }
            else
            {
                status = "Klucz nie ma odpowiedniej długości!";
            }
        }

        public int GetStatusChange(out string status)
        {
            status = "";

            ReaderState.RdrName = Name;
            ReaderState.RdrCurrState = HID.HiDWinscard.SCARD_STATE_UNAWARE;
            ReaderState.RdrEventState = 0;
            ReaderState.UserData = "Mifare Card";
            valueTimeout = 0;
            readerCount = 1;

            if (ReaderList == "")
            {
                status = "SmartCard Removed";
                return 1;
            }
            else
            {
                retVal = HID.SCardGetStatusChange(hContext, valueTimeout, ref ReaderState, readerCount);
                if ((ReaderState.ATRLength == 0) || (retVal != 0))
                {
                    status = "SmartCard Removed";
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void Authentication(byte blockNumber, byte keynum, byte keyNumber, out string status)
        {
            status = "";
            currentBlock = blockNumber;

            HID.HiDWinscard.SCARD_IO_REQUEST sioreq;
            sioreq.dwProtocol = 0x2;
            sioreq.cbPciLength = 8;
            HID.HiDWinscard.SCARD_IO_REQUEST rioreq;
            rioreq.cbPciLength = 8;
            rioreq.dwProtocol = 0x2;

            bcla = 0xFF;
            bins = 0x86;
            bp1 = 0x0;
            bp2 = 0x0; 
            len = 0x5;
            sendBuffer[0] = bcla;
            sendBuffer[1] = bins;
            sendBuffer[2] = bp1;
            sendBuffer[3] = bp2;

            sendBuffer[4] = len;
            sendBuffer[5] = 0x1;           
            sendBuffer[6] = 0x0;           
            sendBuffer[7] = currentBlock;  

            //sendBuffer[8] = 0x60; //Key Type A
            //sendBuffer[8] = 0x61; //Key Type B
            sendBuffer[8] = keyNumber;
            sendBuffer[9] = keynum;  

            sendbufferlen = 0xA;
            receivebufferlen = 255;
            retVal = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
            if (retVal == 0)
            {
                if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                {
                    IsAuthenticated = true;
                    status = "> General Authenticate" + "   Successful \n";

                }
                else
                {
                    IsAuthenticated = false;
                    status = "> General Authenticate" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
                }
            }
            else
            {
                IsAuthenticated = false;
                status = "> General Authenticate" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retVal) + "H\n";
            }
        }

        public string Write(byte blockNumber, out string status, byte[] blockMessage)
        {
            status = "";
            String read_str;

            if (IsAuthenticated)
            {
                HID.HiDWinscard.SCARD_IO_REQUEST sioreq;
                sioreq.dwProtocol = 0x2;
                sioreq.cbPciLength = 8;
                HID.HiDWinscard.SCARD_IO_REQUEST rioreq;
                rioreq.cbPciLength = 8;
                rioreq.dwProtocol = 0x2;

                byte[] writeCmd = Conversions.getArrayBufferToWrite(blockNumber, blockMessage);
                receivebufferlen = 0x10;

                retVal = HID.SCardTransmit(hCard, ref sioreq, writeCmd, writeCmd.Length, ref rioreq, receiveBuffer, ref receivebufferlen);
                if (retVal == 0)
                {
                    if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                    {
                        read_str = Conversions.ToString(receiveBuffer);

                        status = "> WRITE BINARY         ( Block " + blockNumber.ToString() + " )  Successful\n";

                        return read_str.Substring(0, ((int)(receivebufferlen - 2)) * 2);
                    }
                    else
                    {
                        status = "> SCardTransmit" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
                    }
                }
            }
            else
            {
                status = "> SCardTransmit" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retVal) + "H\n";
            }

            return "";
        }

        public string Read(byte blockNumber, out string status)
        {
            status = "";
            String read_str;

            if (IsAuthenticated)
            {
                HID.HiDWinscard.SCARD_IO_REQUEST sioreq;
                sioreq.dwProtocol = 0x2;
                sioreq.cbPciLength = 8;
                HID.HiDWinscard.SCARD_IO_REQUEST rioreq;
                rioreq.cbPciLength = 8;
                rioreq.dwProtocol = 0x2;

                byte[] readCmd = getArrayBufferToRead(blockNumber);
                receivebufferlen = 0x12;

                retVal = HID.SCardTransmit(hCard, ref sioreq, readCmd, readCmd.Length, ref rioreq, receiveBuffer, ref receivebufferlen);
                if (retVal == 0)
                {
                    if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                    {
                        read_str = Conversions.ToString(receiveBuffer);
                        status = "> READ BINARY         ( Block " + blockNumber.ToString() + " )  Successful\n";

                        return read_str.Substring(0, ((int)(receivebufferlen - 2)) * 2);
                    }
                    else
                    {
                        status = "> SCardTransmit" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
                    }
                }
                else
                {
                    status = "> SCardTransmit" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retVal) + "H\n";
                }
            }

            return "";
        }

        public string Increment(byte blockNumber, out string status)
        {
            status = "";
            String read_str;

            if (IsAuthenticated)
            {
                HID.HiDWinscard.SCARD_IO_REQUEST sioreq;
                sioreq.dwProtocol = 0x2;
                sioreq.cbPciLength = 8;
                HID.HiDWinscard.SCARD_IO_REQUEST rioreq;
                rioreq.cbPciLength = 8;
                rioreq.dwProtocol = 0x2;

                byte[] writeCmd = getArrayBufferToIncrement(blockNumber, 0x01);
                receivebufferlen = 0x01;

                retVal = HID.SCardTransmit(hCard, ref sioreq, writeCmd, writeCmd.Length, ref rioreq, receiveBuffer, ref receivebufferlen);
                if (retVal == 0)
                {
                    if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                    {
                        read_str = Conversions.ToString(receiveBuffer);
                        status = "> READ BINARY         ( Block " + blockNumber.ToString() + " )  Successful\n";

                        return read_str.Substring(0, ((int)(receivebufferlen - 2)) * 2);
                    }
                    else
                    {
                        status = "> SCardTransmit" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
                    }
                }
            }
            else
            {
                status = "> SCardTransmit" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retVal) + "H\n";
            }

            return "";
        }

        public string Decrement(byte blockNumber, out string status)
        {
            status = "";
            String read_str;

            if (IsAuthenticated)
            {
                HID.HiDWinscard.SCARD_IO_REQUEST sioreq;
                sioreq.dwProtocol = 0x2;
                sioreq.cbPciLength = 8;
                HID.HiDWinscard.SCARD_IO_REQUEST rioreq;
                rioreq.cbPciLength = 8;
                rioreq.dwProtocol = 0x2;

                byte[] writeCmd = getArrayBufferToDecrement(blockNumber, 0x01);
                receivebufferlen = 0x01;

                retVal = HID.SCardTransmit(hCard, ref sioreq, writeCmd, writeCmd.Length, ref rioreq, receiveBuffer, ref receivebufferlen);
                if (retVal == 0)
                {
                    if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                    {
                        read_str = Conversions.ToString(receiveBuffer);
                        status = "> READ BINARY         ( Block " + blockNumber.ToString() + " )  Successful\n";

                        return read_str.Substring(0, ((int)(receivebufferlen - 2)) * 2);
                    }
                    else
                    {
                        status = "> SCardTransmit" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
                    }
                }
            }
            else
            {
                status = "> SCardTransmit" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retVal) + "H\n";
            }

            return "";
        }

        public byte[] getArrayBufferToRead(byte blockNumber)
        {
            byte[] array = { 0xFF, 0xB0, 0x00, blockNumber, 0x00 };

            return array;
        }

        public byte[] getArrayBufferToWrite(byte blockNumber, byte[] blockNewContent)
        {
            byte[] array = { 0xFF, 0xD6, 0x00, blockNumber, 0x10 };
            var resultArray = new byte[array.Length + blockNewContent.Length];
            array.CopyTo(resultArray, 0);
            blockNewContent.CopyTo(resultArray, array.Length);

            return resultArray;
        }

        public byte[] getArrayBufferToIncrement(byte blockNumber, byte incrementValue)
        {
            byte[] array = { 0xFF, 0xD4, 0x00, blockNumber, 0x04, 0x00, 0x00, 0x00, incrementValue };

            return array;
        }

        public byte[] getArrayBufferToDecrement(byte blockNumber, byte decrementValue)
        {
            byte[] array = { 0xFF, 0xD8, 0x00, blockNumber, 0x04, 0x00, 0x00, 0x00, decrementValue };

            return array;
        }
    }
}
