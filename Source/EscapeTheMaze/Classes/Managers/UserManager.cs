using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;

namespace EscapeTheMaze.Managers
{
	public static class UserManager
	{
		public static List<User> Users { get; private set; } = new();
		public static User CurrentUser => Users[currentUserIndex];
		public static bool IsNewUser { get; private set; }

		private static int currentUserIndex = -1;

		static UserManager() => UserFileManager.ImportUsers();

		public static void ImportUsers()
		{
			Users.Clear();
			UserFileManager.ImportUsers();
		}

		public static void ExportUsers() => UserFileManager.ExportUsers();

		public static void StoreCurrentUserIndex(string username)
		{
			// Create a new user if the user with the specified username doesn't exist
			if (IsNewUser = (currentUserIndex = Users.FindIndex(user => user.Username == username)) < 0)
			{
				var user = new User(username);
				Users.Add(user);

				SortUserList();
				currentUserIndex = Users.IndexOf(user);
			}
		}

		// Sort the list (ascending by username)
		private static void SortUserList() => Users = Users.OrderBy(x => x.Username).ToList();

		private static class UserFileManager
		{
			private const string UsernameCaption    = nameof(User.Username);
			private const string LevelCaption       = nameof(User.Level);
			private const string TopScoreCaption    = nameof(User.TopScore);
			private const string ArmorsCaption      = nameof(User.Armors);
			private const string BalanceCaption     = nameof(User.Balance);
			private const string UpgradesCaption    = nameof(User.Upgrades);
			private const string TimeElapsedCaption = nameof(User.TimeElapsed);

			internal static void ImportUsers()
			{
				if (!File.Exists(Constants.OutputFilePath))
					return;

				XElement xmlData;

				while (true)
				{
					try
					{
						xmlData = XElement.Load(Constants.OutputFilePath);
						break;
					}
					catch (Exception ex)
					{
						AudioManager.Play(AudioEffect.Failure);
						IOManager.StartNewPage(true);

						IOManager.WriteColored(false, false,
							($"There was an error whilst trying to import the users' file\n({Constants.OutputFilePath}).\n\n", ConsoleColor.Red),
							("Error details:\n", ConsoleColor.Gray)
						);
						IOManager.WriteColored(ex.Message, ConsoleColor.Cyan, true, true);
						Console.WriteLine("\nPlease solve the issue and try again.\n\n");

						var keyInfo = IOManager.WaitForKeyPress("Press 'Space' to navigate to the target file location, 'ESC' to permanently discard all\nusers' data (strongly not recommended), or any other key to attempt to load the file\nagain (select this option after solving the issue): ");

						switch (keyInfo.Key)
						{
							case ConsoleKey.Spacebar:
							{
								try
								{
									Process.Start(Constants.OutputDirectoryPath);
								}
								catch (Exception e)
								{
									AudioManager.Play(AudioEffect.Failure);
									IOManager.WriteColored(e.Message + "\n", ConsoleColor.Red, true, true, true);
								}

								break;
							}

							case ConsoleKey.Escape:
							{
								if (IOManager.WriteConfirmationMessage(("Are you sure? the file will be overwritten on game exit and all previous user data\nwill be lost!", ConsoleColor.Red)))
									return;

								break;
							}
						}
					}
				}

				foreach (var element in xmlData.Elements())
				{
					try
					{
						Users.Add(new User()
						{
							Username    = GetElementValue<string>(UsernameCaption),
							Level       = GetElementValue<int>(LevelCaption),
							TopScore    = GetElementValue<int>(TopScoreCaption),
							Armors      = GetElementValue<int>(ArmorsCaption),
							Balance     = GetElementValue<int>(BalanceCaption),
							Upgrades    = (Upgrades)GetElementValue<int>(UpgradesCaption),
							TimeElapsed = TimeSpan.Parse(GetElementValue<string>(TimeElapsedCaption))
						});
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"There was an error trying to import a user\n({ex.Message}).\n");
					}

					T GetElementValue<T>(string name) where T : IConvertible
						=> (T)Convert.ChangeType(element.Element(name).Value, typeof(T));
				}

				SortUserList();
            }

			internal static void ExportUsers()
			{
				if (!Directory.Exists(Constants.OutputDirectoryPath))
					Directory.CreateDirectory(Constants.OutputDirectoryPath);

				if (File.Exists(Constants.OutputFilePath))
				{
					// Remove the output file's hidden & read-only attributes
					var fileAttributes = File.GetAttributes(Constants.OutputFilePath);

					File.SetAttributes(
						Constants.OutputFilePath,
						fileAttributes & ~(FileAttributes.ReadOnly | FileAttributes.Hidden)
					);
				}

				var xmlData = new XDocument(
					new XComment(" This file is used to store the game players' statistics "),
					new XComment(" One small syntactical error can render the whole file unusable, thus edit it at your own risk "),
					new XElement("Users",
						GetUserElements()
					)
				);
				xmlData.Save(Constants.OutputFilePath);

				static IEnumerable<XElement> GetUserElements()
				{
					foreach (var user in Users)
					{
						// Avoid exporting users with zero playtime
						if (user.HasEverPlayed)
						{
							yield return new XElement("User",
								new XElement(UsernameCaption, user.Username),
								new XElement(LevelCaption, user.Level),
								new XElement(TopScoreCaption, user.TopScore),
								new XElement(ArmorsCaption, user.Armors),
								new XElement(BalanceCaption, user.Balance),
								new XElement(UpgradesCaption, (int)user.Upgrades),
								new XElement(TimeElapsedCaption, user.TimeElapsed.ToString())
							);
						}
					}
				}
			}
		}
	}
}
