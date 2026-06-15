using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;

namespace KURAOrderSystem.Database
{
    public class DatabaseManager
    {
        private const string MasterConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=8;";
        private const string AppConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=KuraOrderDb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=8;";
        
        private readonly string _mdfPath;
        private readonly string _ldfPath;

        public bool IsSqlAvailable { get; private set; } = false;

        public DatabaseManager()
        {
            // Define absolute paths for Database.mdf and Database_log.ldf in the project's Database directory
            string baseDir = "c:\\Users\\tanimoto\\Desktop\\KURAOrderSystem\\KURAOrderSystem\\Database";
            _mdfPath = Path.Combine(baseDir, "Database.mdf");
            _ldfPath = Path.Combine(baseDir, "Database_log.ldf");
        }

        public bool InitializeDatabase()
        {
            try
            {
                // Ensure the directory exists
                string? dir = Path.GetDirectoryName(_mdfPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (var connection = new SqlConnection(MasterConnectionString))
                {
                    connection.Open();

                    // Step 1: Check if KuraOrderDb exists in sys.databases
                    bool dbExists = false;
                    string checkDbQuery = "SELECT database_id FROM sys.databases WHERE name = 'KuraOrderDb'";
                    using (var command = new SqlCommand(checkDbQuery, connection))
                    {
                        dbExists = command.ExecuteScalar() != null;
                    }

                    // Step 2: Check if physical MDF file exists
                    bool fileExists = File.Exists(_mdfPath);

                    if (dbExists)
                    {
                        // If DB exists in sys.databases, verify if it points to our physical file.
                        // If it doesn't (e.g. from the previous run without MDF file), drop it so we can create it tied to the physical MDF.
                        string getFilePathQuery = "SELECT physical_name FROM sys.master_files WHERE database_id = DB_ID('KuraOrderDb') AND file_id = 1";
                        string? currentPhysicalPath = null;
                        using (var command = new SqlCommand(getFilePathQuery, connection))
                        {
                            currentPhysicalPath = command.ExecuteScalar() as string;
                        }

                        if (currentPhysicalPath == null || !string.Equals(Path.GetFullPath(currentPhysicalPath), Path.GetFullPath(_mdfPath), StringComparison.OrdinalIgnoreCase))
                        {
                            // Drop the existing DB first to re-link to the local MDF file
                            string dropDbQuery = @"
                                ALTER DATABASE KuraOrderDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                DROP DATABASE KuraOrderDb;";
                            using (var command = new SqlCommand(dropDbQuery, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                            dbExists = false;
                        }
                    }

                    // Step 3: Create or Attach Database
                    if (!dbExists)
                    {
                        if (fileExists)
                        {
                            // Physical file exists, attach it
                            string attachQuery = string.Format(
                                @"CREATE DATABASE [KuraOrderDb] ON (FILENAME = '{0}') FOR ATTACH", _mdfPath);
                            using (var command = new SqlCommand(attachQuery, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Create new database with physical files
                            string createQuery = string.Format(@"
                                CREATE DATABASE [KuraOrderDb] ON PRIMARY 
                                (
                                    NAME = KuraOrderDb_Data, 
                                    FILENAME = '{0}', 
                                    SIZE = 8MB, 
                                    MAXSIZE = UNLIMITED, 
                                    FILEGROWTH = 10%
                                ) 
                                LOG ON 
                                (
                                    NAME = KuraOrderDb_Log, 
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

                // Step 4: Create tables in the attached database
                using (var connection = new SqlConnection(AppConnectionString))
                {
                    connection.Open();

                    // Create Orders table
                    string createOrdersTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
                        BEGIN
                            CREATE TABLE Orders (
                                OrderId INT IDENTITY(1,1) PRIMARY KEY,
                                OrderTime DATETIME NOT NULL DEFAULT GETDATE(),
                                TotalPrice DECIMAL(10,2) NOT NULL,
                                PlateCount INT NOT NULL
                            )
                        END";
                    using (var cmd = new SqlCommand(createOrdersTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create OrderDetails table
                    string createOrderDetailsTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderDetails')
                        BEGIN
                            CREATE TABLE OrderDetails (
                                DetailId INT IDENTITY(1,1) PRIMARY KEY,
                                OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderId) ON DELETE CASCADE,
                                SushiName NVARCHAR(100) NOT NULL,
                                SushiNameEn NVARCHAR(100) NOT NULL,
                                Price INT NOT NULL,
                                Quantity INT NOT NULL,
                                Status NVARCHAR(50) NOT NULL,
                                OrderTime DATETIME NOT NULL DEFAULT GETDATE()
                            )
                        END";
                    using (var cmd = new SqlCommand(createOrderDetailsTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create BikkuraPonResults table
                    string createBikkuraPonTable = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BikkuraPonResults')
                        BEGIN
                            CREATE TABLE BikkuraPonResults (
                                ResultId INT IDENTITY(1,1) PRIMARY KEY,
                                PlayTime DATETIME NOT NULL DEFAULT GETDATE(),
                                PlateMilestone INT NOT NULL,
                                IsWin BIT NOT NULL,
                                PrizeName NVARCHAR(100) NOT NULL
                            )
                        END";
                    using (var cmd = new SqlCommand(createBikkuraPonTable, connection))
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

        public bool SaveOrder(decimal totalAmount, int totalPlates, List<Models.OrderItem> items, out int orderId)
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
                            string insertOrder = "INSERT INTO Orders (TotalPrice, PlateCount) OUTPUT INSERTED.OrderId VALUES (@TotalPrice, @PlateCount)";
                            using (var cmd = new SqlCommand(insertOrder, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TotalPrice", totalAmount);
                                cmd.Parameters.AddWithValue("@PlateCount", totalPlates);
                                orderId = (int)cmd.ExecuteScalar();
                            }

                            // Insert Details
                            string insertDetail = @"
                                INSERT INTO OrderDetails (OrderId, SushiName, SushiNameEn, Price, Quantity, Status, OrderTime) 
                                VALUES (@OrderId, @SushiName, @SushiNameEn, @Price, @Quantity, @Status, @OrderTime)";
                            
                            foreach (var item in items)
                            {
                                using (var cmd = new SqlCommand(insertDetail, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                                    cmd.Parameters.AddWithValue("@SushiName", item.Item.Name);
                                    cmd.Parameters.AddWithValue("@SushiNameEn", item.Item.NameEn);
                                    cmd.Parameters.AddWithValue("@Price", item.Item.Price);
                                    cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    cmd.Parameters.AddWithValue("@Status", item.Status);
                                    cmd.Parameters.AddWithValue("@OrderTime", item.OrderTime);
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

        public bool SaveBikkuraPonResult(int milestone, bool isWin, string prizeName)
        {
            if (!IsSqlAvailable) return false;

            try
            {
                using (var connection = new SqlConnection(AppConnectionString))
                {
                    connection.Open();
                    string insertResult = "INSERT INTO BikkuraPonResults (PlateMilestone, IsWin, PrizeName) VALUES (@Milestone, @IsWin, @PrizeName)";
                    using (var cmd = new SqlCommand(insertResult, connection))
                    {
                        cmd.Parameters.AddWithValue("@Milestone", milestone);
                        cmd.Parameters.AddWithValue("@IsWin", isWin);
                        cmd.Parameters.AddWithValue("@PrizeName", prizeName ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save Bikkura Pon result: {ex.Message}");
                return false;
            }
        }

        public bool ClearAllOrders()
        {
            if (!IsSqlAvailable) return false;

            try
            {
                using (var connection = new SqlConnection(AppConnectionString))
                {
                    connection.Open();
                    // OrderDetails ÇÕ Orders Ç… CASCADE DELETE Ç™ê›íËÇ≥ÇÍÇƒÇ¢ÇÈÇΩÇﬂÅA
                    // Orders ÇçÌèúÇ∑ÇÍÇŒ OrderDetails Ç‡é©ìÆìIÇ…çÌèúÇ≥ÇÍÇÈ
                    string deleteOrders = "DELETE FROM Orders";
                    using (var cmd = new SqlCommand(deleteOrders, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear orders from DB: {ex.Message}");
                return false;
            }
        }

        public List<Models.OrderItem> GetOrderHistory()
        {
            var history = new List<Models.OrderItem>();
            if (!IsSqlAvailable) return history;

            try
            {
                using (var connection = new SqlConnection(AppConnectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT od.SushiName, od.SushiNameEn, od.Price, od.Quantity, od.Status, od.OrderTime 
                        FROM OrderDetails od
                        INNER JOIN Orders o ON od.OrderId = o.OrderId
                        ORDER BY od.OrderTime DESC";
                    
                    using (var cmd = new SqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new Models.SushiItem
                            {
                                Name = reader.GetString(0),
                                NameEn = reader.GetString(1),
                                Price = reader.GetInt32(2)
                            };

                            var orderItem = new Models.OrderItem(item, reader.GetInt32(3))
                            {
                                Status = reader.GetString(4),
                                OrderTime = reader.GetDateTime(5)
                            };

                            history.Add(orderItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to retrieve order history from DB: {ex.Message}");
            }

            return history;
        }
    }
}
