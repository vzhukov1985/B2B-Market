using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UpdateDb_Service.Models;

namespace UpdateDb_Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly static string settingsFile = CoreSettings.b2bDataLocalDir + CoreSettings.SettingsPath + "/" + CoreSettings.AgentSettingsFileName;

        private int updateHour = 10;
        private int updateMinutes = 0;

        private void LoadAgentSettings()
        {
            if (File.Exists(settingsFile))
            {
                XDocument xDoc = XDocument.Load(settingsFile);
                XElement xAgentSettings = xDoc.Root;
                XElement xUpdateTime = xAgentSettings.Element("UpdateTime");
                updateHour = int.Parse(xUpdateTime.Attribute("Hour").Value);
                updateMinutes = int.Parse(xUpdateTime.Attribute("Minutes").Value);
            }
        }

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            CoreSettings.DbConnectionString = "server=localhost;protocol=SOCKET;UserId=b2buser;Password=oD88x-2!Hq;database=b2bmarket_test";
            LoadAgentSettings();
            AdminTGBot.StartBot();
            OperatorTGBot.StartBot();
            
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
             while (!stoppingToken.IsCancellationRequested)
             {
                 UpdateDbProcessor.UpdateDb();
                 DateTime scheduleTime = DateTime.Today.AddDays(1).AddHours(updateHour).AddMinutes(updateMinutes);
                 TimeSpan interval = scheduleTime.Subtract(DateTime.Now);
                 await Task.Delay(interval, stoppingToken);
             }
        }
    }
}
