using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MySQLLauncher.Controls
{
    /// <summary>
    /// UC_PathSelector.xaml 的交互逻辑
    /// </summary>
    public partial class UC_PathSelector : UserControl
    {
        // Using a DependencyProperty as the backing store for IsFileSelect. This enables animation,
        // styling, binding, etc...
        public static readonly DependencyProperty IsFileSelectProperty =
            DependencyProperty.Register("IsFileSelect", typeof(bool), typeof(UC_PathSelector), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for IsOpenFile. This enables animation,
        // styling, binding, etc...
        public static readonly DependencyProperty IsOpenFileProperty =
            DependencyProperty.Register("IsOpenFile", typeof(bool), typeof(UC_PathSelector), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for SelectedPath. This enables animation,
        // styling, binding, etc...
        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register("SelectedPath", typeof(string), typeof(UC_PathSelector), new PropertyMetadata("", OnSelectedPathPropertyChangedCallback));

        private static System.Windows.Forms.FolderBrowserDialog _fbdialog;

        private static Microsoft.Win32.OpenFileDialog _opdialog;

        private static Microsoft.Win32.SaveFileDialog _sfdialog;

        public UC_PathSelector()
        {
            InitializeComponent();
        }

        public bool IsFileSelect
        {
            get { return (bool)GetValue(IsFileSelectProperty); }
            set { SetValue(IsFileSelectProperty, value); }
        }

        public bool IsOpenFile
        {
            get { return (bool)GetValue(IsOpenFileProperty); }
            set { SetValue(IsOpenFileProperty, value); }
        }

        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        private static void OnSelectedPathPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UC_PathSelector ups)
            {
                ups.OnSelectedPathChanged((string)e.OldValue, (string)e.NewValue);
            }
        }

        private void BtnSelectPath_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFileSelect)
            {
                if (_fbdialog == null)
                {
                    _fbdialog = new System.Windows.Forms.FolderBrowserDialog();
                }
                _fbdialog.SelectedPath = SelectedPath;
                if (_fbdialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                SelectedPath = _fbdialog.SelectedPath;
                return;
            }
            if (IsOpenFile)
            {
                if (_opdialog == null)
                {
                    _opdialog = new Microsoft.Win32.OpenFileDialog
                    {
                        CheckPathExists = true
                    };
                }
                _opdialog.FileName = SelectedPath;
                if (_opdialog.ShowDialog() == true)
                {
                    SelectedPath = _opdialog.FileName;
                }
            }
            else
            {
                if (_sfdialog == null)
                {
                    _sfdialog = new Microsoft.Win32.SaveFileDialog
                    {
                        CheckPathExists = true
                    };
                }
                if (_sfdialog.ShowDialog() == true)
                {
                    SelectedPath = _sfdialog.FileName;
                }
            }
        }

        private void OnSelectedPathChanged(string oldValue, string newValue)
        {
            Txt_SelectedPath.Text = newValue;
        }
    }
}