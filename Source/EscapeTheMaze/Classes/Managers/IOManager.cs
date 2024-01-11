using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EscapeTheMaze.Managers
{
	public static class IOManager
	{
		private static Position CursorPosition
		{
			get => new(Console.CursorLeft, Console.CursorTop);
			set => Console.SetCursorPosition(value.Left, value.Top);
		}

		public static void WriteAt(char value, Position position, ConsoleColor color = ConsoleColor.Gray)
			=> WriteAt(value.ToString(), position, color);

		public static void WriteAt(string value, int left, int top, ConsoleColor color = ConsoleColor.Gray, bool wrapText = false)
			=> WriteAt(value, new Position(left, top), color, wrapText);
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void WriteAt(string value, Position position, ConsoleColor color = ConsoleColor.Gray, bool wrapText = false)
		{
			var currentPosition = CursorPosition;
			CursorPosition = position;

			WriteColored(value, color, wrapText: wrapText);
			CursorPosition = currentPosition;
		}

		public static void WriteColored(string value, ConsoleColor color, bool newLine = false, bool wrapText = false, bool startNewPage = false)
		{
			if (startNewPage)
				StartNewPage(true);

			if (wrapText)
				value = value.Wrap();

			Console.ForegroundColor = color;
			Console.Write(newLine ? value + "\n" : value);
			Console.ResetColor();

			if (startNewPage)
				WaitForKeyPress("Press any key to continue...");

			
		}
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void WriteColored(bool startNewPage, bool resetPosition, params (string Value, ConsoleColor Color)[] values)
		{
			Position initialPosition = CursorPosition;

			if (startNewPage)
				StartNewPage(true);

			foreach (var (value, color) in values)
				WriteColored(value, color);

			if (startNewPage)
				WaitForKeyPress("\nPress any key to continue...");
			
			if (resetPosition)
				CursorPosition = initialPosition;
		}

		public static bool WriteConfirmationMessage(params (string Value, ConsoleColor Color)[] values)
		{
			StartNewPage(true);

			foreach (var (value, color) in values)
				WriteColored(value, color);

			return WaitForKeyPress("\n\nPress 'Enter' to accept, or any other key to decline...").Key == ConsoleKey.Enter;
		}

		public static void DrawMenu(params string[] captions)
		{
			const ConsoleColor captionColor = ConsoleColor.Cyan;

			for (int i = 0; i < captions.Length; i++)
			{
				WriteColored(false, false,
					("[", Constants.KeyColor),
					($"{i + 1}", Constants.ValueColor),
					("] ", Constants.KeyColor),
					(captions[i] + "\n", captionColor)
				);
			}
		}

		public static void WriteHeader(string value)
			=> WriteColored($"---- {value} ----", ConsoleColor.Yellow, true, wrapText: true);

		public static void ClearLine(int top) => WriteAt(new string(' ', Constants.WindowWidth), 0, top);

		public static void StartNewPage(bool isCursorVisible = false)
		{
			Console.Clear();
			Console.CursorVisible = isCursorVisible;
		}

		public static ConsoleKeyInfo WaitForKeyPress(string prompt = null, bool acceptOnlyEnter = false, bool playAudioEffect = true)
		{
			ConsoleKeyInfo keyInfo;

			if (prompt is not null)
				Console.Write(prompt);

			if (acceptOnlyEnter)
				while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Enter) { }
			else
				keyInfo = Console.ReadKey(true);

			if (playAudioEffect)
				AudioManager.Play(AudioEffect.MenuNavigation);

			return keyInfo;
		}

		public static void SetBufferHeight(int offset)
		{
			Console.Clear();  // Used to avoid a possible exception after setting the buffer height back
			Console.BufferHeight = Constants.WindowHeight + offset;
		}

		public static void ResetBufferHeight() => SetBufferHeight(Constants.BufferHeightOffset);
	}
}
