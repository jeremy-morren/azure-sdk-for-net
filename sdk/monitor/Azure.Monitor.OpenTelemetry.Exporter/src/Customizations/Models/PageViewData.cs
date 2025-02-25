// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Globalization;
using Azure.Monitor.OpenTelemetry.Exporter.Internals;

namespace Azure.Monitor.OpenTelemetry.Exporter.Models
{
    internal partial class PageViewData
    {
        // Page View https://github.com/open-telemetry/semantic-conventions/blob/23003358584855d127e45e30deb7aa795fcf2dcd/docs/browser/events.md
        public const string EventTypePageView = "browser.page_view";
        public const string AttributePageViewChangeState = "change_state";
        public const string AttributePageViewReferrer = "referrer";
        public const string AttributePageViewTitle = "title";
        public const string AttributePageViewUrl = "url";

        /// <summary>
        /// Returns true if the activity is a PageView (i.e. has tag <c>event</c> with value <c>browser.page_view</c>).
        /// </summary>
        public static bool IsPageView(Activity activity) => activity.OperationName is EventTypePageView or "PageView";

        public PageViewData(int version, string id, string displayName, TimeSpan duration, string? title, string? url, string? referredUri)
            : base(version)
        {
            Properties = new ChangeTrackingDictionary<string, string>();
            Measurements = new ChangeTrackingDictionary<string, double>();

            Duration = duration < SchemaConstants.PageViewData_Duration_LessThanDays
                ? duration.ToString("c", CultureInfo.InvariantCulture)
                : SchemaConstants.Duration_MaxValue;

            Id = id.Truncate(SchemaConstants.PageViewData_Id_MaxLength);
            Name = (title ?? displayName).Truncate(SchemaConstants.PageViewData_Name_MaxLength);
            Url = url.Truncate(SchemaConstants.PageViewData_Url_MaxLength);
            ReferredUri = referredUri.Truncate(SchemaConstants.PageViewData_ReferredUri_MaxLength);
        }

        public PageViewData(int version, Activity activity)
            : this(version,
                activity.SpanId.ToHexString(),
                activity.DisplayName,
                activity.Duration,
                activity.GetTagItem(AttributePageViewTitle) as string,
                activity.GetTagItem(AttributePageViewUrl) as string,
                activity.GetTagItem(AttributePageViewReferrer) as string) {}
    }
}
