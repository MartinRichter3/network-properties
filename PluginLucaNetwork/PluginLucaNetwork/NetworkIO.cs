using System.IO;
using PerseusApi.Network;
using PerseusApi.Generic;
using PerseusApi.Utils;
using System.Collections.Generic;
using System;
using System.Linq;
using PerseusLibS.Data.Network;
using PerseusLibS.Data;

namespace PluginLucaNetwork
{
    public static class NetworkIO
    {
        public static void Read(INetworkData ndata, string outFolder, ProcessInfo processInfo)
        {
            ReadMatrixDataInto(ndata, Path.Combine(outFolder, "networks.txt"), processInfo);
            foreach (var netAttr in ndata.GetStringColumn("guid").Zip(ndata.GetStringColumn("name"), (guid, name) => new { guid, name }))
            {
                var guid = Guid.Parse(netAttr.guid);
                var nodeTable = PerseusFactory.CreateDataWithAnnotationColumns();
                var edgeTable = PerseusFactory.CreateDataWithAnnotationColumns();
                ReadMatrixDataInto(nodeTable, Path.Combine(outFolder, $"{guid}_nodes.txt"), processInfo);
                ReadMatrixDataInto(edgeTable, Path.Combine(outFolder, $"{guid}_edges.txt"), processInfo);
                var graph = new Graph();
                var nodeIndex = new Dictionary<INode, int>();
                var nameToNode = new Dictionary<string, INode>();
                var nodeColumn = nodeTable.GetStringColumn("node");
                
                for (int row = 0; row < nodeTable.RowCount; row++)
                {
                    var node = graph.AddNode();
                    nodeIndex[node] = row;
                    nameToNode[nodeColumn[row]] = node;
                }
                var sourceColumn = edgeTable.GetStringColumn("source");
                var targetColumn = edgeTable.GetStringColumn("target");
                var edgeIndex = new Dictionary<IEdge, int>();
                for (int row = 0; row < edgeTable.RowCount; row++)
                {
                    var source = nameToNode[sourceColumn[row]];
                    var target = nameToNode[targetColumn[row]];
                    var edge = graph.AddEdge(source, target);
                    edgeIndex[edge] = row;
                }
                ndata.AddNetworks(new NetworkInfo(graph, nodeTable, nodeIndex, edgeTable, edgeIndex, netAttr.name, guid));
            }
            ReadMatrixDataInto(ndata, Path.Combine(outFolder, "networks.txt"), processInfo); 
        }

        private static void ReadMatrixDataInto(IDataWithAnnotationColumns data, string file, ProcessInfo processInfo)
        {
            var mdata = PerseusFactory.CreateMatrixData();
            PerseusUtils.ReadMatrixFromFile(mdata, processInfo, file, '\t');
            data.CopyAnnotationColumnsFrom(mdata);
        }

        public static void Write(INetworkData ndata, string inFolder)
        {
            if (!Directory.Exists(inFolder))
            {
                Directory.CreateDirectory(inFolder);
            }
            PerseusUtils.WriteDataWithAnnotationColumns(ndata, Path.Combine(inFolder, "networks.txt"));
            foreach (var network in ndata)
            {
                PerseusUtils.WriteDataWithAnnotationColumns(network.NodeTable, Path.Combine(inFolder, $"{network.Guid}_nodes.txt"));
                PerseusUtils.WriteDataWithAnnotationColumns(network.EdgeTable, Path.Combine(inFolder, $"{network.Guid}_edges.txt"));
            }
        }
    }
}