using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Aloe.CommonLib
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
    }

    /// <summary>
    /// コアのロガー本体
    /// </summary>
    public static class Aloe
    {
        private static readonly object _lock = new();

        private static string _logDirectory = AppContext.BaseDirectory;
        private static string _baseFileName = "app.log";
        private static bool _enableConsole = true;
        private static bool _enableFile = true;

        /// <summary>
        /// 日付付きログファイルのフルパス
        /// 例: app_20251123.log
        /// </summary>
        private static string CurrentLogFilePath
            => Path.Combine(
                _logDirectory,
                $"{Path.GetFileNameWithoutExtension(_baseFileName)}_{DateTime.Now:yyyyMMdd}.log"
            );

        /// <summary>
        /// 初期化（オプション）
        /// </summary>
        public static void Init(
            string? logDirectory = null,
            string? baseFileName = null,
            bool enableConsole = true,
            bool enableFile = true)
        {
            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(logDirectory))
                    _logDirectory = logDirectory!;

                if (!string.IsNullOrWhiteSpace(baseFileName))
                    _baseFileName = baseFileName!;

                _enableConsole = enableConsole;
                _enableFile = enableFile;

                if (_enableFile)
                {
                    Directory.CreateDirectory(_logDirectory);
                }
            }
        }

        /// <summary>
        /// コアのログ出力
        /// </summary>
        public static void Write(
            LogLevel level,
            string message,
            Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            try
            {
                var now = DateTime.Now;
                var fileNameOnly = string.IsNullOrEmpty(callerFilePath)
                    ? "?"
                    : Path.GetFileName(callerFilePath);

                var sb = new StringBuilder();

                // 1行目：メインメッセージ
                sb.Append(now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sb.Append(" [");
                sb.Append(level.ToString().ToUpper());
                sb.Append("] ");
                sb.Append(fileNameOnly);
                sb.Append("(");
                sb.Append(callerLineNumber);
                sb.Append(") ");
                sb.Append(callerMemberName);
                sb.Append(" - ");
                sb.Append(message);

                // 例外があれば改行して ToString()
                if (ex != null)
                {
                    sb.AppendLine();
                    sb.Append(ex.ToString());
                }

                var logText = sb.ToString();

                lock (_lock)
                {
                    if (_enableConsole)
                        WriteToConsole(logText);

                    if (_enableFile)
                        WriteToFile(logText);
                }
            }
            catch
            {
                // ロガー内部の例外は握りつぶす
            }
        }

        // レベル別ショートカット（直接呼びたいとき用）
        public static void Trace(string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Write(LogLevel.Trace, message, ex, callerFilePath, callerMemberName, callerLineNumber);

        public static void Debug(string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Write(LogLevel.Debug, message, ex, callerFilePath, callerMemberName, callerLineNumber);

        public static void Info(string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Write(LogLevel.Info, message, ex, callerFilePath, callerMemberName, callerLineNumber);

        public static void Warn(string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Write(LogLevel.Warn, message, ex, callerFilePath, callerMemberName, callerLineNumber);

        public static void Error(string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Write(LogLevel.Error, message, ex, callerFilePath, callerMemberName, callerLineNumber);

        public static void Fatal(string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Write(LogLevel.Fatal, message, ex, callerFilePath, callerMemberName, callerLineNumber);

        private static void WriteToConsole(string text)
        {
            Console.WriteLine(text);
        }

        private static void WriteToFile(string text)
        {
            var path = CurrentLogFilePath;

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            // UTF-8 (BOMなし) で追記
            using var sw = new StreamWriter(path, append: true, encoding: new UTF8Encoding(false));
            sw.WriteLine(text);
        }
    }

    /// <summary>
    /// どのクラスからでも呼べる拡張メソッド
    /// this.Info("message") みたいに使う
    /// </summary>
    public static class AloeExtensions
    {
        public static void Trace(this object? sender, string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Aloe.Write(LogLevel.Trace, BuildMessage(sender, message), ex,
                callerFilePath, callerMemberName, callerLineNumber);

        public static void Debug(this object? sender, string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Aloe.Write(LogLevel.Debug, BuildMessage(sender, message), ex,
                callerFilePath, callerMemberName, callerLineNumber);

        public static void Info(this object? sender, string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Aloe.Write(LogLevel.Info, BuildMessage(sender, message), ex,
                callerFilePath, callerMemberName, callerLineNumber);

        public static void Warn(this object? sender, string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Aloe.Write(LogLevel.Warn, BuildMessage(sender, message), ex,
                callerFilePath, callerMemberName, callerLineNumber);

        public static void Error(this object? sender, string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Aloe.Write(LogLevel.Error, BuildMessage(sender, message), ex,
                callerFilePath, callerMemberName, callerLineNumber);

        public static void Fatal(this object? sender, string message, Exception? ex = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
            => Aloe.Write(LogLevel.Fatal, BuildMessage(sender, message), ex,
                callerFilePath, callerMemberName, callerLineNumber);

        private static string BuildMessage(object? sender, string message)
        {
            if (sender == null) return message;
            var typeName = sender.GetType().Name;
            return $"[{typeName}] {message}";
        }
    }
}
