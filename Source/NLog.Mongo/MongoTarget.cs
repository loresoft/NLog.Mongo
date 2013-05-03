using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace NLog.Mongo
{
    [Target("Mongo")]
    public class MongoTarget : Target
    {
        private static readonly object _collectionLock = new object();
        private static MongoCollection _mongoCollection = null;

        private static readonly ConcurrentDictionary<string, MongoCollection> _collectionCache = new ConcurrentDictionary<string, MongoCollection>();

        public MongoTarget()
        {
            Fields = new List<MongoField>();
            Properties = new List<MongoField>();
            IncludeDefaults = true;
        }

        [ArrayParameter(typeof(MongoField), "field")]
        public IList<MongoField> Fields { get; private set; }

        [ArrayParameter(typeof(MongoField), "property")]
        public IList<MongoField> Properties { get; private set; }


        public string ConnectionString { get; set; }

        public string ConnectionName { get; set; }


        public bool IncludeDefaults { get; set; }


        public string CollectionName { get; set; }

        public long? CappedCollectionSize { get; set; }

        public long? CappedCollectionMaxItems { get; set; }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (!string.IsNullOrEmpty(ConnectionName))
                ConnectionString = GetConnectionString(ConnectionName);

            if (string.IsNullOrEmpty(ConnectionString))
                throw new NLogConfigurationException("Can not resolve MongoDB ConnectionString. Please make sure the ConnectionString property is set.");

        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            if (logEvents.Length == 0)
                return;

            try
            {
                var documents = logEvents.Select(e => CreateDocument(e.LogEvent));

                var collection = GetCollection();
                collection.InsertBatch(documents);

                foreach (var ev in logEvents)
                    ev.Continuation(null);

            }
            catch (Exception ex)
            {
                if (ex is StackOverflowException || ex is ThreadAbortException || ex is OutOfMemoryException || ex is NLogConfigurationException)
                    throw;

                InternalLogger.Error("Error when writing to MongoDB {0}", ex);

                foreach (var ev in logEvents)
                    ev.Continuation(ex);

            }
        }

        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                var document = CreateDocument(logEvent);
                var collection = GetCollection();
                collection.Insert(document);
            }
            catch (Exception ex)
            {
                if (ex is StackOverflowException || ex is ThreadAbortException || ex is OutOfMemoryException || ex is NLogConfigurationException)
                    throw;

                InternalLogger.Error("Error when writing to MongoDB {0}", ex);
            }
        }


        private BsonDocument CreateDocument(LogEventInfo logEvent)
        {
            var document = new BsonDocument();
            if (IncludeDefaults || Fields.Count == 0)
                AddDefaults(document, logEvent);

            // extra fields
            foreach (var field in Fields)
            {
                var value = field.Layout.Render(logEvent);
                if (!string.IsNullOrWhiteSpace(value))
                    document[field.Name] = value;
            }

            AddProperties(document, logEvent);

            return document;
        }

        private void AddDefaults(BsonDocument document, LogEventInfo logEvent)
        {
            document.Add("TimeStamp", new BsonDateTime(logEvent.TimeStamp));

            if (logEvent.Level != null)
                document.Add("Level", new BsonString(logEvent.Level.Name));

            if (logEvent.LoggerName != null)
                document.Add("LoggerName", new BsonString(logEvent.LoggerName));

            if (logEvent.FormattedMessage != null)
                document.Add("Message", new BsonString(logEvent.FormattedMessage));

            if (logEvent.Exception != null)
                document.Add("Exception", CreateException(logEvent.Exception));


        }

        private void AddProperties(BsonDocument document, LogEventInfo logEvent)
        {
            var propertiesDocument = new BsonDocument();
            foreach (var field in Properties)
            {
                string key = field.Name;
                string value = field.Layout.Render(logEvent);

                if (!string.IsNullOrEmpty(value))
                    propertiesDocument[key] = new BsonString(value);
            }

            var properties = logEvent.Properties ?? Enumerable.Empty<KeyValuePair<object, object>>();
            foreach (var property in properties)
            {
                if (property.Key == null || property.Value == null)
                    continue;

                string key = Convert.ToString(property.Key, CultureInfo.InvariantCulture);
                string value = Convert.ToString(property.Value, CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(value))
                    propertiesDocument[key] = new BsonString(value);
            }

            if (propertiesDocument.ElementCount > 0)
                document.Add("Properties", propertiesDocument);

        }

        private BsonValue CreateException(Exception exception)
        {
            var document = new BsonDocument();
            document.Add("Message", new BsonString(exception.Message));
            document.Add("Text", new BsonString(exception.ToString()));
            document.Add("Type", new BsonString(exception.GetType().ToString()));

            var external = exception as ExternalException;
            if (external != null)
                document.Add("ErrorCode", new BsonInt32(external.ErrorCode));

            document.Add("Source", new BsonString(exception.Source));

            MethodBase method = exception.TargetSite;
            if (method != null)
            {
                document.Add("MethodName", new BsonString(method.Name));

                AssemblyName assembly = method.Module.Assembly.GetName();
                document.Add("ModuleName", new BsonString(assembly.Name));
                document.Add("ModuleVersion", new BsonString(assembly.Version.ToString()));
            }

            return document;
        }


        private MongoCollection GetCollection()
        {
            // cache mongo collection based on target name.
            return _collectionCache.GetOrAdd(Name, k =>
            {
                // create collection
                var mongoUrl = new MongoUrl(ConnectionString);
                var client = new MongoClient(mongoUrl);
                var server = client.GetServer();
                var database = server.GetDatabase(mongoUrl.DatabaseName ?? "NLog");

                string collectionName = CollectionName ?? "Log";

                if (CappedCollectionSize.HasValue && !database.CollectionExists(collectionName))
                {
                    // create capped
                    var options = CollectionOptions
                        .SetCapped(true)
                        .SetMaxSize(CappedCollectionSize.Value);

                    if (CappedCollectionMaxItems.HasValue)
                        options.SetMaxDocuments(CappedCollectionMaxItems.Value);

                    database.CreateCollection(collectionName, options);
                }

                return database.GetCollection(collectionName);
            });
        }


        private static string GetConnectionString(string connectionName)
        {
            if (connectionName == null)
                throw new ArgumentNullException("connectionName");

            var settings = ConfigurationManager.ConnectionStrings[connectionName];
            if (settings == null)
                throw new NLogConfigurationException(
                    string.Format("No connection string named '{0}' could be found in the application configuration file.", connectionName));

            string connectionString = settings.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new NLogConfigurationException(
                    string.Format("The connection string '{0}' in the application's configuration file does not contain the required connectionString attribute.", connectionName));

            return settings.ConnectionString;
        }

    }
}
