using System.Diagnostics;
using System.Net;
using System.Reflection;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Installer
{
    public class Model
    {
        private Version _version;

        private static readonly string BurnBundleInstallDirectoryVariable = "InstallFolder";
        private static readonly string BurnBundleLayoutDirectoryVariable = "WixBundleLayoutDirectory";

        private static readonly string WixInstallWindowsServiceVariable = "InstallWindowsService";
        private static readonly string WixWebInterfacePort = "WebInterfacePort";
        private static readonly string WixWindowsServiceAccount = "WindowsServiceAccount";
        private static readonly string WixWindowsServicePassword = "WindowsServicePassword";

        public Model(BootstrapperApplication bootstrapper)
        {
            this.Bootstrapper = bootstrapper;
            this.Telemetry = new List<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Gets the bootstrapper.
        /// </summary>
        public BootstrapperApplication Bootstrapper { get; private set; }

        /// <summary>
        /// Gets the bootstrapper command-line.
        /// </summary>
        public Command Command { get { return this.Bootstrapper.Command; } }

        /// <summary>
        /// Gets the bootstrapper engine.
        /// </summary>
        public Engine Engine { get { return this.Bootstrapper.Engine; } }

        /// <summary>
        /// Gets the key/value pairs used in telemetry.
        /// </summary>
        public List<KeyValuePair<string, string>> Telemetry { get; private set; }

        /// <summary>
        /// Get or set the final result of the installation.
        /// </summary>
        public int Result { get; set; }

        /// <summary>
        /// Get the version of the install.
        /// </summary>
        public Version Version
        {
            get
            {
                if (null == _version)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);

                    _version = new Version(fileVersion.FileVersion);
                }

                return _version;
            }
        }

        /// <summary>
        /// Get or set the path where the bundle is installed.
        /// </summary>
        public string InstallDirectory
        {
            get
            {
                if (!this.Engine.StringVariables.Contains(BurnBundleInstallDirectoryVariable))
                {
                    return null;
                }

                return this.Engine.StringVariables[BurnBundleInstallDirectoryVariable];
            }

            set
            {
                this.Engine.StringVariables[BurnBundleInstallDirectoryVariable] = value;
            }
        }
        
        /// <summary>
        /// Get or set the path for the layout to be created.
        /// </summary>
        public string LayoutDirectory
        {
            get
            {
                if (!this.Engine.StringVariables.Contains(BurnBundleLayoutDirectoryVariable))
                {
                    return null;
                }

                return this.Engine.StringVariables[BurnBundleLayoutDirectoryVariable];
            }

            set
            {
                this.Engine.StringVariables[BurnBundleLayoutDirectoryVariable] = value;
            }
        }

        public LaunchAction PlannedAction { get; set; }

        /// <summary>
        /// Creates a correctly configured HTTP web request.
        /// </summary>
        /// <param name="uri">URI to connect to.</param>
        /// <returns>Correctly configured HTTP web request.</returns>
        public HttpWebRequest CreateWebRequest(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UserAgent = String.Concat("HadoukenInstall-", this.Version.ToString());

            return request;
        }

        public bool InstallWindowsService
        {
            get
            {
                if (!this.Engine.StringVariables.Contains(WixInstallWindowsServiceVariable))
                    return false;

                return Convert.ToBoolean(Convert.ToInt32(this.Engine.StringVariables[WixInstallWindowsServiceVariable]));
            }

            set { this.Engine.StringVariables[WixInstallWindowsServiceVariable] = Convert.ToInt32(value).ToString(); }
        }

        public int WebInterfacePort
        {
            get
            {
                if (!this.Engine.StringVariables.Contains(WixWebInterfacePort))
                    return 8080;

                return Convert.ToInt32(this.Engine.StringVariables[WixWebInterfacePort]);
            }

            set { this.Engine.StringVariables[WixWebInterfacePort] = value.ToString(); }
        }

        public string WindowsServiceAccount
        {
            get
            {
                if (!this.Engine.StringVariables.Contains(WixWindowsServiceAccount))
                {
                    return null;
                }

                return this.Engine.StringVariables[WixWindowsServiceAccount];
            }

            set
            {
                this.Engine.StringVariables[WixWindowsServiceAccount] = value;
            }
        }

        public string WindowsServicePassword
        {
            get
            {
                if (!this.Engine.StringVariables.Contains(WixWindowsServicePassword))
                {
                    return null;
                }

                return this.Engine.StringVariables[WixWindowsServicePassword];
            }

            set
            {
                this.Engine.StringVariables[WixWindowsServicePassword] = value;
            }
        }
    }
}
