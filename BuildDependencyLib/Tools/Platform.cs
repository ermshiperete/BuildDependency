// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
// Parts based on code by MJ Hutchinson http://mjhutchinson.com/journal/2010/01/25/integrating_gtk_application_mac
using System;

namespace BuildDependency.Tools
{
	public static class Platform
	{
		private const string UnixNameMac = "Darwin";
		private const string UnixNameLinux = "Linux";
		private static bool? _isMono;
		private static string _unixName;
		private static string _sessionManager;

		public static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix;

		public static bool IsLinux => IsUnix && (UnixName == UnixNameLinux);

		public static bool IsMac => IsUnix && (UnixName == UnixNameMac);

		public static bool IsWindows => !IsUnix;

		public static bool IsMono
		{
			get
			{
				if (_isMono == null)
					_isMono = Type.GetType("Mono.Runtime") != null;

				return (bool)_isMono;
			}
		}

		public static bool IsDotNet => !IsMono;

		private static string UnixName
		{
			get
			{
				if (_unixName == null)
				{
					IntPtr buf = IntPtr.Zero;
					try
					{
						buf = System.Runtime.InteropServices.Marshal.AllocHGlobal(8192);
						// This is a hacktastic way of getting sysname from uname()
						if (uname(buf) == 0)
							_unixName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(buf);
					}
					catch
					{
						_unixName = string.Empty;
					}
					finally {
						if (buf != IntPtr.Zero)
							System.Runtime.InteropServices.Marshal.FreeHGlobal (buf);
					}
				}

				return _unixName;
			}
		}


		public static bool IsWasta => IsUnix && System.IO.File.Exists("/etc/wasta-release");

		public static bool IsCinnamon => IsUnix && SessionManager.StartsWith("/usr/bin/cinnamon-session");

		/// <summary>
		/// On a Unix machine this gets the current desktop environment (gnome/xfce/...), on
		/// non-Unix machines the platform name.
		/// </summary>
		public static string DesktopEnvironment
		{
			get
			{
				if (IsUnix)
				{
					// see http://unix.stackexchange.com/a/116694
					// and http://askubuntu.com/a/227669
					var currentDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
					if (string.IsNullOrEmpty(currentDesktop))
					{
						var dataDirs = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
						if (dataDirs != null)
						{
							dataDirs = dataDirs.ToLowerInvariant();
							if (dataDirs.Contains("xfce"))
								currentDesktop = "XFCE";
							else if (dataDirs.Contains("kde"))
								currentDesktop = "KDE";
							else if (dataDirs.Contains("gnome"))
								currentDesktop = "Gnome";
						}
						if (string.IsNullOrEmpty(currentDesktop))
							currentDesktop = Environment.GetEnvironmentVariable("GDMSESSION") ?? string.Empty;
					}
					// Special case for Wasta 12
					else if (currentDesktop == "GNOME" && Environment.GetEnvironmentVariable("GDMSESSION") == "cinnamon")
						currentDesktop = Environment.GetEnvironmentVariable("GDMSESSION");
					if (currentDesktop != null)
						return currentDesktop.ToLowerInvariant();
				}
				return Environment.OSVersion.Platform.ToString();
			}
		}

		/// <summary>
		/// Get the currently running desktop environment (like Unity, Gnome shell etc)
		/// </summary>
		public static string DesktopEnvironmentInfoString
		{
			get
			{
				if (!IsUnix)
					return string.Empty;

				// see http://unix.stackexchange.com/a/116694
				// and http://askubuntu.com/a/227669
				var currentDesktop = DesktopEnvironment;
				var mirSession = Environment.GetEnvironmentVariable("MIR_SERVER_NAME");
				var additionalInfo = string.Empty;
				if (!string.IsNullOrEmpty(mirSession))
					additionalInfo = " [display server: Mir]";
				var gdmSession = Environment.GetEnvironmentVariable("GDMSESSION") ?? "not set";
				return $"{currentDesktop} ({gdmSession}{additionalInfo})";
			}
		}

		private static string SessionManager
		{
			get
			{
				if (_sessionManager == null)
				{
					IntPtr buf = IntPtr.Zero;
					try
					{
						// This is the only way I've figured out to get the session manager.
						buf = System.Runtime.InteropServices.Marshal.AllocHGlobal(8192);
						var len = readlink("/etc/alternatives/x-session-manager", buf, 8192);
						_sessionManager = len > 0 ? System.Runtime.InteropServices.Marshal.PtrToStringAnsi(buf) : string.Empty;
					}
					catch
					{
						_sessionManager = string.Empty;
					}
					finally
					{
						if (buf != IntPtr.Zero)
							System.Runtime.InteropServices.Marshal.FreeHGlobal(buf);
					}
				}
				return _sessionManager;
			}
		}

		[System.Runtime.InteropServices.DllImport ("libc")]
		private static extern int uname (IntPtr buf);

		[System.Runtime.InteropServices.DllImport ("libc")]
		private static extern int readlink(string path, IntPtr buf, int bufsiz);

		[System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "mono_get_runtime_build_info")]
		private static extern string GetMonoVersion();

		public static string MonoVersion => IsMono ? GetMonoVersion() : string.Empty;
	}
}
