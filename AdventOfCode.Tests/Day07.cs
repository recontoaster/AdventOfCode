using Dawn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var instructions = _lines.Select(Instruction.FromString).ToList();

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
            var instructions = _lines.Select(Instruction.FromString);

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
        public void Day07_GetStepsWithNoDependencies()
        {
            // get steps with no dependencies
            var instructions = _lines.Select(Instruction.FromString).ToList();
            var steps = GetSteps(instructions).ToList();

            var noDependencies = GetStepsWithNoDependencies(steps, instructions).ToList();

            Assert.Single(noDependencies);
            Assert.Equal('C', noDependencies[0]);
        }

        private static IEnumerable<char> GetStepsWithNoDependencies(ICollection<char> steps, ICollection<IInstruction> instructions)
        {
            return from s in steps
                   let ii = from i in instructions
                            where i.NextStep == s
                            select i
                   where !ii.Any()
                   select s;
        }

        [Fact]
        public void Day07_GatherDependenciesTests()
        {
            var instructions = _lines.Select(Instruction.FromString).ToList();

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
            var instructions = _lines.Select(Instruction.FromString).ToList();
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
            var instructions = lines.Select(Instruction.FromString);
            var dependenciesDictionary = GatherDependencies(instructions);
            var steps = OrderDependencies(dependenciesDictionary);
            Assert.Equal(expected, new string(steps.ToArray()));
        }

        [Theory]
        [InlineData("")]
        public void Day07_Part02(string expected)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Data", "instructions.dat");
            var lines = path.GetLinesFromFile();
            var instructions = lines.Select(Instruction.FromString).ToList();
            //var steps = GetSteps(instructions).ToList();
            var dependenciesDictionary = GatherDependencies(instructions);
            //var orderedSteps = OrderDependencies(dependenciesDictionary);
            var timeGrid = new TimeGrid(workerCount: 5);

            var steps = new List<char>();

            while (dependenciesDictionary.Any())
            {
                foreach (var (step, dependencies) in from kvp in dependenciesDictionary
                                                     where kvp.Value.All(steps.Contains)
                                                     orderby kvp.Key
                                                     select kvp)
                {
                    timeGrid.AddTask(step, (step - 64) + 0, dependencies.ToArray());
                    dependenciesDictionary.Remove(step);
                    steps.Add(step);
                }
            }

            var actual = timeGrid.ToString();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Day07_BuildTimeGrid()
        {
            // get step with no (or completed) dependencies...
            var instructions = _lines.Select(Instruction.FromString).ToList();
            var steps = GetSteps(instructions).ToList();
            var dependenciesDictionary = GatherDependencies(instructions);
            var timeGrid = new TimeGrid(workerCount: 2);

            foreach (var (step, dependencies) in dependenciesDictionary)
            {
                // add them to the time grid
                timeGrid.AddTask(step, step - 64, dependencies.ToArray());
            }

            var actual = timeGrid.ToString();
            var expected = "CABFDE";// string.Join(Environment.NewLine, new[] { "C.", "C.", "C.", "AF", "BF", "BF", "DF", "DF", "DF", "D.", "E.", "E.", "E.", "E.", "E.", });

            Assert.Equal(expected, actual);
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
    }

    public interface IInstruction
    {
        char NextStep { get; }
        char DependentStep { get; }
    }

    public struct Instruction : IInstruction
    {
        private const string _pattern = @"^Step (?<DependentStep>[A-Z]) must be finished before step (?<NextStep>[A-Z]) can begin\.$";
        private const RegexOptions _regexOptions = RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
        private static readonly Regex _regex = new Regex(_pattern, _regexOptions);

        public char NextStep { get; set; }
        public char DependentStep { get; set; }

        public static IInstruction FromString(string text)
        {
            Guard.Argument(() => text).NotNull().NotEmpty().NotWhiteSpace().Matches(_pattern);

            var match = _regex.Match(text);

            return new Instruction
            {
                NextStep = match.Groups["NextStep"].Value[0],
                DependentStep = match.Groups["DependentStep"].Value[0],
            };
        }

        public override string ToString() => $"Step {DependentStep} must be finished before step {NextStep} can begin.";
    }

    public class TimeGrid
    {
        private readonly int _workerCount;
        private readonly IList<string> _grid;

        public TimeGrid(int workerCount)
        {
            _workerCount = workerCount;
            _grid = new List<string>();
        }

        public char this[int second, int workerId]
        {
            get
            {
                while (_grid.Count <= second)
                {
                    _grid.Add(new string('.', _workerCount));
                }

                return _grid[second][workerId];
            }
            set
            {
                while (_grid.Count <= second)
                {
                    _grid.Add(new string('.', _workerCount));
                }

                var chars = _grid[second].ToCharArray();

                if (chars[workerId] != '.')
                {
                    throw new InvalidOperationException();
                }

                chars[workerId] = value;

                _grid[second] = new string(chars);
            }
        }

        public void AddTask(char name, int duration, params char[] dependencies)
        {
            var (second, workerId) = GetEarliestTaskCanBePerformed(dependencies);

            for (var a = 0; a < duration; a++)
            {
                this[second + a, workerId] = name;
            }
        }

        public (int, int) GetEarliestTaskCanBePerformed(ICollection<char> dependencies)
        {
            int second = 0, workerId = 0;

            if (dependencies.Count > 0)
            {

                // get the last second all the dependent tasks were performed
                foreach (var dependency in dependencies)
                {
                    if (!FindAllForTask(dependency).Any())
                    {
                        throw new InvalidOperationException("Task has not been performed.") { Data = { { nameof(dependency), dependency }, }, };
                    }
                }

                var (maxSecond, currWorkerId) = dependencies.SelectMany(FindAllForTask).OrderByDescending(tuple => tuple.Item1).First();

                maxSecond++;

                // can we use the same worker?
                if (this[maxSecond, currWorkerId] == '.')
                {
                    return (maxSecond, currWorkerId);
                }

                second = maxSecond;
            }

            if (second == _grid.Count)
            {
                _grid.Add(new string('.', _workerCount));
            }

            // find the next free second for any worker
            for (; second < _grid.Count; second++)
            {
                for (workerId = 0; workerId < _workerCount; workerId++)
                {
                    if (this[second, workerId] == '.')
                    {
                        return (second, workerId);
                    }
                }
            }


            throw new InvalidOperationException();
        }

        public IEnumerable<(int, int)> FindAllForTask(char task)
        {
            for (var second = 0; second < _grid.Count; second++)
            {
                for (var workerId = 0; workerId < _workerCount; workerId++)
                {
                    if (this[second, workerId] == task)
                    {
                        yield return (second, workerId);
                    }
                }
            }
        }

        public override string ToString()
        {
            var chars = _grid.Reverse().SelectMany(c => c).Distinct().Where(c => c != '.').Reverse().ToArray();

            return new string(chars);
        }
    }
}
