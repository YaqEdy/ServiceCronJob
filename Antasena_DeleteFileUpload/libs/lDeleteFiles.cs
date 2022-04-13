using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Antasena_DeleteFileUpload.libs
{
    class lDeleteFiles
    {
        public static void deleteFileUpload()
        {
            try
            {
                var iConfig = lGlobal.getConfig().Split('|');
                var start = DateTime.ParseExact(iConfig[0].ToString(), "HH:mm", CultureInfo.InvariantCulture);
                var end = DateTime.ParseExact(iConfig[1].ToString(), "HH:mm", CultureInfo.InvariantCulture);
                var TimeNow = DateTime.ParseExact(DateTime.Now.ToString("HH:mm").Replace(".", ":"), "HH:mm", CultureInfo.InvariantCulture);

                if (start <= TimeNow && end > TimeNow)
                {
                    lGlobal.insertLog("Start Delete");
                    deleteFileUpload_Go(iConfig[2].ToString());
                    lGlobal.insertLog("End Delete");
                }
            }
            catch (Exception ex)
            {
                lGlobal.insertLog("Error: " + ex.Message);
            }
        }

        public static void deleteFileUpload_Go(string path)
        {
            try
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(path);//temp
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    string allDeleteDir = "";
                    bool islog = false;
                    System.IO.DirectoryInfo di_IdPelapor = new DirectoryInfo(dir.FullName);
                    foreach (DirectoryInfo dir1 in di_IdPelapor.GetDirectories())
                    {
                        var dateDir = DateTime.ParseExact(dir1.Name.ToString().Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                        var DateNow = DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture);
                        if (dateDir < DateNow)
                        {
                            dir1.Delete(true);
                            allDeleteDir += dir1.Name.ToString() + "|";
                            islog = true;
                        }
                    }
                    if (islog)
                    {
                        lGlobal.insertLog(dir.Name.ToString() + "#" + allDeleteDir);
                    }
                }

            }
            catch (Exception ex)
            {
                lGlobal.insertLog("Error: " + ex.Message);
            }

        }


    }
}
