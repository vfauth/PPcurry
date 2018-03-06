using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPcurry
{
    [Serializable()]
    class SavedCircuit
    {
        public List<Component> Components;
        public List<Wire> Wires;
        public List<List<Node>> Nodes;

        public SavedCircuit(BoardGrid boardGrid)
        {
            // Save what has to be saved
            Components = boardGrid.ComponentsOnBoard;
            Wires = boardGrid.WiresOnBoard;
            Nodes = boardGrid.Nodes;
        }
    }
}
