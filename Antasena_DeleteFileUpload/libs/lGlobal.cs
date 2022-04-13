using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antasena_DeleteFileUpload.libs
{
    class lGlobal
    {
        const string appSettingPath = "C:\\AntasenaService\\";
        public static string dir = "", start_delete = "03:00", end_delete = "04:30";

        public static string getConfig()
        {
            var myConn = myConnDB();
            SqlCommand cmd = new SqlCommand("Select  sistemvalue from mssistem where  sistemcode = \'pathfileTmp\' ", myConn);
            string path = (string)cmd.ExecuteScalar();
            myConn.Close();

            return start_delete + "|" + end_delete + "|" + path;
        }
        public static SqlConnection myConnDB()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(appSettingPath)
               .AddJsonFile("appsettings.json");
            var config = builder.Build();
            string str = config.GetSection("DbContextSettings:ConnectionString_SQLSERVER").Value.ToString();
            dir = config["dir_log"].ToString();
            start_delete = config["start_delete"].ToString();
            end_delete = config["end_delete"].ToString();

            SqlConnection myConn = new SqlConnection();
            myConn.ConnectionString = str;
            myConn.Open();
            return myConn;
        }
        public static int getInterval()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(appSettingPath)
               .AddJsonFile("appsettings.json");
            var config = builder.Build();
          
            return Convert.ToInt32(config["time_interval"]);
        }
        public static void insertLog(string msg)
        {
            myConnDB();
            var now = DateTime.Now;
            dir = dir + now.ToString("yyyyMM");
            //check folder & create folder
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            //check file & create file
            var path = Path.Combine(dir, now.ToString("yyyyMMdd") + "_DeleteFileUpload.txt");
            if (!System.IO.File.Exists(path))
            {
                File.WriteAllText(path, "");
            }

            //create log in file
            if (System.IO.File.Exists(path))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(now.ToString("yyyy-MM-dd HH:mm:ss") + " #DeleteFileUpload# " + msg + " \n");
                System.IO.File.AppendAllText(path, sb.ToString());
                sb.Clear();
            }

        }

    }
}
