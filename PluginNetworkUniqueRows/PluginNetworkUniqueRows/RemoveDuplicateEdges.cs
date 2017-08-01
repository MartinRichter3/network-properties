using System;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Network;
using BaseLibS.Num;
using System.Linq;

namespace PluginNetworkUniqueRows
{
    public class RemoveDuplicateEdges : INetworkProcessing
    {
        public bool HasButton => false;
        public Bitmap2 DisplayImage => null;
        public string Description => "Collapse duplicate rows (for edges and nodes)";
        public string Name => "Collapse duplicate rows";
        public string Heading => "Basic";
        public bool IsActive => true;
        public float DisplayRank => 16;
        public string[] HelpSupplTables => new string[0];
        public int NumSupplTables => 0;
        public string HelpOutput => "";
        public string[] HelpDocuments => new string[0];
        public int NumDocuments => 0;
        public string Url => ""; // change

        public int GetMaxThreads(Parameters parameters)
        {
            return 1;
        }

        static string _id1 = "Edge Source column";
        static string _id2 = "Edge Target column";
        static string _id3 = "Node Name column";
        static string _string = "Combine text columns by";
        static string _numeric = "Combine numeric columns by";
        static string _multi_numeric = "Combine multi numeric columns by";
        static string _category = "Combine category/text columns by";

        static string[] _numeric_choices = { "median" };
        static string[] _multi_numeric_choices = { "concatenation" };
        static string[] _category_choices = { "union" };
        static string[] _string_choices = { _str_union_split };

        const string _str_union_split = "Union, split on ';'";

        public void ProcessData(INetworkData ndata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo)
        {
            foreach (var network in ndata)
            {
                var id1 = network.EdgeTable.StringColumns[param.GetParam<int>(_id1).Value];
                var id2 = network.EdgeTable.StringColumns[param.GetParam<int>(_id2).Value];
                var id3 = network.NodeTable.StringColumns[param.GetParam<int>(_id3).Value];
                Func<double[], double> combineNumeric;
                Func<string[], string> combineString;
                Func<string[][], string[]> combineCategory;
                Func<double[][], double[]> combineMultiNumeric;
                ParseParameters(param, out combineNumeric, out combineString, out combineCategory, out combineMultiNumeric);//set combine settings
                network.UniqueRows(id1, id2, id3, combineNumeric, combineString, combineCategory, combineMultiNumeric);// overwrite mdata with new unique rows
            }
        }

        private void ParseParameters(Parameters param, out Func<double[], double> combineNumeric, out Func<string[], string> combineString,
            out Func<string[][], string[]> combineCategory, out Func<double[][], double[]> combineMultiNumeric)
        {
            var combineNumericParm = _numeric_choices[param.GetParam<int>(_numeric).Value];//get choice
            switch (combineNumericParm)
            {
                case "median":
                    combineNumeric = ArrayUtils.Median;
                    break;
                default:
                    throw new NotImplementedException($"Method {combineNumericParm} is not implemented");
            }
            var combineStringParam = _string_choices[param.GetParam<int>(_string).Value];// get choice
            switch (combineStringParam)
            {
                case _str_union_split:
                    combineString = Union;
                    break;
                default:
                    throw new NotImplementedException($"Method {combineStringParam} is not implemented");
            }
            var combineCategoryParam = _category_choices[param.GetParam<int>(_category).Value];
            switch (combineCategoryParam)
            {
                case "union":
                    combineCategory = CatUnion;
                    break;
                default:
                    throw new NotImplementedException($"Method {combineCategoryParam} is not implemented");
            }
            var combineMultiNumericParam = _multi_numeric_choices[param.GetParam<int>(_multi_numeric).Value];
            switch (combineMultiNumericParam)
            {
                case "concatenation":
                    combineMultiNumeric = MultiNumUnion;
                    break;
                default:
                    throw new NotImplementedException($"Method {combineMultiNumericParam} is not implemented");
            }
        }

        public static double[] MultiNumUnion(params double[][] nums)
        {
            return nums.SelectMany(x => x).ToArray();
        }

        public static string[] CatUnion(params string[][] strs)
        {
            return strs.SelectMany(x => x).Distinct().ToArray();
        }

        public static string Union(params string[] strs)
        {
            return string.Join(";", strs.SelectMany(x => x.Split(';')).Distinct());
        }

        public Parameters GetParameters(INetworkData ndata, ref string errorString)
        {
            return new Parameters(
                new SingleChoiceParam(_id1)
                {
                    Values = ndata.First().EdgeTable.StringColumnNames,
                    Value = ndata.First().EdgeTable.StringColumnNames.Contains("Source") ? ndata.First().EdgeTable.StringColumnNames.FindIndex(x => x == "Source") : 0
                },
                new SingleChoiceParam(_id2)
                {
                    Values = ndata.First().EdgeTable.StringColumnNames,
                    Value = ndata.First().EdgeTable.StringColumnNames.Contains("Target") ? ndata.First().EdgeTable.StringColumnNames.FindIndex(x => x == "Target") : 0
                },
                new SingleChoiceParam(_id3)
                {
                    Values = ndata.First().NodeTable.StringColumnNames,
                    Value = ndata.First().NodeTable.StringColumnNames.Contains("Node") ? ndata.First().NodeTable.StringColumnNames.FindIndex(x => x == "Node") : 0
                },
                new SingleChoiceParam(_string)
                {
                    Values = _string_choices
                },
                new SingleChoiceParam(_numeric)
                {
                    Values = _numeric_choices
                },
                new SingleChoiceParam(_category)
                {
                    Values = _category_choices
                },
                new SingleChoiceParam(_multi_numeric)
                {
                    Values = _multi_numeric_choices
                }
            );
        }
    }

}