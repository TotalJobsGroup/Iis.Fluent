using System;
using IisConfiguration.Logging;

namespace IisConfiguration.Tests.Stubs
{
	public class LoggerStub : ILogger
	{
		public void Log(string msg)
		{
		}

		public void LogHeading(string msg)
		{
		}

		public void LogError(Exception e)
		{
		}

		public void LogFormat(string msg, params object[] args)
		{
		}
	}
}