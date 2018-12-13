using Dawn;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace AdventOfCode.Tests
{
	public class Day03
	{
		[Theory]
		[InlineData("#123 @ 3,2: 5x4", 123, 3, 2, 5, 4)]
		[InlineData("#1 @ 1,3: 4x4", 1, 1, 3, 4, 4)]
		[InlineData("#2 @ 3,1: 4x4", 2, 3, 1, 4, 4)]
		[InlineData("#3 @ 5,5: 2x2", 3, 5, 5, 2, 2)]
		public void Day03_ClaimConstructorTests(string claimString, int id, int left, int top, int width, int height)
		{
			var actual = new Claim(claimString);

			Assert.Equal(id, actual.Id);
			Assert.Equal(left, actual.Left);
			Assert.Equal(top, actual.Top);
			Assert.Equal(width, actual.Width);
			Assert.Equal(height, actual.Height);
		}

		[Theory]
		[InlineData(4, "#1 @ 1,3: 4x4", "#2 @ 3,1: 4x4", "#3 @ 5,5: 2x2")]
		public void Day03_ClaimOverlapTests(int expected, params string[] claimsStrings)
		{
			var claims = claimsStrings.Select(s => new Claim(s)).ToList();

			var width = claims.Select(c => c.Width + c.Left).Max();
			var height = claims.Select(c => c.Height + c.Top).Max();
			var squares = BuildGrid(width, height);
			squares.AddClaims(claims);

			var overlaps = squares.Where(kvp => kvp.Value.Count > 1).ToList();

			Assert.Equal(expected, overlaps.Count);
		}

		[Theory]
		[InlineData(3, "#1 @ 1,3: 4x4", "#2 @ 3,1: 4x4", "#3 @ 5,5: 2x2")]
		public void Day03_ClaimNonOverlapTests(int expected, params string[] claimStrings)
		{
			var claims = claimStrings.Select(s => new Claim(s)).ToList();
			var width = claims.Select(c => c.Width + c.Left).Max();
			var height = claims.Select(c => c.Height + c.Top).Max();
			var squares = BuildGrid(width, height);
			squares.AddClaims(claims);

			var actual = GetNonOverlap(squares, claims);

			Assert.Equal(expected, actual);
		}

		private static int GetNonOverlap(IDictionary<Point, ICollection<Claim>> squares, IEnumerable<Claim> claims)
		{
			foreach (var claim in claims)
			{
				var overlaps = false;

				foreach (var square in from s in squares
									   let cc = from c in s.Value
												where c.Id == claim.Id
												select c
									   where cc.Any()
									   select s)
				{
					if (square.Value.Count > 1)
					{
						overlaps = true;
						break;
					}
				}

				if (!overlaps)
				{
					return claim.Id;
				}
			}

			return default;
		}

		private static IDictionary<Point, ICollection<Claim>> BuildGrid(int width, int height)
		{
			var grid = new Dictionary<Point, ICollection<Claim>>();

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					var point = new Point(x, y);

					grid[point] = new List<Claim>();
				}
			}

			return grid;
		}

		[Fact]
		public void Day03_Part01()
		{
			var claims = GetClaims().ToList();
			var width = claims.Select(c => c.Width + c.Left).Max();
			var height = claims.Select(c => c.Height + c.Top).Max();
			var squares = BuildGrid(width, height);
			squares.AddClaims(claims);

			var overlaps = squares.Where(kvp => kvp.Value.Count > 1).ToList();

			Assert.Equal(116_920, overlaps.Count);
		}

		[Fact]
		public void Day03_Part02()
		{
			var claims = GetClaims().ToList();
			var width = claims.Select(c => c.Width + c.Left).Max();
			var height = claims.Select(c => c.Height + c.Top).Max();
			var squares = BuildGrid(width, height);
			squares.AddClaims(claims);

			var actual = GetNonOverlap(squares, claims);

			Assert.Equal(382, actual);
		}

		private static IEnumerable<Claim> GetClaims()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Data", "claims.dat");

			using (var reader = new StreamReader(path))
			{
				string line;

				while ((line = reader.ReadLine()) != default)
				{
					yield return new Claim(line);
				}
			}
		}
	}

	public static class ExtensionMethods
	{
		public static IDictionary<Point, ICollection<Claim>> AddClaims(this IDictionary<Point, ICollection<Claim>> grid, IEnumerable<Claim> claims)
		{
			foreach (var claim in claims)
			{
				grid.AddClaim(claim);
			}

			return grid;
		}

		public static IDictionary<Point, ICollection<Claim>> AddClaim(this IDictionary<Point, ICollection<Claim>> grid, Claim claim)
		{
			for (var x = claim.Left; x < claim.Left + claim.Width; x++)
			{
				for (var y = claim.Top; y < claim.Top + claim.Height; y++)
				{
					var point = new Point(x, y);

					grid[point].Add(claim);
				}
			}

			return grid;
		}
	}

	public struct Claim
	{
		private const string _pattern = @"^#(?<Id>\d+) @ (?<Left>\d+),(?<Top>\d+): (?<Width>\d+)x(?<Height>\d+)$";
		private const RegexOptions _regexOptions = RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant;
		private static readonly Regex _regex = new Regex(_pattern, _regexOptions);
		private readonly Rectangle _rectangle;

		public Claim(string claimString)
		{
			Guard
				.Argument(() => claimString)
				.NotNull()
				.NotEmpty()
				.NotWhiteSpace()
				.Matches(_pattern);

			var match = _regex.Match(claimString);

			Id = int.Parse(match.Groups["Id"].Value);
			var left = int.Parse(match.Groups["Left"].Value);
			var top = int.Parse(match.Groups["Top"].Value);
			var width = int.Parse(match.Groups["Width"].Value);
			var height = int.Parse(match.Groups["Height"].Value);

			_rectangle = new Rectangle(left, top, width, height);
		}

		public int Id { get; }
		public int Left => _rectangle.X;
		public int Top => _rectangle.Y;
		public int Width => _rectangle.Width;
		public int Height => _rectangle.Height;
	}
}
