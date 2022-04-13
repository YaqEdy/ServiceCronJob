using Antasena_KursPenutupan.libs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Antasena_KursPenutupan
{
    [RunInstaller(true)]
    public partial class Service1 : ServiceBase
    {
        Timer Timer = new Timer();
        int Interval = 0; // 10000 ms = 10 second  

        public Service1()
        {
            //lSettingKurs.SettingKursAsync();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                lGlobal.insertLog("OnStart Task");
                var dtconfig = lSettingKurs.getConfig();
                Interval = Convert.ToInt32(dtconfig.Rows[0]["ConfigInterval"]);

                Timer.Interval = Interval * 1000 * 60; // 60 seconds
                Timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
                Timer.Start();
            }
            catch (Exception ex)
            {
                lGlobal.insertLog("Error OnStart: " + ex);
            }
        }

        private void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
            {
                var dtconfig = lSettingKurs.getConfig();
                Interval = Convert.ToInt32(dtconfig.Rows[0]["ConfigInterval"]);

                lGlobal.insertLog("OnTimer Start");
                lSettingKurs.SettingKursAsync();
                lGlobal.insertLog("OnTimer End");
            }
            catch (Exception ex)
            {
                lGlobal.insertLog("Error OnTimer: " + ex);
            }
        }

        protected override void OnStop()
        {
            lGlobal.insertLog("OnStop Task");
        }
    }
}
