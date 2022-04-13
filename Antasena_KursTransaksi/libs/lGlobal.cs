using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Antasena_KursTransaksi.libs
{
    class lGlobal
    {
        const string appSettingPath = "C:\\AntasenaService\\";

        public static string dir = "";
        public static string approotpath = "";
        public static string usingcert = "";
        public static string cert = "";
        public static string pwd = "";

        public static SqlConnection myConnDB()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(appSettingPath)
               .AddJsonFile("appsettings.json");
            var config = builder.Build();
            string str = config.GetSection("DbContextSettings:ConnectionString_SQLSERVER").Value.ToString();
            dir = config["dir_log"].ToString();
            approotpath = config["approotpath"].ToString();
            usingcert = config["usingcert"].ToString();
            cert = config["cert"].ToString();
            pwd = config["pwd"].ToString();

            SqlConnection myConn = new SqlConnection();
            myConn.ConnectionString = str;
            myConn.Open();
            return myConn;
        }

        //public static void insertLog(string msg)
        //{
        //    myConnDB();
        //    var now = DateTime.Now;
        //    dir = dir + now.ToString("yyyyMM");
        //    //check folder & create folder
        //    if (!System.IO.Directory.Exists(dir))
        //    {
        //        System.IO.Directory.CreateDirectory(dir);
        //    }

        //    //check file & create file
        //    var path = Path.Combine(dir, now.ToString("yyyyMMdd") + "_KursTransaksi.txt");
        //    if (!System.IO.File.Exists(path))
        //    {
        //        File.WriteAllText(path, "");
        //    }

        //    //create log in file
        //    if (System.IO.File.Exists(path))
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        sb.Append(now.ToString("yyyy-MM-dd HH:mm:ss") + " #UnggahData# " + msg + " \n");
        //        System.IO.File.AppendAllText(path, sb.ToString());
        //        sb.Clear();
        //    }
        //}
        public static void insertLog(string msg)
        {
            var myConn = myConnDB();
            try
            {
                SqlCommand cmd = new SqlCommand("log_ErrorPortal", myConn);
                cmd.Parameters.AddWithValue("@ip", GetLocalIPAddress());
                cmd.Parameters.AddWithValue("@msg", msg);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                myConn.Close();
            }
            catch (Exception ex)
            {
                var now = DateTime.Now;
                dir = dir + now.ToString("yyyyMM");
                //check folder & create folder
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                //check file & create file
                var path = Path.Combine(dir, now.ToString("yyyyMMdd") + "_KursTransaksi.txt");
                if (!System.IO.File.Exists(path))
                {
                    File.WriteAllText(path, "");
                }

                //create log in file
                if (System.IO.File.Exists(path))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(now.ToString("yyyy-MM-dd HH:mm:ss") + " #KursTransaksi# " + msg + "#=#" + ex.Message + "\n");
                    System.IO.File.AppendAllText(path, sb.ToString());
                    sb.Clear();
                }
            }
            finally
            {
                if (myConn != null) myConn.Close();
            }
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }


    }
}
