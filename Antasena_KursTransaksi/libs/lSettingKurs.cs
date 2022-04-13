using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Antasena_KursTransaksi.libs
{
    class lSettingKurs
    {
        public static void SettingKursAsync()
        {
            try
            {
                var dt = getConfig();
                var Kurs_Date = DateTime.Now.AddDays(Convert.ToInt32(dt.Rows[0]["Kurs_Date"]));
                var dtcheckData = checkData(Kurs_Date.ToString("yyyy-MM-dd"));
                if (Convert.ToInt32(dtcheckData.Rows[0]["count"]) == 0)
                {
                    var Kurs_Hour = dt.Rows[0]["Kurs_Hour"].ToString();
                    var Kurs_Hour_End = dt.Rows[0]["Kurs_Hour_End"].ToString();
                    var Kurs_Minute = dt.Rows[0]["Kurs_Minute"].ToString();
                    var Kurs_Holiday = dt.Rows[0]["Kurs_See_Holiday"].ToString();

                    if (Convert.ToInt64(DateTime.Now.ToString("mm")) % Convert.ToInt64(Kurs_Minute) == 0)
                    {
                        lGlobal.insertLog("Start per" + Kurs_Minute+" Menit");

                        bool isholiday = false;
                        if (Kurs_Holiday == "1")
                        {
                            var dtHoliday = getHoliday();
                            dtHoliday = (from row in dtHoliday.AsEnumerable()
                                         where row.Field<string>("TanggalHariLibur").Split('-')[0].ToString() == Kurs_Date.ToString("yyyy")
                                         select row).CopyToDataTable();
                            for (int r = 0; r < dtHoliday.Rows.Count; r++)
                            {
                                if (!isholiday)
                                {
                                    isholiday = (dtHoliday.Rows[r]["TanggalHariLibur"].ToString() == Kurs_Date.ToString("dd-MM-yyyy")) ? true : false;
                                }
                            }
                        }

                        if (!isholiday)
                        {
                            var start = DateTime.ParseExact(Kurs_Hour, "HH:mm", CultureInfo.InvariantCulture);
                            var end = DateTime.ParseExact(Kurs_Hour_End, "HH:mm", CultureInfo.InvariantCulture);
                            var TimeNow = DateTime.ParseExact(DateTime.Now.ToString("HH:mm"), "HH:mm", CultureInfo.InvariantCulture);
                            //TimeSpan duration = DateTime.Parse(jamAkhirTarik).Subtract(DateTime.Parse(jamMulaiTarik));
                            if (start <= TimeNow && end > TimeNow)
                            {
                                lGlobal.insertLog("Start insert database #ConfiKursId:" + dt.Rows[0]["ConfiKursId"].ToString());
                                unggahKurs(dt);
                                lGlobal.insertLog("End insert database #ConfiKursId:" + dt.Rows[0]["ConfiKursId"].ToString());

                                //var iSleep = Convert.ToInt32(Kurs_Minute) * 1000 * 60;//menit
                                //Thread.Sleep(iSleep);
                            }
                        }
                    }
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
            cmd.Parameters.AddWithValue("@tipekurs", "T");
            cmd.Parameters.AddWithValue("@date", strdate);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            myConn.Close();
            return ds.Tables[0];
        }
        public static DataTable getConfig()
        {
            var myConn = lGlobal.myConnDB();
            SqlCommand cmd = new SqlCommand("sk_getSettingKurs", myConn);
            cmd.Parameters.AddWithValue("@fg", "T");
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            myConn.Close();
            return ds.Tables[0];
        }
        public static DataTable getHoliday()
        {
            var myConn = lGlobal.myConnDB();
            SqlCommand cmd = new SqlCommand("hld_getAllHoliday", myConn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            myConn.Close();
            return ds.Tables[0];
        }
        public static void unggahKurs(DataTable dt, bool fg = false)
        {
            try
            {
                string authInfo = dt.Rows[0]["Kurs_Username"].ToString() + ":" + dt.Rows[0]["Kurs_Password"].ToString();
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                var url = (!fg) ? dt.Rows[0]["Kurs_WebService_Address"].ToString() : dt.Rows[0]["Kurs_WebService_Address2"].ToString();
                var Kurs_Date = DateTime.Now.AddDays(Convert.ToInt32(dt.Rows[0]["Kurs_Date"])).ToString("yyyy-MM-dd");
                var soapenv = dt.Rows[0]["Kurs_WebService_Prefix"].ToString();
                var xsd = dt.Rows[0]["Kurs_WebService_XSD"].ToString();

                var restclient = new RestClient(url);

                if (bool.Parse(lGlobal.usingcert) == true)
                {
                    string certFile = Path.Combine(lGlobal.approotpath, lGlobal.cert);
                    X509Certificate2 certificates = new X509Certificate2(certFile, lGlobal.pwd);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    restclient.ClientCertificates = new X509CertificateCollection() { certificates };
                }
                RestRequest request = new RestRequest();
                request.Method = Method.POST;
                request.AddHeader("Authorization", "Basic " + authInfo);
                request.AddHeader("Content-Type", "text/xml;charset=UTF-8");
                request.AddHeader("Cache-Control", "no-cache");
                //request.AddHeader("SOAPAction", "\"getForexRate\"");
                //request.AddHeader("User-Agent", "Apache-HttpClient/4.5.5 (Java/12.0.1)");
                //request.AddHeader("Connection", "Keep-Alive");
                //request.AddHeader("Host", "10.161.175.11:7007");
                //request.AddHeader("Content-Length", "543");
                //request.AddHeader("Accept-Encoding", "gzip,deflate");

                request.AddParameter("undefined",
                    "<soapenv:Envelope xmlns:soapenv=\"" + soapenv + "\" xmlns:xsd=\"" + xsd + "\">\r\n   " +
                    "<soapenv:Header/>\r\n   " +
                    "<soapenv:Body>\r\n      <xsd:ForexRateStructParam>\r\n         " +
                    "<xsd:date>" + Kurs_Date + "</xsd:date>\r\n         " +
                    "<xsd:currencyCode></xsd:currencyCode>\r\n         " +
                    "<xsd:userId>?</xsd:userId>\r\n         " +
                    "<xsd:satkerCode>?</xsd:satkerCode>\r\n      " +
                    "</xsd:ForexRateStructParam>\r\n   " +
                    "</soapenv:Body>\r\n</soapenv:Envelope>", ParameterType.RequestBody);

                var tResponse = restclient.Execute(request);
                if (!tResponse.IsSuccessful && !fg)
                {
                    lGlobal.insertLog(url + "#Error: " + tResponse.StatusDescription + " - " + tResponse.ErrorMessage);
                    unggahKurs(dt, true);
                }
                else if (!tResponse.IsSuccessful && fg)
                {
                    lGlobal.insertLog(url + "#Error: " + tResponse.StatusDescription + " - " + tResponse.ErrorMessage);
                }
                else
                {

                    var strResponse = tResponse.Content.Split("\n".ToCharArray())[1];

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml("<root>" + strResponse + "</root>");
                    //doc.LoadXml(strResponse );
                    strResponse = JObject.Parse(JsonConvert.SerializeXmlNode(doc).ToString())["root"].ToString();
                    var ijob = JObject.Parse(strResponse)["soapenv:Envelope"]["soap-env:Body"]["ns2:ForexRateResponse"]["ns2:ForexRateList"];
                    if (ijob.Count() > 0)
                    {
                        var job = ijob["ns2:ForexRateMap"];
                        if (job.Count() > 0)
                        {
                            string sql = "insert into Kurs(TipeKurs,PublishDate,ValidDate,CurrCode,Beli,Jual,EntryDate,FgPublish,UpdUser)  ";
                            for (int i = 0; i < job.Count(); i++)
                            {
                                var currCode = job[i]["ns2:CurrencyCode"].ToString();
                                var beli = job[i]["ns2:TxBuyRate"].ToString();
                                var jual = job[i]["ns2:TxSellRate"].ToString();
                                var idate = job[i]["ns2:DailyDate"].ToString();

                                sql += (sql.Contains("select")) ? " union all " : "";
                                sql += String.Format(@" select '{0}',{1},convert(datetime,'{2}'),'{3}','{4}','{5}',{6},'{7}','{8}'",
                                    "T",
                                    "GETDATE()",
                                    idate.Replace("+", " "),
                                    currCode,
                                    beli,
                                    jual,
                                    "GETDATE()",
                                    "Y",
                                    "Scheduler");
                            }

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
                        }
                        else
                        {
                            lGlobal.insertLog(url + "#ForexRateMap no data ");
                            //unggahKurs(dt, true);
                        }
                    }
                    else
                    {
                        lGlobal.insertLog(url + "#ForexRateList no data ");
                        //unggahKurs(dt, true);
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
