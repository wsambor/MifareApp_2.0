using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MifareApp_2._0
{
    public static class Constants
    {   
        public static readonly string PASS_CORRECT_CARD = "Please, pass the corresponding Student ID Card";
        public static readonly string PASS_NEXT = "Current Card has been registered. Pass next one";
        public static readonly string INCORRECT_KEY_VALUE = "Incorrect key B value passed";
        public static readonly string AUTH_FAILED = "Authentication failed";
        public static readonly string FILE_NAME_SUFFIX = "_ExamPresence.txt";

        public static readonly int ASCII_OFFSET = 48;
        public static readonly int MASTER_KEY_LENGTH = 16;
        public static readonly int BLOCKS_IN_SECTOR = 4;
        public static readonly int TRAILER_BLOCK_NUMBER = 3;
        public static readonly int TIMER_INTERVAL_TIME = 100;

        public static readonly byte KEY_A_TYPE = 0x60;
        public static readonly byte KEY_B_TYPE = 0x61;
        public static readonly byte STUDENT_ID_BLOCK_NUMBER = 60;
    }
}
