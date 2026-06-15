using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;
using TachinomiOrderSystem.Models;

namespace TachinomiOrderSystem.Database
{
    public class DatabaseManager
    {
        private const string MasterConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=8;";
        private const string AppConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=TachinomiOrderDb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=8;";
        
        private readonly string _mdfPath;
        private readonly string _ldfPath;

        public bool IsSqlAvailable { get; private set; } = false;

        public DatabaseManager()
        {
            string baseDir = "C:\\Users\\tanimoto\\Desktop\\TachinomiOrderSystem\\TachinomiOrderSystem\\Database";
            _mdfPath = Path.Combine(baseDir, "TachinomiDatabase.mdf");
            _ldfPath = Path.Combine(baseDir, "TachinomiDatabase_log.ldf");
        }

        public bool InitializeDatabase()
        {
            try
            {
                string? dir = Path.GetDirectoryName(_mdfPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (var connection = new SqlConnection(MasterConnectionString))
                {
                    connection.Open();

                    bool dbExists = false;
                    string checkDbQuery = "SELECT database_id FROM sys.databases WHERE name = 'TachinomiOrderDb'";
                    using (var command = new SqlCommand(checkDbQuery, connection))
                    {
                        dbExists = command.ExecuteScalar() != null;
                    }

                    bool fileExists = File.Exists(_mdfPath);

                    if (dbExists)
                    {
                        string getFilePathQuery = "SELECT physical_name FROM sys.master_files WHERE database_id = DB_ID('TachinomiOrderDb') AND file_id = 1";
                        string? currentPhysicalPath = null;
                        using (var command = new SqlCommand(getFilePathQuery, connection))
                        {
                            currentPhysicalPath = command.ExecuteScalar() as string;
                        }

                        if (currentPhysicalPath == null || !string.Equals(Path.GetFullPath(currentPhysicalPath), Path.GetFullPath(_mdfPath), StringComparison.OrdinalIgnoreCase))
                        {
                            string dropDbQuery = @"
                                ALTER DATABASE TachinomiOrderDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                DROP DATABASE TachinomiOrderDb;";
                            using (var command = new SqlCommand(dropDbQuery, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                            dbExists = false;
                        }
                    }

                    if (!dbExists)
                    {
                        if (fileExists)
                        {
                            string attachQuery = string.Format(
                                @"CREATE DATABASE [TachinomiOrderDb] ON (FILENAME = '{0}') FOR ATTACH", _mdfPath);
                            using (var command = new SqlCommand(attachQuery, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string createQuery = string.Format(@"
                                CREATE DATABASE [TachinomiOrderDb] ON PRIMARY 
                                (
                                    NAME = TachinomiOrderDb_Data, 
                                    FILENAME = '{0}', 
                                    SIZE = 8MB, 
                                    MAXSIZE = UNLIMITED, 
                                    FILEGROWTH = 10%
                                ) 
                                LOG ON 
                                (
                                    NAME = TachinomiOrderDb_Log, 
                                    FILENAME = '{1}', 
                                    SIZE = 8MB, 
                                    MAXSIZE = 2GB, 
                                    FILEGROWTH = 10%
                                )", _mdfPath, _ldfPath);
                            
                            using (var command = new SqlCommand(createQuery, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }

                using (var connection = new SqlConnection(AppConnectionString))
                {
                    connection.Open();

                    // Create MenuItems table
                    string createMenuTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MenuItems')
                        BEGIN
                            CREATE TABLE MenuItems (
                                ItemId INT PRIMARY KEY,
                                ItemName NVARCHAR(100) NOT NULL,
                                Price INT NOT NULL
                            );
                            
                            INSERT INTO MenuItems (ItemId, ItemName, Price) VALUES (1, N'生ビール', 500);
                            INSERT INTO MenuItems (ItemId, ItemName, Price) VALUES (2, N'枝豆', 300);
                        END";
                    using (var cmd = new SqlCommand(createMenuTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create TachinomiOrder table
                    string createOrdersTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TachinomiOrder')
                        BEGIN
                            CREATE TABLE TachinomiOrder (
                                OrderId INT IDENTITY(1,1) PRIMARY KEY,
                                OrderTime DATETIME NOT NULL DEFAULT GETDATE(),
                                TotalPrice INT NOT NULL
                            )
                        END";
                    using (var cmd = new SqlCommand(createOrdersTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create TachinomiOrderDetails table
                    string createOrderDetailsTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TachinomiOrderDetails')
                        BEGIN
                            CREATE TABLE TachinomiOrderDetails (
                                DetailId INT IDENTITY(1,1) PRIMARY KEY,
                                OrderId INT NOT NULL FOREIGN KEY REFERENCES TachinomiOrder(OrderId) ON DELETE CASCADE,
                                ItemName NVARCHAR(100) NOT NULL,
                                Price INT NOT NULL,
                                Quantity INT NOT NULL
                            )
                        END";
                    using (var cmd = new SqlCommand(createOrderDetailsTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                IsSqlAvailable = true;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Database Init failed: {ex.Message}");
                IsSqlAvailable = false;
                return false;
            }
        }

        public List<MenuItem> GetMenuItems()
        {
            var list = new List<MenuItem>();
            if (!IsSqlAvailable)
            {
                // Fallback defaults
                return new List<MenuItem>
                {
                    new MenuItem { Id = 1, Name = "生ビール", Price = 500 },
                    new MenuItem { Id = 2, Name = "枝豆", Price = 300 }
                };
            }

            try
            {
                using (var connection = new SqlConnection(AppConnectionString))
                {
                    connection.Open();
                    string query = "SELECT ItemId, ItemName, Price FROM MenuItems ORDER BY ItemId";
                    using (var cmd = new SqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new MenuItem
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load menu items: {ex.Message}");
            }
            return list;
        }

        public bool SaveTachinomiOrder(int totalAmount, List<OrderItem> items, out int orderId)
        {
            orderId = -1;
            if (!IsSqlAvailable) return false;

            try
            {
                using (var connection = new SqlConnection(AppConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Insert Order
                            string insertOrder = "INSERT INTO TachinomiOrder (TotalPrice) OUTPUT INSERTED.OrderId VALUES (@TotalPrice)";
                            using (var cmd = new SqlCommand(insertOrder, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TotalPrice", totalAmount);
                                orderId = (int)cmd.ExecuteScalar();
                            }

                            // Insert Details
                            string insertDetail = @"
                                INSERT INTO TachinomiOrderDetails (OrderId, ItemName, Price, Quantity) 
                                VALUES (@OrderId, @ItemName, @Price, @Quantity)";
                            
                            foreach (var item in items)
                            {
                                using (var cmd = new SqlCommand(insertDetail, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                                    cmd.Parameters.AddWithValue("@ItemName", item.Item.Name);
                                    cmd.Parameters.AddWithValue("@Price", item.Item.Price);
                                    cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save order to DB: {ex.Message}");
                return false;
            }
        }

        public List<TachinomiOrderHistoryDto> GetCheckoutHistory()
        {
            var history = new List<TachinomiOrderHistoryDto>();
            if (!IsSqlAvailable) return history;

            try
            {
                using (var connection = new SqlConnection(AppConnectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT o.OrderId, o.OrderTime, o.TotalPrice, od.ItemName, od.Price, od.Quantity
                        FROM TachinomiOrderDetails od
                        INNER JOIN TachinomiOrder o ON od.OrderId = o.OrderId
                        ORDER BY o.OrderTime DESC, od.DetailId ASC";
                    
                    using (var cmd = new SqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        TachinomiOrderHistoryDto? currentOrder = null;
                        while (reader.Read())
                        {
                            int orderId = reader.GetInt32(0);
                            if (currentOrder == null || currentOrder.OrderId != orderId)
                            {
                                currentOrder = new TachinomiOrderHistoryDto
                                {
                                    OrderId = orderId,
                                    OrderTime = reader.GetDateTime(1),
                                    TotalPrice = reader.GetInt32(2),
                                    Details = new List<TachinomiOrderDetailDto>()
                                };
                                history.Add(currentOrder);
                            }

                            currentOrder.Details.Add(new TachinomiOrderDetailDto
                            {
                                ItemName = reader.GetString(3),
                                Price = reader.GetInt32(4),
                                Quantity = reader.GetInt32(5)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to retrieve checkout history: {ex.Message}");
            }

            return history;
        }

        public bool ClearAllHistory()
        {
            if (!IsSqlAvailable) return false;

            try
            {
                using (var connection = new SqlConnection(AppConnectionString))
                {
                    connection.Open();
                    string deleteOrders = "DELETE FROM TachinomiOrder";
                    using (var cmd = new SqlCommand(deleteOrders, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear history from DB: {ex.Message}");
                return false;
            }
        }
    }

    public class TachinomiOrderHistoryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderTime { get; set; }
        public int TotalPrice { get; set; }
        public List<TachinomiOrderDetailDto> Details { get; set; } = new();
        public string FormattedOrderTime => OrderTime.ToString("yyyy/MM/dd HH:mm:ss");
        public string FormattedTotalPrice => $"{TotalPrice}円";
        public string SummaryText => string.Join(", ", Details.ConvertAll(d => $"{d.ItemName}x{d.Quantity}"));
    }

    public class TachinomiOrderDetailDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int Price { get; set; }
        public int Quantity { get; set; }
    }
}
