using Dawn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace AdventOfCode.Tests
{
    public class Day05
    {
        [Theory]
        [InlineData("aA", "")]
        [InlineData("abBA", "")]
        [InlineData("abAB", "abAB")]
        [InlineData("aabAAB", "aabAAB")]
        [InlineData("dabAcCaCBAcCcaDA", "dabCBAcaDA")]
        public void Day05_ProcessPolymerTests(string polymer, string expected)
        {
            Assert.Equal(
                expected,
                ReactPolymer(polymer));
        }

        [Theory]
        [InlineData(9_370)]
        public void Day05_Part01(int expectedCount)
        {
            var polymer = GetPolymerFromFile();

            var result = ReactPolymer(polymer);

            Assert.Equal(expectedCount, result.Length);
        }

        private static string GetPolymerFromFile()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Data", "polymer.dat");

            using (var reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }

        [Theory]
        [InlineData(6_390)]
        public void Day05_Part02(int expectedLength)
        {
            var polymer = GetPolymerFromFile();

            var shortest = int.MaxValue;

            foreach (var reduced in ReducePolymer(polymer))
            {
                var reacted = ReactPolymer(reduced);

                if(reacted.Length < shortest)
                {
                    shortest = reacted.Length;
                }
            }

            Assert.Equal(expectedLength, shortest);
        }

        [Theory]
        [InlineData("dabAcCaCBAcCcaDA", "dbcCCBcCcD", "daAcCaCAcCcaDA", "dabAaBAaDA", "abAcCaCBAcCcaA")]
        public void Day05_ReductionsTest(string polymer, params string[] expectedReductions)
        {
            Assert.Equal(
                expectedReductions,
                ReducePolymer(polymer).ToArray());
        }

        private static IEnumerable<string> ReducePolymer(string polymer)
        {
            foreach (var c in polymer.ToLowerInvariant().Distinct().OrderBy(c => c))
            {
                yield return polymer.Replace(c.ToString(), string.Empty, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public static string ReactPolymer(string polymer)
        {
            Guard
                .Argument(() => polymer)
                .NotNull()
                .NotEmpty()
                .NotWhiteSpace()
                .MinLength(2)
                .Require(s => s.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')));

            IList<char> chars = polymer.ToList();

            while (ProcessUnits(ref chars)) { }

            return new string(chars.ToArray());
        }

        public static bool ProcessUnits(ref IList<char> units)
        {
            for (var i = 0; i < units.Count - 1; i++)
            {
                if (!AreUnitsOppositePolarity(units[i], units[i + 1]))
                {
                    continue;
                }

                units.RemoveAt(i);
                units.RemoveAt(i);

                return true;
            }

            return false;
        }

        public static bool AreUnitsOppositePolarity(char left, char right)
        {
            if (left.ToString().ToLowerInvariant() != right.ToString().ToLowerInvariant())
            {
                return false;
            }

            if (char.IsLower(left) && char.IsLower(right))
            {
                return false;
            }

            if (char.IsUpper(left) && char.IsUpper(right))
            {
                return false;
            }

            return true;
        }
    }
}
