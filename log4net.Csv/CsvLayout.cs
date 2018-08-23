using log4net.Layout;
using System;
using log4net.Core;
using System.IO;
using System.Collections.Generic;
using System.Text;
using log4net.DateFormatter;

namespace log4net.Csv
{
    /// <summary>
    /// Provides a valid and escaped CSV Log Layout.  Each field is surrounded by "" and all
    /// " characters within a field are properly escaped.  This results in log files that can
    /// be safely processed as valid CSV.
    /// </summary>
    /// <remarks>
    /// Supported properties are similar to the Pattern Layout, but do not require the % prefix
    /// 
    /// Supported Properties:
    ///  * date
    ///  * utcdate
    ///  * message
    ///  * logger
    ///  * level
    ///  * class
    ///  * method
    ///  * exception
    ///  * identity
    ///  * thread
    ///  * username
    ///  * appdomain
    ///  * All custom properties are supported with a case sensitive match
    /// </remarks>
    /// <seealso cref="log4net.Layout.LayoutSkeleton" />
    public class CsvLayout : LayoutSkeleton
    {
        /// <summary>The default pattern if none is supplied</summary>
        public const string DefaultPattern = "%level, %message, %logger, %date";
        private readonly Iso8601DateFormatter DateFormatterIso8601 = new Iso8601DateFormatter();

        /// <summary>Initializes a new instance of the <see cref="CsvLayout" /> class.</summary>
        public CsvLayout() : this(DefaultPattern)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CsvLayout" /> class.</summary>
        /// <param name="fields">comma-separated fields to include in logs.</param>
        public CsvLayout(string fields)
        {
            Fields = fields;
        }

        /// <summary>Gets or sets the included fields as parsed from Fields - available after ActivateOptions has been invoked.</summary>
        /// <value>The included fields.</value>
        public IList<string> IncludedFields { get; set; }

        /// <summary>Gets or sets the comma separated string of fields to include in the logs.</summary>
        /// <value>The fields.</value>
        public string Fields { get; set; }

        /// <summary>Activates the component options (parses the fields)</summary>
        public override void ActivateOptions()
        {
            IgnoresException = false;
            IncludedFields = Fields.Split(new char[] { '%', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>Formats the logging event as Comma-Separated-Values (CSV) as defined by the Fields configuration</summary>
        /// <param name="writer">The TextWriter to write the formatted event to</param>
        /// <param name="loggingEvent">The event to format</param>
        /// <remarks>This method is called by an appender to format the <paramref name="loggingEvent" /> as text.</remarks>
        public override void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            var lastIdx = IncludedFields.Count - 1;
            var builder = new StringBuilder();
            for (var i = 0; i < IncludedFields.Count; i++)
            {
                // Get the fields and value
                var field = IncludedFields[i];
                var value = GetFieldValue(field, loggingEvent);

                // Build the escaped field to log
                builder.Append("\"");
                builder.Append(value.ToString().Replace("\"", "\"\""));
                builder.Append("\"");

                // Add a , if there are still fields to process
                if (i < lastIdx)
                {
                    builder.Append(',');
                }
            }

            writer.WriteLine(builder.ToString());
        }

        /// <summary>Gets the value for the named field on the logging event.  Returns empty string if a match cannot be found.</summary>
        /// <param name="field">The field.</param>
        /// <param name="loggingEvent">The logging event.</param>
        /// <returns>string value of the field to log</returns>
        public string GetFieldValue(string field, LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            var value = string.Empty;
            switch (field)
            {
                case "date":
                    using (var writer = new StringWriter(new StringBuilder()))
                    {
                        DateFormatterIso8601.FormatDate(loggingEvent.TimeStamp, writer);
                        value = writer.GetStringBuilder().ToString();
                    }
                    break;
                case "utcdate":
                    using (var writer = new StringWriter(new StringBuilder()))
                    {
                        DateFormatterIso8601.FormatDate(loggingEvent.TimeStampUtc, writer);
                        value = writer.GetStringBuilder().ToString();
                    }
                    break;
                case "message":
                    value = loggingEvent.RenderedMessage;
                    break;
                case "logger":
                    value = loggingEvent.LoggerName;
                    break;
                case "level":
                    value = loggingEvent.Level.Name;
                    break;
                case "class":
                    value = loggingEvent.LocationInformation.ClassName;
                    break;
                case "method":
                    value = loggingEvent.LocationInformation.MethodName;
                    break;
                case "exception":
                    value = loggingEvent.GetExceptionString();
                    break;
                case "identity":
                    value = loggingEvent.Identity;
                    break;
                case "thread":
                    value = loggingEvent.ThreadName;
                    break;
                case "username":
                    value = loggingEvent.UserName;
                    break;
                case "appdomain":
                    value = loggingEvent.Domain;
                    break;
                default:
                    // Attempt to match field against custom log properties
                    value = (loggingEvent.LookupProperty(field) ?? string.Empty).ToString();
                    break;
            }

            return value ?? string.Empty;
        }
    }
}