using Microsoft.Data.Sqlite;
using System;

try
{
    string dbPath = @"c:\Users\Muhammad Aqib\Desktop\ISP\C-Project-ISP_Management\Semester_Project\Semester_Project\app.db";
    using (var connection = new SqliteConnection($"Data Source={dbPath}"))
    {
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "ALTER TABLE PaymentVerifications ADD COLUMN ReceiptPath TEXT;";
        command.ExecuteNonQuery();
        Console.WriteLine("Successfully added ReceiptPath column to PaymentVerifications table.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
}
