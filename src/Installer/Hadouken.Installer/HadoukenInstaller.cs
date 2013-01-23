using System.IO;
using System.Windows.Forms;
using Hadouken.Installer.ViewModels;
using Hadouken.Installer.Views;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Hadouken.Installer
{
    public class HadoukenInstaller : BootstrapperApplication
    {
        /// <summary>
        /// Gets the global model.
        /// </summary>
        static public Model Model { get; private set; }

        /// <summary>
        /// Gets the global view.
        /// </summary>
        static public MainView View { get; private set; }
        // TODO: We should refactor things so we dont have a global View.

        /// <summary>
        /// Gets the global dispatcher.
        /// </summary>
        static public Dispatcher Dispatcher { get; private set; }

        /// <summary>
        /// Starts planning the appropriate action.
        /// </summary>
        /// <param name="action">Action to plan.</param>
        public static void Plan(LaunchAction action)
        {
            Model.PlannedAction = action;
            Model.Engine.Plan(Model.PlannedAction);
        }

        public static void PlanLayout()
        {
            // Either default or set the layout directory
            if (String.IsNullOrEmpty(Model.Command.LayoutDirectory))
            {
                Model.LayoutDirectory = Directory.GetCurrentDirectory();

                // Ask the user for layout folder if one wasn't provided and we're in full UI mode
                if (Model.Command.Display == Display.Full)
                {
                    Dispatcher.Invoke((Action)delegate()
                    {
                        FolderBrowserDialog browserDialog = new FolderBrowserDialog();
                        browserDialog.RootFolder = Environment.SpecialFolder.MyComputer;

                        // Default to the current directory.
                        browserDialog.SelectedPath = Model.LayoutDirectory;
                        DialogResult result = browserDialog.ShowDialog();

                        if (DialogResult.OK == result)
                        {
                            Model.LayoutDirectory = browserDialog.SelectedPath;
                            Plan(Model.Command.Action);
                        }
                        else
                        {
                            View.Close();
                        }
                    }
                    );
                }
            }
            else
            {
                Model.LayoutDirectory = Model.Command.LayoutDirectory;
                Plan(Model.Command.Action);
            }
        }

        protected override void Run()
        {
            Engine.Log(LogLevel.Verbose, "Running the Hadouken BA");

            Model = new Model(this);
            Dispatcher = Dispatcher.CurrentDispatcher;
            var viewModel = new RootViewModel();

            Engine.Detect();

            if (Model.Command.Display == Display.Passive || Model.Command.Display == Display.Full)
            {
                Engine.Log(LogLevel.Verbose, "Creating UI");
                
                View = new MainView(viewModel);
                View.Show();
            }

            Dispatcher.Run();

            Engine.Quit(Model.Result);
        }
    }
}
