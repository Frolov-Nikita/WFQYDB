using System;
using System.Collections.Generic;
using System.Text;
using Term;
using WFQYDB;

namespace WFQYDB_emu
{
    public static class ExtentionsEmu
    {
        public static TermView GetInfo(this WFQYDBemul emu)
        {
            var view = new TermView { FloatDirection = FloatDirection.Vertical }
                .Add($"Emu id:{emu.MyAddress[0]}.{emu.MyAddress[1]}.{emu.MyAddress[2]}.{emu.MyAddress[3]} State: {emu.State}");

            var statusView = new TermView { FloatDirection = FloatDirection.Vertical }
                .Add(new TermView { FloatDirection = FloatDirection.Horisontal }
                    .Add($"(0x{emu.Status.Byte.ToString("X2")}): ")
                    .Add(emu.Status.Start ? " started " : " stopped ", emu.Status.Start ? ConsoleColor.Green : ConsoleColor.White)
                    .Add(" [q]", ConsoleColor.DarkGray))
                .Add(new TermView { FloatDirection = FloatDirection.Horisontal }
                    .Add(emu.Status.ShortCircuit ? " shortCircuit " : " ok ", emu.Status.ShortCircuit ? ConsoleColor.Red : ConsoleColor.White)
                    .Add(" [w]", ConsoleColor.DarkGray))
                .Add(new TermView { FloatDirection = FloatDirection.Horisontal }
                    .Add(emu.Status.OverTemperature ? " OverTemperature " : " ok ", emu.Status.OverTemperature ? ConsoleColor.Red : ConsoleColor.White)
                    .Add(" [e]", ConsoleColor.DarkGray))
                .Add(new TermView { FloatDirection = FloatDirection.Horisontal }
                    .Add(emu.Status.OverLoad ? " OverLoad " : " ok ", emu.Status.OverLoad ? ConsoleColor.Red : ConsoleColor.White)
                    .Add(" [r]", ConsoleColor.DarkGray));

            var paramsView = new TermView { FloatDirection = FloatDirection.Vertical }
                .Add(new TermView { FloatDirection = FloatDirection.Horisontal, IdTmp = 101 }
                    .Add("Up frequency: ")
                    .Add(emu.UpFreq.ToString(), ConsoleColor.White)
                    .Add(" Hz")
                    .Add(" [u]", ConsoleColor.DarkGray))
                .Add(new TermView { FloatDirection = FloatDirection.Horisontal }
                    .Add("Down frequency: ")
                    .Add(emu.DnFreq.ToString(), ConsoleColor.White)
                    .Add(" Hz")
                    .Add(" [d]", ConsoleColor.DarkGray))
                .Add(new TermView { FloatDirection = FloatDirection.Horisontal }
                    .Add("Stoke length: ")
                    .Add(emu.StokeLength.ToString(), ConsoleColor.White)
                    .Add(" sm")
                    .Add(" [s]", ConsoleColor.DarkGray))
                .Add(new TermView { FloatDirection = FloatDirection.Horisontal }
                    .Add("Stoke rate: ")
                    .Add(emu.StokeRate.ToString(), ConsoleColor.White)
                    .Add(" 1/10min")
                    .Add(" [f]", ConsoleColor.DarkGray));

            var dataView = new TermView { FloatDirection = FloatDirection.Horisontal, IdTmp = 100 }
                .Add(statusView)
                .Add(paramsView);

            view.Add(dataView);

            var story = new TermView { FloatDirection = FloatDirection.Vertical }.Add($"Story last {emu.Story.Count} of {emu.Story.Limit}:");
            foreach (var s in emu.Story)
            {
                var color1 = s.Dir == MessageStoryItem.Direction.Tx ? ConsoleColor.Blue : ConsoleColor.Green;
                var color2 = s.Dir == MessageStoryItem.Direction.Tx ? ConsoleColor.DarkBlue : ConsoleColor.DarkGreen;

                story.Add(new TermView { FloatDirection = FloatDirection.Horisontal }
                    .Add($"{s.Dir}:\t", color1)
                    .Add(s.Buffer.ToHexString(0, 1), color2)
                    .Add(s.Buffer.ToHexString(1, 4), color1)
                    .Add(s.Buffer.ToHexString(5, 4), color2)
                    .Add(s.Buffer.ToHexString(9, 1), color1)
                    .Add(s.Buffer.ToHexString(10, 2), color2)
                    .Add(s.Data.ToHexString(), color1)
                    .Add("0x" + s.Crc.ToString("X2"), color2)
                );
            }

            view.Add(story);

            return view;
        }
    }
}
