using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Installer.ViewModels
{
    public enum Error
    {
        UserCancelled = 1223,
    }

    public class RootViewModel : PropertyNotifyBase
    {
        public IntPtr ViewWindowHandle { get; set; }
    }
}
