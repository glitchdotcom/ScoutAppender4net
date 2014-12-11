using System;
using System.Collections.Specialized;
using System.Net;

namespace ScoutAppender4net
{
    public sealed class ScoutAppender : log4net.Appender.AppenderSkeleton
    {
        /// <summary>
        /// The URL of your FogBugz installation, e.g. "https://example.fogbugz.com"
        /// </summary>
        public string FogBugzUrl
        {
            get { return fbUrl; }
            set
            {
                var baseUri = new Uri(value, UriKind.Absolute);
                scoutUri = new Uri(baseUri, "scoutSubmit.asp");
                fbUrl = value;
            }
        }
        private string fbUrl;
        protected Uri scoutUri;

        /// <summary>
        /// The full name of the FogBugz user the case creation or edit should be made as.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The Project that new cases should be created in (must be a valid project name).
        /// Note that if BugzScout appends to an existing case, this field is ignored.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// The Area that new cases should go into (must be a valid area in the ScoutProject).
        /// Note that if BugzScout appends to an existing case, this field is ignored.
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// An email address to associate with the report, often the customer’s email.
        /// This overwrites the correspondent field on the case with each appended occurrence,
        /// so it is automatically included at the end of the case event as well.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// An option to override the normal automatic consolidation of BugzScout reports.
        /// If this is set to <value>true</value>, then a new case will always created from this submission.
        /// </summary>
        public bool ForceNewBug { get; set; }

        protected override bool RequiresLayout { get { return true; } }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            AssertParameters();

            var extra = RenderLoggingEvent(loggingEvent);
            var description = extra.Split(new[] { Environment.NewLine }, 1, StringSplitOptions.RemoveEmptyEntries)[0];

            ScoutSubmit(description, extra);
        }

        void ScoutSubmit(string description, string extra)
        {
            var values = new NameValueCollection
            {
                {"ScoutUserName", Username},
                {"ScoutProject", Project},
                {"ScoutArea", Area},
                {"Description", description},
            };
            if (!string.IsNullOrEmpty(extra)) { values.Add("Extra", extra); }
            if (!string.IsNullOrEmpty(Email)) { values.Add("Email", Email); }
            if (ForceNewBug) { values.Add("ForceNewBug", "1"); }

            // Fire and forget. Should probably not forget.
            new WebClient().UploadValues(scoutUri, "POST", values);
        }

        protected void AssertParameters()
        {
            if (scoutUri == null) { throw new InvalidOperationException("FogBugzUrl must be set correctly"); }
            if (string.IsNullOrEmpty(Username)) { throw new InvalidOperationException("Username must be set"); }
            if (string.IsNullOrEmpty(Project)) { throw new InvalidOperationException("Project must be set"); }
            if (string.IsNullOrEmpty(Area)) { throw new InvalidOperationException("Area must be set"); }
        }
    }
}
