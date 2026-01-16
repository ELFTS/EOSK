using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;  // 添加这个引用以使用Brush、Color、LinearGradientBrush
using System.Runtime.InteropServices; // 添加这个引用以使用Windows API

using System.Windows.Interop;

namespace EOSK.Components
{
    /// <summary>
    /// VirtualKeyboard.xaml 的交互逻辑
    /// </summary>
    public partial class VirtualKeyboard : UserControl
    {
        // 定义按键事件
        public event EventHandler<VirtualKeyEventArgs>? KeySelected;
        
        // 当前选中的按键
        private Key _selectedKey;
        
        // 导入Windows API
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public INPUTUNION u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        // 键盘输入标志
        public const uint KEYEVENTF_KEYUP = 0x0002;

        public VirtualKeyboard()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    var hwnd = new WindowInteropHelper(window).Handle;
                    var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                    SetWindowLong(hwnd, GWL_EXSTYLE, (IntPtr)(extendedStyle.ToInt64() | WS_EX_NOACTIVATE));
                }
            };
        }
        
        // 按键点击事件
        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string keyName)
            {
                // 将字符串转换为Key枚举
                if (Enum.TryParse<Key>(keyName, out Key key))
                {
                    _selectedKey = key;
                    
                    // 发送按键事件
                    SimulateKeyPress(key);
                    
                    // 触发事件，以便更新UI
                    KeySelected?.Invoke(this, new VirtualKeyEventArgs(key));
                }
            }
        }

        // 模拟按键按下和释放
        private void SimulateKeyPress(Key key)
        {
            // 实时捕获当前的前台窗口并设置焦点
            var targetWindowHandle = GetForegroundWindow();
            if (targetWindowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(targetWindowHandle);
            }

            // 获取虚拟键码
            ushort virtualKeyCode = (ushort)KeyInterop.VirtualKeyFromKey(key);
            
            // 创建键盘输入结构
            INPUT[] inputs = new INPUT[2];

            // 按下按键
            inputs[0].type = 1; // INPUT_KEYBOARD
            inputs[0].u.ki.wVk = virtualKeyCode;
            inputs[0].u.ki.wScan = 0;
            inputs[0].u.ki.dwFlags = 0; // 按键按下
            inputs[0].u.ki.time = 0;
            inputs[0].u.ki.dwExtraInfo = IntPtr.Zero;

            // 释放按键
            inputs[1].type = 1; // INPUT_KEYBOARD
            inputs[1].u.ki.wVk = virtualKeyCode;
            inputs[1].u.ki.wScan = 0;
            inputs[1].u.ki.dwFlags = KEYEVENTF_KEYUP; // 按键释放
            inputs[1].u.ki.time = 0;
            inputs[1].u.ki.dwExtraInfo = IntPtr.Zero;

            // 发送输入
            SendInput(2, inputs, Marshal.SizeOf<INPUT>());
        }

        // 刷新虚拟键盘的视觉状态
        public void RefreshVisualState()
        {
            // 遍历Grid中的所有按钮
            foreach (var child in MainGrid.Children)
            {
                if (child is Button button)
                {
                    UpdateButtonVisualState(button);
                }
            }
        }
        
        // 更新单个按钮的视觉状态
        private void UpdateButtonVisualState(Button button)
        {
            // 获取按钮对应的键
            if (button.Tag is string keyName && Enum.TryParse<Key>(keyName, out Key key))
            {
                // 检查是否是当前选中的键
                if (key == _selectedKey)
                {
                    // 当前选中的键使用SecondaryMid颜色 - 改为使用渐变色
                    var secondaryBrush = Application.Current.FindResource("SecondaryHueMidBrush") as Brush ?? CreateGradientBrush(Colors.LightBlue);
                    button.Background = secondaryBrush;
                }
                // 所有按键默认使用默认颜色
                else
                {
                    // 默认颜色
                    button.ClearValue(Button.BackgroundProperty);
                }
            }
        }
        
        // 创建渐变画笔的辅助方法
        private Brush CreateGradientBrush(Color baseColor)
        {
            return new LinearGradientBrush(
                baseColor,
                baseColor,
                new Point(0, 0),
                new Point(0, 1)
            );
        }
        
        // 获取当前选中的按键
        public Key SelectedKey => _selectedKey;
    }

    // 按键事件参数类
    public class VirtualKeyEventArgs : EventArgs
    {
        public Key Key { get; }

        public VirtualKeyEventArgs(Key key)
        {
            Key = key;
        }
    }
}