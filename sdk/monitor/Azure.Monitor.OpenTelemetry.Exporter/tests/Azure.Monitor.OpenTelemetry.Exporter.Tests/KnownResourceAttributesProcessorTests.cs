// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter.Internals;
using Azure.Monitor.OpenTelemetry.Exporter.Models;
using Xunit;

namespace Azure.Monitor.OpenTelemetry.Exporter.Tests;

public class KnownResourceAttributesProcessorTests
{
    [Fact]
    public void OnlyKnownAttributesShouldBeProcessed()
    {
        Assert.All(KnownResourceAttributesProcessor.Attributes.Keys, key =>
        {
            var attribute = new KeyValuePair<string, object>(key, "value");
            Assert.True(
                KnownResourceAttributesProcessor.TryMapAttribute(attribute, out var mappedKey, out var mappedValue));
            Assert.Equal(KnownResourceAttributesProcessor.Attributes[key].Key.ToString(), mappedKey);
            Assert.Equal("value", mappedValue);
        });
    }

    [Fact]
    public void UnknownAttributesShouldNotBeProcessed()
    {
        var attribute = new KeyValuePair<string, object>("unknown", "value");
        Assert.False(KnownResourceAttributesProcessor.TryMapAttribute(attribute, out _, out _));
    }

    [Fact]
    public void AttributesWithLongValuesShouldNotBeProcessed()
    {
        Assert.All(KnownResourceAttributesProcessor.Attributes, kvp =>
        {
            var attribute = new KeyValuePair<string, object>(kvp.Key, new string('a', kvp.Value.MaxLength + 1));
            Assert.False(KnownResourceAttributesProcessor.TryMapAttribute(attribute, out _, out _));
        });
    }

    [Fact]
    public void TelemetryItemShouldCopyTags()
    {
        ActivityTagsProcessor processor = default;
        var additionalTags = KnownResourceAttributesProcessor.Attributes.Values
            .ToDictionary(v => v.Key.ToString(), k => $"{k.Key}-value");
        var resource = new AzureMonitorResource(roleName: null,
            roleInstance: null,
            serviceVersion: null,
            monitorBaseData: null,
            additionalTags: additionalTags);
        var telemetry = new TelemetryItem(new Activity(string.Empty), ref processor, resource, string.Empty, default);
        Assert.All(additionalTags, kvp => Assert.Equal(kvp.Value, telemetry.Tags[kvp.Key]));
        telemetry.Tags["unknown"] = "value";

        // Copy constructor should copy only known tags
        telemetry = new TelemetryItem(telemetryItem: telemetry,
            name: string.Empty, activitySpanId: default, kind: default, activityEventTimeStamp: default);
        Assert.All(additionalTags, kvp => Assert.Equal(kvp.Value, telemetry.Tags[kvp.Key]));
        Assert.DoesNotContain(telemetry.Tags, kvp => kvp.Key == "unknown");
    }
}
