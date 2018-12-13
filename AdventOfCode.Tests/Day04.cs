using Dawn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace AdventOfCode.Tests
{
	public class Day04
	{
		[Fact]
		public void Day04_ParseRecordsTest()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Data", "guard_activities.dat");

			var records = RecordsFromFile(path).ToList();

			Assert.NotNull(records);
			Assert.NotEmpty(records);

			Assert.All(
				records.Where(r => (r.State & (States.FallsAsleep | States.WakesUp)) != default),
				r => Assert.InRange(r.DateTime.TimeOfDay, TimeSpan.Zero, TimeSpan.FromMinutes(59)));

			var dictionary = RecordsToGuardIdDictionary(records);

			// asleep the most
			var id = (from kvp in dictionary
					  orderby kvp.Value.Count descending
					  select kvp.Key
						).First();

			Assert.Equal(2_351, id);

			// worst minute
			var minute = (from kvp in dictionary
						  where kvp.Key == id
						  from dt in kvp.Value
						  let min = dt.Minute
						  group min by min into gg
						  orderby gg.Count() descending
						  select gg.Key
						).First();

			Assert.Equal(36, minute);

			Assert.Equal(84_636, id * minute);
		}

		[Fact]
		public void Day04_ParseRecordsTest_Part2()
		{
			var path = Path.Combine(Environment.CurrentDirectory, "Data", "guard_activities.dat");

			var records = RecordsFromFile(path).ToList();

			Assert.NotNull(records);
			Assert.NotEmpty(records);

			Assert.All(
				records.Where(r => (r.State & (States.FallsAsleep | States.WakesUp)) != default),
				r => Assert.InRange(r.DateTime.TimeOfDay, TimeSpan.Zero, TimeSpan.FromMinutes(59)));

			var dictionary = RecordsToMinuteDictionary(records);

			var query = from kvp in dictionary
						from kvp2 in kvp.Value
						orderby kvp2.Value descending
						select (kvp.Key, kvp2.Key, kvp2.Value);

			var (minute, guardId, count) = query.First();

			Assert.Equal(49, minute);
			Assert.Equal(1_871, guardId);
			Assert.Equal(18, count);
			Assert.Equal(91_679, minute * guardId);
		}

		[Fact]
		public void Day04_ParseExampleRecordsTest()
		{
			var strings = new[]
			{
				"[1518-11-01 00:00] Guard #10 begins shift",
				"[1518-11-01 00:05] falls asleep",
				"[1518-11-01 00:25] wakes up",
				"[1518-11-01 00:30] falls asleep",
				"[1518-11-01 00:55] wakes up",
				"[1518-11-01 23:58] Guard #99 begins shift",
				"[1518-11-02 00:40] falls asleep",
				"[1518-11-02 00:50] wakes up",
				"[1518-11-03 00:05] Guard #10 begins shift",
				"[1518-11-03 00:24] falls asleep",
				"[1518-11-03 00:29] wakes up",
				"[1518-11-04 00:02] Guard #99 begins shift",
				"[1518-11-04 00:36] falls asleep",
				"[1518-11-04 00:46] wakes up",
				"[1518-11-05 00:03] Guard #99 begins shift",
				"[1518-11-05 00:45] falls asleep",
				"[1518-11-05 00:55] wakes up",
			};

			var records = LogsToRecords(strings).ToList();

			Assert.All(
				records.Where(r => (r.State & (States.FallsAsleep | States.WakesUp)) != default),
				r => Assert.InRange(r.DateTime.TimeOfDay, TimeSpan.Zero, TimeSpan.FromMinutes(59)));

			var dictionary = RecordsToGuardIdDictionary(records);

			// asleep the most
			var id = (from kvp in dictionary
					  orderby kvp.Value.Count descending
					  select kvp.Key
						).First();

			Assert.Equal(10, id);

			// worst minute
			var minute = (from kvp in dictionary
						  where kvp.Key == id
						  from dt in kvp.Value
						  let min = dt.Minute
						  group min by min into gg
						  orderby gg.Count() descending
						  select gg.Key
						).First();

			Assert.Equal(24, minute);

			Assert.Equal(240, id * minute);
		}

		[Theory]
		[InlineData("[1518-11-01 00:00] Guard #10 begins shift", "1518-11-01T00:00:00Z", 10, States.BeginsShift)]
		[InlineData("[1518-11-01 00:05] falls asleep", "1518-11-01T00:05:00Z", 10, States.FallsAsleep)]
		[InlineData("[1518-11-01 00:25] wakes up", "1518-11-01T00:25:00Z", 10, States.WakesUp)]
		public void Day04_ParseRecordTest(string s, string expectedDateTimeString, int expectedGuardId, States expectedState)
		{
			var record = Records.FromRecordBases(new[] { RecordBase.FromString(s), }).Single();

			Assert.Equal(DateTime.Parse(expectedDateTimeString), record.DateTime);
			Assert.True(record.GuardId == default || record.GuardId == expectedGuardId);
			Assert.Equal(expectedState, record.State);
		}

		private static IEnumerable<IRecord> RecordsFromFile(string path)
		{
			Guard.Argument(() => path).NotNull().NotEmpty().NotWhiteSpace();

			var lines = new List<string>();

			using (var reader = new StreamReader(path))
			{
				Guard.Argument(() => reader).NotNull().Require(r => r.BaseStream.CanRead);

				for (var line = reader.ReadLine(); line != default; line = reader.ReadLine())
				{
					lines.Add(line);
				}
			}

			return LogsToRecords(lines);
		}

		private static IEnumerable<IRecord> LogsToRecords(IEnumerable<string> logs) => Records.FromRecordBases(logs.Select(RecordBase.FromString));

		private static IDictionary<int, ICollection<DateTime>> RecordsToGuardIdDictionary(IEnumerable<IRecord> records)
		{
			var dictionary = new Dictionary<int, ICollection<DateTime>>();

			foreach (var (guardId, date, times) in from r in records
												   group r by (r.GuardId, r.DateTime.Date) into gg
												   let times = from g in gg
															   where (g.State & (States.FallsAsleep | States.WakesUp)) != default
															   orderby g.DateTime
															   select (g.DateTime, g.State)
												   orderby gg.Key.Date
												   select (gg.Key.GuardId, gg.Key.Date, times.ToList()))
			{
				Assert.Equal(0, times.Count % 2);

				foreach (var start in from t in times
									  where (t.State & States.FallsAsleep) != default
									  select t.DateTime)
				{
					var end = (from t in times
							   where (t.State & States.WakesUp) != default
							   where t.DateTime >= start
							   select t.DateTime
							  ).First();

					for (var dateTime = start; dateTime < end; dateTime = dateTime.Add(TimeSpan.FromMinutes(1)))
					{
						if (dictionary.ContainsKey(guardId))
						{
							dictionary[guardId].Add(dateTime);
						}
						else
						{
							dictionary.Add(guardId, new List<DateTime> { dateTime, });
						}
					}
				}
			}

			return dictionary;
		}

		private static IDictionary<int, IDictionary<int, int>> RecordsToMinuteDictionary(IEnumerable<IRecord> records)
		{
			var dictionary = new Dictionary<int, IDictionary<int, int>>();

			foreach (var (guardId, date, times) in from r in records
												   group r by (r.GuardId, r.DateTime.Date) into gg
												   let times = from g in gg
															   where (g.State & (States.FallsAsleep | States.WakesUp)) != default
															   orderby g.DateTime
															   select (g.DateTime, g.State)
												   orderby gg.Key.Date
												   select (gg.Key.GuardId, gg.Key.Date, times.ToList()))
			{
				Assert.Equal(0, times.Count % 2);

				foreach (var start in from t in times
									  where (t.State & States.FallsAsleep) != default
									  select t.DateTime)
				{
					var end = (from t in times
							   where (t.State & States.WakesUp) != default
							   where t.DateTime >= start
							   select t.DateTime
							  ).First();

					for (var dateTime = start; dateTime < end; dateTime = dateTime.Add(TimeSpan.FromMinutes(1)))
					{
						if (dictionary.ContainsKey(dateTime.Minute))
						{
							if (dictionary[dateTime.Minute].ContainsKey(guardId))
							{
								dictionary[dateTime.Minute][guardId]++;
							}
							else
							{
								dictionary[dateTime.Minute].Add(guardId, 1);
							}
						}
						else
						{
							dictionary.Add(dateTime.Minute, new Dictionary<int, int> { { guardId, 1 }, });
						}
					}
				}
			}

			return dictionary;

		}

		public enum States : byte
		{
			None = 0,
			BeginsShift = 1,
			FallsAsleep = 2,
			WakesUp = 4,
		}

		public interface IRecord
		{
			DateTime DateTime { get; }
			int GuardId { get; }
			States State { get; }
		}

		public interface IRecordBase
		{
			DateTime DateTime { get; }
			string Message { get; }
		}

		public class RecordBase : IRecordBase
		{
			private const RegexOptions _regexOptions = RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant;
			private const string _pattern = @"^\[(?<DateTime>\d{4}-\d{2}-\d{2} \d{2}:\d{2})\] (?<Message>.+)$";
			private static readonly Regex _regex = new Regex(_pattern, _regexOptions);

			public DateTime DateTime { get; private set; }
			public string Message { get; private set; }

			public static IRecordBase FromString(string s)
			{
				Guard.Argument(() => s).NotNull().NotEmpty().NotWhiteSpace().Matches(_pattern);

				var match = _regex.Match(s);

				var dateTime = DateTime.SpecifyKind(DateTime.Parse(match.Groups["DateTime"].Value), DateTimeKind.Utc);
				var message = match.Groups["Message"].Value;

				return new RecordBase { DateTime = dateTime, Message = message, };
			}
		}

		public class Records : List<IRecord>
		{
			private const RegexOptions _regexOptions = RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant;
			private const string _pattern = @"^Guard #(?<GuardId>\d+) begins shift$";
			private static readonly Regex _regex = new Regex(_pattern, _regexOptions);

			public Records(IEnumerable<IRecord> records)
				: base(records)
			{ }

			public static IEnumerable<IRecord> FromRecordBases(IEnumerable<IRecordBase> recordBases)
			{
				var records = new List<IRecord>();

				int guardId = default;

				foreach (var recordBase in from r in recordBases
										   orderby r.DateTime
										   select r)
				{
					IRecord record;

					switch (recordBase.Message)
					{
						case "falls asleep":
							record = new Record(recordBase.DateTime, guardId, States.FallsAsleep);
							break;
						case "wakes up":
							record = new Record(recordBase.DateTime, guardId, States.WakesUp);
							break;
						default:
							var match = _regex.Match(recordBase.Message);
							guardId = int.Parse(match.Groups["GuardId"].Value);
							record = new Record(recordBase.DateTime, guardId, States.BeginsShift);
							break;
					}

					records.Add(record);
				}

				return new Records(records);
			}
		}

		public class Record : IRecord
		{
			public Record(DateTime dateTime, int guardId, States state)
			{
				DateTime = dateTime;
				GuardId = guardId;
				State = state;
			}

			public DateTime DateTime { get; }
			public int GuardId { get; }
			public States State { get; }

			public override string ToString() => $"{GuardId:D5} : {DateTime:O} : {State:F}";
		}
	}
}
