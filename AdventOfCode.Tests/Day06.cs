using Dawn;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace AdventOfCode.Tests
{
	public class Day06
	{
		[Theory]
		[InlineData("1, 1", "1, 6", "8, 3", "3, 4", "5, 5", "8, 9")]
		public void Day06_ToPointsTest(params string[] lines)
		{
			var actual = lines.ToPointsDictionary();

			Assert.NotEmpty(actual);
			Assert.Equal(lines.Length, actual.Count);
			Assert.All(actual.Values, p => Assert.False(p.IsEmpty));
		}

		[Theory]
		[InlineData(5, "1, 1", "1, 6")]
		[InlineData(2, "0, 0", "1, 1")]
		[InlineData(4, "-1, -1", "1, 1")]
		public void Day06_GetManhattanDistanceTest(int expected, params string[] lines)
		{
			var points = lines.ToPointsDictionary();

			var actual = points["a"].GetManhattanDistance(points["b"]);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(10, 10, "1, 1", "1, 6", "8, 3", "3, 4", "5, 5", "8, 9")]
		public void Day06_BuildGridTests(int width, int height, params string[] lines)
		{
			var points = lines.ToPointsDictionary();

			var grid = points.ToGrid();

			grid = grid.GrowGrid(new Size(width, height));

			PopulateGrid(ref grid, points);

			var actual = string.Join(Environment.NewLine, grid.ToStrings());

			var expected = @"aaaaa.cccc
aAaaa.cccc
aaaddecccc
aadddeccCc
..dDdeeccc
bb.deEeecc
bBb.eeee..
bbb.eeefff
bbb.eeffff
bbb.ffffFf";

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(0, "a")]
		[InlineData(1, "b")]
		[InlineData(25, "z")]
		[InlineData(26, "ba")]
		public void Day06_ToAlphabeticalStringTests(int number, string expected)
		{
			var actual = number.ToAlphabeticalString();

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(0, 'a')]
		[InlineData(1, 'b')]
		[InlineData(25, 'z')]
		[InlineData(26, 'b', 'a')]
		public void Day06_ToCharsTests(int number, params char[] expected)
		{
			var actual = number.ToChars().Reverse().ToArray();

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(0, 'a')]
		[InlineData(1, 'b')]
		[InlineData(25, 'z')]
		public void Day06_ToCharTests(int number, char expected)
		{
			Assert.Equal(
				expected,
				number.ToChar());
		}

		[Fact]
		public void Day06_GrowGridTests_OneRowAndOneColumn()
		{
			var grid = new[,] { { 1, 1, }, { 2, 2, }, };

			var actual = grid.GrowGrid(new Size(3, 3));

			Assert.Equal(3, actual.GetLength(0));
			Assert.Equal(3, actual.GetLength(1));
			Assert.Equal(1, actual[0, 0]);
			Assert.Equal(1, actual[0, 1]);
			Assert.Equal(0, actual[0, 2]);
			Assert.Equal(2, actual[1, 0]);
			Assert.Equal(2, actual[1, 1]);
			Assert.Equal(0, actual[1, 2]);
			Assert.Equal(0, actual[2, 0]);
			Assert.Equal(0, actual[2, 1]);
			Assert.Equal(0, actual[2, 2]);
		}

		[Fact]
		public void Day06_GrowGridTests_TwoRowsAndOneColumn()
		{
			var grid = new[,] { { 1, 1, }, { 2, 2, }, };

			var actual = grid.GrowGrid(new Size(4, 3));

			Assert.Equal(4, actual.GetLength(0));
			Assert.Equal(3, actual.GetLength(1));
			Assert.Equal(0, actual[0, 0]);
			Assert.Equal(0, actual[0, 1]);
			Assert.Equal(0, actual[0, 2]);
			Assert.Equal(1, actual[1, 0]);
			Assert.Equal(1, actual[1, 1]);
			Assert.Equal(0, actual[1, 2]);
			Assert.Equal(2, actual[2, 0]);
			Assert.Equal(2, actual[2, 1]);
			Assert.Equal(0, actual[2, 2]);
			Assert.Equal(0, actual[3, 0]);
			Assert.Equal(0, actual[3, 1]);
			Assert.Equal(0, actual[3, 2]);
		}

		[Fact]
		public void Day06_GrowGridTests_TwoRowsAndTwoColumns()
		{
			var grid = new[,] { { 1, 1, }, { 2, 2, }, };

			var actual = grid.GrowGrid(new Size(4, 4));

			Assert.Equal(4, actual.GetLength(0));
			Assert.Equal(4, actual.GetLength(1));
			Assert.Equal(0, actual[0, 0]);
			Assert.Equal(0, actual[0, 1]);
			Assert.Equal(0, actual[0, 2]);
			Assert.Equal(0, actual[0, 3]);
			Assert.Equal(0, actual[1, 0]);
			Assert.Equal(1, actual[1, 1]);
			Assert.Equal(1, actual[1, 2]);
			Assert.Equal(0, actual[1, 3]);
			Assert.Equal(0, actual[2, 0]);
			Assert.Equal(2, actual[2, 1]);
			Assert.Equal(2, actual[2, 2]);
			Assert.Equal(0, actual[2, 3]);
			Assert.Equal(0, actual[3, 0]);
			Assert.Equal(0, actual[3, 1]);
			Assert.Equal(0, actual[3, 2]);
			Assert.Equal(0, actual[3, 3]);
		}

		[Fact]
		public void Day06_ToGridTests()
		{
			var lines = new[] { "1, 1", "1, 6", "8, 3", "3, 4", "5, 5", "8, 9", };
			var points = lines.ToPointsDictionary();
			var grid = points.ToGrid();
			grid = grid.GrowGrid(new Size(10, 10));
			PopulateGrid(ref grid, points);
			var strings = grid.ToStrings();
			var actual = string.Join(Environment.NewLine, strings);
			var expected = @"aaaaa.cccc
aAaaa.cccc
aaaddecccc
aadddeccCc
..dDdeeccc
bb.deEeecc
bBb.eeee..
bbb.eeefff
bbb.eeffff
bbb.ffffFf";

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Day06_GetFinitesTests()
		{
			// Arrange
			var lines = new[] { "1, 1", "1, 6", "8, 3", "3, 4", "5, 5", "8, 9", };
			var points = lines.ToPointsDictionary();
			var grid = points.ToGrid();
			PopulateGrid(ref grid, points);

			// Act
			var finites = grid.GetFinites().ToList();

			// Assert
			Assert.Equal(2, finites.Count);
			Assert.Equal("d", finites[0]);
			Assert.Equal("e", finites[1]);
		}

		[Fact]
		public void Day06_GetSizeOfAreas()
		{
			// Arrange
			var lines = new[] { "1, 1", "1, 6", "8, 3", "3, 4", "5, 5", "8, 9", };
			var points = lines.ToPointsDictionary();
			var grid = points.ToGrid();
			PopulateGrid(ref grid, points);

			// Act
			var sizesDictionary = grid.GetSizeOfAreas();

			// Assert
			Assert.Equal(15, sizesDictionary["a"]);
			Assert.Equal(14, sizesDictionary["b"]);
			Assert.Equal(15, sizesDictionary["c"]);
			Assert.Equal(9, sizesDictionary["d"]);
			Assert.Equal(17, sizesDictionary["e"]);
			Assert.Equal(10, sizesDictionary["f"]);
		}

		[Fact]//(Skip = "takes forever")]
		public void Day06_Part01()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Data", "coords.dat");

			var lines = path.GetLinesFromFile();
			var points = lines.ToPointsDictionary();

			Assert.Equal(50, points.Count);

			var grid = points.ToGrid();

			grid = grid.GrowGrid(new Size(400, 400));

			PopulateGrid(ref grid, points);

			var finites = grid.GetFinites();
			var sizesDictionary = grid.GetSizeOfAreas();

			var largestArea = (from t in sizesDictionary
							   let label = t.Key
							   let size = t.Value
							   where finites.Contains(label)
							   orderby size descending
							   select size
							  ).First();

			Assert.Equal(3223, largestArea);
		}

		[Fact]
		public void Day06_Part02()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Data", "coords.dat");
			var lines = path.GetLinesFromFile();
			var points = lines.ToPointsDictionary();
			var grid = points.ToGrid();
			var answer = grid.GetTotalDistancesToAllPoints(points.Values);
			var filtered = answer.Filter(i => i < 10_000);
			var count = filtered.Count();

			Assert.Equal(40_495, count);
		}

		private static void PopulateGrid(ref string[,] grid, IDictionary<string, Point> pointsDictionary)
		{
			var size = grid.GetSize();

			for (var x = 0; x < size.Width; x++)
			{
				for (var y = 0; y < size.Height; y++)
				{
					var point = new Point(x, y);
					var label = point.GetLabelOfNearestPoint(pointsDictionary);

					grid[x, y] = label;
				}
			}
		}

		[Fact]
		public void Day06_GetEdgeValues()
		{
			var array = new[,] { { 1, 2, 3, 4, 5, }, { 6, 7, 8, 9, 10, }, { 11, 12, 13, 14, 15, }, { 16, 17, 18, 19, 20, }, { 21, 22, 23, 24, 25, }, };

			var actual = array.GetEdgeValues().ToArray();

			Assert.Equal(20, actual.Length);
			Assert.Equal(new[] { 1, 5, 6, 10, 11, 15, 16, 20, 21, 25, 1, 21, 2, 22, 3, 23, 4, 24, 5, 25, }, actual);
		}
	}

	public static partial class ExtensionMethods
	{
		public static IEnumerable<T> Filter<T>(this T[,] grid, Predicate<T> predicate)
		{
			var size = grid.GetSize();

			for (var x = 0; x < size.Width; x++)
			{
				for (var y = 0; y < size.Height; y++)
				{
					var value = grid[x, y];

					if (predicate(value))
					{
						yield return value;
					}
				}
			}
		}

		public static int[,] GetTotalDistancesToAllPoints(this string[,] grid, IEnumerable<Point> points)
		{
			var size = grid.GetSize();
			var result = new int[size.Width, size.Height];

			for (var x = 0; x < size.Width; x++)
			{
				for (var y = 0; y < size.Height; y++)
				{
					result[x, y] = new Point(x, y).GetTotalDistanceToAllPoints(points);
				}
			}

			return result;
		}

		public static int GetTotalDistanceToAllPoints(this Point start, IEnumerable<Point> points)
		{
			var total = 0;

			foreach (var point in points)
			{
				total += start.GetManhattanDistance(point);
			}

			return total;
		}

		public static T[,] GrowGrid<T>(this T[,] array, Size newSize)
		{
			Guard.Argument(() => array).NotNull().NotEmpty();
			Guard.Argument(() => newSize.Width).Min(array.GetLength(0));
			Guard.Argument(() => newSize.Height).Min(array.GetLength(1));

			var oldSize = array.GetSize();
			var result = new T[newSize.Width, newSize.Height];

			var offset = (newSize - oldSize) / 2;

			for (var x = 0; x < oldSize.Width; x++)
			{
				for (var y = 0; y < oldSize.Height; y++)
				{
					result[offset.Width + x, offset.Height + y] = array[x, y];
				}
			}

			return result;
		}

		public static IEnumerable<string> GetLinesFromFile(this string path)
		{
			Guard.Argument(() => path).NotNull().NotEmpty().NotWhiteSpace();

			using (var reader = new StreamReader(path))
			{
				for (var line = reader.ReadLine(); line != default; line = reader.ReadLine())
				{
					yield return line;
				}
			}
		}

		public static IDictionary<string, int> GetSizeOfAreas(this string[,] grid)
		{
			var size = grid.GetSize();

			return (from x in Enumerable.Range(0, size.Width)
					from y in Enumerable.Range(0, size.Height)
					let label = grid[x, y].ToLowerInvariant()
					where label != "."
					group label by label into gg
					select gg
				   ).ToDictionary(gg => gg.Key, gg => gg.Count());
		}

		public static IEnumerable<string> GetFinites(this string[,] grid)
		{
			Guard.Argument(() => grid).NotNull().NotEmpty();

			var size = grid.GetSize();
			var maxX = size.Width - 1;
			var maxY = size.Height - 1;

			var labels = (from x in Enumerable.Range(0, size.Width)
						  from y in Enumerable.Range(0, size.Height)
						  let label = grid[x, y]
						  where label != "."
						  orderby label
						  select label.ToLowerInvariant()
						 ).Distinct();

			var infinites = (from label in grid.GetEdgeValues()
							 where label != "."
							 orderby label
							 select label.ToLowerInvariant()
							).Distinct();

			var finites = labels.Except(infinites).OrderBy(s => s);

			return finites;
		}

		public static IEnumerable<T> GetEdgeValues<T>(this T[,] array)
		{
			var size = array.GetSize();
			var maxX = array.GetLength(0) - 1;
			var maxY = array.GetLength(1) - 1;

			for (var x = 0; x < size.Width; x++)
			{
				yield return array[x, 0];
				yield return array[x, maxY];
			}

			for (var y = 0; y < size.Height; y++)
			{
				yield return array[0, y];
				yield return array[maxY, y];
			}
		}

		public static IEnumerable<string> ToStrings(this string[,] grid)
		{
			Guard.Argument(() => grid).NotNull().NotEmpty();

			var size = grid.GetSize();

			for (var y = 0; y < size.Height; y++)
			{
				var chars = new char[size.Width];

				for (var x = 0; x < size.Width; x++)
				{
					chars[x] = grid[x, y]?[0] ?? default;
				}

				yield return new string(chars);
			}
		}

		public static string[,] ToGrid(this IDictionary<string, Point> pointsDictionary)
		{
			Guard.Argument(() => pointsDictionary).NotNull().NotEmpty();

			var width = pointsDictionary.Values.Select(p => p.X).Max() + 1;
			var height = pointsDictionary.Values.Select(p => p.Y).Max() + 1;

			var grid = new string[width, height];

			foreach (var (label, point) in pointsDictionary)
			{
				grid[point.X, point.Y] = label;
			}

			return grid;
		}

		public static Size GetSize<T>(this T[,] array) => new Size(array.GetLength(0), array.GetLength(1));

		public static string ToAlphabeticalString(this int value)
		{
			Guard.Argument(() => value).NotNegative();

			var chars = value.ToChars().Reverse().ToArray();

			return new string(chars);
		}

		public static IEnumerable<char> ToChars(this int value)
		{
			Guard.Argument(() => value).NotNegative();

			do
			{
				var remainder = value % 26;
				value = value / 26;
				yield return remainder.ToChar();
			}
			while (value > 0);
		}

		public static char ToChar(this int value)
		{
			Guard.Argument(() => value).InRange(0, 25);

			return (char)(value + 97);
		}

		public static IDictionary<string, Point> ToPointsDictionary(this IEnumerable<string> lines)
		{
			Guard.Argument(() => lines).NotNull().NotEmpty();

			var index = 0;
			var dictionary = new Dictionary<string, Point>();

			foreach (var line in lines)
			{
				var values = line.Split(',');
				var x = int.Parse(values[0]);
				var y = int.Parse(values[1]);

				var label = index++.ToAlphabeticalString();

				dictionary.Add(label, new Point(x, y));
			}

			return dictionary;
		}

		public static IDictionary<string, Point> ToPointsDictionary(this string[,] grid)
		{
			var size = grid.GetSize();

			return (from x in Enumerable.Range(0, size.Width)
					from y in Enumerable.Range(0, size.Height)
					let label = grid[x, y]
					where label != default
					where label != "."
					where string.Equals(label, label.ToUpperInvariant(), StringComparison.InvariantCulture)
					select new { label, x, y }
				   ).ToDictionary(a => a.label, a => new Point(a.x, a.y));
		}

		public static int GetManhattanDistance(this Point one, Point two) => Math.Abs(one.X - two.X) + Math.Abs(one.Y - two.Y);

		public static string GetLabelOfNearestPoint(this Point start, IDictionary<string, Point> pointsDictionary)
		{
			//Guard.Argument(() => start).Require(p => p.X >= 0).Require(p => p.Y >= 0);
			Guard.Argument(() => pointsDictionary).NotNull().NotEmpty();
			Guard.Argument(() => pointsDictionary.Keys).Require(ss => !ss.Any(string.IsNullOrWhiteSpace));
			Guard.Argument(() => pointsDictionary.Values).Require(pp => pp.All(p => p.X > 0)).Require(pp => pp.All(p => p.Y > 0));

			var distances = new Dictionary<int, ICollection<string>>();

			foreach (var (label, end) in pointsDictionary)
			{
				var distance = start.GetManhattanDistance(end);

				if (distances.ContainsKey(distance))
				{
					distances[distance].Add(label);
				}
				else
				{
					distances.Add(distance, new List<string> { label, });
				}
			}

			return ResolveDistances(distances);
		}

		private static string ResolveDistances(IDictionary<int, ICollection<string>> distancesDictionary)
		{
			Guard.Argument(() => distancesDictionary).NotNull().NotEmpty();

			var (distance, labels) = distancesDictionary.OrderBy(kvp => kvp.Key).First();

			return ResolveDistance(distance, labels);
		}

		private static string ResolveDistance(int distance, ICollection<string> labels)
		{
			Guard.Argument(() => distance).NotNegative();
			Guard.Argument(() => labels).NotNull().NotEmpty().Require(ss => !ss.Any(string.IsNullOrWhiteSpace));

			if (distance == 0)
			{
				return labels.First().ToUpperInvariant();
			}

			switch (labels.Count)
			{
				case 0:
					throw new InvalidOperationException();
				case 1:
					return labels.First();
				default:
					return ".";
			}
		}
	}
}
