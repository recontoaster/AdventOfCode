using Dawn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace AdventOfCode.Tests
{
	public class Day02
	{
		[Fact]
		public void Day02_Part01()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Data", "boxids.dat");
			var ids = GetBoxIds().ToList();
			var actual = GetChecksumByIds(ids);

			Assert.Equal(7533, actual);
		}

		private static IEnumerable<string> GetBoxIds()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Data", "boxids.dat");

			using (var reader = new StreamReader(path))
			{
				string line;

				while ((line = reader.ReadLine()) != default)
				{
					yield return line;
				}
			}
		}

		[Theory]
		[InlineData(12, "abcdef", "bababc", "abbcde", "abcccd", "aabcdd", "abcdee", "ababab")]
		public void Day02_GetChecksumByIds(int expected, params string[] ids)
		{
			var actual = GetChecksumByIds(ids);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(false, false, "abcdef")]
		[InlineData(true, true, "bababc")]
		[InlineData(true, false, "abbcde")]
		[InlineData(false, true, "abcccd")]
		[InlineData(true, false, "aabcdd")]
		[InlineData(true, false, "abcdee")]
		[InlineData(false, true, "ababab")]
		[InlineData(true, true, "mphcuiszrnjzxwkbgdzqeoyxfa")]
		[InlineData(true, true, "mihcuisgrnjzxwkbgdtqeoylia")]
		[InlineData(true, false, "mphauisvrnjgxwkbgdtqeiylfa")]
		public void Day02_GetChecksumById(bool expectedTwos, bool expectedThrees, string id)
		{
			var (twos, threes) = GetChecksumById(id);

			Assert.Equal(expectedTwos, twos);
			Assert.Equal(expectedThrees, threes);
		}

		private static int GetChecksumByIds(IEnumerable<string> ids)
		{
			int twos = 0, threes = 0;

			foreach (var id in ids)
			{
				var (a, b) = GetChecksumById(id);

				if (a) twos++;
				if (b) threes++;
			}

			return twos * threes;
		}

		private static (bool, bool) GetChecksumById(string id)
		{
			Guard
				.Argument(() => id)
				.NotNull()
				.NotEmpty()
				.NotWhiteSpace()
				.Require(s => Regex.IsMatch(s, "^[a-f]{6}$") || Regex.IsMatch(s, "^[a-z]{26}$"));

			bool twos = false, threes = false;

			for (var c = 'a'; c <= 'z'; c++)
			{
				switch (id.Count(z => z == c))
				{
					case 2:
						twos = true;
						break;
					case 3:
						threes = true;
						break;
				}
			}

			return (twos, threes);
		}

		[Theory]
		[InlineData("fghij", "fguij", "fgij")]
		[InlineData("abcde", "axcye", "ace")]
		public void Day02_CommonLetters(string left, string right, string expected)
		{
			var actual = GetCommonLetters(left, right);

			Assert.Equal(expected, actual);
		}

		private static string GetCommonLetters(string left, string right)
		{
			var result = GetCommonLetters(left.ToCharArray(), right.ToCharArray());

			return new string(result.ToArray());
		}

		private static IEnumerable<char> GetCommonLetters(IList<char> left, IList<char> right)
		{
			for (var a = 0; a < left.Count; a++)
			{
				if (left[a] == right[a])
				{
					yield return left[a];
				}
			}
		}

		[Fact]
		public void Day02_Part02()
		{
			var ids = GetBoxIds().ToList();

			var (left, right, common) = GetCommon(ids);

			Assert.Equal("mphcuasvrnjzzakbgdtqeoylva", left);
			Assert.Equal("mphcuasvrnjzzwkbgdtqeoylva", right);
			Assert.Equal("mphcuasvrnjzzkbgdtqeoylva", common);
		}

		private static (string, string, string) GetCommon(IEnumerable<string> strings)
		{
			foreach (var left in strings)
			{
				foreach (var right in strings)
				{
					if (left == right)
					{
						continue;
					}

					var common = GetCommonLetters(left, right);

					if (common.Length == left.Length - 1)
					{
						return (left, right, common);
					}
				}
			}

			return default;
		}
	}
}
