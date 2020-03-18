using System;

namespace Term
{
    public struct TermChar
    {
        public TermChar(char c = (char)0x0, ConsoleColor bgColor = ConsoleColor.Black, ConsoleColor fgColor = ConsoleColor.Gray)
        {
            val = c | ((int)bgColor << 16) | ((int)fgColor << 20);
        }

        public int val; // nul серым по черному
        
        public TermChar SetVal(char c, ConsoleColor bgColor, ConsoleColor fgColor)
        {
            val = c | ((int)bgColor << 16) | ((int)fgColor << 20);
            return this;
        }

        public char Char
        {
            get => (char)(val & 0xFF_FF);
            set => val = (int)(val & 0xFF_FF_00_00) | value;
        }

        public ConsoleColor Backgroud
        {
            get => (ConsoleColor)((val & 0x0F_00_00) >> 16);
            set => val = (int)(val & 0xFF_F0_FF_FF) | ((int)value << 16);
        }

        public ConsoleColor Foregroud
        {
            get => (ConsoleColor)((val & 0xF0_00_00) >> 20);
            set => val = (int)(val & 0xFF_F0_FF_FF) | ((int)value << 20);
        }
    }
}
