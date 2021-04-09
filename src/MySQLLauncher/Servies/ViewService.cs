using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySQLLauncher.Models;
using MySQLLauncher.Views;

namespace MySQLLauncher.Servies
{
    public class ViewService
    {
        #region Methods

        public MySQLLaunchModel CreateNewModel()
        {
            var model = new MySQLLaunchModel();
            var view = new CreateNewModelView
            {
                Owner = Application.Current.MainWindow,
                DataContext = model
            };

            var rt = view.ShowDialog();

            if (rt == true)
            {
                return model;
            }
            return null;
        }

        #endregion Methods
    }
}