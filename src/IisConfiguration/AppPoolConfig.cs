using System;
using System.Collections.Generic;
using System.Linq;
using IisConfiguration.Logging;
using Microsoft.Web.Administration;

namespace IisConfiguration
{
    /// <summary>
    /// Represents a single application pool in IIS.
    /// </summary>
    public class AppPoolConfig : IDisposable
    {
        private readonly ServerManager _mgr;
        private readonly ILogger _logger;
        private readonly List<string> _currentAppPoolNames = new List<string>();
        private int _privateMemoryLimit = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="logger"></param>
        public AppPoolConfig(ServerManager mgr, ILogger logger)
        {
            _mgr = mgr;
            _logger = logger;
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
            DeleteAppPool(name);
            _currentAppPoolNames.Add(name);

            ApplicationPool applicationPool = _mgr.ApplicationPools.Add(name);
            applicationPool.ManagedRuntimeVersion = runtimeVersion;
            applicationPool.ManagedPipelineMode = managedPipelineMode;
            applicationPool.ProcessModel.IdentityType = identityType;
            applicationPool.Recycling.PeriodicRestart.PrivateMemory = _privateMemoryLimit;
            _logger.Log("Added application pool " + name);

            return this;
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
            _privateMemoryLimit = privateMemoryLimit;
            return AddAppPool(name, runtimeVersion, managedPipelineMode, identityType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public AppPoolConfig SetSpecificTimeToRecycle(TimeSpan time)
        {
			foreach (var applicationPool in GetCurrentAppPools())
            {
                applicationPool.Recycling.PeriodicRestart.Schedule.Add(time);
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idleTimeout"></param>
        /// <param name="pingingEnabled"></param>
        /// <returns></returns>
        public AppPoolConfig WithProcessModel(TimeSpan idleTimeout, bool pingingEnabled)
        {
			foreach (var applicationPool in GetCurrentAppPools())
            {
                applicationPool.ProcessModel.IdleTimeout = idleTimeout;
                applicationPool.ProcessModel.PingingEnabled = pingingEnabled;
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="restartInterval"></param>
        /// <returns></returns>
        public AppPoolConfig WithPeriodicRestart(TimeSpan restartInterval)
        {
			foreach (var applicationPool in GetCurrentAppPools())
            {
                applicationPool.Recycling.PeriodicRestart.Time = restartInterval;
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rapidFailProtection"></param>
        /// <returns></returns>
        public AppPoolConfig WithRapidFailProtection(bool rapidFailProtection)
        {
			foreach (var applicationPool in GetCurrentAppPools())
            {
                applicationPool.Failure["RapidFailProtection"] = rapidFailProtection;
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public AppPoolConfig WithCredentials(string username, string password)
        {
			foreach (var applicationPool in GetCurrentAppPools())
            {
                if (applicationPool.ProcessModel.IdentityType == ProcessModelIdentityType.SpecificUser)
                {
                    applicationPool.ProcessModel.UserName = username;
                    applicationPool.ProcessModel.Password = password;
                }
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appPoolQueueLength"></param>
        /// <returns></returns>
        public AppPoolConfig WithAppPoolQueueLength(int appPoolQueueLength)
        {
            foreach (var applicationPool in GetCurrentAppPools())
            {
                applicationPool.QueueLength = appPoolQueueLength;
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

		private IEnumerable<ApplicationPool> GetCurrentAppPools()
		{
			return from appPool in _mgr.ApplicationPools
				   where _currentAppPoolNames.Contains(appPool.Name)
				   select appPool;
		}

        private void DeleteAppPool(string name)
        {
            if (_mgr.ApplicationPools[name] != null)
                _mgr.ApplicationPools[name].Delete();

            Commit();
        }
    }
}