// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Azure.Monitor.OpenTelemetry.Exporter.Models;
using OpenTelemetry.Resources;

namespace Azure.Monitor.OpenTelemetry.Exporter.Internals;

/// <summary>
/// Processor that handles known resource attributes (mapped to <see cref="ContextTagKeys"/>).
/// </summary>
internal static class KnownResourceAttributesProcessor
{
    /// <summary>
    /// A map of attributes to their corresponding <see cref="ContextTagKeys"/> and maximum length.
    /// </summary>
    public static readonly Dictionary<string, (ContextTagKeys Key, int MaxLength)> Attributes = new(capacity: 4)
    {
        // device.* attributes
        {
            SemanticConventions.AttributeDeviceType,
            (ContextTagKeys.AiDeviceType, SchemaConstants.Tags_AiDeviceType_MaxLength)
        },
        {
            SemanticConventions.AttributeDeviceId,
            (ContextTagKeys.AiDeviceId, SchemaConstants.Tags_AiDeviceId_MaxLength)
        },
        {
            SemanticConventions.AttributeDeviceManufacturer,
            (ContextTagKeys.AiDeviceOemName, SchemaConstants.Tags_AiDeviceOemName_MaxLength)
        },
        {
            SemanticConventions.AttributeDeviceModelId,
            (ContextTagKeys.AiDeviceModel, SchemaConstants.Tags_AiDeviceModel_MaxLength)
        },

        // browser.* attributes
        {
            SemanticConventions.AttributeBrowserLanguage,
            (ContextTagKeys.AiDeviceLocale, SchemaConstants.Tags_AiDeviceLocale_MaxLength)
        },

        // os.* attributes
        {
            SemanticConventions.AttributeOsDescription,
            (ContextTagKeys.AiDeviceOsVersion, SchemaConstants.Tags_AiDeviceOsVersion_MaxLength)
        },
    };

    public static bool TryMapAttribute(KeyValuePair<string, object> attribute, out string key, out string value)
    {
        //Note: If value is longer than the max length, the value will be ignored.
        if (Attributes.TryGetValue(attribute.Key, out var contextTagKey)
            && attribute.Value is string str
            && str.Length <= contextTagKey.MaxLength)
        {
            key = contextTagKey.Key.ToString();
            value = str;
            return true;
        }
        key = null!;
        value = null!;
        return false;
    }

    /// <summary>
    /// Copies values with known <see cref="ContextTagKeys"/> keys from the source dictionary to the destination dictionary.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTags(IDictionary<string, string> source, IDictionary<string, string> destination)
    {
        string? value;
        if (source.TryGetValue(ContextTagKeys.AiDeviceType.ToString(), out value))
        {
            destination[ContextTagKeys.AiDeviceType.ToString()] = value;
        }
        if (source.TryGetValue(ContextTagKeys.AiDeviceId.ToString(), out value))
        {
            destination[ContextTagKeys.AiDeviceId.ToString()] = value;
        }
        if (source.TryGetValue(ContextTagKeys.AiDeviceOemName.ToString(), out value))
        {
            destination[ContextTagKeys.AiDeviceOemName.ToString()] = value;
        }
        if (source.TryGetValue(ContextTagKeys.AiDeviceModel.ToString(), out value))
        {
            destination[ContextTagKeys.AiDeviceModel.ToString()] = value;
        }
        if (source.TryGetValue(ContextTagKeys.AiDeviceLocale.ToString(), out value))
        {
            destination[ContextTagKeys.AiDeviceLocale.ToString()] = value;
        }
        if (source.TryGetValue(ContextTagKeys.AiDeviceOsVersion.ToString(), out value))
        {
            destination[ContextTagKeys.AiDeviceOsVersion.ToString()] = value;
        }
    }
}
