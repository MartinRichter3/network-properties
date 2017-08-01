using System;
using System.Collections.Generic;
using System.Linq;
using BaseLibS.Num;
using PerseusApi.Generic;
using PerseusApi.Network;

namespace PluginNetworkUniqueRows
{
    public static class NetworkDataExtensions
    {

        public static void UniqueRows(this INetworkInfo network, string[] id1, string[] id2, string[] id3, Func<double[], double> combineNumeric, Func<string[], string> combineString, Func<string[][], string[]> combineCategory,
Func<double[][], double[]> combineMultiNumeric)
        {
            network.UniqueEdges(id1, id2, combineNumeric, combineString, combineCategory, combineMultiNumeric);
            network.UniqueNodes(id3, combineNumeric, combineString, combineCategory, combineMultiNumeric);
        }

        public static void UniqueEdges(this INetworkInfo network, string[] id1, string[] id2, Func<double[], double> combineNumeric, Func<string[], string> combineString, Func<string[][], string[]> combineCategory,
    Func<double[][], double[]> combineMultiNumeric)
        {
            var data = network.EdgeTable;
            List<Triple> list = new List<Triple>();
            for (int i = 0; i < id1.Length; i++)
            {
                list.Add(new Triple() { One = id1[i], Two = id2[i], Index = i });
            }
            list.Sort();
            var uniqueIdx = new List<int>();
            var lastId1 = "";
            var lastId2 = "";
            var idxsWithSameId = new List<int>();
            foreach (Triple id in list)
            {
                if (id.One == lastId1 && id.Two == lastId2)
                {
                    idxsWithSameId.Add(id.Index);
                }
                else
                {
                    CombineRows(data, idxsWithSameId, combineNumeric, combineString, combineCategory, combineMultiNumeric);
                    network.RemoveEdge(idxsWithSameId);
                    uniqueIdx.Add(id.Index);
                    idxsWithSameId.Clear();
                    idxsWithSameId.Add(id.Index);
                }
                lastId1 = id.One;
                lastId2 = id.Two;
            }
            CombineRows(data, idxsWithSameId, combineNumeric, combineString, combineCategory, combineMultiNumeric);
            data.ExtractRows(uniqueIdx.ToArray());
        }

        public static void UniqueNodes(this INetworkInfo network, string[] ids, Func<double[], double> combineNumeric, Func<string[], string> combineString, Func<string[][], string[]> combineCategory,
Func<double[][], double[]> combineMultiNumeric)
        {
            var data = network.NodeTable;
            var order = ArrayUtils.Order(ids);
            var uniqueIdx = new List<int>();
            var lastId = "";
            var idxsWithSameId = new List<int>();
            foreach (int j in order)
            {
                var id = ids[j];
                if (id == lastId)
                {
                    idxsWithSameId.Add(j);
                }
                else
                {
                    CombineRows(data, idxsWithSameId, combineNumeric, combineString, combineCategory, combineMultiNumeric);
                    network.RemoveNode(idxsWithSameId);
                    uniqueIdx.Add(j);
                    idxsWithSameId.Clear();
                    idxsWithSameId.Add(j);
                }
                lastId = id;
            }
            CombineRows(data, idxsWithSameId, combineNumeric, combineString, combineCategory, combineMultiNumeric);
            data.ExtractRows(uniqueIdx.ToArray());

        }

        public static void RemoveEdge(this INetworkInfo network, List<int> rowIdxs)
        {
            if (rowIdxs.Count <= 1)
            {
                return;
            }
            for (int i = 0; i < rowIdxs.Count - 1; i++)
            {
                var edge = network.EdgeIndex.FirstOrDefault(x => x.Value == rowIdxs[i]).Key;
                network.Graph.RemoveEdges(edge);
            }
        }

        public static void RemoveNode(this INetworkInfo network, List<int> rowIdxs)
        {
            if (rowIdxs.Count <= 1)
            {
                return;
            }
            for (int i = 0; i < rowIdxs.Count - 1; i++)
            {
                var node = network.NodeIndex.FirstOrDefault(x => x.Value == rowIdxs[i]).Key;
                network.Graph.RemoveNodes(node);
            }
        }

        public static void CombineRows(this IDataWithAnnotationColumns mdata, List<int> rowIdxs, Func<double[], double> combineNumeric, Func<string[], string> combineString, Func<string[][], string[]> combineCategory, Func<double[][], double[]> combineMultiNumeric)
        {
            if (!rowIdxs.Any())
            {
                return;
            }
            var resultRow = rowIdxs[0];
            for (int i = 0; i < mdata.NumericColumnCount; i++)
            {
                var column = mdata.NumericColumns[i];
                var values = ArrayUtils.SubArray(column, rowIdxs);
                column[resultRow] = combineNumeric(values);
            }
            for (int i = 0; i < mdata.StringColumnCount; i++)
            {
                var column = mdata.StringColumns[i];
                var values = ArrayUtils.SubArray(column, rowIdxs);
                column[resultRow] = combineString(values);
            }
            for (int i = 0; i < mdata.CategoryColumnCount; i++)
            {
                var column = mdata.GetCategoryColumnAt(i);
                var values = ArrayUtils.SubArray(column, rowIdxs);
                column[resultRow] = combineCategory(values);
                mdata.SetCategoryColumnAt(column, i);
            }
            for (int i = 0; i < mdata.MultiNumericColumnCount; i++)
            {
                var column = mdata.MultiNumericColumns[i];
                var values = ArrayUtils.SubArray(column, rowIdxs);
                column[resultRow] = combineMultiNumeric(values);
            }
        }
    }
}