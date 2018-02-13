using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SourceDemoParser.Extensions
{
	public static class SourceExtensions
	{
		// Header
		public static int GetTickrate(this SourceDemo demo)
			=> (int)Math.Round(demo.PlaybackTicks / demo.PlaybackTime);
		public static float GetTicksPerSecond(this SourceDemo demo)
			=> demo.PlaybackTime / demo.PlaybackTicks;

		// Data
		public static IReadOnlyCollection<IDemoMessage> GetMessagesByType(this SourceDemo demo, DemoMessageType type)
			=> demo.Messages.Where(message => message.Type == type.MessageType).ToList();
		public static IReadOnlyCollection<IDemoMessage> GetMessagesByTick(this SourceDemo demo, int tick)
			=> demo.Messages.Where(message => message.Tick == tick).ToList();
		public static Task ParseFrames(this SourceDemo demo)
			=> Task.Run(() => demo.Messages
				.ForEach(async (m) => await m.Frame.Parse(demo).ConfigureAwait(false)));

		// Adjustments
		public static Task<SourceDemo> AdjustExact(this SourceDemo demo, int endTick = 0, int startTick = 0)
		{
			if (endTick < 1)
				endTick = demo.Messages.Last(m => m.Tick > 0).Tick;

			var delta = endTick - startTick;
			if (delta < 0)
				throw new Exception("Start tick is greater than end tick.");

			var tps = demo.GetTicksPerSecond();
			demo.PlaybackTicks = delta;
			demo.PlaybackTime = tps * delta;
			return Task.FromResult(demo);
		}
		public static async Task<SourceDemo> AdjustFlagAsync(this SourceDemo demo, string saveFlag = "echo #SAVE#")
		{
			if (demo.Messages.Count == 0)
				throw new InvalidOperationException("Cannot adjust ticks without parsed messages.");

			var flag = demo
				.GetMessagesByType(new Types.ConsoleCmd())
				.FirstOrDefault(m => (m.Frame as ConsoleCmdFrame).ConsoleCommand == saveFlag);
			
			if (flag != null)
			{
				await demo.AdjustExact(flag.Tick).ConfigureAwait(false);
				return demo;
			}
			return demo;
		}
	}
}