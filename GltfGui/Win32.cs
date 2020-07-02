using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using NWindowsKits;

namespace Win32
{
    public static class Win32Extensions
    {
        public static short LOWORD(this long value)
        {
            return (short)value;
        }

        public static short HIWORD(this long value)
        {
            return (short)(value >> 16);
        }

        public static short HIWORD(this ulong value)
        {
            return (short)(value >> 16);
        }
    }

    public class Window
    {
        const string CLASS_NAME = "class";
        readonly string m_title;

        HWND m_hwnd;
        public HWND WindowHandle => m_hwnd;

        public bool QuitWhenClose = true;

        public RECT Rect
        {
            get
            {
                RECT rect = default;
                user32.GetClientRect(m_hwnd, ref rect);
                return rect;
            }
        }

        public int Width
        {
            get
            {
                var rect = Rect;
                return rect.right - rect.left;
            }
        }
        public int Height
        {
            get
            {
                var rect = Rect;
                return rect.bottom - rect.top;
            }
        }

        readonly WNDPROC m_delegate;

        static int s_count;
        readonly string m_className;
        Window(string title, int count)
        {
            m_title = title;
            m_delegate = new WNDPROC(WndProc);
            m_className = $"{CLASS_NAME}{count}";
        }

        public static Window Create(string title = "window", int show = C.SW_SHOW, HWND parent = default)
        {
            var ms = Assembly.GetEntryAssembly().GetModules();
            // var hInstance = Marshal.GetHINSTANCE(ms[0]);
            var hInstance = default(HINSTANCE);

            var window = new Window(title, s_count++);

            var wc = new WNDCLASSEXW
            {
                cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEXW)),
                style = C.CS_VREDRAW | C.CS_HREDRAW | C.CS_DBLCLKS,
                lpszClassName = window.m_className.ToString(),
                lpfnWndProc = window.m_delegate,
                hInstance = hInstance,
                // hCursor = user32.LoadCursorW(default, C.IDC_ARROW),
            };
            var register = user32.RegisterClassExW(ref wc);
            if (register == 0)
            {
                return null;
            }

            var hwnd = user32.CreateWindowExW(0,
                window.m_className,
                window.m_title,
                C.WS_OVERLAPPEDWINDOW,
                C.CW_USEDEFAULT,
                C.CW_USEDEFAULT,
                C.CW_USEDEFAULT,
                C.CW_USEDEFAULT,
                parent,
                default, hInstance, default);
            if (hwnd.ptr == IntPtr.Zero)
            {
                return null;
            }

            window.m_hwnd = hwnd;

            window.Show(show);

            return window;
        }

        long WndProc(HWND hwnd, uint msg, ulong wParam, long lParam)
        {
            switch (msg)
            {
                case C.WM_CLOSE:
                    OnClose?.Invoke();
                    if (DestroyWhenClose == null || DestroyWhenClose())
                    {
                        user32.DestroyWindow(hwnd);
                    }
                    if (QuitWhenClose)
                    {
                        user32.PostQuitMessage(0);
                    }
                    return 0;

                case C.WM_ENABLE:
                    OnEnable?.Invoke(wParam != 0);
                    break;

                case C.WM_SHOWWINDOW:
                    OnShow?.Invoke(wParam != 0);
                    break;

                case C.WM_DESTROY:
                    OnDestroy?.Invoke();
                    return 0;

                case C.WM_MOUSEMOVE:
                    OnMouseMove?.Invoke(lParam.LOWORD(), lParam.HIWORD());
                    return 0;

                case C.WM_LBUTTONDOWN:
                    OnMouseLeftDown?.Invoke(lParam.LOWORD(), lParam.HIWORD());
                    return 0;
                case C.WM_LBUTTONUP:
                    OnMouseLeftUp?.Invoke(lParam.LOWORD(), lParam.HIWORD());
                    return 0;
                case C.WM_LBUTTONDBLCLK:
                    OnMouseLeftDoubleClicked?.Invoke();
                    return 0;
                case C.WM_RBUTTONDOWN:
                    OnMouseRightDown?.Invoke(lParam.LOWORD(), lParam.HIWORD());
                    return 0;
                case C.WM_RBUTTONUP:
                    OnMouseRightUp?.Invoke(lParam.LOWORD(), lParam.HIWORD());
                    return 0;
                case C.WM_MBUTTONDOWN:
                    OnMouseMiddleDown?.Invoke(lParam.LOWORD(), lParam.HIWORD());
                    return 0;
                case C.WM_MBUTTONUP:
                    OnMouseMiddleUp?.Invoke(lParam.LOWORD(), lParam.HIWORD());
                    return 0;
                case C.WM_MOUSEWHEEL:
                    OnMouseWheel?.Invoke(wParam.HIWORD());
                    return 0;

                case C.WM_SIZE:
                    OnResize?.Invoke(lParam.LOWORD(), lParam.HIWORD());
                    return 0;

                case C.WM_PAINT:
                    {
                        var ps = default(PAINTSTRUCT);
                        user32.BeginPaint(hwnd, ref ps);
                        OnPaint?.Invoke();
                        user32.EndPaint(hwnd, ref ps);
                    }
                    return 0;
            }
            return user32.DefWindowProcW(hwnd, msg, wParam, lParam);
        }

        public event Action<int, int> OnMouseLeftDown;
        public event Action<int, int> OnMouseLeftUp;
        public event Action<int, int> OnMouseRightDown;
        public event Action<int, int> OnMouseRightUp;
        public event Action<int, int> OnMouseMiddleDown;
        public event Action<int, int> OnMouseMiddleUp;
        public event Action<int, int> OnMouseMove;
        public event Action<int> OnMouseWheel;
        public event Action OnMouseLeftDoubleClicked;

        public event Action<int, int> OnResize;
        public event Action OnPaint;
        public event Action OnDestroy;
        public event Action OnClose;
        public event Action<bool> OnShow;
        public event Action<bool> OnEnable;
        public Func<bool> DestroyWhenClose; // return false if not destroy

        public void Show(int sw)
        {
            user32.ShowWindow(m_hwnd, sw);
        }

        public void Invalidate()
        {
            RECT rect = default;
            user32.InvalidateRect(m_hwnd, ref rect, 1);
        }

        public void Enable()
        {
            user32.EnableWindow(m_hwnd, 1);
        }

        public void Close()
        {
            //user32.ShowWindow(m_hwnd, SW.HIDE);
            //user32.CloseWindow(m_hwnd);
            user32.PostMessageW(m_hwnd, C.WM_CLOSE, 0, 0);
        }

        public Window CreateModal(int w, int h)
        {
            var window = Window.Create("modal", C.SW_HIDE, this.WindowHandle);

            var sw = user32.GetSystemMetrics(C.SM_CXSCREEN);
            var sh = user32.GetSystemMetrics(C.SM_CYSCREEN);
            user32.SetWindowPos(window.WindowHandle, default,
                 (sw - w) / 2,
                 (sh - h) / 2,
                w, h, 0);

            window.OnShow = show =>
            {
                // disable parent
                if (show)
                {
                    var result = user32.EnableWindow(this.WindowHandle, 0);
                }
            };

            window.DestroyWhenClose = () =>
            {
                // enable parent
                var result = user32.EnableWindow(this.WindowHandle, 1);
                // hide dialog
                window.Show(C.SW_HIDE);
                return false;
            };

            return window;
        }
    }

    public class FPSTimer
    {
        uint m_last;
        readonly int m_msInFrame;

        public FPSTimer(int fps)
        {
            m_last = winmm.timeGetTime();
            m_msInFrame = 1000 / fps - 5;
        }

        public void Update(Action frameAction)
        {
            var now = winmm.timeGetTime();
            var delta = (int)(now - m_last);
            if (delta > m_msInFrame)
            {
                frameAction();
                m_last = now;
            }
            else
            {
                Thread.Sleep(m_msInFrame - delta);
            }
        }
    }

    public class MessageLoop
    {
        class CustomSynchronizationContext : SynchronizationContext
        {
            readonly Queue<(SendOrPostCallback, object)> m_queue = new Queue<(SendOrPostCallback, object)>();
            public override void Post(SendOrPostCallback d, object state)
            {
                lock (((ICollection)m_queue).SyncRoot)
                {
                    m_queue.Enqueue((d, state));
                }
            }

            readonly List<(SendOrPostCallback, object)> m_dequeue = new List<(SendOrPostCallback, object)>();
            public void Process()
            {
                lock (((ICollection)m_queue).SyncRoot)
                {
                    m_dequeue.AddRange(m_queue);
                    m_queue.Clear();
                }

                foreach (var (callback, obj) in m_dequeue)
                {
                    callback(obj);
                }
                m_dequeue.Clear();
            }
        }

        [ThreadStatic]
        static CustomSynchronizationContext s_context;
        public static void EnsureContext()
        {
            if (s_context == null)
            {
                s_context = new CustomSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(s_context);
            }
        }

        public static void ProcessMessage(out bool isQuit)
        {
            EnsureContext();

            MSG msg = default;
            while (user32.PeekMessageW(ref msg, default, 0, 0, C.PM_REMOVE) != 0)
            {
                if (msg.message == C.WM_QUIT)
                {
                    isQuit = true;
                    return;
                }
                user32.TranslateMessage(ref msg);
                user32.DispatchMessageW(ref msg);
            }
            isQuit = false;

            // process tasks
            s_context.Process();
        }

        public static void Run(Action draw, int fps)
        {
            var timer = new FPSTimer(1000 / fps);
            while (true)
            {
                ProcessMessage(out bool isQuit);
                if (isQuit)
                {
                    return;
                }

                timer.Update(draw);
            }
        }
    }
}
