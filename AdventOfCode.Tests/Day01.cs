using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace AdventOfCode.Tests
{
	public class Day01
	{
		[Fact]
		public void Day01_Part01()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Data", "frequencies.dat");

			var answer = 0;

			using (var reader = new StreamReader(path))
			{
				string line;

				while ((line = reader.ReadLine()) != default)
				{
					answer += ParseLine(line);
				}
			}

			Assert.Equal(582, answer);
		}

		private static int ParseLine(string line) => int.Parse(line.Substring(1)) * (line[0] == '+' ? 1 : -1);

		[Fact]
		public void Day01_Part02()
		{
			var answer = DoWork();

			Assert.Equal(488, answer);
		}

		public static int DoWork()
		{
			string line;
			var frequency = 0;
			var frequencies = new List<int>();
			var path = Path.Combine(Environment.CurrentDirectory, "Data", "frequencies.dat");

			while (true)
			{
				using (var reader = new StreamReader(path))
				{
					while ((line = reader.ReadLine()) != default)
					{
						frequency += ParseLine(line);

						if (frequencies.Contains(frequency))
						{
							return frequency;
						}

						frequencies.Add(frequency);
					}
				}
			}
		}
	}
}
