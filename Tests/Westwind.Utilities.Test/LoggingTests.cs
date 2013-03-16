﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;
using Westwind.Utilities.Logging;
using System.Data;
using Westwind.Utilities.Test;

namespace Westwind.Utilities.Logging.Tests
{
    /// <summary>
    /// NOTE:
    /// These tests rely on TestData ConnectionString in the app.config
    /// file that is configured for SQL Server database access.
    /// </summary>
    [TestClass]
    public class LoggingTests
    {
        private const string STR_TestLogFile = "ApplicationLog";
        private const string STR_ConnectionString = "WestwindToolkitSamples";

        public LoggingTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext) 
        //{

        //}
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            DatabaseInitializer.InitializeDatabase();

            // Set up the LogManager once
            SqlLogAdapter adapter = new SqlLogAdapter(STR_ConnectionString);
            adapter.LogFilename = STR_TestLogFile;
            LogManager.Create(adapter);
        }

        [TestMethod]
        public void CreateDeleteLogWithSqlAdapterTest()
        {
            SqlLogAdapter adapter = new SqlLogAdapter(STR_ConnectionString);
            adapter.LogFilename = "ApplicationLog2";

            bool res = adapter.CreateLog();
            Assert.IsTrue(res, "Application Log not Created");

            res = adapter.DeleteLog();
            Assert.IsTrue(res, "Application Log not deleted");

        }

        [TestMethod]
        public void WriteLogEntryWithAdapterTest()
        {
            SqlLogAdapter adapter = new SqlLogAdapter(STR_ConnectionString);
            adapter.LogFilename = STR_TestLogFile;

            WebLogEntry entry = new WebLogEntry();
            entry.ErrorLevel = ErrorLevels.Info;
            entry.Message = "Testing " + DateTime.Now.ToString();

            bool res = adapter.WriteEntry(entry);

            //bool res = adapter.WriteEntry(entry);

            Assert.IsTrue(res, "Entry couldn't be written to database");

            LogEntry entry2 = adapter.GetEntry(entry.Id);

            Assert.IsTrue(entry.Message == entry2.Message);
            Assert.IsTrue(entry.ErrorLevel == entry2.ErrorLevel);
        }

        [TestMethod]
        public void WriteWebLogEntryTest()
        {
            WebLogEntry entry = new WebLogEntry();
            entry.ErrorLevel = ErrorLevels.Info;
            entry.Message = "Testing " + DateTime.Now.ToString();
            entry.IpAddress = "127.0.0.1";
            entry.Referrer = "http://www.west-wind.com/";
            entry.Url = "/wwstore/default.aspx";
            entry.QueryString = "Logout=true";
            entry.RequestDuration = 0.12M;

            bool res = LogManager.Current.WriteEntry(entry);

            Assert.IsTrue(res, "Entry couldn't be written to database");

            LogEntry entry2 = LogManager.Current.GetWebLogEntry(entry.Id);

            Assert.IsTrue(entry.Message == entry2.Message);
            Assert.IsTrue(entry.ErrorLevel == entry2.ErrorLevel);
        }

        [TestMethod]
        public void LogManagerWriteWebEntryTest()
        {
            WebLogEntry entry = new WebLogEntry();
            entry.Message = "Testing " + DateTime.Now.ToString();
            entry.ErrorLevel = ErrorLevels.Info;
            entry.Url = "/wwstore/admin.aspx";
            entry.QueryString = "action=show";
            entry.PostData = "Bogus".PadRight(3000, '.');

            bool res = LogManager.Current.WriteEntry(entry);

            Assert.IsTrue(res, "WriteWebEntry failed");
        }

        [TestMethod]
        public void LogManagerWriteEntryTest()
        {
            WebLogEntry entry = new WebLogEntry();
            entry.Message = "Testing " + DateTime.Now.ToString();
            entry.ErrorLevel = ErrorLevels.Info;
            entry.Details = "Bogus".PadRight(3000, '.');

            bool res = LogManager.Current.WriteEntry(entry);

            Assert.IsTrue(res, "WriteEntry failed");
        }

        [TestMethod]
        public void XmlLogAdapterTest()
        {

            XmlLogAdapter adapter = new XmlLogAdapter() { ConnectionString = TestContext.DeploymentDirectory + @"\applicationlog.xml" };

            WebLogEntry entry = new WebLogEntry()
            {
                Message = "Entered on: " + DateTime.Now.ToString(),
                ErrorLevel = ErrorLevels.Info,
                Details = StringUtils.RandomString(40,true)
            };

            Assert.IsTrue(adapter.WriteEntry(entry), "Failed to write entry to log");
        }
    }
}
