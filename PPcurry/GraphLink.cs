using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPcurry;
using System.Diagnostics;

namespace PPcurry
{
    class GraphLink
    {
        #region Attributes
        public Component LinkComponent { get; set; } // The component constituting the link
        public Tuple<GraphNode, GraphNode> ConnectedNodes { get; } // The nodes linked by this link
        #endregion


        #region Constructor
        public GraphLink(GraphNode node1, GraphNode node2, Component component)
        {
            LinkComponent = component;
            ConnectedNodes = new Tuple<GraphNode, GraphNode>(node1, node2);
        }
        #endregion
    }
}
