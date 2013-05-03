using System;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Mongo
{
    [NLogConfigurationItem]
    public sealed class MongoField 
    {
        public MongoField()
        {
        }

        public MongoField(string name, Layout layout)
        {
            Name = name;
            Layout = layout;
        }

        [RequiredParameter]
        public string Name { get; set; }

        [RequiredParameter]
        public Layout Layout { get; set; }
    }
}