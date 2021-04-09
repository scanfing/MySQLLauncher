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
using MySQLLauncher.Models;

namespace MySQLLauncher.Views
{
    /// <summary>
    /// LaunchModelEditView.xaml 的交互逻辑
    /// </summary>
    public partial class LaunchModelEditView : Window
    {
        private MySQLLaunchModel _editModel = new MySQLLaunchModel();
        private MySQLLaunchModel _srcModel;

        public LaunchModelEditView(MySQLLaunchModel model)
        {
            InitializeComponent();
            _srcModel = model;
            _editModel.Name = _srcModel.Name;
            _editModel.Description = _srcModel.Description;
            DataContext = _editModel;
            Closing += LaunchModelEditView_Closing;
        }

        private void Btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void LaunchModelEditView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult != true)
                return;
            _srcModel.Name = _editModel.Name;
            _srcModel.Description = _editModel.Description;
        }
    }
}