using Antasena_KursTransaksi.libs;
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
using Quartz;
using Quartz.Impl;


namespace Antasena_KursTransaksi
{
    [RunInstaller(true)]
    public partial class Service1 : ServiceBase
    {
        Timer Timer = new Timer();
        int Interval = 0; // 10000 ms = 10 second  
        public Service1()
        {
            //lCron.CRONJOB();
            //lSettingKurs.SettingKursAsync();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            lCron.CRONJOB();

            //lGlobal.insertLog("OnStart Task");
            //var dtconfig = lSettingKurs.getConfig();
            //Interval = Convert.ToInt32(dtconfig.Rows[0]["Kurs_Minute"]);

            //Timer.Interval = Interval * 1000 * 60; // 60 seconds
            //Timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            //Timer.Start();
        }

        //private void OnTimer(object sender, ElapsedEventArgs args)
        //{
        //    var dtconfig = lSettingKurs.getConfig();
        //    Interval = Convert.ToInt32(dtconfig.Rows[0]["Kurs_Minute"]);

        //    lGlobal.insertLog("OnTimer Start");
        //    lSettingKurs.SettingKursAsync();
        //    lGlobal.insertLog("OnTimer End");
        //}

        protected override void OnStop()
        {
            lGlobal.insertLog("OnStop Task");
        }
    }
}
