using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySQLLauncher.Models;
using Newtonsoft.Json;

namespace MySQLLauncher.Servies
{
    public class LaunchConfigService
    {
        #region Fields

        private string _cfgDir;

        #endregion Fields

        #region Constructors

        public LaunchConfigService()
        {
            _cfgDir = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "LaunchConfigs");
            Directory.CreateDirectory(_cfgDir);
        }

        #endregion Constructors

        #region Methods

        public Task<MySQLIniFileModel> GetDefaultIni()
        {
            var ini = new MySQLIniFileModel();
            return Task.FromResult(ini);
        }

        public async Task<List<MySQLLaunchModel>> LoadConfigs()
        {
            var _cfgs = new List<MySQLLaunchModel>();
            foreach (var file in Directory.GetFiles(_cfgDir))
            {
                var content = await Task.Factory.StartNew(() => File.ReadAllText(file));
                var cfg = JsonConvert.DeserializeObject<MySQLLaunchModel>(content);
                _cfgs.Add(cfg);
            }
            return _cfgs;
        }

        public async Task SaveConfig(MySQLLaunchModel cfg)
        {
            Directory.CreateDirectory(_cfgDir);
            var file = Path.Combine(_cfgDir, $"{cfg.ID}.json");
            var content = JsonConvert.SerializeObject(cfg);
            await Task.Factory.StartNew(() => File.WriteAllText(file, content));
        }

        #endregion Methods
    }
}