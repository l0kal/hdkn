using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using ErrorEventArgs = Microsoft.Tools.WindowsInstallerXml.Bootstrapper.ErrorEventArgs;

namespace Hadouken.Installer.ViewModels
{
    /// <summary>
    /// The states of the installation view model.
    /// </summary>
    public enum InstallationState
    {
        Initializing,
        DetectedAbsent,
        DetectedPresent,
        DetectedNewer,
        Applying,
        Applied,
        Failed,
    }

    public class InstallationViewModel : PropertyNotifyBase
    {
        private readonly RootViewModel _root;

        private Dictionary<string, int> _downloadRetries = new Dictionary<string, int>();
        private bool _downgrade;
        private readonly string _homePage = "http://www.hdkn.net";
        private string _message;
        private DateTime cachePackageStart;
        private DateTime _executePackageStart;

        private ICommand _installCommand;
        private ICommand _uninstallCommand;

        public InstallationViewModel(RootViewModel root)
        {
            _root = root;

            _root.PropertyChanged += RootPropertyChanged;

            HadoukenInstaller.Model.Bootstrapper.DetectBegin += DetectBegin;
            HadoukenInstaller.Model.Bootstrapper.DetectRelatedBundle += DetectedRelatedBundle;
            HadoukenInstaller.Model.Bootstrapper.DetectComplete += DetectComplete;
            HadoukenInstaller.Model.Bootstrapper.PlanPackageBegin += PlanPackageBegin;
            HadoukenInstaller.Model.Bootstrapper.PlanComplete += PlanComplete;
            HadoukenInstaller.Model.Bootstrapper.ApplyBegin += ApplyBegin;
            HadoukenInstaller.Model.Bootstrapper.CacheAcquireBegin += CacheAcquireBegin;
            HadoukenInstaller.Model.Bootstrapper.CacheAcquireComplete += CacheAcquireComplete;
            HadoukenInstaller.Model.Bootstrapper.ExecutePackageBegin += ExecutePackageBegin;
            HadoukenInstaller.Model.Bootstrapper.ExecutePackageComplete += ExecutePackageComplete;
            HadoukenInstaller.Model.Bootstrapper.Error += ExecuteError;
            HadoukenInstaller.Model.Bootstrapper.ResolveSource += ResolveSource;
            HadoukenInstaller.Model.Bootstrapper.ApplyComplete += ApplyComplete;
        }

        public ICommand InstallCommand
        {
            get
            {
                if(_installCommand == null)
                    _installCommand = new RelayCommand(param => HadoukenInstaller.Plan(LaunchAction.Install), param => _root.State == InstallationState.DetectedAbsent);

                return _installCommand;
            }
        }

        public bool InstallEnabled
        {
            get { return InstallCommand.CanExecute(this); }
        }

        public ICommand UninstallCommand
        {
            get
            {
                if (_uninstallCommand == null)
                {
                    _uninstallCommand = new RelayCommand(param => HadoukenInstaller.Plan(LaunchAction.Uninstall), param => _root.State == InstallationState.DetectedPresent);
                }

                return _uninstallCommand;
            }
        }

        public bool UninstallEnabled
        {
            get { return UninstallCommand.CanExecute(this); }
        }

        public string Version
        {
            get { return String.Concat("v", HadoukenInstaller.Model.Version.ToString()); }
        }

        /// <summary>
        /// Gets and sets whether the view model considers this install to be a downgrade.
        /// </summary>
        public bool Downgrade
        {
            get
            {
                return _downgrade;
            }

            set
            {
                if (_downgrade == value) return;
                _downgrade = value;
                base.OnPropertyChanged("Downgrade");
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                if (_message == value) return;
                _message = value;
                base.OnPropertyChanged("Message");
            }
        }

        public string Title
        {
            get
            {
                switch (_root.State)
                {
                    case InstallationState.Initializing:
                        return "Initializing...";

                    case InstallationState.DetectedPresent:
                        return "Installed";

                    case InstallationState.DetectedNewer:
                        return "Newer version installed";

                    case InstallationState.DetectedAbsent:
                        return "Not installed";

                    case InstallationState.Applying:
                        switch (HadoukenInstaller.Model.PlannedAction)
                        {
                            case LaunchAction.Install:
                                return "Installing...";

                            case LaunchAction.Repair:
                                return "Repairing...";

                            case LaunchAction.Uninstall:
                                return "Uninstalling...";

                            case LaunchAction.UpdateReplace:
                            case LaunchAction.UpdateReplaceEmbedded:
                                return "Updating...";

                            default:
                                return "Unexpected action state";
                        }

                    case InstallationState.Applied:
                        switch (HadoukenInstaller.Model.PlannedAction)
                        {
                            case LaunchAction.Install:
                                return "Successfully installed";

                            case LaunchAction.Repair:
                                return "Successfully repaired";

                            case LaunchAction.Uninstall:
                                return "Successfully uninstalled";

                            case LaunchAction.UpdateReplace:
                            case LaunchAction.UpdateReplaceEmbedded:
                                return "Successfully updated";

                            default:
                                return "Unexpected action state";
                        }

                    case InstallationState.Failed:
                        if (_root.Canceled)
                        {
                            return "Canceled";
                        }
                        
                        if (LaunchAction.Unknown != HadoukenInstaller.Model.PlannedAction)
                        {
                            switch (HadoukenInstaller.Model.PlannedAction)
                            {
                                case LaunchAction.Install:
                                    return "Failed to install";

                                case LaunchAction.Repair:
                                    return "Failed to repair";

                                case LaunchAction.Uninstall:
                                    return "Failed to uninstall";

                                case LaunchAction.UpdateReplace:
                                case LaunchAction.UpdateReplaceEmbedded:
                                    return "Failed to update";

                                default:
                                    return "Unexpected action state";
                            }
                        }
                        
                        return "Unexpected failure";

                    default:
                        return "Unknown view model state";
                }
            }
        }

        void RootPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                base.OnPropertyChanged("Title");
                base.OnPropertyChanged("CompleteEnabled");
                base.OnPropertyChanged("ExitEnabled");
                base.OnPropertyChanged("RepairEnabled");
                base.OnPropertyChanged("InstallEnabled");
                base.OnPropertyChanged("TryAgainEnabled");
                base.OnPropertyChanged("UninstallEnabled");
            }
        }

        private void DetectBegin(object sender, DetectBeginEventArgs e)
        {
            _root.State = e.Installed ? InstallationState.DetectedPresent : InstallationState.DetectedAbsent;
        }

        private void ApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            HadoukenInstaller.Model.Result = e.Status; // remember the final result of the apply.

            // If we're not in Full UI mode, we need to alert the dispatcher to stop and close the window for passive.
            if (HadoukenInstaller.Model.Command.Display != Display.Full)
            {
                // If its passive, send a message to the window to close.
                if (HadoukenInstaller.Model.Command.Display == Display.Passive)
                {
                    HadoukenInstaller.Model.Engine.Log(LogLevel.Verbose, "Automatically closing the window for non-interactive install");
                    HadoukenInstaller.Dispatcher.BeginInvoke((Action)(() => HadoukenInstaller.View.Close())
                    );
                }
                else
                {
                    HadoukenInstaller.Dispatcher.InvokeShutdown();
                }
            }
            else if (Hresult.Succeeded(e.Status) && HadoukenInstaller.Model.PlannedAction == LaunchAction.UpdateReplace) // if we successfully applied an update close the window since the new Bundle should be running now.
            {
                HadoukenInstaller.Model.Engine.Log(LogLevel.Verbose, "Automatically closing the window since update successful.");
                HadoukenInstaller.Dispatcher.BeginInvoke((Action)(() => HadoukenInstaller.View.Close())
                );
            }

            // Set the state to applied or failed unless the state has already been set back to the preapply state
            // which means we need to show the UI as it was before the apply started.
            if (_root.State != _root.PreApplyState)
            {
                _root.State = Hresult.Succeeded(e.Status) ? InstallationState.Applied : InstallationState.Failed;
            }
        }

        private void ResolveSource(object sender, ResolveSourceEventArgs e)
        {
            int retries = 0;

            _downloadRetries.TryGetValue(e.PackageOrContainerId, out retries);
            _downloadRetries[e.PackageOrContainerId] = retries + 1;

            e.Result = retries < 3 && !String.IsNullOrEmpty(e.DownloadSource) ? Result.Download : Result.Ok;
        }

        private void ExecuteError(object sender, ErrorEventArgs e)
        {
            lock (this)
            {
                if (!_root.Canceled)
                {
                    // If the error is a cancel coming from the engine during apply we want to go back to the preapply state.
                    if (_root.State == InstallationState.Applying && (int)Error.UserCancelled == e.ErrorCode)
                    {
                        _root.State = _root.PreApplyState;
                    }
                    else
                    {
                        Message = e.ErrorMessage;

                        if (HadoukenInstaller.Model.Command.Display == Display.Full)
                        {
                            // On HTTP authentication errors, have the engine try to do authentication for us.
                            if (e.ErrorType == ErrorType.HttpServerAuthentication || e.ErrorType == ErrorType.HttpProxyAuthentication)
                            {
                                e.Result = Result.TryAgain;
                            }
                            else // show an error dialog.
                            {
                                var msgbox = MessageBoxButton.OK;
                                switch (e.UIHint & 0xF)
                                {
                                    case 0:
                                        msgbox = MessageBoxButton.OK;
                                        break;
                                    case 1:
                                        msgbox = MessageBoxButton.OKCancel;
                                        break;
                                    // There is no 2! That would have been MB_ABORTRETRYIGNORE.
                                    case 3:
                                        msgbox = MessageBoxButton.YesNoCancel;
                                        break;
                                    case 4:
                                        msgbox = MessageBoxButton.YesNo;
                                        break;
                                    // default: stay with MBOK since an exact match is not available.
                                }

                                var result = MessageBoxResult.None;
                                HadoukenInstaller.View.Dispatcher.Invoke((Action) delegate()
                                    {
                                        result = MessageBox.Show(HadoukenInstaller.View, e.ErrorMessage, "WiX Toolset",
                                                                 msgbox, MessageBoxImage.Error);
                                    });

                                // If there was a match from the UI hint to the msgbox value, use the result from the
                                // message box. Otherwise, we'll ignore it and return the default to Burn.
                                if ((e.UIHint & 0xF) == (int)msgbox)
                                {
                                    e.Result = (Result)result;
                                }
                            }
                        }
                    }
                }
                else // canceled, so always return cancel.
                {
                    e.Result = Result.Cancel;
                }
            }
        }

        private void ExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
        {
            if (DateTime.MinValue < _executePackageStart)
            {
                this.AddPackageTelemetry("Execute", e.PackageId ?? String.Empty, DateTime.Now.Subtract(_executePackageStart).TotalMilliseconds, e.Status);
                this._executePackageStart = DateTime.MinValue;
            }
        }

        private void ExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
        {
            _executePackageStart = e.ShouldExecute ? DateTime.Now : DateTime.MinValue;
        }

        private void CacheAcquireComplete(object sender, CacheAcquireCompleteEventArgs e)
        {
            AddPackageTelemetry("Cache", e.PackageOrContainerId ?? String.Empty, DateTime.Now.Subtract(cachePackageStart).TotalMilliseconds, e.Status);
        }

        private void CacheAcquireBegin(object sender, CacheAcquireBeginEventArgs e)
        {
            cachePackageStart = DateTime.Now;
        }

        private void ApplyBegin(object sender, ApplyBeginEventArgs e)
        {
            _downloadRetries.Clear();
        }

        private void PlanComplete(object sender, PlanCompleteEventArgs e)
        {
            if (Hresult.Succeeded(e.Status))
            {
                _root.PreApplyState = _root.State;
                _root.State = InstallationState.Applying;
                HadoukenInstaller.Model.Engine.Apply(_root.ViewWindowHandle);
            }
            else
            {
                _root.State = InstallationState.Failed;
            }
        }

        private void PlanPackageBegin(object sender, PlanPackageBeginEventArgs e)
        {
            if (HadoukenInstaller.Model.Engine.StringVariables.Contains("MbaNetfxPackageId") && e.PackageId.Equals(HadoukenInstaller.Model.Engine.StringVariables["MbaNetfxPackageId"], StringComparison.Ordinal))
            {
                e.State = RequestState.None;
            }
        }

        private void DetectComplete(object sender, DetectCompleteEventArgs e)
        {
            // Parse the command line string before any planning.
            this.ParseCommandLine();

            if (LaunchAction.Uninstall == HadoukenInstaller.Model.Command.Action)
            {
                HadoukenInstaller.Model.Engine.Log(LogLevel.Verbose, "Invoking automatic plan for uninstall");
                HadoukenInstaller.Plan(LaunchAction.Uninstall);
            }
            else if (Hresult.Succeeded(e.Status))
            {
                if (Downgrade)
                {
                    // TODO: What behavior do we want for downgrade?
                    _root.State = InstallationState.DetectedNewer;
                }

                if (HadoukenInstaller.Model.Command.Action == LaunchAction.Layout)
                {
                    HadoukenInstaller.PlanLayout();
                }
                else if (HadoukenInstaller.Model.Command.Display != Display.Full)
                {
                    // If we're not waiting for the user to click install, dispatch plan with the default action.
                    HadoukenInstaller.Model.Engine.Log(LogLevel.Verbose, "Invoking automatic plan for non-interactive mode.");
                    HadoukenInstaller.Plan(HadoukenInstaller.Model.Command.Action);
                }
            }
            else
            {
                _root.State = InstallationState.Failed;
            }
        }

        private void DetectedRelatedBundle(object sender, DetectRelatedBundleEventArgs e)
        {
            if (e.Operation == RelatedOperation.Downgrade)
            {
                Downgrade = true;
            }
        }

        private void ParseCommandLine()
        {
            // Get array of arguments based on the system parsing algorithm.
            string[] args = HadoukenInstaller.Model.Command.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].StartsWith("InstallFolder=", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Allow relative directory paths. Also validates.
                    string[] param = args[i].Split(new char[] { '=' }, 2);
                    _root.InstallDirectory = Path.Combine(Environment.CurrentDirectory, param[1]);
                }
            }
        }

        private void AddPackageTelemetry(string prefix, string id, double time, int result)
        {
            lock (this)
            {
                string key = String.Format("{0}Time_{1}", prefix, id);
                string value = time.ToString();
                HadoukenInstaller.Model.Telemetry.Add(new KeyValuePair<string, string>(key, value));

                key = String.Format("{0}Result_{1}", prefix, id);
                value = String.Concat("0x", result.ToString("x"));
                HadoukenInstaller.Model.Telemetry.Add(new KeyValuePair<string, string>(key, value));
            }
        }
    }
}
