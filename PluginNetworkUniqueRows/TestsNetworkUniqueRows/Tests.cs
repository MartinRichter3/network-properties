using NUnit.Framework;
using PerseusApi.Utils;
using PerseusApi.Generic;
using BaseLibS.Num;
using PluginNetworkUniqueRows;
using PerseusApi.Network;
using PerseusLibS.Data.Network;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TestsNetworkUniqueRows
{
    [TestFixture]
    public class Tests : BaseTest
    {
        [Test]
        public void TestNetworkUniqueRows()
        {
            Random RandGen = new Random();
            INetworkData ndata = new NetworkData();
            ndata.Name = "Random network(s)";
            ndata.Description = ndata.Name;
            var n = 3;
            var numNodes = 100;
            var numEdges = 150;
            for (int i = 0; i < n; i++)
            {
                var graph = new Graph();//!!!!
                var nodeTable = PerseusFactory.CreateDataWithAnnotationColumns();
                var nodeIndex = new Dictionary<INode, int>();
                var edgeTable = PerseusFactory.CreateDataWithAnnotationColumns();
                var edgeIndex = new Dictionary<IEdge, int>();
                for (int j = 0; j < numNodes; j++)
                {
                    nodeIndex[graph.AddNode()] = j;
                }
                var nodeNames = Enumerable.Range(0, graph.NumberOfNodes).Select(x => $"node {x}").ToArray();
                nodeTable.AddStringColumn("Node", "", nodeNames);
                var nodes = graph.ToArray();
                var sources = new List<string>();
                var targets = new List<string>();
                for (int j = 0; j < numEdges; j++)
                {
                    var source = nodes[RandGen.Next(0, nodes.Length)];
                    sources.Add(nodeNames[nodeIndex[source]]);
                    var target = nodes[RandGen.Next(0, nodes.Length)];
                    targets.Add(nodeNames[nodeIndex[target]]);
                    edgeIndex[graph.AddEdge(source, target)] = j;
                }
                edgeTable.AddStringColumn("Source", "", sources.ToArray());
                edgeTable.AddStringColumn("Target", "", targets.ToArray());
                var network = new NetworkInfo(graph, nodeTable, nodeIndex, edgeTable, edgeIndex, $"Random {i}");
                ndata.AddNetworks(network);
            }
            foreach (var network in ndata)
            {
                network.UniqueRows(network.EdgeTable.StringColumns[0], network.EdgeTable.StringColumns[1], network.NodeTable.StringColumns[0], ArrayUtils.Median, RemoveDuplicateEdges.Union, RemoveDuplicateEdges.CatUnion, RemoveDuplicateEdges.MultiNumUnion);
                Assert.True(network.EdgeTable.RowCount <= 150);
                var u = network.Graph.NumberOfEdges;
                var v = network.EdgeTable.RowCount;
                var x = network.Graph.NumberOfNodes;
                var y = network.NodeTable.RowCount;
                Assert.True(network.EdgeTable.RowCount == network.Graph.NumberOfEdges);
                Assert.True(network.NodeTable.RowCount == network.Graph.NumberOfNodes);
            }
        }
    }
}
