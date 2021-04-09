using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySQLLauncher.Infrastructure;
using Newtonsoft.Json;

namespace MySQLLauncher.Models
{
    public class MySQLIniFileModel : ModelBase, ICloneable
    {
        #region Fields

        private string _basedir = "";
        private string _character_Set_Server = "UTF8MB4";
        private string _datadir = "";
        private string _default_Authentication_Plugin = "mysql_native_password";
        private string _default_Character_Set = "UTF8MB4";

        private string _default_Storage_Engine = "INNODB";

        private bool _general_log = false;

        private string _general_Log_File = "mysql.log";

        private int _max_Connections = 200;

        private int _port = 3306;

        #endregion Fields

        #region Properties

        public string Basedir
        {
            get => _basedir;
            set => SetProperty(ref _basedir, value);
        }

        public string Character_Set_Server
        {
            get => _character_Set_Server;
            set => SetProperty(ref _character_Set_Server, value);
        }

        public string DataDir
        {
            get => _datadir;
            set => SetProperty(ref _datadir, value);
        }

        public string Default_Authentication_Plugin
        {
            get => _default_Authentication_Plugin;
            set => SetProperty(ref _default_Authentication_Plugin, value);
        }

        public string Default_Character_Set
        {
            get => _default_Character_Set;
            set => SetProperty(ref _default_Character_Set, value);
        }

        public string Default_Storage_Engine
        {
            get => _default_Storage_Engine;
            set => SetProperty(ref _default_Storage_Engine, value);
        }

        /// <summary>
        /// 日志记录开关
        /// </summary>
        public bool General_Log
        {
            get => _general_log;
            set => SetProperty(ref _general_log, value);
        }

        /// <summary>
        /// 日志文件路径
        /// </summary>
        public string General_Log_File
        {
            get => _general_Log_File;
            set => SetProperty(ref _general_Log_File, value);
        }

        [JsonIgnore]
        public string IniFileContent
        {
            get => GetIniContent();
        }

        public int Max_Connections
        {
            get => _max_Connections;
            set => SetProperty(ref _max_Connections, value);
        }

        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        #endregion Properties

        #region Methods

        public object Clone()
        {
            var inimodel = new MySQLIniFileModel();
            inimodel.Basedir = Basedir;
            inimodel.Character_Set_Server = Character_Set_Server;
            inimodel.DataDir = DataDir;
            inimodel.Default_Authentication_Plugin = Default_Authentication_Plugin;
            inimodel.Default_Character_Set = Default_Character_Set;
            inimodel.Default_Storage_Engine = Default_Storage_Engine;
            inimodel.General_Log = General_Log;
            inimodel.General_Log_File = General_Log_File;
            inimodel.Max_Connections = Max_Connections;
            inimodel.Port = Port;

            return inimodel;
        }

        public string GetIniContent(bool includeAuthPlugin = true)
        {
            var str = $"[client]{Environment.NewLine}" +
                $"default-character-set={Default_Character_Set}{Environment.NewLine}" +
                $"[mysqld]{Environment.NewLine}" +
                $"port={Port}{Environment.NewLine}" +
                $"max_connections={Max_Connections}{Environment.NewLine}" +
                $"basedir=\"{Basedir}\"{Environment.NewLine}" +
                $"datadir=\"{DataDir}\"{Environment.NewLine}" +
                $"general-log={(General_Log ? "1" : "0")}{Environment.NewLine}" +
                $"general_log_file=\"{General_Log_File}\"{Environment.NewLine}" +
                $"character-set-server={Character_Set_Server}{Environment.NewLine}" +
                $"default-storage-engine={Default_Storage_Engine}{Environment.NewLine}" +
                $"{(includeAuthPlugin ? $"default_authentication_plugin={Default_Authentication_Plugin}" : "")}";

            return str;
        }

        public override string ToString()
        {
            return GetIniContent();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            if (!args.PropertyName.Equals(nameof(IniFileContent)))
            {
                RaisePropertyChanged(nameof(IniFileContent));
            }
        }

        #endregion Methods
    }
}