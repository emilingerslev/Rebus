﻿using System;
using System.Diagnostics;
using Microsoft.AspNet.SignalR.Client;
using Rebus.Configuration;
using Rebus.Logging;

namespace Rebus.FleetKeeper.Client
{
    public class FleetKeeperClient : IDisposable
    {
        static ILog log;

        readonly HubConnection connection;
        readonly IHubProxy hubProxy;
        readonly Guid clientId;

        static FleetKeeperClient()
        {
            RebusLoggerFactory.Changed += f => log = f.GetCurrentClassLogger();
        }

        public FleetKeeperClient(string uri)
        {
            clientId = Guid.NewGuid();

            log.Info("Establishing hub connection to {0}", uri);
            connection = new HubConnection(uri);

            log.Info("Creating hub proxy");
            hubProxy = connection.CreateHubProxy("RebusHub");
            //hubProxy.On("MessageToClient", (string str) => ReceiveMessage(Deserialize(str)));

            log.Info("Starting connection");
            connection.Start().Wait();
            log.Info("Started!");
        }

        public void OnBusStarted(IBus bus)
        {
            var currentProcess = Process.GetCurrentProcess();
            var processStartInfo = currentProcess.StartInfo;
            var fileName = !string.IsNullOrWhiteSpace(processStartInfo.FileName) ? processStartInfo.FileName : currentProcess.ProcessName;

            hubProxy.Invoke("Receive", new
            {
                ClientId = clientId, 
                //InputQueueAddress = bus.Advanced.Diagnostics.InputQueueName,
                Environment.MachineName, 
                Os = Environment.OSVersion.ToString(), 
                FileName = fileName
            });
        }

        public void OnDispose(IBus bus)
        {
            Dispose();
        }

        public void Dispose()
        {
        }
    }
}