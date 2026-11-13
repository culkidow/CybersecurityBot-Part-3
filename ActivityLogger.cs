using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CybersecurityBot
{
    public class ActivityLogger
    {
        private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "activity.log");
        public void Log(string action)
        {
            File.AppendAllText(_path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {action}{Environment.NewLine}");
        }
        public List<string> GetRecent(int count = 10)
        {
            if (!File.Exists(_path)) return new List<string>();
            return File.ReadAllLines(_path).Reverse().Take(count).Reverse().ToList();
        }
    }
}
