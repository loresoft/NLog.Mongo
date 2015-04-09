using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Fluent;

namespace NLog.Mongo.ConsoleTest
{
    class Program
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            int k = 42;
            int l = 100;

            _logger.Trace("Sample trace message, k={0}, l={1}", k, l);
            _logger.Debug("Sample debug message, k={0}, l={1}", k, l);
            _logger.Info("Sample informational message, k={0}, l={1}", k, l);
            _logger.Warn("Sample warning message, k={0}, l={1}", k, l);
            _logger.Error("Sample error message, k={0}, l={1}", k, l);
            _logger.Fatal("Sample fatal error message, k={0}, l={1}", k, l);
            _logger.Log(LogLevel.Info, "Sample fatal error message, k={0}, l={1}", k, l);

            _logger.Info()
                .Message("Sample informational message, k={0}, l={1}", k, l)
                .Property("Test", "Tesing properties")
                .Write();

            string path = "blah.txt";
            try
            {
                string text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                _logger.Error()
                    .Message("Error reading file '{0}'.", path)
                    .Exception(ex)
                    .Property("Test", "ErrorWrite")
                    .Write();
            }

            Console.ReadLine();
        }
    }
}
