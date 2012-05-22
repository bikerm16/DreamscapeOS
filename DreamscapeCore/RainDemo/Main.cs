using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CursesSharp;

namespace DreamscapeCore.RainDemo
{
    public static class Main
    {
        private static Random rng;

        private static int NextJ(int j)
        {
            j = (j + 5 - 1) % 5;
            if (Curses.HasColors)
            {
                int z = rng.Next(3);
                uint color = Curses.COLOR_PAIR(z);
                if (z > 0)
                    color |= Attrs.BOLD;
                Stdscr.Attr = color;
            }
            return j;
        }

        internal static void Run()
        {
            rng = new Random();
            if (Curses.HasColors)
            {
                Curses.StartColor();
                short bg = Colors.BLACK;
                try
                {
                    Curses.UseDefaultColors();
                    bg = -1;
                }
                catch (CursesException) { }
                Curses.InitPair(1, Colors.BLUE, bg);
                Curses.InitPair(2, Colors.CYAN, bg);
            }
            Curses.Newlines = true;
            Curses.Echo = false;
            Curses.CursorVisibility = 0;
            Stdscr.ReadTimeout = 0;
            Stdscr.Keypad = true;

            int r = Curses.Lines - 4, c = Curses.Cols - 4;
            int[] xpos = new int[5];
            int[] ypos = new int[5];
            for (int j = 0; j < 5; j++)
            {
                xpos[j] = rng.Next(c) + 2;
                ypos[j] = rng.Next(r) + 2;
            }

            for (int j = 0; ; )
            {
                int x = rng.Next(c) + 2;
                int y = rng.Next(r) + 2;

                Stdscr.Add(y, x, '.');

                Stdscr.Add(ypos[j], xpos[j], 'o');

                j = NextJ(j);
                Stdscr.Add(ypos[j], xpos[j], 'O');

                j = NextJ(j);
                Stdscr.Add(ypos[j] - 1, xpos[j], '-');
                Stdscr.Add(ypos[j], xpos[j] - 1, "|.|");
                Stdscr.Add(ypos[j] + 1, xpos[j], '-');

                j = NextJ(j);
                Stdscr.Add(ypos[j] - 2, xpos[j], '-');
                Stdscr.Add(ypos[j] - 1, xpos[j] - 1, "/ \\");
                Stdscr.Add(ypos[j], xpos[j] - 2, "| O |");
                Stdscr.Add(ypos[j] + 1, xpos[j] - 1, "\\ /");
                Stdscr.Add(ypos[j] + 2, xpos[j], '-');

                j = NextJ(j);
                Stdscr.Add(ypos[j] - 2, xpos[j], ' ');
                Stdscr.Add(ypos[j] - 1, xpos[j] - 1, "   ");
                Stdscr.Add(ypos[j], xpos[j] - 2, "     ");
                Stdscr.Add(ypos[j] + 1, xpos[j] - 1, "   ");
                Stdscr.Add(ypos[j] + 2, xpos[j], ' ');

                xpos[j] = x;
                ypos[j] = y;

                switch (Stdscr.GetChar())
                {
                    case 'q':
                    case 'Q':
                        Curses.CursorVisibility = 1;
                        return;
                    case 's':
                        Stdscr.Blocking = true;
                        break;
                    case ' ':
                        Stdscr.Blocking = false;
                        break;
                    default:
                        break;
                }
                Curses.NapMs(50);
            }
        }
    }
}
