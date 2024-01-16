using System;
using System.Net.Http;

namespace EscapeTheMaze.Managers
{
	public static class VersionManager
	{
		public const string RepositoryURL = "https://github.com/Wirmaple73/EscapeTheMaze/";
		private const string LookupURL = "https://raw.githubusercontent.com/Wirmaple73/EscapeTheMaze/main/CurrentVersion.txt";

		public static readonly Version CurrentVersion = new(1, 0, 1);
		public static readonly DateTime BuildDate = new(2024, 1, 16);

		private const string SuccessMessage = "The operation completed successfully.";

		public static (VersionLookupState LookupState, string Message) VersionLookupState
		{
			get
			{
				using (var client = new HttpClient())
				{
					try
					{
						return (new Version(client.GetStringAsync(LookupURL).Result) > CurrentVersion) ?
								(EscapeTheMaze.VersionLookupState.UpdateAvailable, SuccessMessage) :
								(EscapeTheMaze.VersionLookupState.UpToDate, SuccessMessage);
					}
					catch (Exception ex)
					{
						return (EscapeTheMaze.VersionLookupState.ConnectionError, $"{ex.GetType().Name}: {ex.Message}");
					}
				}
			}
		}
	}
}
