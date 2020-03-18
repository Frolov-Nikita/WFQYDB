using System;
using System.Collections.Generic;

namespace Term
{
    public class TermString
    {
        private int length = 0;
        private string text = "";
        private bool isZero = false;

        public TermString() { }

        public TermString(TermChar termChar) 
        {
            if(termChar.Char == 0x0)
            {
                IsZero = true;
                Length = 1;
                return;
            }

            Text = termChar.Char.ToString();
            Backgroud = termChar.Backgroud;
            Foregroud = termChar.Foregroud;
        }

        public TermString(string str)
        {
            Text = str;
        }

        public TermString(TermString other)
        {
            Text = other.Text;
            Backgroud = other.Backgroud;
            Foregroud = other.Foregroud;
        }

        public string Text
        {
            get => text;
            set
            {
                text = value;
                IsZero = false;
            }
        }

        public int Length
        {
            get => length == 0 ? Text.Length : length;
            set => length = value;
        }

        /// <summary>
        /// Курсор перемещается на Length единиц
        /// </summary>
        public bool IsZero 
        { 
            get => isZero || text == default || text[0] == 0x0;
            set => isZero = value; 
        }

        public ConsoleColor Backgroud { get; set; } = ConsoleColor.Black;

        public ConsoleColor Foregroud { get; set; } = ConsoleColor.Gray;

        public override string ToString() => IsZero ? default : Text;
    }

    public class TermLine :List<TermString>
    {
        TermString last => Count > 0 ? this[Count - 1] : null;

        public bool IsBlank => (Count == 0) || ((Count == 1) && (this[0].IsZero));

        public void Add(TermChar tc)
        {
            if (last != null)
            {
                if (tc.Char == 0x0)
                {
                    if (last.IsZero)
                    {
                        last.Length++;
                        return;
                    }
                    else
                    {
                        Add(new TermString(tc));
                        return;
                    }
                }

                if ((!last.IsZero) &&
                    (last.Backgroud == tc.Backgroud) &&
                    (last.Foregroud == tc.Foregroud))
                {
                    last.Text += tc.Char;
                    return;
                }
            }
            
            Add(new TermString(tc));
        }
    }

}
