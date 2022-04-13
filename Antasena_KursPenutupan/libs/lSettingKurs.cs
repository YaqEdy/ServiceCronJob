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
using System.Data.Odbc;

namespace Antasena_KursPenutupan.libs
{
    class lSettingKurs
    {
        public static void SettingKursAsync()
        {
            //unggahKurs();
            try
            {
                var dt = getConfig();
                var ftp = dt.Rows[0]["ConfigURL"].ToString();
                var path = dt.Rows[0]["ConfigPath"].ToString();
                var port = dt.Rows[0]["ConfigPort"].ToString();
                var username = dt.Rows[0]["ConfigUsername"].ToString();
                var password = dt.Rows[0]["ConfigPassword"].ToString();
                var fileName = dt.Rows[0]["ConfigFileName"].ToString();
                var jamMulaiTarik = dt.Rows[0]["ConfigJamTarik"].ToString();
                var jamAkhirTarik = dt.Rows[0]["ConfigJamAkhirTarik"].ToString();
                var interval = dt.Rows[0]["ConfigInterval"].ToString();

                var start = DateTime.ParseExact(jamMulaiTarik, "HH:mm", CultureInfo.InvariantCulture);
                var end = DateTime.ParseExact(jamAkhirTarik, "HH:mm", CultureInfo.InvariantCulture);
                var TimeNow = DateTime.ParseExact(DateTime.Now.ToString("HH:mm"), "HH:mm", CultureInfo.InvariantCulture);
                if (start <= TimeNow && end > TimeNow)
                {

                    lGlobal.insertLog("Start insert database #ConfiKursId:" + dt.Rows[0]["ConfiKursId"].ToString());
                    unggahKurs(ftp, port, path, fileName, username, password);
                    lGlobal.insertLog("End insert database #ConfiKursId:" + dt.Rows[0]["ConfiKursId"].ToString());

                }
            }
            catch (Exception ex)
            {
                lGlobal.insertLog("Error: " + ex.Message);
            }
        }
        public static DataTable checkData(string strdate)
        {
            var myConn = lGlobal.myConnDB();
            SqlCommand cmd = new SqlCommand("sk_checkScheduler", myConn);
            cmd.Parameters.AddWithValue("@tipekurs", "C");
            cmd.Parameters.AddWithValue("@date", strdate);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            myConn.Close();
            return ds.Tables[0];
        }
        public static string checkDataImpala(string strdate)
        {
            var myConnImpala = lGlobal.myConnDB_Impala();
            OdbcCommand cmdA = new OdbcCommand("select cast(count(1) as string) from ip.kph01 where strleft(tanggalvaluta,10)=strleft(cast(cast(unix_timestamp('" + strdate + "', 'dd MMM yyyy') AS TIMESTAMP) AS string),10)", myConnImpala);
            string aa = (string)cmdA.ExecuteScalar();
            myConnImpala.Close();
            return aa;
        }
        public static DataTable getConfig()
        {
            var myConn = lGlobal.myConnDB();
            SqlCommand cmd = new SqlCommand("sk_getSettingKurs", myConn);
            cmd.Parameters.AddWithValue("@fg", "P");
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            myConn.Close();
            return ds.Tables[0];
        }
        //public static void unggahKurs()
        //{
        //    string ftp = "192.168.0.6";
        //    string port = "21";
        //    string path = "/";
        //    string fileName = "kursbi_0930.txt";
        //    string username = "optus_119";
        //    string password = "optus119";
        public static void unggahKurs(string ftp, string port, string path, string fileName, string username, string password)
        {
            try
            {
                var myConn1 = lGlobal.myConnDB();
                SqlCommand cmd1 = new SqlCommand(" select * from MSSistem where SistemCode='PathTo_sk' ", myConn1);
                SqlDataAdapter sda1 = new SqlDataAdapter(cmd1);
                DataSet ds1 = new DataSet();
                sda1.Fill(ds1);
                var dt1 = ds1.Tables[0];
                myConn1.Close();

                var PathTo = dt1.Rows[0]["SistemValue"].ToString();

                Uri target = new Uri("ftp://" + ftp + ":" + port + "/" + path + fileName);

                //Create FTP Request.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                //Enter FTP Server credentials.
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = true;
                request.UseBinary = true;
                request.EnableSsl = false;

                //Fetch the Response and read it into a MemoryStream object.
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo(PathTo);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    using (Stream fileStream = new FileStream(System.IO.Path.Combine(PathTo, fileName), FileMode.CreateNew))
                    {
                        responseStream.CopyTo(fileStream);
                    }
                }

                string iValidDate = "";
                string sql = "insert into Kurs(TipeKurs,PublishDate,ValidDate,CurrCode,Beli,Jual,EntryDate,FgPublish,UpdUser)  ";
                string qImpala = "insert into ip.kph01(pk,tanggalvaluta,jenisvaluta,buyprice,sellprice)  ";

                string paramAll = "KursPenutupan_kph01|1|value";
                string qImpalaSelect = lGlobal.GetQuery(paramAll, null);

                string[] lines = System.IO.File.ReadAllLines(System.IO.Path.Combine(PathTo, fileName));
                for (int i = 1; i < lines.Count(); i++)
                {
                    if (lines[i].Contains("\t"))
                    {
                        string[] rows = lines[i].Split("\t".ToCharArray());
                        if (rows[0].Length >= 3)
                        {
                            if (rows[0].Substring(0, 3) != "IDR" && rows.Count() >= 5)
                            {
                                iValidDate = (iValidDate == "") ? rows[4] : "";
                                //sql
                                sql += (sql.Contains("select")) ? " union all " : "";
                                sql += String.Format(@" select '{0}',{1},convert(datetime,'{2}'),'{3}','{4}','{5}',{6},'{7}','{8}'",
                                    "C",
                                    "GETDATE()",
                                    rows[4] + " " + rows[3],
                                    rows[0].Substring(0, 3),
                                    rows[1].Replace(".", "").Replace(",", "."),
                                    rows[2].Replace(".", "").Replace(",", "."),
                                    "GETDATE()",
                                    "Y",
                                    "Scheduler");
                                //impala
                                var itgl = (rows[4].Substring(1, 1) == " ") ? "0" + rows[4] : rows[4];
                                qImpala += (qImpala.Contains("select")) ? " union all " : "";
                                //qImpala += String.Format(@" select {0},cast(cast(unix_timestamp('{1}','dd MMM yyyy;H:mm:ss') as timestamp) as string),'{2}',CAST(CAST('{3}' as decimal(16,4)) as bigint),CAST(CAST('{4}' as decimal(16,4)) as bigint) ",
                                qImpala += String.Format(@"" + qImpalaSelect,
                                    "uuid()",
                                    itgl + ";" + rows[3],
                                    rows[0].Substring(0, 3),
                                    rows[1].Replace(".", "").Replace(",", "."),
                                    rows[2].Replace(".", "").Replace(",", ".")
                                    );
                            }
                        }
                    }
                }

                var dtcheckData = checkData(iValidDate);
                if (Convert.ToInt32(dtcheckData.Rows[0]["count"]) == 0)
                {
                    #region Insert Sql
                    var myConn = lGlobal.myConnDB();
                    SqlCommand cmd = new SqlCommand(sql, myConn);
                    int a = cmd.ExecuteNonQuery();
                    if (a < 1)
                    {
                        lGlobal.insertLog("Gagal Unggah Data.");
                    }
                    else
                    {
                        lGlobal.insertLog("Berhasil Unggah Data : " + a.ToString() + " Baris");
                    }
                    myConn.Close();
                    #endregion Insert Sql
                }

                #region Insert Impala
                if (checkDataImpala(iValidDate) == "0")
                {
                    var myConnImpala = lGlobal.myConnDB_Impala();
                    OdbcCommand odbccmd = new OdbcCommand(qImpala, myConnImpala);
                    int b = odbccmd.ExecuteNonQuery();
                    lGlobal.insertLog("b = " + b.ToString());
                    if (b < 1)
                    {
                        lGlobal.insertLog("Gagal Unggah Data Impala.");
                    }
                    else
                    {
                        lGlobal.insertLog("Berhasil Unggah Data Imapala" + b.ToString() + " Baris");
                    }
                    myConnImpala.Close();
                }
                #endregion Insert Impala

            }
            catch (Exception ex)
            {
                lGlobal.insertLog("Error: " + ex.Message);
            }

        }


    }
}
