NLog.Mongo
==========

Writes NLog messages to MongoDB.

##Download

The NLog.Mongo library is available on nuget.org via package name `NLog.Mongo`.

To install NLog.Mongo, run the following command in the Package Manager Console

    PM> Install-Package NLog.Mongo
    
More information about NuGet package avaliable at
https://nuget.org/packages/NLog.Mongo

##Development Builds

Development builds are available on the myget.org feed.  A development build is promoted to the main NuGet feed when it's determined to be stable. 

In your Package Manager settings add the following package source for development builds:
http://www.myget.org/F/loresoft/

##Configuration Syntax

    <targets>
      <target xsi:type="Mongo"
              name="String"
              connectionName="String"
              connectionString="String"
              collectionName="String"
              cappedCollectionSize="Long"
              cappedCollectionMaxItems="Long"
              includeDefaults="Boolean">
        
        <!-- repeated --> 
        <field name="String" Layout="Layout" />
        
        <!-- repeated --> 
        <property name="String" Layout="Layout" />
      </target>
    </targets>


##Parameters

###General Options

_name_ - Name of the target.

###Connection Options

_connectionName_ - The name of the connection string to get from the config file. 

_connectionString_ - Connection string. When provided, it overrides the values specified in connectionName. 

###Collection Options
_collectionName_ - The name of the MongoDB collection to write logs to.  

_cappedCollectionSize_ - If the collection doesn't exist, it will be create as a capped collection with this max size.

_cappedCollectionMaxItems_ - If the collection doesn't exist, it will be create as a capped collection with this max number of items.  _cappedCollectionSize_ must also be set when using this setting.

###Document Options

_includeDefaults_ - Specifies if the default document is created when writing to the collection.  Defaults to true.

_field_ - Specifies a root level document field. There can be multiple fields specified.

_property_ - Specifies a dictionary property on the Properties field. There can be multiple properties specified.
.
##Document Structure

###Default Document

The following is a json example of the default document structure.  To not include the default structure, set _includeDefaults_ to false.

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
            "ThreadID" : "10",
            "ProcessID" : "21932",
            "ProcessName" : "C:\\Projects\\github\\NLog.Mongo\\Source\\NLog.Mongo.ConsoleTest\\bin\\Debug\\NLog.Mongo.ConsoleTest.exe",
            "UserName" : "pwelter",
            "Test" : "ErrorWrite",
            "CallerMemberName" : "Main",
            "CallerFilePath" : "c:\\Projects\\github\\NLog.Mongo\\Source\\NLog.Mongo.ConsoleTest\\Program.cs",
            "CallerLineNumber" : "43"
        }
    }