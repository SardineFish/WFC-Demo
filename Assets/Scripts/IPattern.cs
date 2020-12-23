using System.Collections.Generic;

namespace WFC
{
    public interface IPattern<TAdjacent> where TAdjacent : IPattern<TAdjacent>
    {
        float Weight { get; }
        IEnumerable<TAdjacent> GetAdjacent(int i);
    }
}