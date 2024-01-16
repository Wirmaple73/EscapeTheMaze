using System;
using System.Linq;
using EscapeTheMaze.Managers;

namespace EscapeTheMaze
{
	public static class Program
	{
		public static bool IsDebugModeEnabled { get; private set; }

		private static void Main(string[] args)
		{
			ProcessArguments();

			UIManager.DisplayLogo();
			UIManager.DisplayLoginPageAndStoreUser();

			ExitHandler.Initialize();

			while (true)
			{
				switch (UIManager.DisplayMainMenu())
				{
					case SelectedMenuItem.Play:
						if (UIManager.DisplayGameGuide())
							GameManager.Play();

						break;

					case SelectedMenuItem.Leaderboard:
						UIManager.DisplayLeaderboard();
						break;

					case SelectedMenuItem.UpgradeShop:
						UIManager.DisplayUpgradeShop();
						break;

					case SelectedMenuItem.ChangeAccount:
						UIManager.DisplayLoginPageAndStoreUser();
						break;

					case SelectedMenuItem.CheckForUpdates:
						UIManager.DisplayUpdatePage();
						break;

					case SelectedMenuItem.About:
						UIManager.DisplayAboutPage();
						break;

					case SelectedMenuItem.Exit:
						UserManager.ExportUsers();
						UIManager.DisplayExitPage();
						return;

					case SelectedMenuItem.ImportUsers:
						UserManager.ImportUsers();
						UIManager.DisplayLoginPageAndStoreUser();
						break;

					case SelectedMenuItem.ExportUsers:
						UserManager.ExportUsers();
						break;
				}
			}

			void ProcessArguments() => IsDebugModeEnabled = args.Contains("-debug");
		}
	}
}
