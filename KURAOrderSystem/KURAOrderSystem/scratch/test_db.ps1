$connString = "Server=(localdb)\MSSQLLocalDB;Database=KuraOrderDb;Integrated Security=True;Connection Timeout=8;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
try {
    $conn.Open()
    $transaction = $conn.BeginTransaction()
    try {
        $cmd = $conn.CreateCommand()
        $cmd.Transaction = $transaction
        $cmd.CommandText = "INSERT INTO Orders (TotalPrice, PlateCount) OUTPUT INSERTED.OrderId VALUES (@TotalPrice, @PlateCount)"
        $cmd.Parameters.AddWithValue("@TotalPrice", 115.0) | Out-Null
        $cmd.Parameters.AddWithValue("@PlateCount", 1) | Out-Null
        $orderId = $cmd.ExecuteScalar()
        Write-Host "Inserted OrderId: $orderId"

        $cmd2 = $conn.CreateCommand()
        $cmd2.Transaction = $transaction
        $cmd2.CommandText = "INSERT INTO OrderDetails (OrderId, SushiName, SushiNameEn, Price, Quantity, Status) VALUES (@OrderId, @SushiName, @SushiNameEn, @Price, @Quantity, @Status)"
        $cmd2.Parameters.AddWithValue("@OrderId", $orderId) | Out-Null
        $cmd2.Parameters.AddWithValue("@SushiName", "まぐろ") | Out-Null
        $cmd2.Parameters.AddWithValue("@SushiNameEn", "Tuna") | Out-Null
        $cmd2.Parameters.AddWithValue("@Price", 115) | Out-Null
        $cmd2.Parameters.AddWithValue("@Quantity", 1) | Out-Null
        $cmd2.Parameters.AddWithValue("@Status", "注文済") | Out-Null
        $cmd2.ExecuteNonQuery() | Out-Null
        Write-Host "Inserted Detail!"

        $transaction.Commit()
        Write-Host "Transaction committed!"
    } catch {
        $transaction.Rollback()
        $ex = $_.Exception
        Write-Host "Failed inside transaction: $ex"
    }
    $conn.Close()
} catch {
    $ex = $_.Exception
    Write-Host "Failed to connect/open: $ex"
}
