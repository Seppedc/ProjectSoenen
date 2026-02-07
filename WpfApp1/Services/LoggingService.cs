using ProjectSoenen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSoenen.Services
{
    public static class LoggingService
    {
        private static string _logFilePath = "Data/logs.txt";
        public static void Log(string parameter, string oldValue, string newValue)
        {
            var user = AppSession.CurrentUser;

            string logEntry =  $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | User: {user?.Username} (ID:{user?.Id}) | " +
                           $"Param: {parameter} | {oldValue} -> {newValue}";
            System.IO.File.AppendAllLines(_logFilePath, new[] { logEntry });
        }
    }
}
