using System;

namespace IisConfiguration.Exceptions
{
	/// <summary>
	/// 
	/// </summary>
	public class IisConfigurationException : Exception
	{
		/// <summary>
		/// 
		/// </summary>
		public IisConfigurationException() { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public IisConfigurationException(string message, params object[] args) : base(string.Format(message, args)) { }
	}
}
