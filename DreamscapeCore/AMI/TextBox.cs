using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CursesSharp;

namespace DreamscapeCore.AMI
{
    public class TextBox
    {
        public Pad pad;
        private int yPos, xPos, nlines, ncols;
        private int yScroll, xScroll;
        private int bufferWidth, bufferHeight;
        private int curY, curX;
        private int lastLine;
        private Window title, status;
        private bool modified;

        public TextBox(int nlines, int ncols, int y, int x, Window title, Window status)
        {
            this.bufferWidth = 2000;
            this.bufferHeight = 2000;
            this.pad = new Pad(this.bufferHeight, this.bufferWidth);
            this.nlines = nlines;
            this.ncols = ncols;
            this.yPos = y;
            this.xPos = x;
            this.yScroll = 0;
            this.xScroll = 0;
            this.lastLine = 0;
            this.status = status;
            this.title = title;
            this.modified = false;
        }

        public void Fill(string[] data)
        {
            //Random random = new Random();
            //for (int line = 0; line < this.nlines; line++)
            //{
            //    for (int col = 0; col < this.ncols-70; col++)
            //    {
            //        this.pad.Add(line, col, (char)('a' + random.Next(0, 26)));
            //    }
            //    this.pad.AttrOn(CursesSharp.Attrs.INVIS);
            //    this.pad.Add('$');
            //    this.pad.AttrOff(CursesSharp.Attrs.INVIS);
            //    this.lastLine++;
            //}
            //this.lastLine--;
            //Move(0, 0);

            if (data == null)
            {
                this.pad.AttrOn(CursesSharp.Attrs.INVIS);
                this.pad.Add(0, 0, '$');
                this.pad.AttrOff(CursesSharp.Attrs.INVIS);
                Move(0, 0);
                return;
            }

            for (int i = 0; i < data.Length; i++)
            {
                this.lastLine++;
                this.pad.Add(i, 0, data[i]);
                this.pad.AttrOn(CursesSharp.Attrs.INVIS);
                this.pad.Add('$');
                this.pad.AttrOff(CursesSharp.Attrs.INVIS);
            }
            this.lastLine--;
            Move(0, 0);
        }

        private int GetEOL(int line)
        {
            int curY, curX;
            this.pad.GetCursorYX(out curY, out curX);

            for (int x = 0; x < this.bufferWidth; x++)
            {
                uint ch = this.pad.ReadOutputChar(line, x);
                if ((ch & CursesSharp.Attrs.CHARTEXT) == (char)'$')
                    if ((ch & CursesSharp.Attrs.ATTRIBUTES) == CursesSharp.Attrs.INVIS)
                    { this.pad.Move(curY, curX); return x; }
            }

            this.pad.Move(curY, curX);

            return -1;
        }

        public string[] Edit()
        {
            Curses.Echo = false;
            this.pad.Keypad = true;
            this.pad.Refresh(this.yScroll, this.xScroll, this.yPos, this.xPos, this.yPos + this.nlines - 1, this.xPos + this.ncols - 1);
            while (true)
            {
                int c = this.pad.GetChar();
                this.pad.GetCursorYX(out curY, out curX);
                if (this.modified)
                    this.title.Add(0, 80 - "Modified".Length - 2, "Modified");
                this.title.Refresh();
                switch (c)
                {
                    case 24:     //Ctrl-X
                        if (!this.modified)
                            return null;
                        string[] output = new string[this.lastLine + 1];
                        for (int i = 0; i < this.lastLine + 1; i++)
                            output[i] = this.pad.ReadOutputString(i, 0, GetEOL(i));
                        Move(curY, curX);
                        return output;
                    case CursesSharp.Keys.RIGHT:
                        if (curX == GetEOL(curY))
                            Move(curY + 1, 0);
                        else
                            Move(curY, curX + 1);
                        break;
                    case CursesSharp.Keys.LEFT:
                        if (curX == 0 && curY != 0)
                            Move(curY - 1, GetEOL(curY - 1));
                        else
                            Move(curY, curX - 1);
                        break;
                    case CursesSharp.Keys.UP:
                        Move(curY - 1, curX);
                        break;
                    case CursesSharp.Keys.DOWN:
                        Move(curY + 1, curX);
                        break;
                    case CursesSharp.Keys.END:
                        Move(curY, GetEOL(curY));
                        break;
                    case CursesSharp.Keys.HOME:
                        Move(curY, 0);
                        break;
                    case '\n':
                        this.modified = true;
                        int oldEOL = GetEOL(curY);
                        string after = this.pad.ReadOutputString(curY, curX, oldEOL - curX);
                        if (after == null) after = "";
                        this.pad.AttrOn(CursesSharp.Attrs.INVIS);
                        this.pad.Add(curY, curX, '$');
                        this.pad.AttrOff(CursesSharp.Attrs.INVIS);
                        this.pad.Move(curY, curX + 1);
                        this.pad.ClearToEol();
                        MoveLinesDown(curY + 1);
                        this.pad.Add(curY + 1, 0, after);
                        this.pad.AttrOn(CursesSharp.Attrs.INVIS);
                        this.pad.Add(curY + 1, after.Length, '$');
                        this.pad.AttrOff(CursesSharp.Attrs.INVIS);
                        Move(curY + 1, 0);
                        break;
                    case '\b':
                        if (!(curX == 0 && curY == 0))
                            this.modified = true;
                        if (curX == 0)
                        {
                            if (curY != 0)
                            {
                                string str = this.pad.ReadOutputString(curY, curX, GetEOL(curY) - curX);
                                if (str == null) str = "";
                                this.pad.Move(curY, curX);
                                this.pad.ClearToEol();
                                MoveLinesUp(curY + 1);
                                int connection = GetEOL(curY - 1);
                                this.pad.Move(curY - 1, connection);
                                this.pad.Add(str);
                                this.pad.AttrOn(CursesSharp.Attrs.INVIS);
                                this.pad.Add('$');
                                this.pad.AttrOff(CursesSharp.Attrs.INVIS);
                                Move(curY - 1, connection);
                            }
                        }
                        else
                        {
                            string str = this.pad.ReadOutputString(curY, curX, GetEOL(curY) - curX);
                            if (str == null) str = "";
                            this.pad.Move(curY, curX - 1);
                            this.pad.ClearToEol();
                            this.pad.Add(str);
                            this.pad.AttrOn(CursesSharp.Attrs.INVIS);
                            this.pad.Add('$');
                            this.pad.AttrOff(CursesSharp.Attrs.INVIS);
                            Move(curY, curX - 1);
                        }
                        break;
                    case Keys.DC:
                        if (!(curX == GetEOL(curY) && curY == this.lastLine))
                            this.modified = true;
                        if (curX == GetEOL(curY))
                        {
                            if (curY != this.lastLine)
                            {
                                string str = this.pad.ReadOutputString(curY + 1, 0, GetEOL(curY + 1));
                                if (str == null) str = "";
                                this.pad.Move(curY + 1, 0);
                                this.pad.ClearToEol();
                                MoveLinesUp(curY + 2);
                                int connection = GetEOL(curY);
                                this.pad.Move(curY, connection);
                                this.pad.Add(str);
                                this.pad.AttrOn(CursesSharp.Attrs.INVIS);
                                this.pad.Add('$');
                                this.pad.AttrOff(CursesSharp.Attrs.INVIS);
                                Move(curY, connection);
                            }
                        }
                        else
                        {
                            string str = this.pad.ReadOutputString(curY, curX + 1, GetEOL(curY) - curX - 1);
                            if (str == null) str = "";
                            this.pad.Move(curY, curX);
                            this.pad.ClearToEol();
                            this.pad.Add(str);
                            this.pad.AttrOn(CursesSharp.Attrs.INVIS);
                            this.pad.Add('$');
                            this.pad.AttrOff(CursesSharp.Attrs.INVIS);
                            Move(curY, curX);
                        }
                        break;
                    case Keys.NPAGE:
                        if (this.yScroll + this.nlines > this.lastLine || this.lastLine - this.yScroll + this.nlines < this.nlines - 1)
                            this.yScroll = this.lastLine - this.nlines + 1;
                        else
                            this.yScroll += this.nlines;
                        Move(curY + this.nlines, curX);
                        break;
                    case Keys.PPAGE:
                        if (this.yScroll - this.nlines < 0 || this.yScroll - this.nlines < this.nlines - 1)
                            this.yScroll = 0;
                        else
                            this.yScroll -= this.nlines;
                        Move(curY - this.nlines, curX);
                        break;
                    case 3:     //Ctrl-C
                        int linePercent = (int)(((double)(curY + 1) / (this.lastLine + 1)) * 100);
                        int colPercent = (int)(((double)(curX + 1) / (GetEOL(curY))) * 100);
                        int charPercent = (int)(((double)CountChars(0, 0, curY, curX) / CountChars(0, 0, this.lastLine, GetEOL(this.lastLine))) * 100);
                        string toStatus = "[ line " + (curY + 1).ToString() + "/" + (this.lastLine + 1).ToString() + " (" + linePercent.ToString() + "%), " +
                            "col " + (curX + 1).ToString() + "/" + (GetEOL(curY)).ToString() + " (" + colPercent.ToString() + "%), " +
                            "char " + CountChars(0, 0, curY, curX).ToString() + "/" + CountChars(0, 0, this.lastLine, GetEOL(this.lastLine)).ToString() + " (" + charPercent.ToString() + "%) ]";
                        this.status.Move(0, 0); this.status.ClearToEol();
                        this.status.Add(0, (80 - toStatus.Length) / 2, toStatus);
                        this.status.Refresh();
                        this.pad.Refresh(this.yScroll, this.xScroll, this.yPos, this.xPos, this.yPos + this.nlines - 1, this.xPos + this.ncols - 1);
                        break;
                    default:
                        this.modified = true;
                        InsertBefore(curY, curX, (char)c);
                        break;
                }
            }
        }

        private int CountChars(int begY, int begX, int endY, int endX)
        {
            int counter = 0;

            if (endY != begY)
            {
                counter += GetEOL(begY) - begX + 1;     //First line
                counter += endX + 1;                    //Last line
            }
            else
                return endX - begX + 1;

            for (int line = begY + 1; line < endY; line++)
                counter += GetEOL(line) + 1;

            return counter;
        }

        private void MoveLinesDown(int begin)
        {
            for (int line = this.lastLine; line >= begin; line--)
            {
                this.pad.Move(line, 0);
                string copy = this.pad.ReadOutputString(GetEOL(line));
                this.pad.Move(line, 0);
                this.pad.ClearToEol();
                this.pad.Add(line + 1, 0, copy);
                this.pad.AttrOn(CursesSharp.Attrs.INVIS);
                this.pad.Add(line + 1, copy.Length, '$');
                this.pad.AttrOff(CursesSharp.Attrs.INVIS);
            }
            this.pad.AttrOn(CursesSharp.Attrs.INVIS);
            this.pad.Add(begin, 0, '$');
            this.pad.AttrOff(CursesSharp.Attrs.INVIS);
            this.lastLine++;
        }

        private void MoveLinesUp(int begin)
        {
            for (int line = begin; line <= this.lastLine; line++)
            {
                this.pad.Move(line, 0);
                string copy = this.pad.ReadOutputString(GetEOL(line));
                this.pad.Move(line, 0);
                this.pad.ClearToEol();
                this.pad.Add(line - 1, 0, copy);
                this.pad.AttrOn(CursesSharp.Attrs.INVIS);
                this.pad.Add(line - 1, copy.Length, '$');
                this.pad.AttrOff(CursesSharp.Attrs.INVIS);
            }
            this.pad.Move(this.lastLine, 0);
            this.pad.ClearToEol();
            this.lastLine--;
        }

        private void InsertBefore(int y, int x, char c)
        {
            string after = this.pad.ReadOutputString(y, x, GetEOL(y) - x);
            this.pad.Add(y, x, c);
            this.pad.Add(y, x + 1, after);
            this.pad.AttrOn(CursesSharp.Attrs.INVIS);
            this.pad.Add(y, x + 1 + after.Length, '$');
            this.pad.AttrOff(CursesSharp.Attrs.INVIS);
            Move(y, x + 1);
        }

        private void Move(int y, int x)
        {

            //Moving vertically
            if (y > this.bufferHeight - 1)
                y = this.bufferHeight - 1;
            else if (y < 0)
                y = 0;

            if (y > this.lastLine)
                y = lastLine;
            if (y > this.yScroll + this.nlines - 1)
                this.yScroll = y - this.nlines + 1;
            else if (y < this.yScroll)
                this.yScroll = y;

            //Moving horizontally
            if (x > this.bufferWidth - 1)
                x = bufferWidth - 1;
            else if (x < 0)
                x = 0;

            if (x > GetEOL(y))
                x = GetEOL(y);
            if (x > this.xScroll + this.ncols - 1)
                this.xScroll = x - this.ncols + 1;
            else if (x < this.xScroll)
                this.xScroll = x;

            //Really moving
            this.pad.Move(y, x);
            this.pad.Refresh(this.yScroll, this.xScroll, this.yPos, this.xPos, this.yPos + this.nlines - 1, this.xPos + this.ncols - 1);
        }
    }
}
