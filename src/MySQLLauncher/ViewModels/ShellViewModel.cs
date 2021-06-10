using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MySQLLauncher.Infrastructure;
using MySQLLauncher.Models;
using MySQLLauncher.Servies;
using PrismTemplate.Infrastructure;

namespace MySQLLauncher.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        #region Fields

        private Dictionary<string, MySQLInstanceModel> _CfgInstanceDict = new Dictionary<string, MySQLInstanceModel>();

        private MySQLInstanceModel _currentInstance;
        private MySQLLaunchModel _currentLaunchModel;

        private Dispatcher _dispatcher;
        private MySQLIniFileModel _editModel;

        private LaunchConfigService _launchCfgService;

        private LogService _logService;
        private MySQLConfigService _mySQLCfgService;

        private MySQLService _mySQLService;
        private string _statusText = "Ready";
        private ViewService _viewService;

        #endregion Fields

        #region Constructors

        public ShellViewModel()
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            MySQLLaunchModels = new ObservableCollection<MySQLLaunchModel>();
            MySQLInstanceModels = new ObservableCollection<MySQLInstanceModel>();

            _viewService = new ViewService();
            _launchCfgService = new LaunchConfigService();
            _mySQLCfgService = new MySQLConfigService();
            _mySQLService = new MySQLService();
            _mySQLService.NewInstaceFound += _mySQLService_NewInstaceFound;
            _mySQLService.InstanceLost += _mySQLService_InstanceLost;

            _logService = new LogService();
            _logService.LogContentChanged += _logService_LogContentChanged;

            CommandCreateNew = new CommandImpl(OnRequestCreateNewLaunchModel);
            CommandCopyCurrentLaunchModel = new CommandImpl(OnRequestCopyCurrentLaunchModel);
            CommandRenameSelectedModel = new CommandImpl(OnRequestRenameSelectedModel);
            CommandDeleteSelectedLaunchModel = new CommandImpl(OnRequestDeleteSelectedLaunchModel);

            CommandInitMySQLDataDir = new CommandImpl(OnRequestInitMySQLDataDir);
            CommandStartMySQL = new CommandImpl<MySQLLaunchModel>(OnRequestStartMySQL, CanStartMySQL);
            CommandStopMySQL = new CommandImpl<MySQLLaunchModel>(OnRequestStopMySQL, CanStopMySQL);
            CommandReStartMySQL = new CommandImpl<MySQLLaunchModel>(OnRequestReStartMySQL, CanReStartMySQL);

            CommandRefreshIni = new CommandImpl(OnRequestRefreshIni);
            CommandSaveIni = new CommandImpl(OnRequestSaveIni);

            _ = LoadModels();
        }

        #endregion Constructors

        #region Properties

        public CommandImpl CommandCopyCurrentLaunchModel { get; private set; }

        public CommandImpl CommandCreateNew { get; private set; }

        public CommandImpl CommandDeleteSelectedLaunchModel { get; private set; }

        public CommandImpl CommandInitMySQLDataDir { get; private set; }

        public CommandImpl CommandRefreshIni { get; private set; }

        public CommandImpl CommandRenameSelectedModel { get; private set; }

        public CommandImpl<MySQLLaunchModel> CommandReStartMySQL { get; private set; }

        public CommandImpl CommandSaveIni { get; private set; }

        public CommandImpl<MySQLLaunchModel> CommandStartMySQL { get; private set; }

        public CommandImpl<MySQLLaunchModel> CommandStopMySQL { get; private set; }

        public MySQLIniFileModel CurrentIniModel
        {
            get => _editModel;
            set => SetProperty(ref _editModel, value);
        }

        public MySQLInstanceModel CurrentInstance
        {
            get => _currentInstance;
            set => SetProperty(ref _currentInstance, value);
        }

        public MySQLLaunchModel CurrentModel
        {
            get => _currentLaunchModel;
            set
            {
                if (SetProperty(ref _currentLaunchModel, value))
                {
                    CurrentIniModel = value?.IniModel ?? new MySQLIniFileModel();

                    if (value != null && _CfgInstanceDict.TryGetValue(value.ID, out var instanceModel))
                        CurrentInstance = instanceModel;
                }
            }
        }

        public ObservableCollection<MySQLInstanceModel> MySQLInstanceModels { get; private set; }

        public ObservableCollection<MySQLLaunchModel> MySQLLaunchModels { get; private set; }

        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        #endregion Properties

        #region Methods

        protected override void InitViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            if (args.PropertyName == nameof(CurrentModel))
            {
                if (CurrentModel != null && _CfgInstanceDict.TryGetValue(CurrentModel.ID, out var instance))
                    CurrentInstance = instance;
                else
                    CurrentInstance = null;
                CommandStartMySQL.RaiseCanExecuteChanged();
                CommandStopMySQL.RaiseCanExecuteChanged();
                CommandReStartMySQL.RaiseCanExecuteChanged();
            }
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            if (_mySQLService.IsRunning) return;
            _mySQLService.Start();
        }

        protected override void OnViewUnLoaded()
        {
            _mySQLService.StopWatcher();
            base.OnViewUnLoaded();
        }

        private void _logService_LogContentChanged(object sender, string e)
        {
            StatusText = e;
        }

        private void _mySQLService_InstanceLost(object sender, Core.MySQLInstance instance)
        {
            _dispatcher.Invoke(() =>
            {
                var instanceModel = MySQLInstanceModels.FirstOrDefault(p => p.CoreProcessId == instance.CoreProcess.Id && p.ShellProcessId == instance.ShellProcess.Id);
                if (instanceModel == null)
                    return;

                instanceModel.Clear();
                MySQLInstanceModels.Remove(instanceModel);

                var id = Path.GetFileNameWithoutExtension(instance.InstanceIniPath);
                _CfgInstanceDict[id] = instanceModel;
                if (CurrentModel?.ID == id)
                    RaisePropertyChanged(nameof(CurrentModel));
            });
        }

        private void _mySQLService_NewInstaceFound(object sender, Core.MySQLInstance instance)
        {
            _dispatcher.Invoke(() =>
            {
                var instanceModel = MySQLInstanceModels.FirstOrDefault(p => p.ShellProcessId == instance.ShellProcess.Id && p.CoreProcessId == instance.CoreProcess.Id);
                if (instanceModel == null)
                {
                    instanceModel = new MySQLInstanceModel(instance.InstancePath, instance.InstanceIniPath, instance.DataDir, instance.ShellProcess.Id, instance.CoreProcess.Id);
                    instanceModel.Status = Core.MySQLInstanceStatus.Running;
                    MySQLInstanceModels.Add(instanceModel);
                    var id = Path.GetFileNameWithoutExtension(instance.InstanceIniPath);
                    _CfgInstanceDict[id] = instanceModel;
                    if (CurrentModel?.ID == id)
                        RaisePropertyChanged(nameof(CurrentModel));
                }
            });
        }

        private bool CanReStartMySQL(MySQLLaunchModel arg)
        {
            if (arg == null)
                return false;
            if (_CfgInstanceDict.TryGetValue(CurrentModel.ID, out var instance))
            {
                return instance.Status == Core.MySQLInstanceStatus.Running;
            }
            return false;
        }

        private bool CanStartMySQL(MySQLLaunchModel arg)
        {
            if (arg == null)
                return false;

            if (_CfgInstanceDict.TryGetValue(CurrentModel.ID, out var instance))
            {
                return instance.Status == Core.MySQLInstanceStatus.None || instance.Status == Core.MySQLInstanceStatus.Stopped;
            }
            return true;
        }

        private bool CanStopMySQL(MySQLLaunchModel arg)
        {
            if (arg == null)
                return false;
            if (_CfgInstanceDict.TryGetValue(CurrentModel.ID, out var instance))
            {
                return instance.Status == Core.MySQLInstanceStatus.Running;
            }
            return false;
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogApi.LogException(e.Exception);
            e.Handled = true;
        }

        private async Task LoadModels()
        {
            var lst = await _launchCfgService.LoadConfigs();
            MySQLLaunchModels.AddRange(lst);
            CurrentModel = MySQLLaunchModels.FirstOrDefault();
        }

        private void OnRequestCopyCurrentLaunchModel()
        {
            if (_currentLaunchModel == null)
                return;

            var newmodel = _currentLaunchModel.Clone() as MySQLLaunchModel;
            MySQLLaunchModels.Add(newmodel);
            CurrentModel = newmodel;
        }

        private async void OnRequestCreateNewLaunchModel()
        {
            var model = _viewService.CreateNewModel();
            if (model == null) return;

            await _launchCfgService.SaveConfig(model);
            MySQLLaunchModels.Add(model);
        }

        private void OnRequestDeleteSelectedLaunchModel()
        {
            MySQLLaunchModels.Remove(CurrentModel);
        }

        private void OnRequestInitMySQLDataDir()
        {
            var inspath = Path.Combine(CurrentModel.IniModel.Basedir, "bin", "mysqld.exe");
            var iniPath = _mySQLCfgService.GenerateIniFile(CurrentModel);
            _mySQLService.InitMySQLDataDir(inspath, iniPath);
        }

        private async void OnRequestRefreshIni()
        {
            if (CurrentModel is null)
                CurrentIniModel = await _launchCfgService.GetDefaultIni();
            else
                CurrentIniModel = CurrentModel.IniModel.Clone() as MySQLIniFileModel;
        }

        private async void OnRequestRenameSelectedModel()
        {
            if (_currentLaunchModel == null)
                return;

            await _launchCfgService.SaveConfig(CurrentModel);
        }

        private async void OnRequestReStartMySQL(MySQLLaunchModel model)
        {
            if (_CfgInstanceDict.TryGetValue(model.ID, out var instance))
            {
                instance.Status = Core.MySQLInstanceStatus.Stoping;
                _mySQLService.StopMySQL(instance);
                instance.Status = Core.MySQLInstanceStatus.Stopped;
                await Task.Delay(10);
                instance.Status = Core.MySQLInstanceStatus.Starting;
                await _mySQLService.StartMySQL(instance.InstancePath, instance.InstanceIniPath);
            }
        }

        private async void OnRequestSaveIni()
        {
            if (CurrentModel is null) return;
            CurrentModel.IniModel = CurrentIniModel.Clone() as MySQLIniFileModel;
            await _launchCfgService.SaveConfig(CurrentModel);
        }

        private async void OnRequestStartMySQL(MySQLLaunchModel model)
        {
            if (model is null)
                return;
            var inspath = Path.Combine(model.IniModel.Basedir, "bin", "mysqld.exe");
            var inipath = _mySQLCfgService.GenerateIniFile(model);
            await _mySQLService.StartMySQL(inspath, inipath);
        }

        private void OnRequestStopMySQL(MySQLLaunchModel model)
        {
            if (_CfgInstanceDict.TryGetValue(model.ID, out var instance))
            {
                _mySQLService.StopMySQL(instance);
            }
        }

        #endregion Methods
    }
}