using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Hadouken.Installer.ViewModels
{
    public enum Error
    {
        UserCancelled = 1223,
    }

    public class RootViewModel : PropertyNotifyBase
    {
        private ICommand _cancelCommand;
        private ICommand _closeCommand;

        private bool _canceled;
        private InstallationState _state;

        public RootViewModel()
        {
            InstallationViewModel = new InstallationViewModel(this);
            ProgressViewModel = new ProgressViewModel(this);
        }

        public InstallationViewModel InstallationViewModel { get; private set; }
        public ProgressViewModel ProgressViewModel { get; private set; }

        public IntPtr ViewWindowHandle { get; set; }

        /// <summary>
        /// Gets and sets the state of the view's model before apply begins in order to return to that state if cancel or rollback occurs.
        /// </summary>
        public InstallationState PreApplyState { get; set; }

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(param => HadoukenInstaller.View.Close());
                }

                return _closeCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(param =>
                    {
                        lock (this)
                        {
                            Canceled = (MessageBox.Show(HadoukenInstaller.View, "Are you sure you want to cancel?", "WiX Toolset", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes);
                        }
                    },
                    param => State == InstallationState.Applying);
                }

                return _cancelCommand;
            }
        }

        public bool CancelEnabled
        {
            get { return CancelCommand.CanExecute(this); }
        }

        public bool Canceled
        {
            get
            {
                return _canceled;
            }

            set
            {
                if (_canceled == value) return;
                _canceled = value;
                base.OnPropertyChanged("Canceled");
            }
        }

        public string InstallDirectory
        {
            get { return HadoukenInstaller.Model.InstallDirectory; }

            set
            {
                if (HadoukenInstaller.Model.InstallDirectory != value)
                {
                    HadoukenInstaller.Model.InstallDirectory = value;
                    base.OnPropertyChanged("InstallDirectory");
                }
            }
        }

        /// <summary>
        /// Gets and sets the state of the view's model.
        /// </summary>
        public InstallationState State
        {
            get
            {
                return _state;
            }

            set
            {
                if (_state == value) return;

                _state = value;

                // Notify all the properties derived from the state that the state changed.
                base.OnPropertyChanged("State");
                base.OnPropertyChanged("CancelEnabled");
            }
        }

        public bool InstallWindowsService
        {
            get { return HadoukenInstaller.Model.InstallWindowsService; }

            set
            {
                if (HadoukenInstaller.Model.InstallWindowsService == value) return;

                HadoukenInstaller.Model.InstallWindowsService = value;
                base.OnPropertyChanged("InstallWindowsService");
            }
        }
    }
}
