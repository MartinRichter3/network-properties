using BaseLibS.Graph;
using BaseLibS.Param;
using BaseLibS.Num;
using PerseusApi.Network;
using System;
using System.Collections.Generic;
using PerseusLibS.Data;
using System.Linq;

namespace PluginLucaNetwork.Python
{
    public class CalcProperties : PluginLucaNetwork.NetworkProcessing
    {
        public override string Name => "Calculate network properties";
        public override string Description => "Calculate network properties via python script";
        protected override string CodeFilter => "Python script, *.py | *.py";
        public override Bitmap2 DisplayImage => null;
        protected override Parameters AddParameters(INetworkData ndata)
        {
            Boolean undirected = CheckDataUndirected(ndata);
            Boolean connected = CheckDataConnected(ndata);
            var valuesList = new List<String>() { "Network \t Density", "Network \t Average node connectivity", "Network \t Node connectivity", "Network \t Edge connectivity", "Network \t Diameter", "Network \t Radius", "Node \t Degree", "Node \t Out-degree", "Node \t Degree centrality", "Node \t In-degree centrality", "Node \t Out-degree centrality", "Node \t Closeness centrality", "Node \t Betweenness centrality", "Node \t Average neighbor degree", "Node \t Clustering coefficient", "Edge \t Betweenness centrality" };
            string[] values = UpdateParameterPropertyValues(valuesList, undirected, connected);
            string[] directedOrNot = { undirected ? "Undirected" : "Directed" };
            Parameters parameters = new Parameters();
            parameters.AddParameterGroup(new Parameter[] {new MultiChoiceParam("Properties", new int[0] { })
            {
                Values = values,
                Help = "Please select here the properties that should be calculated."
            } }, "", false);
            parameters.AddParameterGroup(new Parameter[] { new SingleChoiceParam ("Graph type")
            {
                Values = directedOrNot,
                Visible= false,
            } }, "", false);
            return parameters;
        }

        private String[] UpdateParameterPropertyValues(List<string> valuesList, Boolean undirected, Boolean connected)
        {
			if (undirected)
			{
				valuesList.Remove(valuesList.Single(x => x.Equals("Node \t Out-degree")));
				valuesList.Remove(valuesList.Single(x => x.Equals("Node \t In-degree centrality")));
				valuesList.Remove(valuesList.Single(x => x.Equals("Node \t Out-degree centrality")));
			}
            if (!connected)
            {
                valuesList.Remove(valuesList.Single(x => x.Equals("Network \t Diameter")));
                valuesList.Remove(valuesList.Single(x => x.Equals("Network \t Radius")));
                valuesList.Remove(valuesList.Single(x => x.Equals("Node \t Clustering coefficient")));
            }
            else
            {
				if (!undirected)
				{
					valuesList.Remove(valuesList.Single(x => x.Equals("Node \t Clustering coefficient")));
				}
            }
            return valuesList.ToArray();
        }

        public Boolean CheckDataUndirected(INetworkData ndata)
        {
            foreach (var network in ndata)
            {
                if (!CheckNetworkUndirected(network))
                {
                    return false;
                }
            }
            return true;
        }

        public Boolean CheckNetworkUndirected(INetworkInfo network)
        {
            var sourceColumn = network.EdgeTable.GetStringColumn("source");
            var targetColumn = network.EdgeTable.GetStringColumn("target");
            List<int> noDoubleCheck = new List<int> { };
            Boolean found = false;
            for (int row = 0; row < network.EdgeTable.RowCount; row++)
            {
                found = false;
                if (noDoubleCheck.Exists(element => element == row)) { }
                else
                {
                    for (int row2 = 0; row2 < network.EdgeTable.RowCount; row2++)
                    {
                        if (sourceColumn[row] == targetColumn[row2] && targetColumn[row] == sourceColumn[row2])
                        {
                            noDoubleCheck.Add(row2);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Boolean CheckDataConnected(INetworkData ndata)
        {
            foreach (var network in ndata)
            {
                if (!CheckNetworkConnected(network))
                {
                    return false;
                }
            }
            return true;
        }

        public Boolean CheckNetworkConnected(INetworkInfo network)
        {
            IGraph g = network.Graph;
            var nodeEnum = g.GetEnumerator();
            //for every node: Do weak connectivity BFS -> in whole: strong connectivity BFS
            while (nodeEnum.MoveNext())
            {
                Dictionary<INode, Boolean> queue = new Dictionary<INode, Boolean>
                {
                    [nodeEnum.Current] = false
                };
                queue = DoBFS(queue, network);
                if (queue.Count != g.NumberOfNodes)
                {
                    return false;
                }
            }
            return true;
        }

        private Dictionary<INode, Boolean> DoBFS(Dictionary<INode, Boolean> queue, INetworkInfo network)
        {
            var unvis = queue.FirstOrDefault(x => x.Value == false).Key;
            while (unvis != null)
            {
                queue[unvis] = true;//update current node as visited
                queue = PushNeighborsToQueue(queue, unvis, network);// push neighbors to queue
                unvis = queue.FirstOrDefault(x => x.Value == false).Key;//search for next unvisited node
            }
            return queue;
        }

        private Dictionary<INode, Boolean> PushNeighborsToQueue(Dictionary<INode, Boolean> queue, INode node, INetworkInfo netw)
        {
            List<INode> neighbors = GetNeighbors(node);
            foreach (var neighbor in neighbors)
            {
                if (!queue.ContainsKey(neighbor))
                {
                    queue[neighbor] = false;
                }
            }
            return queue;
        }

        private List<INode> GetNeighbors(INode node)
        {
            List<IEdge> edges = node.OutEdges;
            List<INode> neighbors = new List<INode>();
            foreach (var x in edges)
            {
                neighbors.Add(x.Target);
            }
            return neighbors;
        }
    }
}