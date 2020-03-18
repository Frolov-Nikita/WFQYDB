using System;
using System.Collections.Generic;
using System.Text;

namespace Term
{
    class Program
    {
        static void Main(string[] args)
        {
            var term = Term.Instance;

            TermView v = new TermView
            {
                Text = "AAA",
                Width = 80,
                FloatDirection = FloatDirection.Vertical,

            };

            TermView subV = new TermView
            {
                Text = "BBB",
                Backgroud = ConsoleColor.Gray,
                Foregroud = ConsoleColor.Black,
                Width = 20,
                Align = Align.Right,
            };

            v.Add(subV);

            var begin = DateTime.Now;
            for (var i = 0; i < 100; i++)
            {
                subV.Text = $"i = {i}";
                term.Render(v);
            }   

            var dur = DateTime.Now - begin;

            v.Text = "Dur is: " + dur.ToString();
            term.Render(v);



        }
    }
}
