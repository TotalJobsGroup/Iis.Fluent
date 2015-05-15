using System;
using System.Diagnostics;
using System.Text;
using IisConfiguration.Logging;
using Microsoft.Web.Administration;
using Microsoft.Win32;

namespace IisConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public class WebServerConfig : IDisposable
    {
		private static readonly string ASPNET_REGIIS_V2_PATH = @"C:\Windows\Microsoft.NET\Framework\v2.0.50727\aspnet_regiis";
		private static readonly string ASPNET_REGIIS_V4_PATH = @"C:\Windows\Microsoft.NET\Framework\v2.0.50727\aspnet_regiis";

        private readonly ILogger _logger;
        private readonly ServerManager _mgr;

		/// <summary>
		/// 
		/// </summary>
		public bool IsIis7OrAbove
		{
			get
			{
				RegistryKey regKeyPath = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\InetStp\\");
				if (regKeyPath != null)
					return (int)regKeyPath.GetValue("MajorVersion") >= 7;

				return false;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public WebServerConfig(ILogger logger)
        {
            _logger = logger;
            _mgr = new ServerManager();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public SiteConfig AddSite(string name, int id, int port)
        {
            var siteConfig = new SiteConfig(_mgr, _logger);
            return siteConfig.AddSite(name, id, port);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SiteConfig GetSite(string name)
        {
            var siteConfig = new SiteConfig(_mgr, _logger);
            return siteConfig.GetSite(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool SiteExists(string name)
        {
            return _mgr.Sites[name] != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="runtimeVersion"></param>
        /// <param name="managedPipelineMode"></param>
        /// <param name="identityType"></param>
        /// <returns></returns>
        public AppPoolConfig AddAppPool(string name, string runtimeVersion, ManagedPipelineMode managedPipelineMode, ProcessModelIdentityType identityType)
        {
            var appPoolConfig = new AppPoolConfig(_mgr, _logger);
            return appPoolConfig.AddAppPool(name, runtimeVersion, managedPipelineMode, identityType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="runtimeVersion"></param>
        /// <param name="managedPipelineMode"></param>
        /// <param name="identityType"></param>
        /// <param name="privateMemoryLimit"></param>
        /// <returns></returns>
        public AppPoolConfig AddAppPool(string name, string runtimeVersion, ManagedPipelineMode managedPipelineMode, ProcessModelIdentityType identityType, int privateMemoryLimit)
        {
            var appPoolConfig = new AppPoolConfig(_mgr, _logger);
            return appPoolConfig.AddAppPool(name, runtimeVersion, managedPipelineMode, identityType, privateMemoryLimit);
        }

		/// <summary>
		/// Encrypts a web.config section using the aspnet_iis tool with RsaProtectedConfigurationProvider.
		/// </summary>
		/// <param name="siteId">The id of the site the web.config belongs to.</param>
		/// <param name="sectionName">The section to encrypt, i.e. connectionStrings. It will encrypt a
		/// config file that's referenced via the configSource= attribute.</param>
		/// <param name="dotNet4">Whether to use the .NET 4 framework aspnet_regiis tool (true by default).</param>
		/// <returns>The current SiteConfig.</returns>
		public void EncryptWebConfigSection(int siteId, string sectionName, bool dotNet4 = true)
		{
			var argumentBuilder = new StringBuilder();
			argumentBuilder.AppendFormat("-pe {0} ", sectionName);
			argumentBuilder.Append("-app \"/\" ");
			argumentBuilder.Append("-prov \"RsaProtectedConfigurationProvider\" ");
			argumentBuilder.AppendFormat("-site {0}", siteId);

			if (dotNet4)
			{
				_logger.Log(string.Format("Encrypting web.config section (.NET4): {0} {1}", ASPNET_REGIIS_V4_PATH, argumentBuilder));
				Process.Start(ASPNET_REGIIS_V4_PATH, argumentBuilder.ToString());
			}
			else
			{
				_logger.Log(string.Format("Encrypting web.config section (.NET2): {0} {1}", ASPNET_REGIIS_V2_PATH, argumentBuilder));
				Process.Start(ASPNET_REGIIS_V2_PATH, argumentBuilder.ToString());
			}
		}

		/// <summary>
		/// Commits all changes made to the <see cref="ServerManager"/>
		/// </summary>
		/// <returns></returns>
		public WebServerConfig Commit()
		{
			_mgr.CommitChanges();
			return this;
		}

		/// <summary>
		/// Commits any changes, and disposes of the underlying <see cref="ServerManager"/>.
		/// </summary>
		public void Dispose()
		{
			Commit();
			_mgr.Dispose();
		}
    }
}