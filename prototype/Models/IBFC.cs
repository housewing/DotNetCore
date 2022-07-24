using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Net.Http;
using Newtonsoft.Json.Linq;

using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;

using System.Net.Mail;
using System.Net.Mime;
using System.Text;

using Microsoft.AspNetCore.Http;

namespace prototype.Models
{
    public class IBFC : IIBFC
    {
        private IConfiguration configuration;
        private readonly IHttpContextAccessor accessor;
        string docPath = "wwwroot/temp/{0}.txt";

        public IBFC(IConfiguration _config, IHttpContextAccessor _accessor)
        {
            configuration = _config;
            accessor = _accessor;
        }

        public void Init(LoginMsg loginMsg)
        {
            string ts = accessor.HttpContext.Session.GetString("LoginTimeStamp");
            using (StreamWriter outputFile = new StreamWriter(string.Format(docPath, ts)))
            {
                outputFile.WriteLine(loginMsg.Id + "&" + loginMsg.Mail);
                for (int i = 0; i < 4; i++)
                    outputFile.WriteLine("");
            }
        }

        public LoginMsg ReadInit()
        {
            int count = 0;
            string ts = accessor.HttpContext.Session.GetString("LoginTimeStamp");
            LoginMsg loginMsg = new LoginMsg();
            using (StreamReader sr = new StreamReader(string.Format(docPath, ts)))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    count++;
                    List<string> tmp = line.Split("&").ToList();
                    if (count == 1) {
                        loginMsg.Id = tmp[0];
                        loginMsg.Mail = tmp[1];
                    } else if (count == 2) {
                        loginMsg.Index1List = tmp;
                    } else if (count == 3) {
                        loginMsg.Index2List = tmp;
                    } else if (count == 4) {
                        List<MortgageInfo> MortgageList = new List<MortgageInfo>();
                        foreach (string t in tmp)
                        {
                            if (t != "")
                            {
                                string[] arr = t.Split();
                                MortgageList.Add(new MortgageInfo() { Id = arr[0], City = arr[1], Area = arr[2], Type = arr[3], Price = arr[4], Num = arr[5] });
                            }
                        }
                        loginMsg.MortgageList = MortgageList;
                    } else if (count == 5) {
                        List<StockInfo> StockList = new List<StockInfo>();
                        foreach (string t in tmp)
                        {
                            if (t != "")
                            {
                                string[] arr = t.Split();
                                StockList.Add(new StockInfo() { Id = arr[0], Stock = arr[1], Name = arr[2], StockType = arr[3], Price = arr[4], Num = arr[5] });
                            }
                        }
                        loginMsg.StockList = StockList;
                    }
                }
            }
            return loginMsg;
        }

        public void WriteInit(LoginMsg loginMsg)
        {
            string ts = accessor.HttpContext.Session.GetString("LoginTimeStamp");
            using (StreamWriter outputFile = new StreamWriter(string.Format(docPath, ts)))
            {
                outputFile.WriteLine(loginMsg.Id + "&" + loginMsg.Mail);
                outputFile.WriteLine(string.Join("&", loginMsg.Index1List));
                outputFile.WriteLine(string.Join("&", loginMsg.Index2List));

                string tmp1 = "", tmp2 = "";
                foreach (MortgageInfo tmp in loginMsg.MortgageList)
                    tmp1 += tmp.Id + " " + tmp.City + " " + tmp.Area + " " + tmp.Type + " " + tmp.Price + " " + tmp.Num + "&";
                outputFile.WriteLine(tmp1);

                foreach (StockInfo tmp in loginMsg.StockList)
                    tmp2 += tmp.Id + " " + tmp.Stock + " " + tmp.Name + " " + tmp.StockType + " " + tmp.Price + " " + tmp.Num + "&";
                outputFile.WriteLine(tmp2);
            }
        }

        public void SendMail(string content)
        {
            try
            {
                LoginMsg loginMsg = ReadInit();
                string[] arr = content.Split("&");
                string bodyContent = @"您的申貸資料參考如下

信用參考額度(千元)    ：{0}
參考可貸款利率(%)     ：{1}
不動產評估(千元)      ：{2}
股票評估(千元)        ：{3}
參考可貸款總額度(千元)：{4}
借貸資金用途          ：{5}
還款來源              ：{6}
                ";

                MailMessage message = new MailMessage();
                message.From = new MailAddress("xxx@xxx.com.tw", "XXXX");
                message.To.Add(loginMsg.Mail);
                message.Subject = "敬愛的投資人您好，感謝您造訪XXXX金融公司，並完成貸款評估資料。";
                message.Body = string.Format(bodyContent, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6]);

                SmtpClient SmtpServer = new SmtpClient("xxx.xxx.xxx.xxx");
                SmtpServer.Port = 25;
                SmtpServer.EnableSsl = false;

                SmtpServer.Send(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("<======================");
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine("======================>");
            }
        }

        public List<string> GetIndexs()
        {
            List<string> IndexList = new List<string>() { "", "", "", "", "", "", "", "", "", "", "", "" };
            LoginMsg loginMsg = ReadInit();
            List<string> Index1List = loginMsg.Index1List;
            List<string> Index2List = loginMsg.Index2List;

            double i1Score = SumIndex1();
            double i2Score = SumIndex2();
            double sumScore = i1Score + i2Score;
            if (Index1List.Count() > 0 || Index2List.Count() > 0)
            {
                if (sumScore < 30)
                    IndexList[2] = "9     高度風險";
                else if (sumScore < 40)
                    IndexList[2] = "8     高度風險";
                else if (sumScore < 50)
                    IndexList[2] = "7     高度風險";
                else if (sumScore < 60)
                    IndexList[2] = "6     中度風險";
                else if (sumScore < 70)
                    IndexList[2] = "5     中度風險";
                else if (sumScore < 80)
                    IndexList[2] = "4     低度風險";
                else if (sumScore < 90)
                    IndexList[2] = "3     低度風險";
                else if (sumScore < 100)
                    IndexList[2] = "2     低度風險";
                else
                    IndexList[2] = "1     低度風險";
            }
            IndexList[0] = i1Score.ToString();
            IndexList[1] = i2Score.ToString();
            IndexList[3] = sumScore.ToString();

            if (Index1List.Count() > 1 && Index1List[22] != "") //淨值
            {
                int i = int.Parse(IndexList[0]);
                int equity = int.Parse(Index1List[22]);
                double val1, val2;
                if (i < 5)
                {
                    val1 = equity * 0.45;
                    val2 = equity * 0.55;
                }
                else if (i < 7)
                {
                    val1 = equity * 0.4;
                    val2 = equity * 0.5;
                }
                else
                {
                    val1 = equity * 0.2;
                    val2 = equity * 0.25;
                }

                //外規:單一客戶最大放款，無擔保淨值5%，足額擔保淨值12%，公司淨值24798175千元(2018/08/29)
                double maxCredit = 24798175 * 0.05;
                if (val2 > maxCredit)
                {
                    val1 = maxCredit;
                    val2 = maxCredit;
                }
                IndexList[4] = String.Format("{0:N}", val1) + " ~ " + String.Format("{0:N}", val2);
            }

            if (IndexList[2] != "")
            {
                int val = int.Parse(IndexList[2].Substring(0, 1));
                if (val == 1)
                    IndexList[6] = "0.4";
                else if (val == 2)
                    IndexList[6] = "0.5";
                else if (val == 3)
                    IndexList[6] = "0.55";
                else if (val == 4)
                    IndexList[6] = "0.6";
                else if (val == 5)
                    IndexList[6] = "0.7";
                else if (val == 6)
                    IndexList[6] = "0.85";
                else if (val == 7)
                    IndexList[6] = "1.1";
                else if (val == 8)
                    IndexList[6] = "1.4";
                else if (val == 9)
                    IndexList[6] = "1.8";
            }

            if (IndexList[6] != "")
            {
                using (SqlConnection sqlConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
                {
                    sqlConnection.Open();
                    string blr = sqlConnection.QuerySingleOrDefault<string>(@"SELECT TOP 1 r30 from blr order by d_date desc");
                    IndexList[5] = blr; //次級市場利率

                    double val3 = (double.Parse(blr) + double.Parse(IndexList[6]) + 0.1) * 0.9;
                    double val4 = (double.Parse(blr) + double.Parse(IndexList[6]) + 0.1) * 1.2;
                    IndexList[7] = String.Format("{0:N}", val3) + " ~ " + String.Format("{0:N}", val4);
                }
            }


            double sMortage = SumMortgageInfo();
            double sStock = SumStockInfo();
            IndexList[8] = String.Format("{0:N}", sMortage);
            IndexList[9] = String.Format("{0:N}", sStock);
            IndexList[10] = String.Format("{0:N}", sMortage + sStock);

            if (IndexList[4] != "")
            {
                string[] tmp = IndexList[4].Split(" ~ ");
                double maxCollateral = 24798175 * 0.12;
                double low = double.Parse(tmp[0]) + (sMortage + sStock);
                double up = double.Parse(tmp[1]) + (sMortage + sStock);
                if (up > maxCollateral)
                    IndexList[11] = String.Format("{0:N}", maxCollateral) + " ~ " + String.Format("{0:N}", maxCollateral);
                else
                    IndexList[11] = String.Format("{0:N}", low) + " ~ " + String.Format("{0:N}", up);
            }

            return IndexList;
        }



        // ***** 財務 *****
        public List<string> GetIndex1s()
        {
            LoginMsg loginMsg = ReadInit();
            if (loginMsg.Index1List.Count() < 2)
            {
                loginMsg.Index1List = new List<string>() { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
                                              "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
                                              "", "", "", "", "", "", "", "", "", "" };

                using (SqlConnection sqlConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
                {
                    sqlConnection.Open();
                    Object tmp = sqlConnection.QuerySingleOrDefault<Object>(@"SELECT guy_no, name, industry, date1, date2, date3, f3100c, f3100b, f3100a, fK002
                                                                                ,f3200, f3395, f3900, f0100, f0112, f0122, f0136, f0170, f0300, f1100, f1400, fK011, f2000
                                                                          FROM kbf3 WHERE guy_no = @aguy_no", new { aguy_no = loginMsg.Id });
                    if (tmp != null)
                    {
                        List<string> sl = tmp.ToString().Split(',').ToList();

                        int count = 0;
                        for (int i = 1; i < sl.Count(); i++)
                        {
                            if (sl[i].Contains("NULL"))
                            {
                                if (i > 1 && i < 7)
                                {
                                    loginMsg.Index1List[count] = "";
                                }
                                else
                                {
                                    loginMsg.Index1List[count] = "0";
                                }
                            }
                            else
                            {
                                string[] ss = sl[i].Split("'");
                                loginMsg.Index1List[count] = ss[1];
                            }
                            count++;
                        }
                    }
                    else
                    {
                        loginMsg.Index1List[0] = loginMsg.Id;
                        for (int i = 1; i < 23; i++)
                        {
                            loginMsg.Index1List[i] = "0";
                        }
                    }
                }
                WriteInit(loginMsg);
            }
            return loginMsg.Index1List;
        }

        public bool UpdateIndex1(string i1l)
        {
            bool flag = false;
            LoginMsg loginMsg = ReadInit();
            loginMsg.Index1List = i1l.Split("&").ToList();
            WriteInit(loginMsg);
            return flag;
        }

        public double SumIndex1()
        {
            int count = 0;
            double sum = 0;
            LoginMsg loginMsg = ReadInit();
            foreach (string tmp in loginMsg.Index1List)
            {
                if (tmp != "" && count > 22)
                {
                    sum += float.Parse(tmp);
                }
                count++;
            }
            return sum;
        }



        // ***** 經營 *****
        public List<string> GetIndex2s()
        {
            LoginMsg loginMsg = ReadInit();
            if (loginMsg.Index2List.Count() < 2)
            {
                loginMsg.Index2List = new List<string> { "3", "2", "10", "4", "2", "4", "3", "2", "2", "0", "0", "2", "3", "3",
                                                "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
                WriteInit(loginMsg);
            }
            return loginMsg.Index2List;
        }

        public bool UpdateIndex2(string i2l)
        {
            bool flag = false;
            LoginMsg loginMsg = ReadInit();
            loginMsg.Index2List = i2l.Split("&").ToList();
            WriteInit(loginMsg);
            return flag;
        }

        public double SumIndex2()
        {
            double sum = 0;
            LoginMsg loginMsg = ReadInit();
            foreach (string tmp in loginMsg.Index2List)
            {
                if (tmp != "")
                {
                    sum += float.Parse(tmp);
                }
            }
            return sum;
        }

        

        // ***** 股票 *****
        public List<StockInfo> GetStockInfos()
        {
            LoginMsg loginMsg = ReadInit();
            return loginMsg.StockList;
        }

        public StockInfo GetStockInfoById(string id)
        {
            LoginMsg loginMsg = ReadInit();
            foreach (StockInfo tmp in loginMsg.StockList)
            {
                if (tmp.Id == id)
                {
                    return tmp;
                }
            }
            return new StockInfo();
        }

        public async Task<bool> AddStockInfo(StockInfo s, string StockType)
        {
            LoginMsg loginMsg = ReadInit();

            s.Id = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            string url = string.Format("http://mis.twse.com.tw/stock/api/getStockInfo.jsp?ex_ch={0}_{1}.tw&json=1&delay=0&_={2}", StockType, s.Stock, s.Id);

            // Create a New HttpClient object and dispose it when done, so the app doesn't leak resources
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Above three lines can be replaced with new helper method below
                    // string responseBody = await client.GetStringAsync(uri);

                    //System.Diagnostics.Debug.WriteLine("<======================");
                    //System.Diagnostics.Debug.WriteLine(responseBody);
                    //System.Diagnostics.Debug.WriteLine("======================>");

                    //if (response != null)
                    //{
                    //    return JObject.Parse(responseBody);
                    //}

                    bool flag = false;
                    JObject jsonData = JObject.Parse(responseBody);
                    JArray items = (JArray)jsonData["msgArray"];
                    int length = items.Count;
                    if (length < 1)
                    {
                        return flag;
                    }
                    else
                    {
                        if (StockType == "tse")
                            s.StockType = "上市";
                        else if (StockType == "otc")
                            s.StockType = "上櫃";

                        s.Name = (string)jsonData["msgArray"][0]["n"];
                        string currentPrice = (string)jsonData["msgArray"][0]["z"]; //當盤成交價
                        string yesterdayPrice = (string)jsonData["msgArray"][0]["y"]; //昨天收盤價
                        if (currentPrice != "-")
                            s.Price = currentPrice;
                        else
                            s.Price = yesterdayPrice;

                        loginMsg.StockList.Add(s);
                        WriteInit(loginMsg);

                        flag = true;

                        return flag;
                    }
                }
                catch (HttpRequestException e)
                {
                    System.Diagnostics.Debug.WriteLine("<======================");
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    System.Diagnostics.Debug.WriteLine("======================>");
                    return false;
                }
            }
        }

        public async Task<bool> UpdateStockInfo(StockInfo s)
        {
            bool flag = false;
            LoginMsg loginMsg = ReadInit();
            foreach (StockInfo tmp in loginMsg.StockList)
            {
                if (tmp.Id == s.Id)
                {
                    tmp.Num = s.Num;
                    flag = true;
                    await Task.Delay(100);
                    break;
                }
            }
            WriteInit(loginMsg);
            return flag;
        }

        public bool DeleteStockInfo(string id)
        {
            try
            {
                bool flag = false;
                LoginMsg loginMsg = ReadInit();
                foreach (StockInfo tmp in loginMsg.StockList)
                {
                    if (tmp.Id == id)
                    {
                        loginMsg.StockList.Remove(tmp);
                        flag = true;
                        break;
                    }
                }
                WriteInit(loginMsg);
                return flag;
            }
            catch
            {
                return false;
            }
        }

        public double SumStockInfo()
        {
            double sum = 0;
            LoginMsg loginMsg = ReadInit();
            foreach (StockInfo tmp in loginMsg.StockList)
            {
                sum += float.Parse(tmp.Price) * float.Parse(tmp.Num) * 0.6;
            }
            return sum;
        }



        // ***** 不動產抵押 *****
        public List<MortgageInfo> GetMortgageInfos()
        {
            LoginMsg loginMsg = ReadInit();
            return loginMsg.MortgageList;
        }

        public MortgageInfo GetMortgageInfoById(string id)
        {
            LoginMsg loginMsg = ReadInit();
            foreach (MortgageInfo tmp in loginMsg.MortgageList)
            {
                if (tmp.Id == id)
                {
                    return tmp;
                }
            }
            return new MortgageInfo();
        }

        public async Task<bool> AddMortgageInfo(MortgageInfo m)
        {
            LoginMsg loginMsg = ReadInit();
            using (SqlConnection sqlConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
            {
                await sqlConnection.OpenAsync();
                MortgageInfo tmp = await sqlConnection.QuerySingleOrDefaultAsync<MortgageInfo>("SELECT * FROM mortgage WHERE city = @city and area = @area", new { city = m.City, area = m.Area });

                tmp.Id = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
                tmp.Num = m.Num;

                loginMsg.MortgageList.Add(tmp);
                WriteInit(loginMsg);

                if (tmp.Price != null)
                    return true;
                else
                    return false;
            }
        }

        public async Task<bool> UpdateMortgageInfo(MortgageInfo m)
        {
            bool flag = false;
            LoginMsg loginMsg = ReadInit();
            foreach (MortgageInfo tmp in loginMsg.MortgageList)
            {
                if (tmp.Id == m.Id)
                {
                    tmp.Price = m.Price;
                    tmp.Num = m.Num;
                    flag = true;
                    await Task.Delay(100);
                    break;
                }
            }
            WriteInit(loginMsg);

            return flag;
        }

        public bool DeleteMortgageInfo(string id)
        {
            try
            {
                bool flag = false;
                LoginMsg loginMsg = ReadInit();
                foreach (MortgageInfo tmp in loginMsg.MortgageList)
                {
                    if (tmp.Id == id)
                    {
                        loginMsg.MortgageList.Remove(tmp);
                        flag = true;
                        break;
                    }
                }
                WriteInit(loginMsg);

                return flag;
            }
            catch
            {
                return false;
            }
        }

        public double SumMortgageInfo()
        {
            var type = new Dictionary<string, double>()
            {
                { "A+", 0.9 },
                { "A", 0.8 },
                { "B", 0.7 },
                { "C", 0.6 },
                { "D", 0.5 }
            };

            double sum = 0;
            LoginMsg loginMsg = ReadInit();
            foreach (MortgageInfo tmp in loginMsg.MortgageList)
            {
                if (tmp.Price != null)
                {
                    sum += float.Parse(tmp.Price) * float.Parse(tmp.Num) * type[tmp.Type];
                }
            }
            return sum;
        }
    }
}
