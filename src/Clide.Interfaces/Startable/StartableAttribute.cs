﻿using System;
using System.ComponentModel.Composition;

namespace Clide
{
    /// <summary>
    /// Provides the metadata attriute to export a component that needs to be started
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StartableAttribute : ExportAttribute
    {
        /// <summary>
        /// Creates an instance of <see cref="StartableAttribute"/>
        /// </summary>
        /// <param name="context">
        /// Specifies the context when the component should be started.
        /// The value can be a Guid string which it will be automatically parsed into <see cref="StartableAttribute.ContextGuid"/>
        /// </param>
        /// <param name="order">
        /// Specifies the order value for the startable component
        /// </param>
        public StartableAttribute(string context, double order = 1000)
            : base(typeof(IStartable))
        {
            Context = context;

            Guid guid;
            if (Guid.TryParse(context, out guid))
                ContextGuid = guid;
        }

        /// <summary>
        /// Gets the context when the component should be started
        /// </summary>
        public string Context { get; }

        /// <summary>
        /// Gets the context as a Guid if it could be parsed
        /// </summary>
        public Guid ContextGuid { get; }

        /// <summary>
        /// Gets the order value for the startable component
        /// </summary>
        public double Order { get; }
    }
}
