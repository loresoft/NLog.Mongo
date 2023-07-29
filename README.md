# NLog.Mongo

Writes NLog messages to MongoDB.

[![Build status](https://github.com/loresoft/NLog.Mongo/workflows/Build/badge.svg)](https://github.com/loresoft/NLog.Mongo/actions)

[![NuGet Version](https://img.shields.io/nuget/v/NLog.Mongo.svg?style=flat-square)](https://www.nuget.org/packages/NLog.Mongo/) 

[![Coverage Status](https://coveralls.io/repos/github/loresoft/NLog.Mongo/badge.svg?branch=master)](https://coveralls.io/github/loresoft/NLog.Mongo?branch=master)

## Download

The NLog.Mongo library is available on nuget.org via package name `NLog.Mongo`.

To install NLog.Mongo, run the following command in the Package Manager Console

    PM> Install-Package NLog.Mongo
    
More information about NuGet package avaliable at
<https://nuget.org/packages/NLog.Mongo>

## Configuration Syntax

```xml
<extensions>
  <add assembly="NLog.Mongo"/>
</extensions>

<targets>
  <target xsi:type="Mongo"
          name="String"
          connectionName="String"
          connectionString="String"
          collectionName="String"
          cappedCollectionSize="Long"
          cappedCollectionMaxItems="Long"
          databaseName="String"
          includeDefaults="Boolean">
    
    <!-- repeated --> 
    <field name="String" layout="Layout" bsonType="Boolean|DateTime|Double|Int32|Int64|String"  />
    
    <!-- repeated --> 
    <property name="String" layout="Layout" bsonType="Boolean|DateTime|Double|Int32|Int64|String"  />
  </target>
</targets>
```

## Parameters

### General Options

_name_ - Name of the target.

### Connection Options

_connectionName_ - The name of the connection string to get from the config file. 

_connectionString_ - Connection string. When provided, it overrides the values specified in connectionName. 

_databaseName_ - The name of the database, overrides connection string database.

### Collection Options
_collectionName_ - The name of the MongoDB collection to write logs to.  

_cappedCollectionSize_ - If the collection doesn't exist, it will be create as a capped collection with this max size.

_cappedCollectionMaxItems_ - If the collection doesn't exist, it will be create as a capped collection with this max number of items.  _cappedCollectionSize_ must also be set when using this setting.

### Document Options

_includeDefaults_ - Specifies if the default document is created when writing to the collection.  Defaults to true.

_field_ - Specifies a root level document field. There can be multiple fields specified.

_property_ - Specifies a dictionary property on the Properties field. There can be multiple properties specified.

_includeEventProperties_ - Specifies if LogEventInfo Properties should be automatically included.  Defaults to true.

## Examples

### Default Configuration with Extra Properties

#### NLog.config target

```xml
<target xsi:type="Mongo"
        name="mongoDefault"
        connectionString="mongodb://localhost/Logging"
        collectionName="DefaultLog"
        cappedCollectionSize="26214400">
  <property name="ThreadID" layout="${threadid}" bsonType="Int32" />
  <property name="ThreadName" layout="${threadname}" />
  <property name="ProcessID" layout="${processid}" bsonType="Int32" />
  <property name="ProcessName" layout="${processname:fullName=true}" />
  <property name="UserName" layout="${windows-identity}" />
</target>
```

#### Default Output JSON

```JSON
{
    "_id" : ObjectId("5184219b545eb455aca34390"),
    "Date" : ISODate("2013-05-03T20:44:11Z"),
    "Level" : "Error",
    "Logger" : "NLog.Mongo.ConsoleTest.Program",
    "Message" : "Error reading file 'blah.txt'.",
    "Exception" : {
        "Message" : "Could not find file 'C:\\Projects\\github\\NLog.Mongo\\Source\\NLog.Mongo.ConsoleTest\\bin\\Debug\\blah.txt'.",
        "Text" : "System.IO.FileNotFoundException: Could not find file 'C:\\Projects\\github\\NLog.Mongo\\Source\\NLog.Mongo.ConsoleTest\\bin\\Debug\\blah.txt' ...",
        "Type" : "System.IO.FileNotFoundException",
        "Source" : "mscorlib",
        "MethodName" : "WinIOError",
        "ModuleName" : "mscorlib",
        "ModuleVersion" : "4.0.0.0"
    },
    "Properties" : {
        "ThreadID" : 10,
        "ProcessID" : 21932,
        "ProcessName" : "C:\\Projects\\github\\NLog.Mongo\\Source\\NLog.Mongo.ConsoleTest\\bin\\Debug\\NLog.Mongo.ConsoleTest.exe",
        "UserName" : "pwelter",
        "Test" : "ErrorWrite",
        "CallerMemberName" : "Main",
        "CallerFilePath" : "c:\\Projects\\github\\NLog.Mongo\\Source\\NLog.Mongo.ConsoleTest\\Program.cs",
        "CallerLineNumber" : "43"
    }
}
```

### Custom Document Fields

#### NLog.config target

```xml
<target xsi:type="Mongo"
        name="mongoCustom"
        includeDefaults="false"
        connectionString="mongodb://localhost"
        collectionName="CustomLog"
        databaseName="Logging"
        cappedCollectionSize="26214400">
  <field name="Date" layout="${date}" bsonType="DateTime" />
  <field name="Level" layout="${level}"/>
  <field name="Message" layout="${message}" />
  <field name="Logger" layout="${logger}"/>
  <field name="Exception" layout="${exception:format=tostring}" />
  <field name="ThreadID" layout="${threadid}" bsonType="Int32" />
  <field name="ThreadName" layout="${threadname}" />
  <field name="ProcessID" layout="${processid}" bsonType="Int32" />
  <field name="ProcessName" layout="${processname:fullName=true}" />
  <field name="UserName" layout="${windows-identity}" />
</target>
```

#### Custom Document Fields JSON output

```JSON
{
    "_id" : ObjectId("5187abc2545eb467ecce9184"),
    "Date" : ISODate("2015-02-02T17:31:20.728Z"),
    "Level" : "Debug",
    "Message" : "Sample debug message",
    "Logger" : "NLog.Mongo.ConsoleTest.Program",
    "ThreadID" : 9,
    "ProcessID" : 26604,
    "ProcessName" : "C:\\Projects\\github\\NLog.Mongo\\Source\\NLog.Mongo.ConsoleTest\\bin\\Debug\\v4.5\\NLog.Mongo.ConsoleTest.exe",
    "UserName" : "pwelter"
}
```

### Custom Object Properties

#### NLog.config target

```xml
<target xsi:type="Mongo"
        name="mongoCustomJsonProperties"
        includeEventProperties="false"
        connectionString="mongodb://localhost"
        collectionName="CustomLog"
        databaseName="Logging"
        cappedCollectionSize="26214400">
    <field name="Properties" bsonType="Object">
        <layout type="JsonLayout" includeAllProperties="true" includeMdlc="true" maxRecursionLimit="10">
            <attribute name="ThreadID" layout="${threadid}" encode="false" />
            <attribute name="ProcessID" layout="${processid}" encode="false" />
            <attribute name="ProcessName" layout="${processname:fullName=false}" />
        </layout>
    </field>
</target>
```

#### Custom Object Properties JSON output

```JSON
{
    "_id" : ObjectId("5184219b545eb455aca34390"),
    "Date" : ISODate("2013-05-03T20:44:11Z"),
    "Level" : "Error",
    "Logger" : "NLog.Mongo.ConsoleTest.Program",
    "Message" : "Error reading file 'blah.txt'.",
    "Exception" : {
        "Message" : "Could not find file 'C:\\Projects\\github\\NLog.Mongo\\Source\\NLog.Mongo.ConsoleTest\\bin\\Debug\\blah.txt'.",
        "Text" : "System.IO.FileNotFoundException: Could not find file 'C:\\Projects\\github\\NLog.Mongo\\Source\\NLog.Mongo.ConsoleTest\\bin\\Debug\\blah.txt' ...",
        "Type" : "System.IO.FileNotFoundException",
        "Source" : "mscorlib",
        "MethodName" : "WinIOError",
        "ModuleName" : "mscorlib",
        "ModuleVersion" : "4.0.0.0"
    },
    "Properties" : {
        "ThreadID" : 10,
        "ProcessID" : 21932,
        "ProcessName" : "NLog.Mongo.ConsoleTest.exe",
        "Product": { "Name": "Foo", "Id": 42 }
    }
}
```
