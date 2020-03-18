using System;
using System.Collections.Generic;
using System.Text;

namespace Term
{
    /// <summary>
    /// Консольный аналог <div/>
    /// </summary>
    public class TermView : List<TermView>
    {
        //TODO: доделать

        public TermView() {}
        public TermView(TermView original){
            Backgroud = original.Backgroud;
            Foregroud = original.Foregroud;
            FloatDirection = original.FloatDirection;
            Align = original.Align;
            Decoration = original.Decoration;
        }

        private int width = 0;
        private int height = 0;

        public int IdTmp { get; set; }

        public string Text { get; set; } = "";

        int MinWidth => 2 + Text.Length;

        int MinHeight => (int)Math.Ceiling((double)Text.Length / (Width - 1.9999));

        public int Width {
            get 
            {
                var w = width > 2 ? width : MinWidth;
                return w;
            }
            set => width = value; }

        public int Height {
            get 
            {
                var h = height > 1 ? height : MinHeight;
                return h;
            }
            set => height = value; }

        //public bool CanWordWrap { get; set; } = false;

        public Align Align { get; set; } = Align.Left;

        public FloatDirection FloatDirection { get; set; } = FloatDirection.Horisontal;

        public ConsoleColor Backgroud { get; set; } = ConsoleColor.Black;

        public ConsoleColor Foregroud { get; set; } = ConsoleColor.Gray;

        public Decoration Decoration { get; set; } = Decoration.None;

        public new TermView Add(TermView item)
        {
            if(item != null)
                base.Add(item);
            return this;
        }

        public TermView Add(string text) =>
            Add(text, this.Foregroud, this.Backgroud);

        public TermView Add(string text, ConsoleColor foregroud) => 
            Add(text, foregroud, this.Backgroud);

        public TermView Add(string text, ConsoleColor foregroud, ConsoleColor backgroud)
        {
            Add(new TermView
            {
                Text = text,
                Backgroud = backgroud,
                Foregroud = foregroud,
            });
            return this;
        }

        string Fit(string text, int len)
        {
            var txt = text.Trim();
            if ((len > 0) && (txt.Length == len))
                return txt;

            if (len == -1)
                return txt;

            if (len < 2)
                return "";

            if (txt.Length > len)
                return txt.Substring(0, len - 2) + "..";


            //val.Length < len
            var padLength = len - txt.Length;
            switch (Align)
            {
                case Align.Left:
                    return txt;// + new string(' ', padLength); //лишнее
                case Align.Center:
                    int padLengthL = padLength / 2;
                    int padLengthR = padLength - padLengthL;
                    return new string(' ', padLengthL) + txt + new string(' ', padLengthR);
                case Align.Right:
                    return new string(' ', padLength) + txt;
                default:
                    return txt;
            }
        }

        // int max(int a, int b) => a > b ? a : b;

        public TermRect Calc(int x = 0, int y = 0)
        {
            int cx = x;
            int cy = y;

            var rect = new TermRect();
            foreach (var v in this)
            {
                var subRect = v.Calc(cx, cy);
                if (FloatDirection == FloatDirection.Horisontal)
                    cx += subRect.Width;
                else
                    cy += subRect.Height;
                rect.Add(subRect);
            }

            rect.Add(new TermRect { 
                X0 = cx,
                Y0 = cy,
                Width = Width,
                Height = Height,
            });
            return rect;
        }

        public TermRect Build(ref TermChar[,] frame, int x = 0, int y = 0)
        {
            TermRect rect = new TermRect {X0 = x, Y0 = y };
            int nextX = x;
            int nextY = y;

            // 0) Блок должен влезать
            if ((frame.GetLength(1) < nextY + Height) || (frame.GetLength(0) < nextX + Width))
                return rect;

            // 1) Отображаем дочерние блоки
            foreach (var subView in this)
            {
                if (subView == null)
                    continue;

                var r = subView.Build(ref frame, nextX, nextY);

                if (r == null)
                    break;

                rect.Add(r);

                if (FloatDirection == FloatDirection.Horisontal)
                    nextX += r.Width;// + 1;
                else
                    nextY += r.Height;// + 1;
            }

            // 2) Отображаем текущий блок           
            if (nextX + Width > frame.GetLength(0)) // Тут уже область не отображается
                return rect;

            var txt = Fit(Text, Width);

            for (var i = 0; i < txt.Length; i++)
                frame[nextX + i, nextY].SetVal(
                    txt[i], 
                    Backgroud, 
                    Foregroud);

            rect.Add(new TermRect {
                X0 = nextX,
                Width = txt.Length + 1,
                Y0 = nextY,
                Height = 1,
            });

            return rect;
        }
    }
}
