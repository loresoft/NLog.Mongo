using System;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Mongo
{
    /// <summary>
    /// A configuration item for MongoDB target.
    /// </summary>
    [NLogConfigurationItem]
    public sealed class MongoField 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoField"/> class.
        /// </summary>
        public MongoField()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoField"/> class.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="layout">The layout used to generate the value for the field.</param>
        public MongoField(string name, Layout layout)
        {
            Name = name;
            Layout = layout;
        }

        /// <summary>
        /// Gets or sets the name of the MongoDB field.
        /// </summary>
        /// <value>
        /// The name of the MongoDB field.
        /// </value>
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout used to generate the value for the field.
        /// </summary>
        /// <value>
        /// The layout used to generate the value for the field.
        /// </value>
        [RequiredParameter]
        public Layout Layout { get; set; }
    }
}