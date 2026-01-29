// <copyright file="EventBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.PowerToys.Telemetry.Events
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using System.Reflection;

    /// <summary>
    /// A base class to implement properties that are common to all telemetry events.
    /// </summary>
    [EventData]
    public class EventBase
    {
        private string version;

        [SuppressMessage("Naming", "CA1707:Remove underscores from member name", Justification = "Telemetry schema requires underscores.")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Serialized event data requires instance members.")]
        public bool UTCReplace_AppSessionGuid => true;

        public string EventName { get; set; }

        public string Version
        {
            get
            {
                if (string.IsNullOrEmpty(this.version))
                {
                    this.version = GetVersionFromAssembly();
                }

                return this.version;
            }
        }

        private static string GetVersionFromAssembly()
        {
            // For consistency this should be formatted the same way as
            // https://github.com/microsoft/PowerToys/blob/710f92d99965109fd788d85ebf8b6b9e0ba1524a/src/common/common.cpp#L635
            var assemblyVersion = Assembly.GetExecutingAssembly()?.GetName()?.Version ?? new Version();
            return $"v{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
        }
    }
}
