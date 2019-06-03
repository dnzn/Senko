namespace Polymer
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using static Konsole.Parameters;
    using Drawing = System.Drawing;

    public static partial class Polymer
    {

    }

    public partial class Konsole
    {
        //public static Dictionary<ConsoleColor, object> DynamicColor = new Dictionary<ConsoleColor, object>();
        public class Dye
        {
            public struct DynamicColor
            {
                readonly dynamic _Color;

                public DynamicColor(string color)
                {
                    _Color = color;
                }

                public DynamicColor(Drawing.Color color)
                {
                    _Color = color;
                }

                string GetColor()
                {
                    return _Color.ToString();
                }

                public static implicit operator string(DynamicColor color)
                {
                    return color.GetColor();
                }

                public static implicit operator DynamicColor(string color)
                {
                    return new DynamicColor(color);
                }

                public static implicit operator DynamicColor(Drawing.Color color)
                {
                    return new DynamicColor(color);
                }
            }

            public DynamicColor Alias { get; private set; }
            public ConsoleColor ConsoleColor { get; private set; }
            
            public Dye(Drawing.Color color, ConsoleColor consoleColor)
            {
                Alias = color;
                ConsoleColor = consoleColor;
            }

            public Dye(string color, ConsoleColor consoleColor)
            {
                Alias = color;
                ConsoleColor = consoleColor;
            }

            public static implicit operator ConsoleColor(Dye color)
            {
                return color.ConsoleColor;
            }
        }
        
        void test ()
        {
            Dye c = new Dye("color", ConsoleColor.Black);

            ConsoleColor color = c;
        }

        public partial class Parameters
        {
            public partial class Color
            {
                public struct ColorAlias
                {
                    string Name;
                    ConsoleColor ConsoleColor;
                }

                // Code below was derived from the answer in the following StackOverflow.com link:
                // https://stackoverflow.com/questions/7937256/custom-text-color-in-c-sharp-console-application/11188358#11188358
                // I have added my own methods and overrides and modified some of original ones

                // Copyright Alex Shvedov
                // Modified by MercuryP with color specifications
                // Modified further by me to integrate with my Polymer.Konsole class

                [StructLayout(LayoutKind.Sequential)]
                internal struct COORD
                {
                    internal short X;
                    internal short Y;
                }

                [StructLayout(LayoutKind.Sequential)]
                internal struct SMALL_RECT
                {
                    internal short Left;
                    internal short Top;
                    internal short Right;
                    internal short Bottom;
                }

                [StructLayout(LayoutKind.Sequential)]
                internal struct COLORREF
                {
                    internal uint ColorDWORD;

                    internal COLORREF(Drawing.Color color)
                    {
                        ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
                    }

                    internal COLORREF(uint r, uint g, uint b)
                    {
                        ColorDWORD = r + (g << 8) + (b << 16);
                    }

                    internal Drawing.Color GetColor()
                    {
                        return Drawing.Color.FromArgb((int)(0x000000FFU & ColorDWORD),
                                              (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
                    }

                    internal void SetColor(Drawing.Color color)
                    {
                        ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
                    }
                }

                [StructLayout(LayoutKind.Sequential)]
                internal struct CONSOLE_SCREEN_BUFFER_INFO_EX
                {
                    internal int cbSize;
                    internal COORD dwSize;
                    internal COORD dwCursorPosition;
                    internal ushort wAttributes;
                    internal SMALL_RECT srWindow;
                    internal COORD dwMaximumWindowSize;
                    internal ushort wPopupAttributes;
                    internal bool bFullscreenSupported;
                    internal COLORREF black;
                    internal COLORREF darkBlue;
                    internal COLORREF darkGreen;
                    internal COLORREF darkCyan;
                    internal COLORREF darkRed;
                    internal COLORREF darkMagenta;
                    internal COLORREF darkYellow;
                    internal COLORREF gray;
                    internal COLORREF darkGray;
                    internal COLORREF blue;
                    internal COLORREF green;
                    internal COLORREF cyan;
                    internal COLORREF red;
                    internal COLORREF magenta;
                    internal COLORREF yellow;
                    internal COLORREF white;
                }

                const int STD_OUTPUT_HANDLE = -11;                                        // per WinBase.h
                internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);    // per WinBase.h

                [DllImport("kernel32.dll", SetLastError = true)]
                private static extern IntPtr GetStdHandle(int nStdHandle);

                [DllImport("kernel32.dll", SetLastError = true)]
                private static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX csbe);

                [DllImport("kernel32.dll", SetLastError = true)]
                private static extern bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX csbe);

                // Set a specific console color to an RGB color
                // The default console colors used are gray (foreground) and black (background)
                public static int SetColor(ConsoleColor consoleColor, Drawing.Color targetColor)
                {
                    return SetColor(consoleColor, targetColor.R, targetColor.G, targetColor.B);
                }

                public static int SetColor(string name, ConsoleColor consoleColor, uint r, uint g, uint b)
                {
                    return SetColor(consoleColor, r, g, b);
                }

                static int SetColor(ConsoleColor color, uint r, uint g, uint b)
                {
                    CONSOLE_SCREEN_BUFFER_INFO_EX csbe = new CONSOLE_SCREEN_BUFFER_INFO_EX();
                    csbe.cbSize = (int)Marshal.SizeOf(csbe);                    // 96 = 0x60
                    IntPtr hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE);    // 7
                    if (hConsoleOutput == INVALID_HANDLE_VALUE)
                    {
                        return Marshal.GetLastWin32Error();
                    }
                    bool brc = GetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe);
                    if (!brc)
                    {
                        return Marshal.GetLastWin32Error();
                    }

                    switch (color)
                    {
                        case ConsoleColor.Black:
                            csbe.black = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.DarkBlue:
                            csbe.darkBlue = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.DarkGreen:
                            csbe.darkGreen = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.DarkCyan:
                            csbe.darkCyan = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.DarkRed:
                            csbe.darkRed = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.DarkMagenta:
                            csbe.darkMagenta = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.DarkYellow:
                            csbe.darkYellow = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.Gray:
                            csbe.gray = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.DarkGray:
                            csbe.darkGray = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.Blue:
                            csbe.blue = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.Green:
                            csbe.green = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.Cyan:
                            csbe.cyan = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.Red:
                            csbe.red = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.Magenta:
                            csbe.magenta = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.Yellow:
                            csbe.yellow = new COLORREF(r, g, b);
                            break;
                        case ConsoleColor.White:
                            csbe.white = new COLORREF(r, g, b);
                            break;
                    }
                    ++csbe.srWindow.Bottom;
                    ++csbe.srWindow.Right;
                    brc = SetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe);
                    if (!brc)
                    {
                        return Marshal.GetLastWin32Error();
                    }
                    return 0;
                }
            }
        }
    }
}
