using System;
using System.IO;

namespace EscapeTheMaze
{
	// Global constants
	public static class Constants
	{
		public const int WindowWidth  = 100;
		public const int WindowHeight = 30;

		public const int BufferHeightOffset = 1;

		public const ConsoleColor KeyColor	 = ConsoleColor.Gray;
		public const ConsoleColor ValueColor = ConsoleColor.Green;

		public static readonly string OutputDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EscapeTheMaze");
		public static readonly string OutputFilePath	  = Path.Combine(OutputDirectoryPath, "Users.xml");
	}
}
