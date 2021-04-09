using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySQLLauncher.Infrastructure;

namespace MySQLLauncher.Models
{
    public class MySQLLaunchModel : ModelBase, ICloneable
    {
        #region Fields

        private string _desc = "LaunchModel Description";

        private MySQLIniFileModel _iniModel;
        private string _name = "LaunchModel";

        #endregion Fields

        #region Constructors

        public MySQLLaunchModel()
        {
            ID = Guid.NewGuid().ToString();
            IniModel = new MySQLIniFileModel();
        }

        #endregion Constructors

        #region Properties

        public string Description
        {
            get => _desc;
            set => SetProperty(ref _desc, value);
        }

        public string ID { get; set; }

        public MySQLIniFileModel IniModel
        {
            get => _iniModel;
            set => SetProperty(ref _iniModel, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        #endregion Properties

        #region Methods

        public object Clone()
        {
            var model = new MySQLLaunchModel();
            model.Name = Name + "(复制)";
            model.Description = Description;
            model.IniModel = IniModel.Clone() as MySQLIniFileModel;

            return model;
        }

        #endregion Methods
    }
}