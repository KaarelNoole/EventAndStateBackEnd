using System;
using System.Data.SqlClient;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        // Replace these placeholders with your actual SQL Server details
        string connectionString = "Data Source=10.100.80.67;Initial Catalog=minubaas;User ID=minunimi;Password=test;";

        // Replace with your specific event log details
        string eventLogName = "Application";

        // SQL query to insert a record into the 'EventLogTable' table
        string insertQuery = "INSERT INTO EventCamera (EventTime, Source, Event) VALUES (@EventTime, @Source, @Event)";

        // Create a new EventLog instance
        EventLog eventLog = new EventLog(eventLogName);

        // Iterate through the entries in the event log
        foreach (EventLogEntry entry in eventLog.Entries)
        {
            // Sample data for the new record
            DateTime eventTime = entry.TimeGenerated;
            string source = entry.Source;
            string eventText = entry.Message;

            // Create a SqlConnection object and open the connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create a SqlCommand object with the insert query and connection
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    // Add parameters to the query
                    command.Parameters.AddWithValue("@EventTime", eventTime);
                    command.Parameters.AddWithValue("@Source", source);
                    command.Parameters.AddWithValue("@Event", eventText);

                    try
                    {
                        // Execute the query
                        int rowsAffected = command.ExecuteNonQuery();

                        // Check if the query was successful
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Record inserted successfully.");
                        }
                        else
                        {
                            Console.WriteLine("No records inserted.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }
        }

        Console.ReadLine(); // Pause to see the output
    }
}
