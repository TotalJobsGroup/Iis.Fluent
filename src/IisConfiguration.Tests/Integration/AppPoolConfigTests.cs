using System;
using IisConfiguration.Tests.Stubs;
using Microsoft.Web.Administration;
using NUnit.Framework;

namespace IisConfiguration.Tests.Integration
{
    [TestFixture]
    public class AppPoolConfigTests
    {
        private const string TestAppPoolName = "test";

        [Test]
        public void should_create_apppool_with_apppool_queuelength_and_rapid_fail_protection()
        {
            var webServerConfig = new WebServerConfig(new LoggerStub());

            webServerConfig
                .AddAppPool(TestAppPoolName, "v2.0", ManagedPipelineMode.Integrated, ProcessModelIdentityType.LocalService)
                .WithAppPoolQueueLength(10)
                .WithRapidFailProtection(true)
                .Commit();

            var serverManger = new ServerManager();
            var appPool = serverManger.ApplicationPools[TestAppPoolName];
            Assert.That(appPool.QueueLength, Is.EqualTo(10));
            Assert.That(appPool.Failure["RapidFailProtection"].ToString(), Is.EqualTo("True"));
        }

        [Test]
        public void should_create_app_pool()
        {
            var webServerConfig = new WebServerConfig(new LoggerStub());

            webServerConfig
                .AddAppPool(TestAppPoolName, "v2.0", ManagedPipelineMode.Integrated, ProcessModelIdentityType.LocalService)
                .Commit();
            
            var serverManger = new ServerManager();
            var appPool = serverManger.ApplicationPools[TestAppPoolName];
            Assert.That(appPool.ManagedRuntimeVersion, Is.EqualTo("v2.0"));
            Assert.That(appPool.ManagedPipelineMode, Is.EqualTo(ManagedPipelineMode.Integrated));
            Assert.That(appPool.ProcessModel.IdentityType, Is.EqualTo(ProcessModelIdentityType.LocalService));
        }

        [Test]
        public void should_create_apppool_with_timeout_and_pinging_not_enabled()
        {
            var webServerConfig = new WebServerConfig(new LoggerStub());

            webServerConfig
                .AddAppPool(TestAppPoolName, "v2.0", ManagedPipelineMode.Integrated, ProcessModelIdentityType.LocalService)
                .WithProcessModel(TimeSpan.FromDays(1), false)
                .Commit();

            var serverManger = new ServerManager();
            var appPool = serverManger.ApplicationPools[TestAppPoolName];
            Assert.That(appPool.ProcessModel.PingingEnabled, Is.False);
            Assert.That(appPool.ProcessModel.IdleTimeout, Is.EqualTo(TimeSpan.FromDays(1)));
        }

        [Test]
        public void should_set_private_memory_limit_to_unlimited_by_default()
        {
            var webServerConfig = new WebServerConfig(new LoggerStub());

            webServerConfig
                .AddAppPool(TestAppPoolName, "v2.0", ManagedPipelineMode.Integrated, ProcessModelIdentityType.LocalService)
                .Commit();

            var serverManger = new ServerManager();
            var appPool = serverManger.ApplicationPools[TestAppPoolName];
            Assert.That(appPool.Recycling.PeriodicRestart.PrivateMemory, Is.EqualTo(0));
        }

        [Test]
        public void should_set_private_memory_limit()
        {
            var webServerConfig = new WebServerConfig(new LoggerStub());

            webServerConfig
                .AddAppPool(TestAppPoolName, "v2.0", ManagedPipelineMode.Integrated, ProcessModelIdentityType.LocalService, 666)
                .Commit();

            var serverManger = new ServerManager();
            var appPool = serverManger.ApplicationPools[TestAppPoolName];
            Assert.That(appPool.Recycling.PeriodicRestart.PrivateMemory, Is.EqualTo(666));
        }

        [SetUp]
        public void Setup()
        {
            DeleteTestAppPool();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteTestAppPool();
        }

        private static void DeleteTestAppPool()
        {
            var mgr = new ServerManager();
            if (mgr.ApplicationPools[TestAppPoolName] != null)
                mgr.ApplicationPools[TestAppPoolName].Delete();
            mgr.CommitChanges();
        }
    }
}
