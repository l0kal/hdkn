﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using Hadouken.Hosting;
using Hadouken.Events;

namespace Hadouken.Hosts.WindowsService
{
    public class HdknService : ServiceBase
    {
        private IEventBroker _eventBroker;
        private IHost _host;

        public HdknService()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.AutoLog = true;
            this.ServiceName = "Hadouken";
        }

        protected override void OnStart(string[] args)
        {
            _eventBroker = Kernel.Resolver.Get<IEventBroker>();
            _eventBroker.Start();

            _host = Kernel.Resolver.Get<IHost>();
            _host.Load();
        }

        protected override void OnStop()
        {
            if (_host != null)
                _host.Unload();

            _eventBroker.Stop();
        }
    }
}
