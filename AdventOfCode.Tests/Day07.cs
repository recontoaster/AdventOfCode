using Dawn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace AdventOfCode.Tests
{
	public class Day07
	{
		private static readonly string[] _lines = new[]
		{
			"Step C must be finished before step A can begin.",
			"Step C must be finished before step F can begin.",
			"Step A must be finished before step B can begin.",
			"Step A must be finished before step D can begin.",
			"Step B must be finished before step E can begin.",
			"Step D must be finished before step E can begin.",
			"Step F must be finished before step E can begin.",
		};

		[Fact]
		public void Day07_ParseInstructionTests()
		{
			var instructions = _lines.Select(ParseInstruction).ToList();

			Assert.Equal(7, instructions.Count);
			Assert.All(instructions, s => Assert.NotEqual(default, s.NextStep));
			Assert.All(instructions, s => Assert.True(char.IsUpper(s.NextStep)));
			Assert.All(instructions, s => Assert.NotEqual(default, s.DependentStep));
			Assert.All(instructions, s => Assert.True(char.IsUpper(s.DependentStep)));
			Assert.All(instructions, s => Assert.NotEqual(s.NextStep, s.DependentStep));
		}

		[Fact]
		public void Day07_GetStepsTests()
		{
			var instructions = _lines.Select(ParseInstruction);

			var steps = GetSteps(instructions).ToList();

			Assert.Equal(6, steps.Count);
			Assert.All(steps, c => Assert.NotEqual(default, c));
			Assert.All(steps, c => Assert.True(char.IsUpper(c)));
			Assert.Equal(steps.Count, steps.Distinct().Count());
			Assert.Contains('A', steps);
			Assert.Contains('B', steps);
			Assert.Contains('C', steps);
			Assert.Contains('D', steps);
			Assert.Contains('E', steps);
			Assert.Contains('F', steps);
		}

		[Fact]
		public void Day07_ienarsthdniatdher()
		{
			// get steps with no dependencies
			var instructions = _lines.Select(ParseInstruction).ToList();
			var steps = GetSteps(instructions).ToList();

			var noDependencies = (from s in steps
								  let b = from i in instructions
										  where i.NextStep == s
										  select 1
								  where !b.Any()
								  select s
								 ).ToList();

			Assert.Single(noDependencies);
			Assert.Equal('C', noDependencies[0]);
		}

		[Fact]
		public void Day07_GatherDependenciesTests()
		{
			var instructions = _lines.Select(ParseInstruction).ToList();

			var dependenciesDictionary = GatherDependencies(instructions);

			Assert.Equal(6, dependenciesDictionary.Count);
			Assert.Equal(new[] { 'C', }, dependenciesDictionary['A'].ToArray());
			Assert.Equal(new[] { 'A', }, dependenciesDictionary['B'].ToArray());
			Assert.Equal(new char[0], dependenciesDictionary['C'].ToArray());
			Assert.Equal(new[] { 'A', }, dependenciesDictionary['D'].ToArray());
			Assert.Equal(new[] { 'B', 'D', 'F', }, dependenciesDictionary['E'].ToArray());
			Assert.Equal(new[] { 'C', }, dependenciesDictionary['F'].ToArray());
		}

		[Fact]
		public void Day07_OrderByDependencies()
		{
			var instructions = _lines.Select(ParseInstruction).ToList();
			var dependenciesDictionary = GatherDependencies(instructions);

			var steps = OrderDependencies(dependenciesDictionary);

			Assert.Equal("CABDFE", new string(steps.ToArray()));
		}

		[Theory]
		[InlineData("IJLFUVDACEHGRZPNKQWSBTMXOY")]
		public void Day07_Part01(string expected)
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Data", "instructions.dat");
			var lines = path.GetLinesFromFile();
			var instructions = lines.Select(ParseInstruction);
			var dependenciesDictionary = GatherDependencies(instructions);
			var steps = OrderDependencies(dependenciesDictionary);
			Assert.Equal(expected, new string(steps.ToArray()));
		}

		private static char[,] BuildTimeGrid(IDictionary<char, ICollection<char>> dependenciesDictionary, int workerCount = 1)
		{
			var steps = OrderDependencies(dependenciesDictionary).ToList();

			throw new NotImplementedException();
		}

		private static IEnumerable<char> OrderDependencies(IDictionary<char, ICollection<char>> dependenciesDictionary)
		{
			var steps = new List<char>();

			while (dependenciesDictionary.Any())
			{
				var runNext = (from kvp in dependenciesDictionary
							   where kvp.Value.All(steps.Contains)
							   orderby kvp.Key
							   select kvp.Key
							  ).First();

				dependenciesDictionary.Remove(runNext);

				steps.Add(runNext);
			}

			return steps;
		}

		private static IDictionary<char, ICollection<char>> GatherDependencies(IEnumerable<IInstruction> instructions)
		{
			Guard
				.Argument(() => instructions)
				.NotNull()
				.NotEmpty()
				.Require(ii => ii.All(c => c.NextStep != c.DependentStep))
				.Require(ii => ii.All(c => char.IsUpper(c.NextStep)))
				.Require(ii => ii.All(c => char.IsUpper(c.DependentStep)));

			var steps = GetSteps(instructions).ToList();

			var query = from s in steps
						let dd = from i in instructions
								 where i.NextStep == s
								 select i.DependentStep
						select new { s, dd, };

			var dictionary = query.ToDictionary(a => a.s, a => (ICollection<char>)a.dd.ToList());

			return dictionary;
		}

		private static IEnumerable<char> GetSteps(IEnumerable<IInstruction> instructions) => instructions.SelectMany(i => new[] { i.DependentStep, i.NextStep, }).Distinct();

		private const string _pattern = @"^Step (?<DependentStep>[A-Z]) must be finished before step (?<NextStep>[A-Z]) can begin\.$";
		private const RegexOptions _regexOptions = RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
		private static readonly Regex _regex = new Regex(_pattern, _regexOptions);

		private static IInstruction ParseInstruction(string s)
		{
			Guard.Argument(() => s).NotNull().NotEmpty().NotWhiteSpace().Matches(_pattern);

			var match = _regex.Match(s);

			return new Instruction
			{
				NextStep = match.Groups["NextStep"].Value[0],
				DependentStep = match.Groups["DependentStep"].Value[0],
			};
		}
	}

	public interface IInstruction
	{
		char NextStep { get; }
		char DependentStep { get; }
	}

	public struct Instruction : IInstruction
	{
		public char NextStep { get; set; }
		public char DependentStep { get; set; }
	}
}
