using System;
using System.Configuration;
using Microsoft.Web.Administration;

namespace IisConfiguration.Configuration
{
	/// <summary>
	/// Retrieves settings from the appSettings section of the config file.
	/// </summary>
	public class EnvironmentalConfig
    {
		/// <summary>
		/// Gets the 'Environment' key, which should be 'development' or 'release'.
		/// </summary>
        public virtual Environment Environment
        {
            get
            {
                return (Environment)Enum.Parse(typeof(Environment),
                                               ConfigurationManager.AppSettings["Environment"]);
            }
        }

		/// <summary>
		/// Gets the 'ServiceRoot' key, the folder path for the WCF service path (if any).
		/// </summary>
		public virtual string ServiceRoot
        {
            get { return ConfigurationManager.AppSettings["ServiceRoot"]; }
        }

        /// <summary>
        /// Gets the 'ApiRoot' key, the folder path for the api path.
        /// </summary>
        public virtual string ApiRoot
        {
            get { return ConfigurationManager.AppSettings["ApiRoot"]; }
        }

		/// <summary>
		/// Gets the 'WebRoot' key, the folder path for the website.
		/// </summary>
		public virtual string WebRoot
        {
            get { return ConfigurationManager.AppSettings["WebRoot"]; }
        }

		/// <summary>
		/// Gets the 'PingingEnabled' key, which defaults to true if it doesn't exist.
		/// </summary>
		public virtual bool PingingEnabled
        {
            get { return bool.Parse(ConfigurationManager.AppSettings["PingingEnabled"] ?? "true"); }
        }

		/// <summary>
		/// Gets the 'IdleTimeout' key from the configuration. Defaults to 00:00:00 if the key
		/// does not exist. 
		/// </summary>
		public virtual TimeSpan IdleTimeout
        {
            get { return TimeSpan.Parse(ConfigurationManager.AppSettings["IdleTimeout"] ?? "00:00:00"); }
        }

		/// <summary>
		/// Gets the 'SslPassword' key from the configuration, which is the password for the
		/// private key of the SSL certificate.
		/// </summary>
		public virtual string SslPassword
        {
			get { return ConfigurationManager.AppSettings["SslPassword"]; }
        }

		/// <summary>
		/// Gets the 'SslPfxPath' from the configuration, which is the path to the SSL key.
		/// </summary>
		public virtual string SslPfxPath
        {
			get { return ConfigurationManager.AppSettings["SslPfxPath"]; }
        }

        /// <summary>
        /// Gets the 'AppPoolIdentityType' key from configuration, which is the AppPool Identity Type
        /// </summary>
        public ProcessModelIdentityType AppPoolIdentityType
        {
            get { return (ProcessModelIdentityType)Enum.Parse(typeof(ProcessModelIdentityType), ConfigurationManager.AppSettings["AppPoolIdentityType"]); }
        }

        /// <summary>
        /// Gets the 'AppPoolUser' key from configuration, which is the AppPool username
        /// </summary>
        public string AppPoolUser
        {
            get { return ConfigurationManager.AppSettings["AppPoolUser"]; }
        }

        /// <summary>
        /// Gets the 'AppPoolPassword' key from configuration, which is the AppPool password
        /// </summary>
        public string AppPoolPassword
        {
            get { return ConfigurationManager.AppSettings["AppPoolPassword"]; }
        }
    }
}