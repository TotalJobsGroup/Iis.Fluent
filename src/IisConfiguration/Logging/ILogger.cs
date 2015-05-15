using System;

namespace IisConfiguration.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        void Log(string msg);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        void LogFormat(string msg, params object[] args);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        void LogHeading(string msg);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        void LogError(Exception e);
    }
}