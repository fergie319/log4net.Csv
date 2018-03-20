using log4net.Core;
using log4net.Csv;
using log4net.DateFormatter;
using log4net.Util;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace CsvTests
{
    [TestFixture]
    public class CsvLayoutTests
    {
        [Test]
        public void CsvLayout_Format_NullTextWriter_ArgumentNullException()
        {
            var fakeLoggingEvent = new LoggingEvent(new LoggingEventData());
            var underTest = new CsvLayout();
            Assert.Throws<ArgumentNullException>(() => underTest.Format(null, fakeLoggingEvent));
        }

        [Test]
        public void CsvLayout_Format_ActivateOptions_Success()
        {
            var fakeFields = "field1, field2, field3";
            var underTest = new CsvLayout(fakeFields);
            underTest.ActivateOptions();

            Assert.AreEqual(fakeFields, underTest.Fields);
            CollectionAssert.AreEqual(
                new[] { "field1", "field2", "field3" },
                underTest.IncludedFields);
        }

        [Test]
        public void CsvLayout_ActivateOptions_DefaultFields_Success()
        {
            var underTest = new CsvLayout();
            underTest.ActivateOptions();

            Assert.AreEqual(CsvLayout.DefaultPattern, underTest.Fields);
        }

        [Test]
        public void CsvLayout_Format_DefaultFields_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                TimeStampUtc = new DateTime(2012, 12, 14, 12, 20, 42),
                Level = Level.Debug,
                Message = "The answer",
                LoggerName = "fortytwo"
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.Format(fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual("\"DEBUG\",\"The answer\",\"fortytwo\",\"2012-12-14 07:20:42,000\"\r\n", result);
        }

        [Test]
        public void CsvLayout_Format_HonourFieldOrder_Success()
        {
            // Fake Data
            var fakeFields = "date, logger, level, message";
            var fakeLoggingData = new LoggingEventData()
            {
                TimeStampUtc = new DateTime(2012, 12, 14, 12, 20, 42),
                Level = Level.Debug,
                Message = "The answer",
                LoggerName = "fortytwo"
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout(fakeFields);
            underTest.ActivateOptions();
            var result = underTest.Format(fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual("\"2012-12-14 07:20:42,000\",\"fortytwo\",\"DEBUG\",\"The answer\"\r\n", result);
        }

        [Test]
        public void CsvLayout_Format_EscapeDoubleQuotes_Success()
        {
            // Fake Data
            var fakeFields = "message";
            var fakeLoggingData = new LoggingEventData()
            {
                Message = "Th\"e \"ans\"wer",
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout(fakeFields);
            underTest.ActivateOptions();
            var result = underTest.Format(fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual("\"Th\"\"e \"\"ans\"\"wer\"\r\n", result);
        }
        
        [Test]
        public void CsvLayout_GetFieldValue_NullLoggingEvent_ArgumentNullException()
        {
            // Fake Data
            var fakeFields = "message";
            var fakeLoggingData = new LoggingEventData()
            {
                Message = "Th\"e \"ans\"wer",
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout(fakeFields);
            underTest.ActivateOptions();
            var result = underTest.Format(fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual("\"Th\"\"e \"\"ans\"\"wer\"\r\n", result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_NullProperty_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                Message = null
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("message", fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(string.Empty, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_CustomProperty_Success()
        {
            // Fake Data
            var fakePropertyName = "Towels";
            var fakePropertyValue = "Of Course";
            var fakeLoggingData = new LoggingEventData()
            {
                Properties = new PropertiesDictionary()
            };
            fakeLoggingData.Properties[fakePropertyName] = fakePropertyValue;
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue(fakePropertyName, fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(fakePropertyValue, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_UnknownProperty_Success()
        {
            // Fake Data
            var fakePropertyName = "Panic";
            var fakeLoggingData = new LoggingEventData();
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue(fakePropertyName, fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(string.Empty, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_Date_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                TimeStamp = new DateTime(2012, 12, 14, 12, 20, 42)
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("date", fakeLoggingEvent);

            // Verify Results
            var isoDateFormatter = new Iso8601DateFormatter();
            var stringWriter = new StringWriter(new StringBuilder());
            isoDateFormatter.FormatDate(fakeLoggingData.TimeStamp, stringWriter);
            var expectedDateString = stringWriter.GetStringBuilder().ToString();
            CollectionAssert.AreEqual(expectedDateString, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_UtcDate_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                TimeStampUtc = new DateTime(2012, 12, 14, 12, 20, 42)
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("utcdate", fakeLoggingEvent);

            // Verify Results
            var isoDateFormatter = new Iso8601DateFormatter();
            var stringWriter = new StringWriter(new StringBuilder());
            isoDateFormatter.FormatDate(fakeLoggingData.TimeStampUtc, stringWriter);
            var expectedDateString = stringWriter.GetStringBuilder().ToString();
            CollectionAssert.AreEqual(expectedDateString, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_Message_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                Message = "The answer",
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("message", fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual("The answer", result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_Logger_Success()
        {
            // Fake Data
            var fakeLoggerName = "Bob";
            var fakeLoggingData = new LoggingEventData()
            {
                LoggerName = fakeLoggerName
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("logger", fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(fakeLoggerName, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_Level_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                Level = Level.Error
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("level", fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(Level.Error.DisplayName, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_ClassAndMethod_Success()
        {
            // Fake Data
            var fakeClassName = "Foo";
            var fakeMethodName = "Bar";
            var fakeFileName = "FooBar.exe";
            var fakeLineNumber = "42";
            var fakeLoggingData = new LoggingEventData()
            {
                LocationInfo = new LocationInfo(fakeClassName, fakeMethodName, fakeFileName, fakeLineNumber)
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var classResult = underTest.GetFieldValue("class", fakeLoggingEvent);
            var methodResult = underTest.GetFieldValue("method", fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(fakeClassName, classResult);
            CollectionAssert.AreEqual(fakeMethodName, methodResult);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_Exception_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                ExceptionString = new Exception("Don't Panic!").ToString()
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("exception", fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(fakeLoggingData.ExceptionString, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_Identity_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                Identity = "Bob"
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("identity", fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(fakeLoggingData.Identity, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_Thread_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                ThreadName = "OriginalBob"
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("thread", fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(fakeLoggingData.ThreadName, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_UserName_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                UserName = "Ryker"
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("username", fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(fakeLoggingData.UserName, result);
        }

        [Test]
        public void CsvLayout_GetFieldValue_BuiltInProperties_AppDomain_Success()
        {
            // Fake Data
            var fakeLoggingData = new LoggingEventData()
            {
                Domain = "Homer"
            };
            var fakeLoggingEvent = new LoggingEvent(fakeLoggingData);

            // Execute Test
            var underTest = new CsvLayout();
            underTest.ActivateOptions();
            var result = underTest.GetFieldValue("appdomain", fakeLoggingEvent);

            // Verify Results
            CollectionAssert.AreEqual(fakeLoggingData.Domain, result);
        }
    }
}
