using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPcurry;
using System.Diagnostics;

namespace PPcurry
{
    public class GraphNode
    {
        #region Attributes
        public List<Node> GridNodes { get; } // The Nodes constituting this GraphNode
        public List<GraphLink> ConnectedLinks { get; } = new List<GraphLink>(); // The links connected to this node
        public List<GraphNode> LinkedNodes { get; } = new List<GraphNode>(); // The nodes connected to this node by links
        public int Id { get; set; } // The ID of the node
        #endregion


        #region Constructor
        public GraphNode(Node node)
        {
            GridNodes = new List<Node> { node };
        }
        #endregion


        #region Methods
        /// <summary>
        /// Add a link to the node
        /// </summary>
        public void AddLink(GraphLink link)
        {
            // Add the link
            if (!ConnectedLinks.Contains(link))
            {
                ConnectedLinks.Add(link);
            }
            // Add the linked node
            if (link.ConnectedNodes.Item1 != this)
            {
                if (!LinkedNodes.Contains(link.ConnectedNodes.Item1))
                {
                    LinkedNodes.Add(link.ConnectedNodes.Item1);
                }
            }
            else
            {
                if (!LinkedNodes.Contains(link.ConnectedNodes.Item2))
                {
                    LinkedNodes.Add(link.ConnectedNodes.Item2);
                }
            }
        }
        #endregion
    }
}
