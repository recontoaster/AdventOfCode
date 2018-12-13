using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Xunit;

namespace AdventOfCode.Tests
{
    public class Grid
    {
        private readonly IEnumerable<Point> _points;
        private readonly byte[,] _grid;

        public Grid(IEnumerable<Point> points)
        {
            var width = points.Select(p => p.X).Max() + 1;
            var height = points.Select(p => p.Y).Max() + 1;

            _grid = new byte[width, height];
            _points = points;

            PopulateGrid();
        }

        public byte this[int x, int y] => _grid[x, y];

        private void PopulateGrid()
        {
            for (var x = 0; x < _grid.GetLength(0); x++)
            {
                for (var y = 0; y < _grid.GetLength(1); y++)
                {
                    var distances = GetDistances(new Point(x, y));

                    _grid[x, y] = (byte)GetIndexOfClosest(distances);
                }
            }
        }

        private IEnumerable<int> GetDistances(Point start) => _points.Select(p => start.GetManhattanDistance(p));

        private static int GetIndexOfClosest(IEnumerable<int> distances)
        {
            return distances
                .Select((distance, index) => new { distance, index, })
                .OrderBy(a => a.distance)
                .First()
                .index;
        }
    }

    public class Day06
    {
        [Fact]
        public void Day06_GetPointsFromFileTests()
        {
            var points = GetPointsFromFile().ToList();

            Assert.NotEmpty(points);
            Assert.Equal(50, points.Count);
            Assert.All(points, p => Assert.False(p.IsEmpty));
        }

        private static IEnumerable<Point> GetPointsFromFile()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Data", "coords.dat");

            using (var reader = new StreamReader(path))
            {
                for (var line = reader.ReadLine(); line != default; line = reader.ReadLine())
                {
                    var ints = line.Split(',').Select(int.Parse).ToList();

                    yield return new Point(ints[0], ints[1]);
                }
            }
        }

        [Fact]
        public void Day06_ProcessCoordsTest()
        {
            var points = new[]
            {
                new Point(1, 1),
                new Point(1, 6),
                new Point(8, 3),
                new Point(3, 4),
                new Point(5, 5),
                new Point(8, 9),
            };

            var grid = new Grid(points);

            var grid = new string[10, 10];

            grid[1, 1] = "A";
            grid[1, 6] = "B";
            grid[8, 3] = "C";
            grid[3, 4] = "D";
            grid[5, 5] = "E";
            grid[8, 9] = "F";

            var counts = (from f in GetFinites(grid)
                          join c in GetCounts(grid) on f.Key equals c.Key
                          select c
                         ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Assert.Equal(9, counts["D"]);
            Assert.Equal(17, counts["E"]);
        }

        [Fact]
        public void Day06_Part01()
        {
            var points = GetPointsFromFile().ToList();

            var grid = ToStringArray(points);

            var counts = (from f in GetFinites(grid)
                          join c in GetCounts(grid) on f.Key equals c.Key
                          orderby c.Value descending
                          select c
                         ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Assert.Equal(46, counts.Count);
            Assert.Equal(6973, counts.Values.First());
        }

        private static readonly Random _random = new Random();

        private static string[,] ToStringArray(IEnumerable<Point> points)
        {
            var maxX = points.Select(p => p.X).Max() + 1;
            var maxY = points.Select(p => p.Y).Max() + 1;

            var values = new string[maxX, maxY];

            foreach (var point in points)
            {
                var value = Enumerable.Range(0, 10).Select(_ => _random.Next(26)).Select(i => (char)(i + 'A'));

                values[point.X, point.Y] = new string(value.ToArray());
            }

            return values;
        }

        [Fact]
        public void Day06_TestNotOnEdge()
        {
            var grid = new string[10, 10];

            grid[1, 1] = "A";
            grid[1, 6] = "B";
            grid[8, 3] = "C";
            grid[3, 4] = "D";
            grid[5, 5] = "E";
            grid[8, 9] = "F";

            var finites = GetFinites(grid);

            Assert.Equal(2, finites.Count);
            Assert.True(finites.ContainsKey("D"));
            Assert.True(finites.ContainsKey("E"));
            Assert.Equal(new Point(3, 4), finites["D"]);
            Assert.Equal(new Point(5, 5), finites["E"]);
        }

        private static IDictionary<string, Point> GetFinites(string[,] grid)
        {
            var dictionary = ToDictionary(grid);

            var minX = dictionary.Values.Select(p => p.X).Min();
            var maxX = dictionary.Values.Select(p => p.X).Max();
            var minY = dictionary.Values.Select(p => p.Y).Min();
            var maxY = dictionary.Values.Select(p => p.Y).Max();

            return (from kvp in dictionary
                    where kvp.Value.X > minX && kvp.Value.X < maxX && kvp.Value.Y > minY && kvp.Value.Y < maxY
                    select kvp
                   ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static IDictionary<string, Point> ToDictionary(string[,] grid)
        {
            return (from x in Enumerable.Range(0, grid.GetLength(0))
                    from y in Enumerable.Range(0, grid.GetLength(1))
                    let s = grid[x, y]
                    where s?.All(char.IsUpper) ?? false
                    select new { x, y, s }
                   ).ToDictionary(a => a.s, a => new Point(a.x, a.y));
        }

        private static void PopulateGrid(ref string[,] grid)
        {
            var dictionary = ToDictionary(grid);

            for (var x = 0; x < grid.GetLength(0); x++)
            {
                for (var y = 0; y < grid.GetLength(1); y++)
                {
                    grid[x, y] = GetValue(x, y, dictionary);
                }
            }
        }

        private static IDictionary<string, int> GetCounts(string[,] grid)
        {
            PopulateGrid(ref grid);

            var counts = new Dictionary<string, int>();

            foreach (var value in dictionary.Keys)
            {
                counts.Add(value, 0);

                for (var x = 0; x < grid.GetLength(0); x++)
                {
                    for (var y = 0; y < grid.GetLength(1); y++)
                    {
                        if (value == grid[x, y].ToUpperInvariant())
                        {
                            counts[value]++;
                        }
                    }
                }
            }

            return counts;
        }

        private static string GetValue(int x, int y, IDictionary<string, Point> dictionary)
        {
            var results = new Dictionary<string, int>();

            foreach (var (c, point) in dictionary)
            {
                results.Add(c, new Point(x, y).GetManhattanDistance(point));
            }

            var (value, distance) = results.OrderBy(kvp => kvp.Value).First();

            if (results.Values.Where(i => i == distance).Count() > 1)
            {
                return ".";
            }

            if (distance == 0)
            {
                return value.ToUpperInvariant();
            }

            return value.ToLowerInvariant();
        }

        [Theory]
        [InlineData(new[] { 0, 0, }, new[] { 1, 1, }, 2)]
        [InlineData(new[] { -1, -1, }, new[] { 1, 1, }, 4)]
        public void Day06_ManhattanDistanceTests(int[] one, int[] two, int expected)
        {
            Assert.Equal(
                expected,
                new Point(one[0], one[1]).GetManhattanDistance(new Point(two[0], two[1])));
        }
    }

    public static class ExtensionMethods
    {
        public static int GetManhattanDistance(this Point one, Point two)
        {
            int x = Math.Max(one.X, two.X) - Math.Min(one.X, two.X);
            int y = (Math.Max(one.Y, two.Y) - Math.Min(one.Y, two.Y));

            return x + y;
        }
    }
}
