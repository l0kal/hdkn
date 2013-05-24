using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Plugins.PluginEngine.FullTrustHelpers
{
    [SecuritySafeCritical]
    public class AssemblyResolver
    {
        private readonly string _basePath;

        public AssemblyResolver(string basePath)
        {
            (new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, basePath)).Assert();

            _basePath = basePath;

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        private System.Reflection.Assembly Resolve(object sender, ResolveEventArgs args)
        {
            foreach (var file in Directory.GetFiles(_basePath, "*.dll"))
            {
                var asmName = AssemblyName.GetAssemblyName(file);

                if (asmName.FullName == args.Name)
                    return Assembly.LoadFile(file);
            }

            return null;
        }
    }
}
