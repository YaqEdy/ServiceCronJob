using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;
using System.Data;

namespace Antasena_KursPenutupan.libs
{
    class lGlobal
    {
        const string appSettingPath = "C:\\AntasenaService\\";
        public static string dir = "";
        public static string insertImpalaonly = "";

        public static SqlConnection myConnDB()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(appSettingPath)
               .AddJsonFile("appsettings.json");
            var config = builder.Build();
            string str = config.GetSection("DbContextSettings:ConnectionString_SQLSERVER").Value.ToString();
            dir = config["dir_log"].ToString();

            SqlConnection myConn = new SqlConnection();
            myConn.ConnectionString = str;
            myConn.Open();
            return myConn;
        }

        public static OdbcConnection myConnDB_Impala()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(appSettingPath)
               .AddJsonFile("appsettings.json");
            var config = builder.Build();
            string str = config.GetSection("DbContextSettings:ConnectionString_IMPALA").Value.ToString();
            dir = config["dir_log"].ToString();

            OdbcConnection myConn = new OdbcConnection();
            myConn.ConnectionString = str;
            myConn.Open();
            return myConn;
        }

        public static string GetQuery(string paramAll, string paramDt)
        {
            var myConn_ = myConnDB();
            SqlCommand cmd = new SqlCommand("rpt_getQuery", myConn_);
            cmd.Parameters.AddWithValue("@paramAll", paramAll);
            cmd.Parameters.AddWithValue("@paramDt", paramDt);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            myConn_.Close();

            return ds.Tables[0].Rows[0]["query"].ToString();
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
            var path = Path.Combine(dir, now.ToString("yyyyMMdd") + "_KursPenutupan.txt");
            if (!System.IO.File.Exists(path))
            {
                File.WriteAllText(path, "");
            }

            //create log in file
            if (System.IO.File.Exists(path))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(now.ToString("yyyy-MM-dd HH:mm:ss") + " #UnggahData# " + msg + " \n");
                System.IO.File.AppendAllText(path, sb.ToString());
                sb.Clear();
            }
            
        }

    }
}
