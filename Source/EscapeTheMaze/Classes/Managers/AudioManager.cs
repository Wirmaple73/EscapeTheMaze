using System;
using System.IO;
using System.Media;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace EscapeTheMaze.Managers
{
	public static class AudioManager
	{
		private static readonly MediaPlayer primaryPlayer	 = new();
		private static readonly MediaPlayer secondaryPlayer  = new();
		private static readonly SoundPlayer backgroundPlayer = new();

		private static readonly Random random = new();

		private static bool isBackgroundPlayerPlaying = false;

		public static void Play(AudioEffect effect)
		{
			string fileName = $"Resources/Audio/{GetSelectedAudioFileName()}.wav";

			if (!File.Exists(fileName))
				return;

			// Play the footstep sounds with the secondary player to avoid audio conflicts
			InternalPlay(effect is AudioEffect.Footstep or AudioEffect.BootsOfLeaping ? secondaryPlayer : primaryPlayer);

			void InternalPlay(MediaPlayer player)
			{
				player.Open(new Uri(fileName, UriKind.Relative));
				player.Play();
			}

			string GetSelectedAudioFileName()
			{
				if (!Enum.IsDefined(effect))
					throw new ArgumentException("Attempted to play an unknown sound effect.", nameof(effect));

				string fileName = effect.ToString();

				// Handle both single audio effects and effects with multiple variations
				return effect switch
				{
					AudioEffect.BootsOfLeaping						  => GetRandomAudioFileName(fileName, 2),
					AudioEffect.Footstep or AudioEffect.WallCollision => GetRandomAudioFileName(fileName, 4),
					_ => fileName,
				};
			}

			static string GetRandomAudioFileName(string fileName, int numVariants)
				=> $"{fileName}{random.Next(1, numVariants + 1)}";
		}

		public static void PlayLooping(BackgroundSound effect)
		{
			if (!Enum.IsDefined(effect))
				throw new ArgumentException("Attempted to play an unknown sound effect.", nameof(effect));

			// Playing a new sound will overwrite the current one if it's playing
			isBackgroundPlayerPlaying = true;

			using (backgroundPlayer)
			{
				backgroundPlayer.SoundLocation = $"Resources/Audio/{effect}.wav";
				backgroundPlayer.PlayLooping();
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void StopPlayback()
		{
			// No need to stop the player if it's idle
			if (!isBackgroundPlayerPlaying)
				return;

			isBackgroundPlayerPlaying = false;

			using (backgroundPlayer)
				backgroundPlayer.Stop();
		}
	}
}
