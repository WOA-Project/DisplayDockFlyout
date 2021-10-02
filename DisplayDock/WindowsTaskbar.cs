/*
 * 
 * The following code was taken from https://github.com/File-New-Project/EarTrumpet
 * whose license is reproduced below:
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2015
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
*/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DisplayDockFlyout
{
    [StructLayout(LayoutKind.Sequential)]
    struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public AppBarEdge uEdge;
        public RECT rect;
        public int lParam;
    }

    internal enum AppBarEdge : uint
    {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }

    internal enum AppBarMessage : uint
    {
        // ...
        GetState = 0x00000004,
        GetTaskbarPos = 0x00000005
        // ...
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public override string ToString() => $"[Left={Left},Top={Top},Right={Right},Bottom={Bottom}]";
        public bool Contains(System.Drawing.Point pt) => pt.X >= Left && pt.X <= Right && pt.Y >= Top && pt.Y <= Bottom;
    }
    class Shell32
    {
        [DllImport("shell32.dll", PreserveSig = true)]
        public static extern UIntPtr SHAppBarMessage(
            AppBarMessage dwMessage,
            ref APPBARDATA pData);

        [Flags]
        public enum AppBarState
        {
            ABS_AUTOHIDE = 1
        }
    }

    public class User32
    {
        [DllImport("user32.dll", PreserveSig = true)]
        public static extern uint GetDpiForWindow(IntPtr hWnd);


        [DllImport("user32.dll", PreserveSig = true)]
        public static extern bool GetWindowRect(
            IntPtr hwnd,
            out RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern IntPtr FindWindow(
            [MarshalAs(UnmanagedType.LPWStr)]string lpClassName,
            string lpWindowName);
    }
    public sealed class WindowsTaskbar
    {
        public struct State
        {
            public Position Location;
            public RECT Size;
            public Screen ContainingScreen;
            public bool IsAutoHideEnabled;
        }

        // Must match AppBarEdge enum
        public enum Position
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        public static uint Dpi => User32.GetDpiForWindow(GetHwnd());

        public static State Current
        {
            get
            {
                var hWnd = GetHwnd();
                var state = new State
                {
                    ContainingScreen = Screen.FromHandle(hWnd),
                };
                var appBarData = new APPBARDATA
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)),
                    hWnd = hWnd
                };

                // SHAppBarMessage: Understands Taskbar auto-hide
                // state (the window is positioned across screens).
                if (Shell32.SHAppBarMessage(AppBarMessage.GetTaskbarPos, ref appBarData) != UIntPtr.Zero)
                {
                    state.Size = appBarData.rect;
                    state.Location = (Position)appBarData.uEdge;
                }
                else
                {
                    User32.GetWindowRect(hWnd, out state.Size);

                    if (state.ContainingScreen != null)
                    {
                        var screen = state.ContainingScreen;
                        if (state.Size.Bottom == screen.Bounds.Bottom && state.Size.Top == screen.Bounds.Top)
                        {
                            state.Location = (state.Size.Left == screen.Bounds.Left) ? Position.Left : Position.Right;
                        }
                        if (state.Size.Right == screen.Bounds.Right && state.Size.Left == screen.Bounds.Left)
                        {
                            state.Location = (state.Size.Top == screen.Bounds.Top) ? Position.Top : Position.Bottom;
                        }
                    }
                }

                var appBarState = (Shell32.AppBarState)Shell32.SHAppBarMessage(AppBarMessage.GetState, ref appBarData);
                state.IsAutoHideEnabled = appBarState.HasFlag(Shell32.AppBarState.ABS_AUTOHIDE);

                Trace.WriteLine($"WindowsTaskbar Current: Location={state.Location}, AutoHide={state.IsAutoHideEnabled}, Taskbar={hWnd}, Size={state.Size}, Monitor={state.ContainingScreen}");
                return state;
            }
        }

        private static IntPtr GetHwnd() => User32.FindWindow("Shell_TrayWnd", null);
    }
}