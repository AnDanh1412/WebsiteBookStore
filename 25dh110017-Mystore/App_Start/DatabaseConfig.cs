using System;
using System.IO;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace _25dh110017_Mystore
{
    public static class DatabaseConfig
    {
        public static void Initialize()
        {
            try
            {
                // Danh sách các máy chủ SQL có thể có trên máy tính của sinh viên/thầy cô
                string[] serverCandidates = new string[]
                {
                    @"(LocalDB)\MSSQLLocalDB",
                    @".",
                    @".\SQLEXPRESS"
                };

                foreach (var server in serverCandidates)
                {
                    try
                    {
                        using (var masterConn = new SqlConnection($"Server={server};Integrated Security=True;Connect Timeout=3;"))
                        {
                            masterConn.Open();

                            // Kiểm tra database MyStore đã tồn tại trên máy chủ này chưa
                            bool exists = false;
                            using (var checkCmd = new SqlCommand("SELECT database_id FROM sys.databases WHERE name = N'MyStore'", masterConn))
                            {
                                var res = checkCmd.ExecuteScalar();
                                exists = (res != null && res != DBNull.Value);
                            }

                            // Nếu chưa có, tự động đọc file MyStore_Database.sql và chạy ngầm
                            if (!exists)
                            {
                                string sqlFilePath = HostingEnvironment.MapPath("~/App_Data/MyStore_Database.sql");
                                if (string.IsNullOrEmpty(sqlFilePath) || !File.Exists(sqlFilePath))
                                {
                                    sqlFilePath = HostingEnvironment.MapPath("~/MyStore_Database.sql");
                                }
                                if (string.IsNullOrEmpty(sqlFilePath) || !File.Exists(sqlFilePath))
                                {
                                    sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "MyStore_Database.sql");
                                }

                                if (File.Exists(sqlFilePath))
                                {
                                    string script = File.ReadAllText(sqlFilePath);
                                    string[] batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                                    foreach (var batch in batches)
                                    {
                                        string sql = batch.Trim();
                                        if (string.IsNullOrEmpty(sql)) continue;

                                        using (var cmd = new SqlCommand(sql, masterConn))
                                        {
                                            cmd.CommandTimeout = 60;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Thử máy chủ tiếp theo nếu máy chủ này không hoạt động
                    }
                }
            }
            catch
            {
                // Bỏ qua ngoại lệ ngoài ý muốn để không làm ảnh hưởng khởi động web
            }
        }
    }
}

