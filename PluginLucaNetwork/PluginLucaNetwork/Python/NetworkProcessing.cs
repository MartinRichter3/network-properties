using System;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Network;

namespace PluginLucaNetwork.Python
{
    public class NetworkProcessing : PluginLucaNetwork.NetworkProcessing
    {
        public override string Name => "=> Python =>";
        public override string Description => "Run Python Script with Network as input";
        protected override string CodeFilter => "Python script, *.py | *.py";
        public override Bitmap2 DisplayImage => null;

        protected override Parameters AddParameters(INetworkData ndata)
        {
            return new Parameters();
        }
    }
}