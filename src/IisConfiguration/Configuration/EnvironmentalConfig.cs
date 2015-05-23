using System;
using System.Configuration;
using System.IO;
using Microsoft.Web.Administration;

namespace IisConfiguration.Configuration
{
	/// <summary>
	/// Retrieves settings from the appSettings section of the config file.
	/// </summary>
	public class EnvironmentalConfig
	{
		private const string SRC_PATH = "{src-path}";

		/// <summary>
		/// Gets the 'Environment' key, which should be 'development' or 'release'.
		/// </summary>
		public virtual Environment Environment
		{
			get
			{
				return (Environment)Enum.Parse(typeof(Environment), ConfigurationManager.AppSettings["Environment"]);
			}
		}

		/// <summary>
		/// Gets the 'ServiceRoot' key, the folder path for the web API/WCF service path (if any). {src-path} will be replace with the closest directory called 'src' when in development.
		/// </summary>
		public virtual string ServiceRoot
		{
			get
			{
				string root = ConfigurationManager.AppSettings["ServiceRoot"];
				return FormatDirectory(root);
			}
		}

		/// <summary>
		/// Gets the 'WebRoot' key, the folder path for the website. {src-path} will be replace with the closest directory called 'src' when in development.
		/// </summary>
		public virtual string WebRoot
		{
			get
			{
				string root = ConfigurationManager.AppSettings["WebRoot"];
				return FormatDirectory(root);
			}
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

		/// <summary>
		/// {src-path} will be replace with the closest directory called 'src' when in development.
		/// </summary>
		protected string FormatDirectory(string path)
		{
			path = path ?? string.Empty;

			if (Environment == Environment.Development)
			{
				if (path.ToLower().Contains(SRC_PATH))
				{
					string srcFolder = FindSrcFolder();
					path = path.Replace(SRC_PATH, srcFolder);
				}
			}

			return path;
		}

		/// <summary>
		/// Navigates up the current directory structure to find 'src'.
		/// </summary>
		private string FindSrcFolder()
		{
			string initialDirectory = System.Environment.CurrentDirectory;
			const string src = "src";

			DirectoryInfo directoryInfo;
			for (directoryInfo = new DirectoryInfo(initialDirectory); directoryInfo.GetDirectories(src).Length == 0; directoryInfo = directoryInfo.Parent)
			{
				if (directoryInfo.Parent == null)
				{
					throw new InvalidOperationException(string.Format("Unable to find 'src' folder, traversed up to '{0}'.", directoryInfo.FullName));
				}
			}

			return Path.Combine(directoryInfo.FullName, src);
		}
	}
}
