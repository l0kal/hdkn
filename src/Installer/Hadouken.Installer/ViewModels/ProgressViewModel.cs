using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace Hadouken.Installer.ViewModels
{
    public class ProgressViewModel : PropertyNotifyBase
    {
        private readonly RootViewModel _root;
        private Dictionary<string, int> executingPackageOrderIndex;

        private int progressPhases;
        private int progress;
        private int cacheProgress;
        private int executeProgress;
        private string message;

        public ProgressViewModel(RootViewModel root)
        {
            _root = root;
            executingPackageOrderIndex = new Dictionary<string, int>();

            _root.PropertyChanged += RootPropertyChanged;

            HadoukenInstaller.Model.Bootstrapper.ExecuteMsiMessage += ExecuteMsiMessage;
            HadoukenInstaller.Model.Bootstrapper.ExecuteProgress += ApplyExecuteProgress;
            HadoukenInstaller.Model.Bootstrapper.PlanBegin += PlanBegin;
            HadoukenInstaller.Model.Bootstrapper.PlanPackageComplete += PlanPackageComplete;
            HadoukenInstaller.Model.Bootstrapper.Progress += ApplyProgress;
            HadoukenInstaller.Model.Bootstrapper.CacheAcquireProgress += CacheAcquireProgress;
            HadoukenInstaller.Model.Bootstrapper.CacheComplete += CacheComplete;
        }

        public bool ProgressEnabled
        {
            get { return _root.State == InstallationState.Applying; }
        }

        public int Progress
        {
            get
            {
                return progress;
            }

            set
            {
                if (progress == value) return;
                
                progress = value;
                base.OnPropertyChanged("Progress");
            }
        }

        public string Message
        {
            get
            {
                return message;
            }

            set
            {
                if (message == value) return;

                message = value;
                base.OnPropertyChanged("Message");
            }
        }

        private void RootPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                base.OnPropertyChanged("ProgressEnabled");
            }
        }

        private void PlanBegin(object sender, PlanBeginEventArgs e)
        {
            lock (this)
            {
                progressPhases = (HadoukenInstaller.Model.PlannedAction == LaunchAction.Layout) ? 1 : 2;
                executingPackageOrderIndex.Clear();
            }
        }

        private void PlanPackageComplete(object sender, PlanPackageCompleteEventArgs e)
        {
            if (e.Execute == ActionState.None) return;

            lock (this)
            {
                Debug.Assert(!executingPackageOrderIndex.ContainsKey(e.PackageId));
                executingPackageOrderIndex.Add(e.PackageId, executingPackageOrderIndex.Count);
            }
        }

        private void ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            lock (this)
            {
                Message = e.Message;
                e.Result = _root.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void ApplyProgress(object sender, ProgressEventArgs e)
        {
            lock (this)
            {
                e.Result = _root.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void CacheAcquireProgress(object sender, CacheAcquireProgressEventArgs e)
        {
            lock (this)
            {
                cacheProgress = e.OverallPercentage;
                Progress = (cacheProgress + executeProgress) / progressPhases;
                e.Result = _root.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void CacheComplete(object sender, CacheCompleteEventArgs e)
        {
            lock (this)
            {
                cacheProgress = 100;
                Progress = (cacheProgress + executeProgress) / progressPhases;
            }
        }

        private void ApplyExecuteProgress(object sender, ExecuteProgressEventArgs e)
        {
            lock (this)
            {

                executeProgress = e.OverallPercentage;
                Progress = (cacheProgress + executeProgress) / 2; // always two phases if we hit execution.

                if (HadoukenInstaller.Model.Command.Display == Display.Embedded)
                {
                    HadoukenInstaller.Model.Engine.SendEmbeddedProgress(e.ProgressPercentage, this.Progress);
                }

                e.Result = _root.Canceled ? Result.Cancel : Result.Ok;
            }
        }
    }
}
