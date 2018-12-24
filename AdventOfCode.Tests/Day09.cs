using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace AdventOfCode.Tests
{
    public class Day09
    {
        [Theory]
        [InlineData(",", 0, "(0)")]
        [InlineData("(0),", 1, " 0 (1)")]
        [InlineData(" 0 (1),", 2, " 0 (2) 1")]
        [InlineData(" 0 (2) 1,", 3, " 0  2  1 (3)")]
        [InlineData(" 0  2  1 (3),", 4, " 0 (4) 2  1  3")]
        [InlineData(" 0 (4) 2  1  3,", 5, " 0  4  2 (5) 1  3")]
        [InlineData(" 0  4  2 (5) 1  3,", 6, " 0  4  2  5  1 (6) 3")]
        [InlineData(" 0  4  2  5  1 (6) 3,", 7, " 0  4  2  5  1  6  3 (7)")]
        [InlineData(" 0  4  2  5  1  6  3 (7),", 8, " 0 (8) 4  2  5  1  6  3  7")]
        [InlineData(" 0 (8) 4  2  5  1  6  3  7,", 9, " 0  8  4 (9) 2  5  1  6  3  7")]
        [InlineData(" 0  8  4 (9) 2  5  1  6  3  7,", 10, " 0  8  4  9  2(10) 5  1  6  3  7")]
        [InlineData(" 0  8  4  9  2(10) 5  1  6  3  7,", 11, " 0  8  4  9  2 10  5(11) 1  6  3  7")]
        [InlineData(" 0  8  4  9  2 10  5(11) 1  6  3  7,", 12, " 0  8  4  9  2 10  5 11  1(12) 6  3  7")]
        [InlineData(" 0  8  4  9  2 10  5 11  1(12) 6  3  7,", 13, " 0  8  4  9  2 10  5 11  1 12  6(13) 3  7")]
        [InlineData(" 0  8  4  9  2 10  5 11  1 12  6(13) 3  7,", 14, " 0  8  4  9  2 10  5 11  1 12  6 13  3(14) 7")]
        [InlineData(" 0  8  4  9  2 10  5 11  1 12  6 13  3(14) 7,", 15, " 0  8  4  9  2 10  5 11  1 12  6 13  3 14  7(15)")]
        [InlineData(" 0  8  4  9  2 10  5 11  1 12  6 13  3 14  7(15),", 16, " 0(16) 8  4  9  2 10  5 11  1 12  6 13  3 14  7 15")]
        [InlineData(" 0(16) 8  4  9  2 10  5 11  1 12  6 13  3 14  7 15,", 17, " 0 16  8(17) 4  9  2 10  5 11  1 12  6 13  3 14  7 15")]
        [InlineData(" 0 16  8(17) 4  9  2 10  5 11  1 12  6 13  3 14  7 15,", 18, " 0 16  8 17  4(18) 9  2 10  5 11  1 12  6 13  3 14  7 15")]
        [InlineData(" 0 16  8 17  4(18) 9  2 10  5 11  1 12  6 13  3 14  7 15,", 19, " 0 16  8 17  4 18  9(19) 2 10  5 11  1 12  6 13  3 14  7 15")]
        [InlineData(" 0 16  8 17  4 18  9(19) 2 10  5 11  1 12  6 13  3 14  7 15,", 20, " 0 16  8 17  4 18  9 19  2(20)10  5 11  1 12  6 13  3 14  7 15")]
        [InlineData(" 0 16  8 17  4 18  9 19  2(20)10  5 11  1 12  6 13  3 14  7 15,", 21, " 0 16  8 17  4 18  9 19  2 20 10(21) 5 11  1 12  6 13  3 14  7 15")]
        [InlineData(" 0 16  8 17  4 18  9 19  2 20 10(21) 5 11  1 12  6 13  3 14  7 15,", 22, " 0 16  8 17  4 18  9 19  2 20 10 21  5(22)11  1 12  6 13  3 14  7 15")]
        [InlineData(" 0 16  8 17  4 18  9 19  2 20 10 21  5(22)11  1 12  6 13  3 14  7 15,", 23, " 0 16  8 17  4 18(19) 2 20 10 21  5 22 11  1 12  6 13  3 14  7 15")]
        [InlineData(" 0 16  8 17  4 18(19) 2 20 10 21  5 22 11  1 12  6 13  3 14  7 15,", 24, " 0 16  8 17  4 18 19  2(24)20 10 21  5 22 11  1 12  6 13  3 14  7 15")]
        [InlineData(" 0 16  8 17  4 18 19  2(24)20 10 21  5 22 11  1 12  6 13  3 14  7 15,", 25, " 0 16  8 17  4 18 19  2 24 20(25)10 21  5 22 11  1 12  6 13  3 14  7 15")]
        [InlineData(" 0 16  8 17  4 18 19  2 24 20(25)10 21  5 22 11  1 12  6 13  3 14  7 15,", 26, " 0 16  8 17  4 18 19  2 24 20 25 10 (26) 21  5 22 11  1 12  6 13  3 14  7 15")]

        public void Day09_AddMarble(string before, int marbleNo, string after)
        {
            var (marbles, currentIndex) = Parse(before);

            AddMarble(ref marbles, ref currentIndex, marbleNo);

            var (expectedMarbles, expectedCurrentIndex) = Parse(after);

            Assert.Equal(expectedMarbles, marbles);
            Assert.Equal(expectedCurrentIndex, currentIndex);
        }

        [Fact]
        public void Day09_AddTwentyFourthMarble()
        {
            var input = "0 16  8 17  4 18  9 19  2 20 10 21  5(22)11  1 12  6 13  3 14  7 15";

            var (marbles, current) = Parse(input);

            Assert.Equal(23, marbles.Count);
            Assert.Equal(13, current);

            var score = AddMarble(ref marbles, ref current);

            Assert.Equal(32, score);
            Assert.Equal(22, marbles.Count);
            Assert.Equal(6, current);
        }

        [Fact]
        public void Day09_AddTwentyFifthMarble()
        {
            var before = "0 16  8 17  4 18(19) 2 20 10 21  5 22 11  1 12  6 13  3 14  7 15";
            var after = "0 16  8 17  4 18 19  2(24)20 10 21  5 22 11  1 12  6 13  3 14  7 15";

            var (marbles, current) = Parse(input);

            Assert.Equal(23, marbles.Count);
            Assert.Equal(6, current);

            var score = AddMarble(ref marbles, ref current, 24);

            Assert.Equal(32, score);
            Assert.Equal(22, marbles.Count);
            Assert.Equal(6, current);
        }

        [Theory]
        [InlineData("0, (4), 2, 1, 3,", new[] { 0, 4, 2, 1, 3, }, 1)]
        [InlineData("0  8  4  9  2 10  5 11  1 12  6 13  3(14) 7", new[] { 0, 8, 4, 9, 2, 10, 5, 11, 1, 12, 6, 13, 3, 14, 7, }, 13)]
        public void Day09_ParserTest(string input, int[] expectedMarbles, int expectedCurrentIndex)
        {
            var (marbles, currentIndex) = Parse(input);

            Assert.Equal(expectedMarbles, marbles);
            Assert.Equal(expectedCurrentIndex, currentIndex);
        }

        private const RegexOptions _regexOptions = RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant;
        private static readonly Regex _regex = new Regex(@"(?<Current>\()?(?<Value>\d+)\)?", _regexOptions);

        private static (IList<int>, int?) Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return (new List<int> { }, default);
            }

            var index = 0;
            int? curr = default;
            var values = new List<int>();

            var matches = _regex.Matches(input);

            foreach (Match match in matches)
            {
                var value = int.Parse(match.Groups["Value"].Value);

                values.Add(value);

                if (match.Groups["Current"].Success)
                {
                    curr = index;
                }

                index++;
            }

            return (values, curr);
        }

        private static int AddMarble(ref IList<int> marbles, ref int? currentMarbleIndex, int marbleNo)
        {
            switch (marbleNo)
            {
                case 0:
                    marbles.Add(0);
                    currentMarbleIndex = 0;
                    return 0;
            }

            if (!currentMarbleIndex.HasValue)
            {
                marbles.Add(0);
                currentMarbleIndex = 0;
                return 0;
            }

            switch (marbles.Count)
            {
                case 1:
                    marbles.Add(1);
                    currentMarbleIndex = 1;
                    return 0;
                case 2:
                    marbles.Insert(index: 1, item: 2);
                    currentMarbleIndex = 1;
                    return 0;
            }

            var newMarble = marbles.Count;
            var newMarbleIndex = currentMarbleIndex.Value + 2;

            if ((marbleNo % 23) != 0)
            {
                if (newMarbleIndex == marbles.Count)
                {
                    marbles.Add(newMarble);
                }
                else
                {
                    newMarbleIndex %= marbles.Count;
                    marbles.Insert(index: newMarbleIndex, item: newMarble);
                }

                currentMarbleIndex = newMarbleIndex;

                return 0;
            }

            var indexOfMarbleToRemove = currentMarbleIndex.Value - 7;
            var valueOfRemovedMarble = marbles[indexOfMarbleToRemove];
            marbles.RemoveAt(indexOfMarbleToRemove);
            currentMarbleIndex = indexOfMarbleToRemove;
            return newMarble + valueOfRemovedMarble;
        }
    }
}
