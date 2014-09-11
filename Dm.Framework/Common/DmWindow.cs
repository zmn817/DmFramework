﻿// -----------------------------------------------------------------------
//  <copyright file="DmWindow.cs" company="柳柳软件">
//      Copyright (c) 2014 66SOFT. All rights reserved.
//  </copyright>
//  <site>http://www.66soft.net</site>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-27 13:18</last-date>
// -----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;


namespace Dm.Framework.Common
{
    public class DmWindow
    {
        private readonly DmPlugin _dm;
        private readonly int _hwnd;
        private readonly DmSystem _system;

        public DmWindow(DmPlugin dm, int hwnd)
        {
            _hwnd = hwnd;
            _dm = dm;
            _system = new DmSystem(dm);
            BindType = WindowBindType.None;
            IsBind = false;
        }

        public int Hwnd { get { return _hwnd; } }

        public DmPlugin Dm { get { return _dm; } }

        public DmSystem System { get { return _system; } }

        public string Title
        {
            get { return Dm.GetWindowTitle(_hwnd); }
            set
            {
                if (!IsAlive)
                {
                    return;
                }
                Dm.SetWindowText(_hwnd, value);
            }
        }

        public string Class { get { return Dm.GetWindowClass(_hwnd); } }

        public int ProcessId { get { return Dm.GetWindowProcessId(_hwnd); } }

        public bool IsBind { get; private set; }

        /// <summary>
        /// 获取 窗口的绑定模式
        /// </summary>
        public WindowBindType BindType { get; private set; }

        /// <summary>
        /// 获取 窗口外部输入锁定类型
        /// </summary>
        public InputLockType InputLockType { get; private set; }

        public string ProcessPath { get { return Dm.GetWindowProcessPath(_hwnd); } }

        public Tuple<int, int> ClientSize
        {
            get
            {
                int w, h;
                Dm.GetClientSize(_hwnd, out w, out h);
                return new Tuple<int, int>(w, h);
            }
            set
            {
                if (!IsAlive)
                {
                    return;
                }
                Dm.SetClientSize(_hwnd, value.Item1, value.Item2);
            }
        }

        public Tuple<int, int, int, int> Rect
        {
            get
            {
                int x1, y1, x2, y2;
                Dm.GetWindowRect(_hwnd, out x1, out y1, out x2, out y2);
                return new Tuple<int, int, int, int>(x1, y1, x2, y2);
            }
        }

        /// <summary>
        /// 获取 窗口是否存在
        /// </summary>
        public bool IsAlive { get { return Dm.GetWindowState(_hwnd, 0) == 1; } }

        /// <summary>
        /// 获取 窗口是否激活
        /// </summary>
        public bool IsActive { get { return Dm.GetWindowState(_hwnd, 1) == 1; } }

        /// <summary>
        /// 获取 窗口是否显示
        /// </summary>
        public bool IsShowed { get { return Dm.GetWindowState(_hwnd, 2) == 1; } }

        public bool IsMin { get { return Dm.GetWindowState(_hwnd, 3) == 1; } }

        public bool IsMax { get { return Dm.GetWindowState(_hwnd, 4) == 1; } }

        public bool IsOnTop { get { return Dm.GetWindowState(_hwnd, 5) == 1; } }

        /// <summary>
        /// 获取 窗口是否无响应
        /// </summary>
        public bool IsDead { get { return Dm.GetWindowState(_hwnd, 6) == 1; } }

        /// <summary>
        /// 前台绑定，独占鼠标
        /// </summary>
        public bool BindNormal()
        {
            if (IsBind)
            {
                if (BindType == WindowBindType.Normal)
                {
                    return true;
                }
                UnBind();
            }
            int dmRet = Dm.BindWindow(_hwnd, DmBindDisplay.normal, DmBindMouse.normal, DmBindKeypad.normal, DmBindMode._0);
            if (dmRet == 0)
            {
                IsBind = false;
                BindType = WindowBindType.None;
                return false;
            }
            BindType = WindowBindType.Normal;
            IsBind = true;
            return true;
        }

        /// <summary>
        /// 半后台绑定，不锁定鼠标输入
        /// </summary>
        public bool BindHalfBackground()
        {
            if (IsBind)
            {
                if (BindType == WindowBindType.Half)
                {
                    return true;
                }
                UnBind();
            }
            RestoreAndNotActive();
            int dmRet = Dm.BindWindow(_hwnd, DmBindDisplay.dx, DmBindMouse.dx2, DmBindKeypad.dx, DmBindMode._0);
            if (dmRet == 0)
            {
                IsBind = false;
                BindType = WindowBindType.None;
                return false;
            }
            BindType = WindowBindType.Half;
            IsBind = true;
            return true;
        }

        /// <summary>
        /// 全后台绑定，锁定鼠标输入
        /// </summary>
        public bool BindFullBackground()
        {
            if (IsBind)
            {
                if (BindType == WindowBindType.Full)
                {
                    return true;
                }
                UnBind();
            }
            RestoreAndNotActive();
            int dmRet = Dm.BindWindow(_hwnd, DmBindDisplay.dx, DmBindMouse.dx, DmBindKeypad.dx, DmBindMode._0);
            if (dmRet == 0)
            {
                IsBind = false;
                BindType = WindowBindType.None;
                return false;
            }
            BindType = WindowBindType.Full;
            IsBind = true;
            return true;
        }

        public bool UnBind()
        {
            if (!IsBind)
            {
                return true;
            }
            int dmRet = Dm.UnBindWindow();
            if (dmRet == 0)
            {
                IsBind = true;
                return false;
            }
            IsBind = false;
            BindType = WindowBindType.None;
            return true;
        }

        public bool Move(int x, int y)
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.MoveWindow(_hwnd, x, y);
            Dm.Delay(500);
            Tuple<int, int, int, int> rect = Rect;
            return rect.Item1 == x && rect.Item2 == 2;
        }

        public bool Close()
        {
            if (!IsAlive)
            {
                return true;
            }
            Dm.SetWindowState(_hwnd, 0);
            return !IsAlive;
        }

        public bool Active()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 1);
            return IsActive;
        }

        public bool Min()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 2);
            return IsMin;
        }

        public bool MinAndClean()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 3);
            return IsMin;
        }

        public bool Max()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 4);
            return IsMax;
        }

        public bool RestoreAndNotActive()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 5);
            return !IsMin && !IsActive;
        }

        public bool Hide()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 6);
            return !IsShowed;
        }

        public bool Show()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 7);
            return IsShowed;
        }

        public bool OnTop()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 8);
            return IsOnTop;
        }

        public bool CancelOnTop()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 9);
            return !IsOnTop;
        }

        public bool Disable()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 10);
            return !IsActive;
        }

        public bool Enabled()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 11);
            return IsActive;
        }

        public bool RestoreAndActive()
        {
            if (!IsAlive)
            {
                return false;
            }
            Dm.SetWindowState(_hwnd, 12);
            return IsActive;
        }

        public bool SetSize(int width, int height)
        {
            if (!IsAlive)
            {
                return false;
            }

            Dm.SetWindowSize(_hwnd, width, height);
            Tuple<int, int, int, int> rect = Rect;
            return rect.Item3 - rect.Item1 == width && rect.Item4 - rect.Item2 == height;
        }

        public bool SetTransparent(int trans)
        {
            if (!IsAlive)
            {
                return false;
            }
            return Dm.SetWindowTransparent(_hwnd, trans) == 1;
        }

        public bool SendPaste()
        {
            if (!IsAlive)
            {
                return false;
            }
            return Dm.SendPaste(_hwnd) == 1;
        }

        public bool SendString(string content)
        {
            if (!IsAlive)
            {
                return false;
            }
            return Dm.SendString(_hwnd, content) == 1;
        }

        public bool SendString2(string content)
        {
            if (!IsAlive)
            {
                return false;
            }
            return Dm.SendString2(_hwnd, content) == 1;
        }

        /// <summary>
        /// 锁定窗口输入
        /// </summary>
        /// <param name="lockType">锁定类型</param>
        /// <param name="isSetType">是否改变窗口锁定类型</param>
        /// <returns></returns>
        public bool LockInput(InputLockType lockType, bool isSetType = true)
        {
            bool flag = _dm.LockInput((int)lockType) == 1;
            if (flag && isSetType)
            {
                InputLockType = lockType;
            }
            return flag;
        }

        /// <summary>
        /// 任务栏闪烁，默认闪烁30次，间隔500毫秒
        /// </summary>
        /// <param name="count">闪烁总次数</param>
        /// <param name="interval">闪烁时间间隔，单位毫秒</param>
        public void FlashWindow(int count = 10, int interval = 500)
        {
            IntPtr hwnd = new IntPtr(_hwnd);
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    bool invert = i % 2 == 0;
                    FlashWindow(hwnd, invert);
                    Thread.Sleep(interval);
                    if (IsActive)
                    {
                        break;
                    }
                }
                FlashWindow(hwnd, true);
            });
        }

        #region 私有方法

        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool invert);

        #endregion
    }


    public enum WindowBindType
    {
        /// <summary>
        /// 表示窗口未绑定
        /// </summary>
        None,
        /// <summary>
        /// 表示窗口前台绑定
        /// </summary>
        Normal,
        /// <summary>
        /// 表示窗口半后台绑定
        /// </summary>
        Half,
        /// <summary>
        /// 表示窗口全后台绑定
        /// </summary>
        Full
    }


    public enum InputLockType
    {
        /// <summary>
        /// 不锁定输入
        /// </summary>
        None = 0,
        /// <summary>
        /// 锁定鼠标键盘输入
        /// </summary>
        MouseAndKeyboard = 1,
        /// <summary>
        /// 锁定鼠标输入
        /// </summary>
        Mouse = 2,
        /// <summary>
        /// 锁定键盘输入 
        /// </summary>
        Keyboard = 3
    }
}