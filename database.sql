CREATE DATABASE IF NOT EXISTS cybersecuritybot;
USE cybersecuritybot;

CREATE TABLE IF NOT EXISTS cyber_tasks (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    reminder_at DATETIME NULL,
    is_completed BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
