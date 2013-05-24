using System;
using Hadouken.Common.Http;

namespace Hadouken.Common.Plugins
{
    public abstract class Plugin
    {
        private readonly IHubConnection _hubConnection;
        
        protected Plugin(IHubConnection hubConnection)
        {
            if(hubConnection == null)
                throw new ArgumentNullException("hubConnection");

            _hubConnection = hubConnection;
        }

        /// <summary>
        /// Gets the IHubConnection to use for listening to events.
        /// </summary>
        protected IHubConnection Hub
        {
            get { return _hubConnection; }
        }

        public abstract void Load();

        public virtual void Unload()
        {
        }
    }
}
