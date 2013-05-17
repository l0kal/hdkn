﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hadouken.Common.Data.FluentNHibernate;
using Hadouken.Common.Messaging;
using Hadouken.Hosts.WindowsService;
using NUnit.Framework;
using Hadouken.Impl.Data;
using Moq;
using Hadouken.Data.Models;
using System.IO;
using Hadouken.Configuration;
using Hadouken.Common;

namespace Hadouken.UnitTests.Data
{
    [TestFixture]
    public class DataRepositoryTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            var startupPath = Path.GetDirectoryName(typeof(DataRepositoryTests).Assembly.Location);

            foreach (string file in Directory.GetFiles(startupPath, "*.dll"))
            {
                Assembly.LoadFile(file);
            }

            HdknConfig.ConfigManager = new MemoryConfigManager();

            if (File.Exists("test.db"))
                File.Delete("test.db");

            // apply migrations

            var runner = new DefaultMigratorRunner();
            runner.Up(AppDomain.CurrentDomain.Load("Hadouken.Impl"));
        }

        [Test]
        public void Can_CRUD_records()
        {
            var repo = new FluentNHibernateDataRepository("Data Source=test.db; Version=3;");

            // Saving
            repo.Save(new Setting() { Key = "test", Value = "test" });
            Assert.IsTrue(repo.List<Setting>(st => st.Key == "test").Count() == 1);

            // Updating
            var s = repo.Single<Setting>(q => q.Key == "test");
            s.Key = "test2";

            repo.Update(s);
            Assert.IsTrue(repo.Single<Setting>(s.Id).Key == "test2");

            // Deleting
            repo.Delete(s);
            Assert.IsTrue(repo.Single<Setting>(st => st.Key == "test2") == null);
        }
    }
}
