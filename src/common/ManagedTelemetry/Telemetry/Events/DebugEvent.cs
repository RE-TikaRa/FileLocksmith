// <copyright file="DebugEvent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.PowerToys.Telemetry.Events
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;

    [EventData]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public class DebugEvent : EventBase, IEvent
    {
        public string Message { get; set; }

        public PartA_PrivTags PartA_PrivTags => PartA_PrivTags.ProductAndServicePerformance;
    }
}
