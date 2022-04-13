using Antasena_DeleteFileUpload.libs;
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

namespace Antasena_DeleteFileUpload
{
    public partial class Service1 : ServiceBase
    {
        Timer Timer = new Timer();
        int Interval = 0; // 10000 ms = 10 second  
        public Service1()
        {
            //lDeleteFiles.deleteFileUpload();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            lGlobal.insertLog("OnStart Task");
            Interval = lGlobal.getInterval();

            Timer.Interval = Interval * 1000 * 60; // 60 seconds
            Timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            Timer.Start();
        }

        private void OnTimer(object sender, ElapsedEventArgs args)
        {
            Interval = lGlobal.getInterval();

            lGlobal.insertLog("OnTimer Start");
            lDeleteFiles.deleteFileUpload();
            lGlobal.insertLog("OnTimer End");
        }

        protected override void OnStop()
        {
            lGlobal.insertLog("OnStop Task");
        }
    }
}
