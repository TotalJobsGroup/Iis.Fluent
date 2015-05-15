using System;
using System.IO;
using System.Linq;
using IisConfiguration.Tests.Stubs;
using Microsoft.Web.Administration;
using NUnit.Framework;

namespace IisConfiguration.Tests.Integration
{
	[TestFixture]
	public class SiteConfigTests
	{
		private const string TestSiteName = "test";

		[Test]
		public void should_add_site_with_alternate_logging_directory()
		{
			// Arrange
			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 8887)
				.WithLogDirectory(@"C:\logs")
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];

			Assert.That(site.Id, Is.EqualTo(563));
			Assert.That(site.Bindings.ByProtocol("http").BindingInformation, Is.EqualTo("*:8887:"));
			Assert.That(site.LogFile.Directory, Is.EqualTo(@"C:\logs"));
		}

		[Test]
		public void should_remove_virtual_directory()
		{
			// Arrange
			const string virtualDirectoryPhysicalPath = @"C:\temp\test";
			const string iisPath = "/test";

			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 8887)
				.AddVirtualDirectory(iisPath, virtualDirectoryPhysicalPath)
				.Commit();

			serverConfig
				.GetSite(TestSiteName)
				.DeleteVirtualDirectory("/test")
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];

			Assert.That(site.Applications[0].VirtualDirectories[iisPath], Is.Null);
		}

		[Test]
		public void should_add_site()
		{
			// Arrange
			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 8887)
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];

			Assert.That(site.Id, Is.EqualTo(563));
			Assert.That(site.Bindings.ByProtocol("http").BindingInformation, Is.EqualTo("*:8887:"));
			Assert.That(site.ServerAutoStart, Is.True);
		}

		[Test]
		public void should_add_site_with_application()
		{
			// Arrange
			const string applicationPhysicalPath = @"C:\temp";
			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 8887)
				.AddApplication("/", applicationPhysicalPath, TestSiteName)
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];
			Assert.That(site.Applications[0].ApplicationPoolName, Is.EqualTo(TestSiteName));
			Assert.That(site.Applications[0].VirtualDirectories[0].PhysicalPath, Is.EqualTo(applicationPhysicalPath));
		}

		[Test]
		public void should_add_virtual_directory()
		{
			// Arrange
			const string virtualDirectoryPhysicalPath = @"C:\temp\test";
			const string iisPath = "/test";
			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 8887)
				.AddVirtualDirectory(iisPath, virtualDirectoryPhysicalPath)
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];

			Assert.That(site.Applications[0].VirtualDirectories[iisPath].PhysicalPath,
				Is.EqualTo(virtualDirectoryPhysicalPath));
		}

		[Test]
		public void should_add_virtual_directory_with_config_substitution()
		{
			// Arrange
			string iisPath = "/test";
			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 8887)
				.AddVirtualDirectory(iisPath, @"{WebRoot}\mysite")
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];

			Assert.That(site.Applications[0].VirtualDirectories[iisPath].PhysicalPath, Is.EqualTo(@"d:\websites\mysite"));
		}

		[Test]
		public void should_create_site_with_ssl_port()
		{
			// Arrange
			string certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "iisconfiguration.pfx");
			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 8887)
				.WithSecurePort(4887, certPath, "password")
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];

			Assert.That(site.Bindings.ByProtocol("https").BindingInformation, Is.EqualTo("*:4887:"));
		}

		[Test]
		public void should_add_binding()
		{
			// Arrange
			string hostname = "";
			string expectedBindingInfo = "*:4887:";
			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 4887)
				.AddBinding(4887, hostname, false)
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];
			Binding binding = site.Bindings.First(x => x.BindingInformation == expectedBindingInfo);
			Assert.That(binding, Is.Not.Null);
		}

		[Test]
		public void should_delete_binding()
		{
			// Arrange
			string hostname = "helloworld.local";
			string bindingInfo = "*:4887:helloworld.local";
			var serverConfig = new WebServerConfig(new LoggerStub());

			serverConfig
				.AddSite(TestSiteName, 563, 4887)
				.AddBinding(4887, hostname, false)
				.Commit();

			// Act
			serverConfig
				.GetSite(TestSiteName)
				.DeleteBinding(4887, hostname)
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];

			Binding binding = site.Bindings.FirstOrDefault(x => x.BindingInformation == bindingInfo);
			Assert.That(binding, Is.Null);

			Assert.That(site.Bindings.ByProtocol("http").BindingInformation, Is.EqualTo("*:4887:"));
		}

		[Test]
		public void should_add_protocol()
		{
			// Arrange
			string hostname = "helloworld.local";
			string expectedProtocol = "http,net.pipe";
			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 4887)
				.AddBinding(4887, hostname, false)
				.AddProtocol(4887, hostname, "net.pipe")
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];
			var application = site.Applications.First();
			Assert.AreEqual(expectedProtocol, application.EnabledProtocols);
		}

		[Test]
		public void should_delete_protocol()
		{
			// Arrange
			string hostname = "helloworld.local";
			string expectedProtocol = "net.pipe";
			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 4887)
				.AddBinding(4887, hostname, false)
				.AddProtocol(4887, hostname, "net.pipe")
				.DeleteProtocol(4887, hostname, "http")
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];
			var application = site.Applications.First();
			Assert.AreEqual(expectedProtocol, application.EnabledProtocols);
		}

		[Test]
		public void should_add_non_http_binding()
		{
			// Arrange
			string expectedBindingInfo = "helloworld.local";
			var serverConfig = new WebServerConfig(new LoggerStub());

			// Act
			serverConfig
				.AddSite(TestSiteName, 563, 4887)
				.AddNonHttpBinding(expectedBindingInfo, "net.pipe")
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];
			Binding binding = site.Bindings.First(x => x.BindingInformation == expectedBindingInfo);
			Assert.That(binding, Is.Not.Null);
		}

		[Test]
		public void should_delete_non_http_binding()
		{
			// Arrange
			string hostname = "helloworld.local";
			string bindingInfo = "*:helloworld.local";
			var serverConfig = new WebServerConfig(new LoggerStub());

			serverConfig
				.AddSite(TestSiteName, 563, 4887)
				.AddNonHttpBinding(hostname, "net.pipe")
				.Commit();

			// Act
			serverConfig
				.GetSite(TestSiteName)
				.DeleteNonHttpBinding(hostname, "net.pipe")
				.Commit();

			// Assert
			var mgr = new ServerManager();
			var site = mgr.Sites[TestSiteName];

			Binding binding = site.Bindings.FirstOrDefault(x => x.BindingInformation == bindingInfo);
			Assert.That(binding, Is.Null);
		}

		[SetUp]
		public void Setup()
		{
			DeleteTestSite();
		}

		[TearDown]
		public void TearDown()
		{
			DeleteTestSite();
		}

		private static void DeleteTestSite()
		{
			var mgr = new ServerManager();

			if (mgr.Sites[TestSiteName] != null)
			{
				mgr.Sites[TestSiteName].Delete();
				mgr.CommitChanges();
			}
		}
	}
}