namespace NLog.Mongo.Tests;

public class LoggerTest
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    [Fact]
    public void Write()
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

        _logger.ForInfoEvent()
            .Message("Sample informational message, k={0}, l={1}", k, l)
            .Property("Test", "Tesing properties")
            .Log();

        string t = "tenant1";
        // The property name must match the one written in the NLog.config
        using (ScopeContext.PushProperty("TenantId", t))
        {
            _logger.Trace("Sample trace message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Debug("Sample debug message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Info("Sample informational message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Warn("Sample warning message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Error("Sample error message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Fatal("Sample fatal error message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Log(LogLevel.Info, "Sample fatal error message, k={0}, l={1}, t={2}", k, l, t);
        }

        t = "tenant2";
        // The property name must match the one written in the NLog.config
        using (ScopeContext.PushProperty("TenantId", t))
        {
            _logger.Trace("Sample trace message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Debug("Sample debug message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Info("Sample informational message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Warn("Sample warning message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Error("Sample error message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Fatal("Sample fatal error message, k={0}, l={1}, t={2}", k, l, t);
            _logger.Log(LogLevel.Info, "Sample fatal error message, k={0}, l={1}, t={2}", k, l, t);
        }

        string path = "blah.txt";

        try
        {
            string text = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            _logger.ForErrorEvent()
                .Message("Error reading file '{0}'.", path)
                .Exception(ex)
                .Property("Test", "ErrorWrite")
                .Log();
        }
    }

}
