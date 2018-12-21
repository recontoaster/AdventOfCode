using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace AdventOfCode.Tests
{
    public class Day08
    {
        [Theory]
        [InlineData("2 3 0 3 10 11 12 1 1 0 1 99 2 1 1 2", 138)]
        public void Day08_Parse(string input, int expected)
        {
            var numbers = input.Split(' ').Select(byte.Parse);
            var numbersQueue = new Queue<byte>(numbers);

            var node = BuildNode(ref numbersQueue);

            Assert.Equal(expected, AddMetadata(node));
        }

        [Theory]
        [InlineData(36_307)]
        public void Day08_Part01(int expected)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Data", "node_numbers.dat");
            var lines = path.GetLinesFromFile();

            var numbers = lines.First().Split(' ').Select(byte.Parse);
            var numbersQueue = new Queue<byte>(numbers);

            var node = BuildNode(ref numbersQueue);

            Assert.Equal(expected, AddMetadata(node));
        }

        private static INode BuildNode(ref Queue<byte> numbersQueue)
        {
            var header = new Header { ChildNodeCount = numbersQueue.Dequeue(), MetadataEntriesCount = numbersQueue.Dequeue(), };
            var node = new Node { Header = header, };

            for (var a = 0; a < node.Header.ChildNodeCount; a++)
            {
                var childNode = BuildNode(ref numbersQueue);
                node.ChildNodes.Add(childNode);
            }

            for (var a = 0; a < node.Header.MetadataEntriesCount; a++)
            {
                node.Metadata.Add(numbersQueue.Dequeue());
            }

            return node;
        }

        private static int AddMetadata(INode node)
        {
            var count = 0;

            foreach (var childNode in node.ChildNodes)
            {
                count += AddMetadata(childNode);
            }

            foreach (var metadata in node.Metadata)
            {
                count += metadata;
            }

            return count;
        }
    }

    public interface INode
    {
        ICollection<INode> ChildNodes { get; }
        IHeader Header { get; }
        ICollection<byte> Metadata { get; }
    }

    public class Node : INode
    {
        public ICollection<INode> ChildNodes { get; } = new List<INode>();
        public IHeader Header { get; set; }
        public ICollection<byte> Metadata { get; } = new List<byte>();
    }

    public interface IHeader
    {
        byte ChildNodeCount { get; }
        byte MetadataEntriesCount { get; }
    }

    public class Header : IHeader
    {
        public byte ChildNodeCount { get; set; }
        public byte MetadataEntriesCount { get; set; }
    }
}
