using System;
using System.Threading;
using NLog.Common;

namespace NLog.Mongo
{
    internal static class ExceptionHelper
    {
        private const string LoggedKey = "NLog.ExceptionLoggedToInternalLogger";

        /// <summary>
        /// Mark this exception as logged to the <see cref="InternalLogger"/>.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static void MarkAsLoggedToInternalLogger(this Exception exception)
        {
            if (exception != null)
            {
                exception.Data[LoggedKey] = true;
            }
        }

        /// <summary>
        /// Is this exception logged to the <see cref="InternalLogger"/>? 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns><c>true</c>if the <paramref name="exception"/> has been logged to the <see cref="InternalLogger"/>.</returns>
        public static bool IsLoggedToInternalLogger(this Exception exception)
        {
            if (exception != null)
            {
                return exception.Data[LoggedKey] as bool? ?? false;
            }
            return false;
        }


        /// <summary>
        /// Determines whether the exception must be rethrown and logs the error to the <see cref="InternalLogger"/> if <see cref="IsLoggedToInternalLogger"/> is <c>false</c>.
        /// 
        /// Advised to log first the error to the <see cref="InternalLogger"/> before calling this method.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><c>true</c>if the <paramref name="exception"/> must be rethrown, <c>false</c> otherwise.</returns>
        public static bool MustBeRethrown(this Exception exception)
        {
            if (exception.MustBeRethrownImmediately())
                return true;

            var isConfigError = exception is NLogConfigurationException;

            //we throw always configuration exceptions (historical)
            if (!exception.IsLoggedToInternalLogger())
            {
                var level = isConfigError ? LogLevel.Warn : LogLevel.Error;
                InternalLogger.Log(exception, level, "Error has been raised.");
            }

            //if ThrowConfigExceptions == null, use  ThrowExceptions
            var shallRethrow = isConfigError ? (LogManager.ThrowConfigExceptions ?? LogManager.ThrowExceptions) : LogManager.ThrowExceptions;
            return shallRethrow;
        }

        /// <summary>
        /// Determines whether the exception must be rethrown immediately, without logging the error to the <see cref="InternalLogger"/>.
        /// 
        /// Only used this method in special cases.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><c>true</c>if the <paramref name="exception"/> must be rethrown, <c>false</c> otherwise.</returns>
        public static bool MustBeRethrownImmediately(this Exception exception)
        {

#if !NETSTANDARD1_5
            if (exception is StackOverflowException)
                return true;

            if (exception is ThreadAbortException)
                return true;
#endif

            if (exception is OutOfMemoryException)
                return true;

            return false;
        }
    }
}
