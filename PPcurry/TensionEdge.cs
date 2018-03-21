using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPcurry;

namespace PPcurry
{
    class TensionEdge
    {
        #region Attributes
        private Component EdgeComponent { get; set; } //The value of the variable is null if the node is the Edge represents a Wire
        private Tuple<TensionNode,TensionNode> ConnectedNodes ;
        #endregion

        #region Constructor
        public TensionEdge(Component edgeComponent, Tuple<TensionNode, TensionNode> connectedNodes)
        {
            EdgeComponent = edgeComponent;
            ConnectedNodes = connectedNodes;
        }
        #endregion
    }
}
