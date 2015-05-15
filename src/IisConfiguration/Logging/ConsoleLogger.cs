using System;

namespace IisConfiguration.Logging
{
	/// <summary>
	/// Logs everything to Console.WriteLine
	/// </summary>
    public class ConsoleLogger : ILogger
    {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
        public void Log(string msg)
        {
            Console.WriteLine("   " + msg);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="args"></param>
        public void LogFormat(string msg, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("   " + msg, args);
            }
            else
            {
                Console.Write("   " + msg);
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
        public void LogHeading(string msg)
        {
            Console.WriteLine("");
            Console.WriteLine("****************************************************************************");
            Console.WriteLine("   " + msg);
            Console.WriteLine("****************************************************************************");
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
        public void LogError(Exception e)
        {
            LogHeading("Failed importing IIS settings");
            Console.Error.WriteLine(e);
        }
    }
}
