using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySQLLauncher.Models;

namespace MySQLLauncher.Servies
{
    public class MySQLConfigService
    {
        #region Fields

        private string _cfgDir;

        #endregion Fields

        #region Constructors

        public MySQLConfigService()
        {
            _cfgDir = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "LaunchInis");
            Directory.CreateDirectory(_cfgDir);
        }

        #endregion Constructors

        #region Methods

        public void Backup()
        {
        }

        public string GenerateIniFile(MySQLLaunchModel model)
        {
            var filename = model.ID + ".ini";
            var dstfile = Path.Combine(_cfgDir, filename);
            SaveConfigToIniFile(model.IniModel, dstfile);
            return dstfile;
        }

        public void SaveConfigToIniFile(MySQLIniFileModel iniModel, string iniFilePath)
        {
            var dir = Path.GetDirectoryName(iniFilePath);
            Directory.CreateDirectory(dir);
            File.WriteAllText(iniFilePath, iniModel.IniFileContent, Encoding.GetEncoding("GB2312"));
        }

        #endregion Methods
    }
}