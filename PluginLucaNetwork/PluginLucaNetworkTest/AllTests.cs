using NUnit.Framework;
using PerseusApi.Generic;
using PerseusApi.Network;
using PerseusLibS.Data.Network;
using PluginLucaNetwork;
using PluginLucaNetwork.Python;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PluginLucaNetworkTest
{
	[TestFixture]
	public class AllTests : BaseTest
	{
		[Test]
		public void TestRead()
		{
			string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string path = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\NetworkFiles"));
			INetworkData ndata = new NetworkData();
			NetworkIO.Read(ndata, path, new ProcessInfo(new Settings(), status => { }, progress => { }, 1, i => { }));
			var numberOfNetworks = 0;
			foreach (var network in ndata)
			{
				Assert.AreEqual(1, network.NodeTable.StringColumnCount);
				Assert.AreEqual(0, network.NodeTable.NumericColumnCount);
				Assert.AreEqual(0, network.NodeTable.CategoryColumnCount);
				Assert.AreEqual(0, network.NodeTable.MultiNumericColumnCount);
				Assert.AreEqual(2, network.EdgeTable.StringColumnCount);
				Assert.AreEqual(0, network.EdgeTable.NumericColumnCount);
				Assert.AreEqual(0, network.EdgeTable.CategoryColumnCount);
				Assert.AreEqual(0, network.EdgeTable.MultiNumericColumnCount);
				// generic
				Assert.AreEqual(network.Graph.NumberOfEdges, network.EdgeTable.RowCount);
				Assert.AreEqual(network.Graph.NumberOfNodes, network.NodeTable.RowCount);
				Assert.AreEqual("DataWithAnnotationColumns", network.NodeTable.GetType().Name, network.EdgeTable.GetType().Name);
				numberOfNetworks++;
			}
			Assert.AreEqual(numberOfNetworks, ndata.RowCount);
		}

		[Test]
		public void TestReadAndUndirected()
		{
			string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string path = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\NetworkFiles"));
			INetworkData ndata = new NetworkData();
			NetworkIO.Read(ndata, path, new ProcessInfo(new Settings(), status => { }, progress => { }, 1, i => { }));
			CalcProperties calcs = new CalcProperties();
			List<Boolean> observedWhat = new List<Boolean> { };
			foreach (var network in ndata)
			{
				observedWhat.Add(calcs.CheckNetworkUndirected(network));
			}
			Assert.False(observedWhat.ElementAt(0));
			Assert.True(observedWhat.ElementAt(1));
			Assert.True(observedWhat.ElementAt(2));
			Assert.True(observedWhat.ElementAt(3));
			Assert.False(observedWhat.ElementAt(4));
			Assert.False(observedWhat.ElementAt(5));
		}

		[Test]
		public void TestReadAndConnected()
		{
			string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string path = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\NetworkFiles"));
			INetworkData ndata = new NetworkData();
			NetworkIO.Read(ndata, path, new ProcessInfo(new Settings(), status => { }, progress => { }, 1, i => { }));
			CalcProperties calcs = new CalcProperties();
			List<Boolean> observedWhat = new List<Boolean> { };
			foreach (var network in ndata)
			{
				observedWhat.Add(calcs.CheckNetworkConnected(network));
			}
			Assert.False(observedWhat.ElementAt(0));
			Assert.True(observedWhat.ElementAt(1));
			Assert.True(observedWhat.ElementAt(2));
			Assert.False(observedWhat.ElementAt(3));
			Assert.True(observedWhat.ElementAt(4));
			Assert.False(observedWhat.ElementAt(5));
		}

		[Test]
		public void TestWrite()
		{
			string path = "C:\\Users\\ldeininger\\Documents\\CellCycle6Matrices";
			INetworkData ndata = new NetworkData();
			NetworkIO.Read(ndata, path, new ProcessInfo(new Settings(), status => { }, progress => { }, 1, i => { }));
			NetworkIO.Write(ndata, "C:\\Users\\ldeininger\\Documents\\CellCycle6Matrices\\test");
		}
	}
}