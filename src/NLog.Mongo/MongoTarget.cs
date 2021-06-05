using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Mongo
{
    /// <summary>
    /// NLog message target for MongoDB.
    /// </summary>
    [Target("Mongo")]
    public class MongoTarget : Target
    {
        private struct MongoConnectionKey : IEquatable<MongoConnectionKey>
        {
            private readonly string ConnectionString;
            private readonly string CollectionName;
            private readonly string DatabaseName;

            public MongoConnectionKey(string connectionString, string collectionName, string databaseName)
            {
                ConnectionString = connectionString ?? string.Empty;
                CollectionName = collectionName ?? string.Empty;
                DatabaseName = databaseName ?? string.Empty;
            }

            public bool Equals(MongoConnectionKey other)
            {
                return ConnectionString == other.ConnectionString
                    && CollectionName == other.CollectionName
                    && DatabaseName == other.DatabaseName;
            }

            public override int GetHashCode()
            {
                return ConnectionString.GetHashCode() ^ CollectionName.GetHashCode() ^ DatabaseName.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return obj is MongoConnectionKey && Equals((MongoConnectionKey)obj);
            }
        }

        private static readonly ConcurrentDictionary<MongoConnectionKey, IMongoCollection<BsonDocument>> _collectionCache = new ConcurrentDictionary<MongoConnectionKey, IMongoCollection<BsonDocument>>();
        private Func<AsyncLogEventInfo, BsonDocument> _createDocumentDelegate;
        private static readonly LogEventInfo _defaultLogEvent = NLog.LogEventInfo.CreateNullEvent();

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoTarget"/> class.
        /// </summary>
        public MongoTarget()
        {
            Fields = new List<MongoField>();
            Properties = new List<MongoField>();
            IncludeDefaults = true;
            OptimizeBufferReuse = true;
            IncludeEventProperties = true;
        }

        /// <summary>
        /// Gets the fields collection.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        [ArrayParameter(typeof(MongoField), "field")]
        public IList<MongoField> Fields { get; private set; }

        /// <summary>
        /// Gets the properties collection.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        [ArrayParameter(typeof(MongoField), "property")]
        public IList<MongoField> Properties { get; private set; }

        /// <summary>
        /// Gets or sets the connection string name string.
        /// </summary>
        /// <value>
        /// The connection name string.
        /// </value>
        public string ConnectionString
        {
            get => (_connectionString as SimpleLayout)?.Text;
            set => _connectionString = value ?? string.Empty;
        }
        private Layout _connectionString;

        /// <summary>
        /// Gets or sets the name of the connection.
        /// </summary>
        /// <value>
        /// The name of the connection.
        /// </value>
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the default document format.
        /// </summary>
        /// <value>
        ///   <c>true</c> to use the default document format; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeDefaults { get; set; }

        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public string DatabaseName
        {
            get => (_databaseName as SimpleLayout)?.Text;
            set => _databaseName = value ?? string.Empty;
        }
        private Layout _databaseName;

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public string CollectionName
        {
            get => (_collectionName as SimpleLayout)?.Text;
            set => _collectionName = value ?? string.Empty;
        }
        private Layout _collectionName;

        /// <summary>
        /// Gets or sets the size in bytes of the capped collection.
        /// </summary>
        /// <value>
        /// The size of the capped collection.
        /// </value>
        public long? CappedCollectionSize { get; set; }

        /// <summary>
        /// Gets or sets the capped collection max items.
        /// </summary>
        /// <value>
        /// The capped collection max items.
        /// </value>
        public long? CappedCollectionMaxItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include per-event properties in the payload sent to MongoDB
        /// </summary>
        public bool IncludeEventProperties { get; set; }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        /// <exception cref="NLog.NLogConfigurationException">Can not resolve MongoDB ConnectionString. Please make sure the ConnectionString property is set.</exception>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (!string.IsNullOrEmpty(ConnectionName))
                ConnectionString = GetConnectionString(ConnectionName);

            var connectionString = _connectionString?.Render(_defaultLogEvent);
            if (string.IsNullOrEmpty(connectionString))
                throw new NLogConfigurationException("Can not resolve MongoDB ConnectionString. Please make sure the ConnectionString property is set.");
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count == 0)
                return;

            try
            {
                if (_createDocumentDelegate == null)
                    _createDocumentDelegate = e => CreateDocument(e.LogEvent);

                var documents = logEvents.Select(_createDocumentDelegate);
                var collection = GetCollection(logEvents[logEvents.Count - 1].LogEvent.TimeStamp);
                collection.InsertMany(documents);

                for (int i = 0; i < logEvents.Count; ++i)
                    logEvents[i].Continuation(null);
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error when writing to MongoDB {0}", ex);

                if (ex.MustBeRethrownImmediately())
                    throw;

                for (int i = 0; i < logEvents.Count; ++i)
                    logEvents[i].Continuation(ex);

                if (ex.MustBeRethrown())
                    throw;
            }
        }

        /// <summary>
        /// Writes logging event to the log target.
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                var document = CreateDocument(logEvent);
                var collection = GetCollection(logEvent.TimeStamp);
                collection.InsertOne(document);
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error when writing to MongoDB {0}", ex);
                throw;
            }
        }

        private BsonDocument CreateDocument(LogEventInfo logEvent)
        {
            var document = new BsonDocument();
            if (IncludeDefaults || Fields.Count == 0)
                AddDefaults(document, logEvent);

            // extra fields
            for (int i = 0; i < Fields.Count; ++i)
            {
                var value = GetValue(Fields[i], logEvent);
                if (value != null)
                    document[Fields[i].Name] = value;
            }

            AddProperties(document, logEvent);

            return document;
        }

        private void AddDefaults(BsonDocument document, LogEventInfo logEvent)
        {
            document.Add("Date", new BsonDateTime(logEvent.TimeStamp));

            if (logEvent.Level != null)
                document.Add("Level", new BsonString(logEvent.Level.Name));

            if (logEvent.LoggerName != null)
                document.Add("Logger", new BsonString(logEvent.LoggerName));

            if (logEvent.FormattedMessage != null)
                document.Add("Message", new BsonString(logEvent.FormattedMessage));

            if (logEvent.Exception != null)
                document.Add("Exception", CreateException(logEvent.Exception));
        }

        private void AddProperties(BsonDocument document, LogEventInfo logEvent)
        {
            if ((IncludeEventProperties && logEvent.HasProperties) || Properties.Count > 0)
            {
                var propertiesDocument = new BsonDocument();
                for (int i = 0; i < Properties.Count; ++i)
                {
                    string key = Properties[i].Name;
                    var value = GetValue(Properties[i], logEvent);

                    if (value != null)
                        propertiesDocument[key] = value;
                }

                if (IncludeEventProperties && logEvent.HasProperties)
                {
                    foreach (var property in logEvent.Properties)
                    {
                        if (property.Key == null || property.Value == null)
                            continue;

                        string key = Convert.ToString(property.Key, CultureInfo.InvariantCulture);
                        if (string.IsNullOrEmpty(key))
                            continue;

                        string value = Convert.ToString(property.Value, CultureInfo.InvariantCulture);
                        if (string.IsNullOrEmpty(value))
                            continue;

                        if (key.IndexOf('.') >= 0)
                            key = key.Replace('.', '_');

                        propertiesDocument[key] = new BsonString(value);
                    }
                }

                if (propertiesDocument.ElementCount > 0)
                    document.Add("Properties", propertiesDocument);
            }
        }

        private BsonValue CreateException(Exception exception)
        {
            if (exception == null)
                return BsonNull.Value;

            if (exception is AggregateException aggregateException)
            {
                aggregateException = aggregateException.Flatten();
                if (aggregateException.InnerExceptions?.Count == 1)
                {
                    exception = aggregateException.InnerExceptions[0];
                }
                else
                {
                    exception = aggregateException;
                }
            }

            var document = new BsonDocument();
            document.Add("Message", new BsonString(exception.Message));
            document.Add("BaseMessage", new BsonString(exception.GetBaseException().Message));
            document.Add("Text", new BsonString(exception.ToString()));
            document.Add("Type", new BsonString(exception.GetType().ToString()));

#if !NETSTANDARD1_5
            if (exception is ExternalException external)
                document.Add("ErrorCode", new BsonInt32(external.ErrorCode));
#endif
            document.Add("HResult", new BsonInt32(exception.HResult));
            document.Add("Source", new BsonString(exception.Source ?? string.Empty));

#if !NETSTANDARD1_5
            var method = exception.TargetSite;
            if (method != null)
            {
                document.Add("MethodName", new BsonString(method.Name ?? string.Empty));

                AssemblyName assembly = method.Module?.Assembly?.GetName();
                if (assembly != null)
                {
                    document.Add("ModuleName", new BsonString(assembly.Name));
                    document.Add("ModuleVersion", new BsonString(assembly.Version.ToString()));
                }
            }
#endif

            return document;
        }


        private BsonValue GetValue(MongoField field, LogEventInfo logEvent)
        {
            var value = (field.Layout != null ? RenderLogEvent(field.Layout, logEvent) : string.Empty).Trim();
            if (string.IsNullOrEmpty(value))
                return null;

            BsonValue bsonValue = null;
            switch (field.BsonTypeCode)
            {
                case TypeCode.Boolean:
                    MongoConvert.TryBoolean(value, out bsonValue);
                    break;
                case TypeCode.DateTime:
                    MongoConvert.TryDateTime(value, out bsonValue);
                    break;
                case TypeCode.Double:
                    MongoConvert.TryDouble(value, out bsonValue);
                    break;
                case TypeCode.Int32:
                    MongoConvert.TryInt32(value, out bsonValue);
                    break;
                case TypeCode.Int64:
                    MongoConvert.TryInt64(value, out bsonValue);
                    break;
                case TypeCode.Object:
                    MongoConvert.TryJsonObject(value, out bsonValue);
                    break;
            }

            return bsonValue ?? new BsonString(value);
        }

        private IMongoCollection<BsonDocument> GetCollection(DateTime timestamp)
        {
            if (_defaultLogEvent.TimeStamp < timestamp)
                _defaultLogEvent.TimeStamp = timestamp;

            string connectionString = _connectionString != null ? RenderLogEvent(_connectionString, _defaultLogEvent) : string.Empty;
            string collectionName = _collectionName != null ? RenderLogEvent(_collectionName, _defaultLogEvent) : string.Empty;
            string databaseName = _databaseName != null ? RenderLogEvent(_databaseName, _defaultLogEvent) : string.Empty;

            if (string.IsNullOrEmpty(connectionString))
                throw new NLogConfigurationException("Can not resolve MongoDB ConnectionString. Please make sure the ConnectionString property is set.");

            // cache mongo collection based on target name.
            var key = new MongoConnectionKey(connectionString, collectionName, databaseName);
            if (_collectionCache.TryGetValue(key, out var mongoCollection))
                return mongoCollection;

            return _collectionCache.GetOrAdd(key, k =>
            {
                // create collection
                var mongoUrl = new MongoUrl(connectionString);

                databaseName = !string.IsNullOrEmpty(databaseName) ? databaseName : (mongoUrl.DatabaseName ?? "NLog");
                collectionName = !string.IsNullOrEmpty(collectionName) ? collectionName : "Log";
                InternalLogger.Info("Connecting to MongoDB collection {0} in database {1}", collectionName, databaseName);

                var client = new MongoClient(mongoUrl);

                // Database name overrides connection string
                var database = client.GetDatabase(databaseName);

                if (CappedCollectionSize.HasValue)
                {
                    InternalLogger.Debug("Checking for existing MongoDB collection {0} in database {1}", collectionName, databaseName);
                    
                    var filterOptions = new ListCollectionNamesOptions { Filter = new BsonDocument("name", collectionName) };
                    if (!database.ListCollectionNames(filterOptions).Any())
                    {
                        InternalLogger.Debug("Creating new MongoDB collection {0} in database {1}", collectionName, databaseName);

                        // create capped
                        var options = new CreateCollectionOptions
                        {
                            Capped = true,
                            MaxSize = CappedCollectionSize,
                            MaxDocuments = CappedCollectionMaxItems
                        };

                        database.CreateCollection(collectionName, options);
                    }
                }

                var collection = database.GetCollection<BsonDocument>(collectionName);
                InternalLogger.Debug("Retrieved MongoDB collection {0} from database {1}", collectionName, databaseName);
                return collection;
            });
        }

        private static string GetConnectionString(string connectionName)
        {
            if (connectionName == null)
                throw new ArgumentNullException(nameof(connectionName));

#if NETSTANDARD1_5 || NETSTANDARD2_0
            return null;
#else
            var settings = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName];
            if (settings == null)
                throw new NLogConfigurationException($"No connection string named '{connectionName}' could be found in the application configuration file.");

            string connectionString = settings.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new NLogConfigurationException($"The connection string '{connectionName}' in the application's configuration file does not contain the required connectionString attribute.");

            return connectionString;
#endif
        }
    }
}
