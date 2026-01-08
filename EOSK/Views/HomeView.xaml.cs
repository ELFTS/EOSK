using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EOSK.Components;

namespace EOSK.Views
{
    /// <summary>
    /// HomeView.xaml 的交互逻辑
    /// </summary>
    public partial class HomeView : UserControl
    {
        private Key _selectedKey = Key.None;
        
        public HomeView()
        {
            InitializeComponent();
            
            // 订阅卸载事件以清理资源
            Unloaded += HomeView_Unloaded;
        }
        
        // 用户控件卸载时取消订阅事件
        private void HomeView_Unloaded(object sender, RoutedEventArgs e)
        {
            // 没有需要取消订阅的事件
        }
        
        
        // 当虚拟键盘上的按键被选中时
        private void VirtualKeyboardControl_KeySelected(object sender, VirtualKeyEventArgs e)
        {
            _selectedKey = e.Key;
            
            // 移除了更新SelectedKeyText的代码
        }

        // 移除了UpdateSoundPathDisplay方法
        
        // 设置服务引用
        public void SetServices()
        {
        }
    }
}