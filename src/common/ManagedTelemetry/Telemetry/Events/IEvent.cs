// <copyright file="IEvent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.PowerToys.Telemetry.Events
{
    using System.Diagnostics.CodeAnalysis;

    public interface IEvent
    {
        [SuppressMessage("Naming", "CA1707:Remove underscores from member name", Justification = "Telemetry schema requires underscores.")]
        PartA_PrivTags PartA_PrivTags { get; }
    }
}
