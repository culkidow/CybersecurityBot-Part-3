# WEchat Cybersecurity Assistant — Part 3

This is a Windows Forms GUI project extending the Part 2 chatbot. Existing chatbot engine, memory manager, sentiment analyzer, and audio greeting remain included.

## Features added
- Task assistant: create cybersecurity tasks with description and optional date/time reminder; list, mark completed, and delete tasks.
- MySQL persistence using MySqlConnector.
- Cybersecurity quiz with 12 multiple-choice / true-false questions, feedback after every answer, and final score.
- Basic NLP simulation using keyword and phrase matching in chat: task requests, show tasks, start quiz, and activity log requests.
- Activity log records task operations, quiz events, application start, and NLP/chat interactions. The activity tab displays the latest 10 actions.
- Windows Forms tabbed GUI.

## Requirements
- Windows and Visual Studio with .NET 10 SDK (Windows Forms workload)
- MySQL Server

## Configure MySQL
1. Start MySQL Server.
2. Create the database:
   ```sql
   CREATE DATABASE cybersecuritybot;
   ```
3. Open `TaskStorage.cs` and replace `YOUR_PASSWORD` in the connection string with your MySQL root password (or preferably use a dedicated database account).
4. Run the project. The app creates the `cyber_tasks` table automatically on startup.

A reference SQL schema is supplied in `database.sql`.

## Run
Open `CybersecurityBot.slnx` or `CybersecurityBot.csproj` in Visual Studio, restore NuGet packages, and run. The app is Windows GUI only.

## NLP commands to try
- `Add task review privacy settings`
- `Remind me to enable two-factor authentication`
- `Show tasks`
- `Start quiz`
- `Show activity log`
- Existing Part 2 topics: password, phishing, privacy, malware, and memory recall

For task commands in chat, the detected task title is populated into the Task Assistant form; click **Add task** to persist it, and choose the optional reminder before saving.

## GitHub submission checklist
The assignment asks for at least six meaningful commits and three releases. Make these through GitHub/your local Git repository as work progresses; this ZIP does not fabricate commit history or releases.
