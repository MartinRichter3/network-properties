using System;

namespace PluginNetworkUniqueRows
{
    class Triple : IComparable<Triple>
    {
        public string One { get; set; }
        public string Two { get; set; }
        public int Index { get; set; }

        public int CompareTo(Triple other)
        {
            if (this.One.Equals(other.One))
            {
                return this.Two.CompareTo(other.Two);
            }
			return this.One.CompareTo(other.One);
        }
    }
}