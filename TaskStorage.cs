using MySqlConnector;
using System;
using System.Collections.Generic;

namespace CybersecurityBot
{
    public sealed class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? ReminderAt { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TaskStorage
    {
        // Set this connection string to your local MySQL credentials/database.
        private readonly string _connectionString =
            "Server=localhost;Database=cybersecuritybot;User ID=root;Password=YOUR_PASSWORD;";

        public void Initialize()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var cmd = new MySqlCommand(@"CREATE TABLE IF NOT EXISTS cyber_tasks (
                id INT AUTO_INCREMENT PRIMARY KEY,
                title VARCHAR(200) NOT NULL,
                description TEXT NOT NULL,
                reminder_at DATETIME NULL,
                is_completed BOOLEAN NOT NULL DEFAULT FALSE,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )", connection);
            cmd.ExecuteNonQuery();
        }

        public List<CyberTask> GetAll()
        {
            var tasks = new List<CyberTask>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT id,title,description,reminder_at,is_completed FROM cyber_tasks ORDER BY is_completed, reminder_at, id DESC", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new CyberTask {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Description = reader.GetString("description"),
                    ReminderAt = reader.IsDBNull(reader.GetOrdinal("reminder_at")) ? null : reader.GetDateTime("reminder_at"),
                    IsCompleted = reader.GetBoolean("is_completed")
                });
            }
            return tasks;
        }

        public void Add(string title, string description, DateTime? reminder)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var cmd = new MySqlCommand("INSERT INTO cyber_tasks(title,description,reminder_at) VALUES(@title,@description,@reminder)", connection);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@reminder", (object?)reminder ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SetCompleted(int id, bool completed)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var cmd = new MySqlCommand("UPDATE cyber_tasks SET is_completed=@done WHERE id=@id", connection);
            cmd.Parameters.AddWithValue("@done", completed);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var cmd = new MySqlCommand("DELETE FROM cyber_tasks WHERE id=@id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
