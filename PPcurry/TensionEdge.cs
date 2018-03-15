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
        private TensionNode[] ConnectedNodes = new TensionNode[2];  
        #endregion

        #region Constructor
        public TensionEdge(Component edgeComponent)
        {
            EdgeComponent = edgeComponent;
        }
        #endregion
    }
}
