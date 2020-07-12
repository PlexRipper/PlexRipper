// MIT License
// Copyright (c) 2020 litetex
// See also https://github.com/litetex/CoreFramework/blob/develop/CoreFramework.Logging/Log.cs

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace PlexRipper.Domain
{
    // TODO Add summaries describing how the log levels should be used
    public static class Log
    {
        private static string FormatForException(this string message, Exception ex)
        {
            return $"{message}: {(ex != null ? ex.ToString() : "")}";
        }

        private static string FormatForContext(
            this string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            var methodName = memberName;

            return $"[{fileName}.{methodName}] => {message}";
        }

        public static void Verbose(
           string message,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Verbose(
               message
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Verbose(
           string message,
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Verbose(
               message
                  .FormatForException(ex)
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Verbose(
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")

        {

            Serilog.Log.Verbose(
               (ex != null ? ex.ToString() : "")
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Debug(
                 string message,
                 [CallerMemberName] string memberName = "",
                 [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Debug(
               message
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Debug(
           string message,
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Debug(
               message
                  .FormatForException(ex)
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Debug(
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {

            Serilog.Log.Debug(
               (ex != null ? ex.ToString() : "")
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Information(
           string message,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Information(
               message
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Information(
           string message,
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {

            Serilog.Log.Information(
               message
                  .FormatForException(ex)
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Information(
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {

            Serilog.Log.Information(
               (ex != null ? ex.ToString() : "")
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Warning(string message,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {

            Serilog.Log.Warning(
               message
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Warning(
           string message,
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {

            Serilog.Log.Warning(
               message
                  .FormatForException(ex)
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Warning(
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {

            Serilog.Log.Warning(
               (ex != null ? ex.ToString() : "")
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Error(
           string message,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {

            Serilog.Log.Error(
               message
                  .FormatForContext(memberName, sourceFilePath)
              );
        }

        public static void Error(
           string message,
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {

            Serilog.Log.Error(
               message
                  .FormatForException(ex)
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Error(
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {

            Serilog.Log.Error(
               (ex != null ? ex.ToString() : "")
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Fatal(
           string message,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {
            FatalAction();

            Serilog.Log.Error(
               message
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Fatal(
           string message,
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {
            FatalAction();

            Serilog.Log.Error(
               message
                  .FormatForException(ex)
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        public static void Fatal(
           Exception ex,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "")
        {
            FatalAction();

            Serilog.Log.Error(
               (ex != null ? ex.ToString() : "")
                  .FormatForContext(memberName, sourceFilePath)
               );
        }

        private static void FatalAction()
        {
            Environment.ExitCode = -1;
        }
    }
}
