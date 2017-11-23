using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UpdateNotifyInternal
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("=                                                               =");
            Console.WriteLine("=                     超期提醒修改程序                          =");
            Console.WriteLine("=                                                               =");
            Console.WriteLine("=================================================================");

            Console.WriteLine("请确认已经配置好数据库设置，配置名【SQLConn】。");
            Console.WriteLine("如果没有配置，请配置完成后重新启动程序。");

            Console.WriteLine("按任意键开始");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine("超期提醒邮件发送频率：");
            Console.WriteLine("[1] 每天");
            Console.WriteLine("[2] 每小时");
            Console.WriteLine("[3] 每周一");
            Console.WriteLine("[4] 每周一、三、五");
            Console.WriteLine();
            Console.WriteLine("请选择超期提醒邮件发送频率：");
            int interval = 0;

            L001:
            while (true)
            {
                string key = Console.ReadLine();
                if (!int.TryParse(key, out interval) || interval > 4)
                {
                    Console.WriteLine("输入错误，请重新输入");
                }
                else
                {
                    break;
                }
            }
            Console.WriteLine($"选择的是：{interval},是否确认？[Y]/[N]");
            if (string.Compare(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase) != 0)
            {
                Console.WriteLine("请选择超期提醒邮件发送频率：");
                goto L001;
            }
            Console.WriteLine("开始执行。。。。。。。");
            Console.WriteLine("正在查询数据。。。。。。。");
            var table = GetAllData();
            Console.WriteLine($"共{table.Rows.Count.ToString()}条数据");
            Console.WriteLine("正在组织数据。。。。。。");

            foreach (DataRow dataRow in table.Rows)
            {
                try
                {
                    Dictionary<string, object> dataItem = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataRow.Field<string>("OVERREMIND"));
                    if (!dataItem.ContainsKey("CYCLE"))
                    {
                        continue;
                    }
                    dataItem["CYCLE"] = interval;
                    dataRow.SetField("OVERREMIND", JsonConvert.SerializeObject(dataItem));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("正在组织数据完成。。。。。。");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("更新数据开始。。。。");
            Console.WriteLine("【注意】【WARNING】这个阶段可能执行很长时间，请不要关闭程序，避免数据混乱");
            Console.ResetColor();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                foreach (DataRow dataRow in table.Rows)
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    using (SqlCommand cmd = new SqlCommand("UPDATE WFTASK SET OVERREMIND=@O WHERE ID=@D", connection))
                    {
                        cmd.Parameters.AddRange(new[]
                        {
                            new SqlParameter("@O", SqlDbType.VarChar) {Value = dataRow.Field<string>("OVERREMIND")},
                            new SqlParameter("@D", SqlDbType.VarChar) {Value = dataRow.Field<string>("ID")}
                        });
                        cmd.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
            Console.WriteLine("更新结束。。。。，按任意键退出");
            Console.ReadLine();
        }


        private static DataTable GetAllData()
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ID,OVERREMIND FROM dbo.WFTASK WHERE OVERREMIND IS NOT NULL", connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }
                connection.Close();
            }
            return ds.Tables[0];
        }
    }
}
