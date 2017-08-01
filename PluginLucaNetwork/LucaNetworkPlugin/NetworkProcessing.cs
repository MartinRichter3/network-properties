using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using PerseusApi.Network;
using System;

namespace PluginLucaNetwork
{
    public abstract class NetworkProcessing : INetworkProcessing
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        protected abstract Parameters AddParameters(INetworkData ndata);
        public string Heading => "External Processing";
        public bool HasButton => false;
        public string HelpOutput { get; }
        public string[] HelpSupplTables { get; }
        public int NumSupplTables { get; }
        public string[] HelpDocuments { get; }
        public int NumDocuments { get; }
        public string Url { get; }
        public virtual Bitmap2 DisplayImage => null;
        public float DisplayRank => 100;
        public bool IsActive => true;
        public int GetMaxThreads(Parameters parameters) => 1;

        protected virtual string CodeLabel => "Script file";
        protected virtual string InterpreterLabel => "Executable";
        protected virtual string InterpreterFilter => "Interpreter, *.exe|*.exe";
        protected virtual string CodeFilter => "Script";

        protected virtual string GetCodeFile(Parameters param)
        {
            var codeFile = param.GetParam<string>(CodeLabel).Value;
            return codeFile;
        }

        public void ProcessData(INetworkData ndata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents,
            ProcessInfo processInfo)
        {
            var remoteExe = param.GetParam<string>(InterpreterLabel).Value;//get parameters??
            var inFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());//create outfolder
            Directory.CreateDirectory(inFolder);//create outfolder directory
            NetworkIO.Write(ndata, inFolder);//write graphs in folder
            var paramFile = Path.GetTempFileName();//paramfile
            using (var f = new StreamWriter(paramFile))
            {
                param.Convert(ParamUtils.ConvertBack);
                var serializer = new XmlSerializer(param.GetType());
                serializer.Serialize(f, param);
            }
            var outFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());//create outfolder path
            Directory.CreateDirectory(outFolder);//create outfolder directory
            var codeFile = GetCodeFile(param);//get codefile
            var args = $"{codeFile} {paramFile} {inFolder} {outFolder}";
            Debug.WriteLine($"executing > {remoteExe} {args}");//
            var externalProcessInfo = new ProcessStartInfo //processinfo with args={code,para,in,out}
            {
                FileName = remoteExe,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var process = Process.Start(externalProcessInfo))
            {
                var output = process.StandardOutput;
                string line;
                while ((line = output.ReadLine()) != null)
                {
                    Debug.WriteLine($"remote stdout > {line}");
                }
                var error = process.StandardOutput;
                while ((line = error.ReadLine()) != null)
                {
                    Debug.WriteLine($"remote error > {line}");
                }
                process.WaitForExit();
            }
            ndata.Clear();
            NetworkIO.Read(ndata, outFolder, processInfo);//read in matrices from outfolder
        }

        public Parameters GetParameters(INetworkData ndata, ref string errString)
        {
            Parameters parameters = AddParameters(ndata);
            parameters.AddParameterGroup(new Parameter[] { new FileParam(CodeLabel) { Filter = CodeFilter } }, "specific", false);
            parameters.AddParameterGroup(new Parameter[] { new FileParam(InterpreterLabel) { Filter = InterpreterFilter } }, "generic", false);
            return parameters;
        }

    }
}

