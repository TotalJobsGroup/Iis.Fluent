using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.Administration;
using System.Configuration;
using IisConfiguration.Logging;

namespace IisConfiguration
{
	/// <summary>
	/// 
	/// </summary>
	public class SiteConfig : IDisposable
	{
		private readonly IList<long> _currentSiteIds = new List<long>();
		private readonly ServerManager _mgr;
		private readonly ILogger _logger;

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="mgr"></param>
	    /// <param name="logger"></param>
		public SiteConfig(ServerManager mgr, ILogger logger)
		{
			_mgr = mgr;
			_logger = logger;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="site"></param>
		/// <returns></returns>
		public SiteConfig AddSite(Site site)
		{
			DeleteSite(site.Name);
			_currentSiteIds.Add(site.Id);
			_logger.Log("Added site " + site.Name);

			_mgr.Sites.Add(site);
			return this;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public SiteConfig AddSite(string name, int port)
        {
            return AddSite(name, port, port);
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
			DeleteSite(name);
			_currentSiteIds.Add(id);
			_logger.Log("Added site " + name);

			var site = _mgr.Sites.Add(name, "", port);
			site.Id = id;
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="port"></param>
		/// <param name="hostname"></param>
		/// <param name="isHttps"></param>
		/// <returns></returns>
		public SiteConfig AddBinding(int port, string hostname, bool isHttps)
		{
			// * is the ip address
			string bindingInformation = string.Format("*:{0}:{1}", port, hostname);
			string protocol = isHttps ? "https" : "http";

			foreach (Site site in GetCurrentSites())
			{
				// See if it exists first
				bool exists = false;
				foreach (Binding binding in site.Bindings)
				{
					if (binding.BindingInformation == bindingInformation)
					{
						exists = true;
						break;
					}
				}

				if (!exists)
				{
					site.Bindings.Add(bindingInformation, protocol);
					_logger.Log("Added binding " + hostname + " for " + site);
				}
				else
				{
					_logger.Log("Binding " + hostname + " for " + site + " already exists!");
				}
			}

			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bindingInformation"></param>
		/// <param name="protocol"></param>
		/// <returns></returns>
		public SiteConfig AddNonHttpBinding(string bindingInformation, string protocol)
		{
			foreach (var site in GetCurrentSites())
			{
				var toRemove = site.Bindings.Where(binding => binding.BindingInformation == bindingInformation).ToList();

				foreach (var binding in toRemove)
				{
					site.Bindings.Remove(binding);
					_logger.Log("Binding " + bindingInformation + " for " + site + " removed!");
				}

				site.Bindings.Add(bindingInformation, protocol);
				_logger.Log("Added binding " + bindingInformation + " for " + site);
			}

			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="port"></param>
		/// <param name="hostname"></param>
		/// <returns></returns>
		public SiteConfig DeleteBinding(int port, string hostname)
		{
			string bindingInformation = string.Format("*:{0}:{1}", port, hostname);

			foreach (Site site in GetCurrentSites())
			{
				Binding siteBinding = null;
				foreach (Binding binding in site.Bindings)
				{
					if (binding.BindingInformation == bindingInformation)
					{
						siteBinding = binding;
						break;
					}
				}

				if (siteBinding != null)
				{
					site.Bindings.Remove(siteBinding);
					_logger.Log("Deletedf binding " + hostname + " for " + site + ".");
				}
				else
				{
					_logger.Log("Binding " + hostname + " for " + site + " doesn't exist!");
				}
			}

			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hostname"></param>
		/// <param name="protocol"></param>
		/// <returns></returns>
		public SiteConfig DeleteNonHttpBinding(string hostname, string protocol)
		{
			string bindingInformation = string.Format("*:{0}", hostname);

			foreach (Site site in GetCurrentSites())
			{
				Binding siteBinding = null;
				foreach (Binding binding in site.Bindings)
				{
					if (binding.BindingInformation == bindingInformation)
					{
						siteBinding = binding;
						break;
					}
				}

				if (siteBinding != null)
				{
					site.Bindings.Remove(siteBinding);
					_logger.Log("Removed binding " + hostname + " for " + site + ".");
				}
				else
				{
					_logger.Log("Binding " + hostname + " for " + site + " doesn't exist!");
				}
			}

			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public SiteConfig GetSite(string name)
		{
			var site = _mgr.Sites[name];
			if (site != null)
				_currentSiteIds.Add(site.Id);

			return this;
		}

		private void DeleteSite(string name)
		{
			if (_mgr.Sites[name] != null)
			{
				_mgr.Sites.Remove(_mgr.Sites[name]);
				Commit();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="applicationPath"></param>
		/// <param name="physicalPath"></param>
		/// <param name="applicationPool"></param>
		/// <returns></returns>
		public SiteConfig AddApplication(string applicationPath, string physicalPath, string applicationPool)
		{
			DeleteApplication(applicationPath);
			physicalPath = SubstituteConfigTokens(physicalPath);

			foreach (var site in GetCurrentSites())
			{
				site.Applications.Add(applicationPath, physicalPath);
				site.Applications[applicationPath].ApplicationPoolName = applicationPool;
			}
			_logger.Log("Added application " + applicationPath + " running under " + applicationPool + " app pool");
			return this;
		}

		private string SubstituteConfigTokens(string path)
		{
			foreach (string key in ConfigurationManager.AppSettings.Keys)
			{
				path = path.Replace("{" + key + "}", ConfigurationManager.AppSettings[key]);
			}
			return path;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="applicationPath"></param>
		/// <returns></returns>
		public SiteConfig DeleteApplication(string applicationPath)
		{
			foreach (var site in GetCurrentSites())
			{
				var application = site.Applications[applicationPath];
				if (application != null)
					site.Applications.Remove(application);
			}
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="iisPath"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public SiteConfig AddVirtualDirectory(string iisPath, string filePath)
		{
			return AddVirtualDirectory("/", iisPath, filePath);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="applicationPath"></param>
		/// <param name="iisPath"></param>
		/// <param name="physicalPath"></param>
		/// <returns></returns>
		public SiteConfig AddVirtualDirectory(string applicationPath, string iisPath, string physicalPath)
		{
			physicalPath = SubstituteConfigTokens(physicalPath);
			foreach (var site in GetCurrentSites())
			{
				var applicationToAddFolder = site.Applications.Single(x => x.Path == applicationPath);
				applicationToAddFolder.VirtualDirectories.Add(iisPath, physicalPath);
			}
			_logger.Log("Added virtual directory " + iisPath);
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="iisPath"></param>
		/// <returns></returns>
		public SiteConfig DeleteVirtualDirectory(string iisPath)
		{
			return DeleteVirtualDirectory("/", iisPath);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="applicationPath"></param>
		/// <param name="iisPath"></param>
		/// <returns></returns>
		public SiteConfig DeleteVirtualDirectory(string applicationPath, string iisPath)
		{
			foreach (var site in GetCurrentSites())
			{
				var applicationToAddFolder = site.Applications.Single(x => x.Path == applicationPath);
				if (applicationToAddFolder.VirtualDirectories[iisPath] != null)
					applicationToAddFolder.VirtualDirectories[iisPath].Delete();
			}

			_logger.Log("Removed virtual directory " + iisPath);
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		public SiteConfig WithPort(int port)
		{
			foreach (var currentSite in GetCurrentSites())
			{
				currentSite.Bindings.Add("*:" + port + ":", "http");
			}
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sslPort"></param>
		/// <param name="certificatePfxPath"></param>
		/// <param name="certificatePassword"></param>
		/// <returns></returns>
		public SiteConfig WithSecurePort(int sslPort, string certificatePfxPath, string certificatePassword)
		{
			// SSL is set up in three places in IIS7
			// The certificate comes from a pfx file and is imported into the machine's certificate store
			// The port that the website listens for SSL traffic on is stored in the applicationHost.config for the site
			// The mapping between the port and the certificate is stored in the HTTP.sys configuration
			// More info here: http://learn.iis.net/page.aspx/144/how-to-set-up-ssl-on-iis/

			var certificate = new X509Certificate2(certificatePfxPath, certificatePassword,
												   X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet |
												   X509KeyStorageFlags.Exportable);
			AddToCertStore(certificate, new X509Store(StoreName.My, StoreLocation.LocalMachine));
			AddToCertStore(certificate, new X509Store(StoreName.Root, StoreLocation.LocalMachine));

			foreach (var currentSite in GetCurrentSites())
			{
				// You can see whether the http.sys binding has worked by running this command: netsh http show ssl
				currentSite.Bindings.Add("*:" + sslPort + ":", certificate.GetCertHash(), "MY");
			}

			_logger.Log("Installed SSL certificate path " + certificatePfxPath + " on port " + sslPort);
			return this;
		}

		private void AddToCertStore(X509Certificate2 certificate, X509Store store)
		{
			store.Open(OpenFlags.ReadWrite);
			store.Remove(certificate);

			// It is installed into the LocalMachine/Personal/certificates store 
			// to see it has installed, open up mmc -> file/add remove snap-in -> choose certificates -> computer account
			store.Add(certificate);
			store.Close();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="enableLogging"></param>
		/// <returns></returns>
		public SiteConfig WithLogging(bool enableLogging)
		{
			foreach (var site in GetCurrentSites())
			{
				site.LogFile.Enabled = enableLogging;
			}
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="enableLogging"></param>
		/// <param name="extraLogFlagsToAdd"></param>
		/// <returns></returns>
		public SiteConfig WithLogging(bool enableLogging, LogExtFileFlags extraLogFlagsToAdd)
		{
			foreach (var site in GetCurrentSites())
			{
				site.LogFile.Enabled = enableLogging;
				site.LogFile.LogExtFileFlags |= extraLogFlagsToAdd;
			}
			return this;
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="logDirectory"></param>
	    /// <returns></returns>
	    public SiteConfig WithLogDirectory(string logDirectory)
		{
			foreach (var site in GetCurrentSites())
			{
				site.LogFile.Directory = logDirectory;
			}
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="port"></param>
		/// <param name="hostname"></param>
		/// <param name="protocol"></param>
		/// <returns></returns>
		public SiteConfig AddProtocol(int port, string hostname, string protocol)
		{
			// * is the ip address
			string bindingInformation = string.Format("*:{0}:{1}", port, hostname);

			foreach (Site site in GetCurrentSites())
			{
				if (site.Bindings.Any(x => x.BindingInformation == bindingInformation))
				{
					foreach (var application in site.Applications)
					{
						if (application.EnabledProtocols.Contains(protocol))
						{
							_logger.Log("Protocol " + protocol + " for " + site + " already exists!");
							break;
						}
						application.EnabledProtocols =
							application.EnabledProtocols.Insert(application.EnabledProtocols.Length, "," + protocol);

						_logger.Log("Added protocol " + hostname + " for " + site);
					}
				}
			}

			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="port"></param>
		/// <param name="hostname"></param>
		/// <param name="protocol"></param>
		/// <returns></returns>
		public SiteConfig DeleteProtocol(int port, string hostname, string protocol)
		{
			// * is the ip address
			string bindingInformation = string.Format("*:{0}:{1}", port, hostname);

			foreach (Site site in GetCurrentSites())
			{
				if (site.Bindings.Any(x => x.BindingInformation == bindingInformation))
				{
					foreach (var application in site.Applications)
					{
						if (application.EnabledProtocols.Contains(protocol))
						{
							var protocols = application.EnabledProtocols.Split(',').ToList();

							protocols.Remove(protocol);

							application.EnabledProtocols =
								application.EnabledProtocols = string.Join(",", protocols.ToArray());

							_logger.Log("Removed protocol " + protocol + " for " + site);
							break;
						}
					}
				}
			}

			return this;
		}

		/// <summary>
		/// Commits all changes made to the <see cref="ServerManager"/>
		/// </summary>
		/// <returns></returns>
		public void Commit()
		{
			_mgr.CommitChanges();
		}

		/// <summary>
		/// Commits any changes, and disposes of the underlying <see cref="ServerManager"/>.
		/// </summary>
		public void Dispose()
		{
			Commit();
			_mgr.Dispose();
		}

		private IEnumerable<Site> GetCurrentSites()
		{
			return from site in _mgr.Sites
				   where _currentSiteIds.Contains(site.Id)
				   select site;
		}
	}
}