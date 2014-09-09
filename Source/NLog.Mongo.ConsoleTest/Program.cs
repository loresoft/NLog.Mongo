﻿using NLog.Fluent;
using System;
using System.IO;

namespace NLog.Mongo.ConsoleTest
{
    class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
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
