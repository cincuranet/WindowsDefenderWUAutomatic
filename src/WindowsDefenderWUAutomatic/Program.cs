using System;
using System.IO;
using System.Linq;
using System.Reflection;
using WUApiLib;

namespace WindowsDefenderWUAutomatic
{
	class Program
	{
		static int Main(string[] args)
		{
			var session = new UpdateSession();
			var searcher = session.CreateUpdateSearcher();
			Log("Searching...");
			var searchResult = searcher.Search("IsInstalled=0 And IsHidden=0");
			var defenderUpdates = searchResult.Updates.Cast<IUpdate>()
				.Where(u => u.Title.IndexOf("Definition Update for Windows Defender", StringComparison.Ordinal) >= 0)
				.ToArray();
			if (defenderUpdates.Any())
			{
				Log("Going to install:");
				var updates = new UpdateCollection();
				foreach (var item in defenderUpdates)
				{
					updates.Add(item);
					Log("\t" + item.Title);
				}

				var downloader = session.CreateUpdateDownloader();
				downloader.Updates = updates;
				Log("Downloading...");
				var downloadResult = downloader.Download();
				Log("Result: {0} [0x{1}]", downloadResult.ResultCode, downloadResult.HResult.ToString("X"));
				if (downloadResult.ResultCode != OperationResultCode.orcSucceeded)
					return -1 * (int)downloadResult.ResultCode;

				var updater = session.CreateUpdateInstaller();
				updater.Updates = updates;
				Log("Installing...");
				var updateResult = updater.Install();
				Log("Result: {0} [0x{1}]", updateResult.ResultCode, updateResult.HResult.ToString("X"));
				if (updateResult.ResultCode != OperationResultCode.orcSucceeded)
					return -1 * (int)updateResult.ResultCode;

				return 0;
			}
			else
			{
				Log("Nothing to install.");
				return 0;
			}
		}

		static void Log(string message)
		{
			Console.WriteLine(message);
			var fileMessage = string.Format("{0}|{1}", DateTimeOffset.Now.ToString(), message);
			File.AppendAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "log.log"), new[] { fileMessage });
		}
		static void Log(string format, params object[] args)
		{
			Log(string.Format(format, args));
		}
	}
}
