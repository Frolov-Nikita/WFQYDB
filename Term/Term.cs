using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Term
{
    public delegate void OnKeyPressedDelegate(object sender, ConsoleKeyInfo keyInfo);
    public delegate void CancelKeyPressDelegate(object sender, ConsoleCancelEventArgs e);

    public class Term
    {
        CancellationTokenSource cts;
        Task inputTask;

        TermChar[,] lastFrame = new TermChar[0,0];
        //TermChar[,] curFrame = new TermChar[0, 0];// TODO: оптимизация хранения текущего буфера. Релокация только при изменении размера экрана.

        private Term()
        {
            cts = new CancellationTokenSource();

            inputTask = Task.Run(async() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var keyInfo = Console.ReadKey();
                        OnKeyPressed?.Invoke(this, keyInfo);
                    }
                    await Task.Delay(100, cts.Token);
                }
            }, cts.Token);
            Console.CancelKeyPress += Console_CancelKeyPress;            
        }

        public void Render(TermView view)
        {
            // Отрендерим новый frame
            int cWidth = Console.WindowWidth;
            int cHeight = Console.WindowHeight;

            TermChar[,] frame = new TermChar[cWidth, cHeight];

            TermChar nullChar = new TermChar { val = 0 };

            TermView baseView = new TermView { Width = cWidth, Height = cHeight };
            baseView.Add(view);
            baseView.Build(ref frame);

            // Отрисовка
            for (var y = 0; y < frame.GetLength(1); y++)
            {
                TermLine line = new TermLine();

                for (var x = 0; x < frame.GetLength(0); x++)
                {
                    var ch = frame[x, y];

                    if (lastFrame.GetLength(1) > y)
                    {
                        if (ch.val == lastFrame[x, y].val)
                            ch = nullChar;
                        else
                        {
                            if ((ch.val == 0) && (lastFrame[x, y].val != 0))
                                ch.Char = ' ';
                        }
                    }

                    line.Add(ch);
                }

                //Отрисовываем строку
                if (!line.IsBlank)
                {
                    int cx = 0;
                    Console.CursorTop = y;
                    Console.CursorLeft = cx;
                    foreach (var s in line)
                    {
                        if (s.IsZero)
                        {
                            var newX = Console.CursorLeft + s.Length;
                            if (newX >= cWidth)
                                break;
                            else
                                Console.CursorLeft = newX;
                        }
                        else
                        {
                            Console.BackgroundColor = s.Backgroud;
                            Console.ForegroundColor = s.Foregroud;
                            Console.Write(s.Text);
                        }
                    }
                }
            }
            // обрежем у текущего фрейма пустой хвост


            lastFrame = frame;
        }

        public void RenderFull(TermView view)
        {
            // Отрендерим новый frame
            int cWidth = Console.WindowWidth;
            int cHeight = Console.WindowHeight;

            TermChar[,] frame = new TermChar[cWidth, cHeight];

            TermView baseView = new TermView { Width = cWidth, Height = cHeight };
            baseView.Add(view);
            baseView.Build(ref frame);

            for (var y = 0; y < frame.GetLength(1); y++)
            {
                TermLine line = new TermLine();
                for (var x = 0; x < frame.GetLength(0); x++)
                    line.Add(frame[x,y]);

                //Отрисовываем строку
                if (!line.IsBlank)
                {
                    int cx = 0;
                    Console.CursorTop = y;
                    Console.CursorLeft = cx;
                    foreach (var s in line)
                    {
                        if (s.IsZero)
                        {
                            var newX = Console.CursorLeft + s.Length;
                            if (newX >= cWidth)
                                break;
                            Console.CursorLeft = newX;
                        }
                        else
                        {
                            Console.BackgroundColor = s.Backgroud;
                            Console.ForegroundColor = s.Foregroud;
                            Console.Write(s.Text);
                        }
                    }
                }
            }                
        }

        public void RenderFullFull(TermView view)
        {
            // Отрендерим новый frame
            int cWidth = Console.WindowWidth;
            int cHeight = Console.WindowHeight;

            TermChar[,] frame = new TermChar[cWidth, cHeight];

            TermView baseView = new TermView { Width = cWidth, Height = cHeight };
            baseView.Add(view);
            baseView.Build(ref frame);

            ConsoleColor bg = ConsoleColor.Black;
            ConsoleColor fg = ConsoleColor.Gray;
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
            bool isLastNull = true;

            for (var y = 0; y < frame.GetLength(1); y++)
                for (var x = 0; x < frame.GetLength(0); x++)
                {
                    var c = frame[x, y];
                    if (c.Char != 0)
                    {
                        if (bg != c.Backgroud)
                            Console.BackgroundColor = bg = c.Backgroud;
                        if (fg != c.Foregroud)
                            Console.ForegroundColor = fg = c.Foregroud;

                        if (isLastNull)
                        {
                            Console.CursorLeft = x;
                            Console.CursorTop = y;
                            isLastNull = false;
                        }

                        Console.Write(c.Char);
                    }
                    else
                        isLastNull = true;
                }
        }

        ~Term()
        {
            cts.Cancel();
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) =>
            CancelKeyPress?.Invoke(this, e);

        public static Term Instance { get; } = new Term();

        public event OnKeyPressedDelegate OnKeyPressed;
        public event CancelKeyPressDelegate CancelKeyPress;


        public string Title { get => Console.Title; set => Console.Title = value; }

        public void Write(string str) => Console.Write(str);



        public void WriteLine(string str) => Console.WriteLine(str);
    }
}
