using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace bcdsettings
{
	class Program
	{
		static void Main(string[] args)
		{
			Process process = new Process();
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.UseShellExecute = true;
			startInfo.WorkingDirectory = @"C:\Windows\System32";
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = "/c bcdedit.exe /enum all";
			startInfo.Verb = "runas";
			process.StartInfo = startInfo;
			process.Start();
			while (!process.HasExited) Thread.Sleep(100);
			Console.WriteLine(process.StandardOutput.ReadToEnd());

			BcdStoreAccessor bcd = new BcdStoreAccessor();
			bcd.SetIntegerElement((uint)BcdConstants.BcdOSLoaderElementTypes.BcdOSLoaderInteger_BootStatusPolicy, 1);
			bcd.SetBooleanElement((uint)BcdConstants.BcdLibraryElementTypes.BcdLibraryBoolean_AutoRecoveryEnabled, false);
			bcd.SetIntegerElement((uint)BcdConstants.BcdLibraryElementTypes.BcdLibraryInteger_DisplayMessageOverride, 1);
		}
	}

	public class BcdStoreAccessor
	{
		public enum BcdOSLoader_BootStatusPolicy
		{
			DisplayAllFailures = 0,
			IgnoreAllFailures = 1,
			IgnoreShutdownFailures = 2,
			IgnoreBootFailures = 3,
			IgnoreCheckpointFailures = 4,
			DisplayShutdownFailures = 5,
			DisplayBootFailures = 6,
			DisplayCheckpointFailures
		}

		private ConnectionOptions connectionOptions;
		private ManagementScope managementScope;
		private ManagementPath managementPath;

		public BcdStoreAccessor()
		{
			connectionOptions = new ConnectionOptions();
			connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
			connectionOptions.EnablePrivileges = true;

			managementScope = new ManagementScope("root\\WMI", connectionOptions);

			managementPath = new ManagementPath("root\\WMI:BcdObject.Id=\"{fa926493-6f1c-4193-a414-58f0b2456d1e}\",StoreFilePath=\"\"");
		}

		public void SetIntegerElement(uint type, int value)
		{
			ManagementObject currentBootloader = new ManagementObject(managementScope, managementPath, null);
			currentBootloader.InvokeMethod("SetIntegerElement", new object[] { type, value });
		}

		public void SetBooleanElement(uint type, bool value)
		{
			ManagementObject currentBootloader = new ManagementObject(managementScope, managementPath, null);
			currentBootloader.InvokeMethod("SetBooleanElement", new object[] { type, value });
		}

		public ManagementBaseObject[] EnumerateElements()
		{
			ManagementObject currentBootloader = new ManagementObject(managementScope, managementPath, null);
			ManagementBaseObject inParams = currentBootloader.GetMethodParameters("EnumerateElements");
			ManagementBaseObject outParams = currentBootloader.InvokeMethod("EnumerateElements", inParams, null);
			return ((ManagementBaseObject[])(outParams.Properties["Elements"].Value));
		}

		public ManagementBaseObject GetElement(uint type)
		{
			ManagementObject currentBootloader = new ManagementObject(managementScope, managementPath, null);
			ManagementBaseObject inParams = currentBootloader.GetMethodParameters("GetElement");
			inParams["Type"] = type;
			ManagementBaseObject outParams = currentBootloader.InvokeMethod("GetElement", inParams, null);
			return ((ManagementBaseObject)(outParams.Properties["Element"].Value));
		}
	}
}
