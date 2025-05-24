//#define DISABLE_LAZYDEBUG
#if !DEVELOPMENT_BUILD
#define DISABLE_STACK_TRACE
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace LazyUI
{
    /// <summary>
    /// デバッグログ管理
    /// ログをファイルへ書き込む。
    /// var logEnable = LazyPlayerPrefs.GetValue("Debug.Log.Enable", false);
    /// </summary>
    public static class LazyDebug
    {
#if !DISABLE_LAZYDEBUG || DEVELOPMENT_BUILD
        private enum LogCommand
        {
            Log,
            Warning,
            Error,
            Assert,
        }
        private struct LogStruct
        {
            public LogCommand cmd;
            public long ticks;
#if !DISABLE_STACK_TRACE
            public string stack;
#endif
            public string msg;
        }
        private static readonly StringBuilder sb = new();

        private static bool initialized = false;
        private static StreamWriter streamWriter = null;
        private static long ticks = 0;

        private static string LogFilePath
        {
            get
            {
                return Application.dataPath + "/../Debug.log";
            }
        }
        private static void Log(LogCommand type, string msg)
        {
            var ls = new LogStruct
            {
                cmd = type,
                ticks = DateTime.Now.Ticks,
#if !DISABLE_STACK_TRACE
                stack = StackTraceUtility.ExtractStackTrace(),
#endif
                msg = msg
            };
            lock (sb)
            {
                OutputLog(ls);
            }
        }
        private static void OutputLog(LogStruct ls)
        {
            var dt = (ls.ticks - ticks) / 10000;
            if (dt > 999999)
            {
                dt = 999999;
            }
            ticks = ls.ticks;
            sb.Append(dt.ToString("d6"));
            sb.Append(": ");
            sb.Append(ls.msg);
#if !DISABLE_STACK_TRACE
            sb.Append('\n');
            sb.Append(TrimLines(ls.stack, 3));
#endif
            var msg = sb.ToString();
            sb.Length = 0;
            switch (ls.cmd)
            {
                case LogCommand.Log:
                    UnityEngine.Debug.Log(msg);
                    break;
                case LogCommand.Warning:
                    UnityEngine.Debug.LogWarning(msg);
                    break;
                case LogCommand.Error:
                    UnityEngine.Debug.LogError(msg);
                    break;
                case LogCommand.Assert:
                    UnityEngine.Debug.LogAssertion(msg);
                    break;
                default:
                    break;
            }
        }
        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            // 受信したログをファイルへ書き込み。
            // ファイル書き込みが無効な時でもerrorやassertのログが来た場合、それ以降強制的にファイルへ書き込む。
            if ((streamWriter == null) && ((type == LogType.Warning) || (type == LogType.Log)))
            {
                return;
            }
            lock (sb)
            {
                if (streamWriter == null)
                {
                    SetupLogFile();
                }
                streamWriter.WriteLine($"{type}: {condition}");
            }
        }

        private static void OnApplicationLowMemory()
        {
            LogWarning("Application.lowMemory");
        }

        private static void OnApplicationQuitting()
        {
            Log("Application.quitting");
#if false
            lock (sb)
            {
                Application.logMessageReceivedThreaded -= OnLogMessageReceived;
                streamWriter?.Dispose();
                streamWriter = null;
            }
#endif
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            lock (sb)
            {
                if (initialized)
                {
                    return;
                }
                initialized = true;
                Application.logMessageReceivedThreaded += OnLogMessageReceived;
                Application.quitting += OnApplicationQuitting;
                Application.lowMemory += OnApplicationLowMemory;
#if DEVELOPMENT_BUILD
                var logEnable = true;
#else
                var logEnable = LazyPlayerPrefs.GetValue("Debug.Log.Enable", false);
#endif
                if (logEnable)
                {
                    SetupLogFile();
                }
            }
        }
        private static void SetupLogFile()
        {
            streamWriter = new StreamWriter(LogFilePath, false, Encoding.UTF8);
            streamWriter.AutoFlush = true;
        }

#if !DISABLE_STACK_TRACE
        private static string TrimLines(string text, int num)
        {
            var idx = -1;
            for (int i = 0; i < num; i++)
            {
                var ix = text.IndexOf('\n', idx + 1);
                if (ix < 0)
                {
                    break;
                }
                idx = ix;
            }
            if (idx >= 0)
            {
                text = text[(idx + 1)..];
            }
            return text;
        }
#endif
#endif


#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void Log(string text)
        {
#if !DISABLE_LAZYDEBUG
            Log(LogCommand.Log, text);
#else
            UnityEngine.Debug.Log(text);
#endif
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void Log(bool print, string text)
        {
            if (!print)
            {
                return;
            }
#if !DISABLE_LAZYDEBUG
            Log(LogCommand.Log, text);
#else
            UnityEngine.Debug.Log(text);
#endif
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void Log(System.Exception ec)
        {
#if !DISABLE_LAZYDEBUG
#if DISABLE_STACK_TRACE
            Log(LogCommand.Log, ec.Message);
#else
            Log(LogCommand.Log, ec.ToString());
#endif
#else
            UnityEngine.Debug.Log(ec.Message);
#endif
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void LogWarning(string text)
        {
#if !DISABLE_LAZYDEBUG
            Log(LogCommand.Warning, text);
#else
            UnityEngine.Debug.LogWarning(text);
#endif
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void LogWarning(bool print, string text)
        {
            if (!print)
            {
                return;
            }
#if !DISABLE_LAZYDEBUG
            Log(LogCommand.Warning, text);
#else
            UnityEngine.Debug.LogWarning(text);
#endif
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void LogWarning(System.Exception ec)
        {
#if !DISABLE_LAZYDEBUG
#if DISABLE_STACK_TRACE
            Log(LogCommand.Warning, ec.Message);
#else
            Log(LogCommand.Warning, ec.ToString());
#endif
#else
            UnityEngine.Debug.LogWarning(ec.Message);
#endif
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void LogError(string text)
        {
#if !DISABLE_LAZYDEBUG
            Log(LogCommand.Error, text);
#else
            UnityEngine.Debug.LogError(text);
#endif
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void LogError(bool print, string text)
        {
            if (!print)
            {
                return;
            }
#if !DISABLE_LAZYDEBUG
            Log(LogCommand.Error, text);
#else
            UnityEngine.Debug.LogError(text);
#endif
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void LogError(System.Exception ec)
        {
#if !DISABLE_LAZYDEBUG
#if DISABLE_STACK_TRACE
            Log(LogCommand.Error, ec.Message);
#else
            Log(LogCommand.Error, ec.ToString());
#endif
#else
            UnityEngine.Debug.LogError(ec.Message);
#endif
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void LogException(System.Exception ec)
        {
#if !DISABLE_LAZYDEBUG
#if DISABLE_STACK_TRACE
            Log(LogCommand.Error, ec.Message);
#else
            Log(LogCommand.Error, ec.ToString());
#endif
#else
            UnityEngine.Debug.LogException(ec);
#endif
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void Assert(bool cond, string message)
        {
            if (cond)
            {
                return;
            }
            throw new AssertionFailedException(message);
        }
#if DISABLE_LAZYDEBUG && !DEVELOPMENT_BUILD
        [Conditional("XXX_DISABLE_LAZYDEBUG_XXX")]
#endif
        public static void Assert(bool cond)
        {
            if (cond)
            {
                return;
            }
            var message = GetCaller();
            throw new AssertionFailedException(message);
        }
        private static string GetCaller()
        {
            var name = typeof(LazyDebug).FullName;
            var frames = StackTraceUtility.ExtractStackTrace().Split('\n');
            for (int i = 2; i < frames.Length; i++)
            {
                if (!frames[i].StartsWith(name))
                {
                    return frames[i];
                }
            }
            return "";
        }
        public class AssertionFailedException : Exception
        {
            public AssertionFailedException() : base() { }
            public AssertionFailedException(string message) : base("Assertion failed: " + message) { }
        }
    }
}
