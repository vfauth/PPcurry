using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PPcurry
{
    class Graph
    {
        #region Attributes
        public BoardGrid BoardGrid { get; } // The BoardGrid associated to the graph
        public List<GraphNode> Nodes { get; } = new List<GraphNode>(); // The Nodes of the graph
        public List<GraphLink> Links { get; } = new List<GraphLink>(); // The Links of the graph
        #endregion


        #region Constructor
        public Graph(BoardGrid boardGrid)
        {
            BoardGrid = boardGrid;
            GenerateGraph();
        }
        #endregion


        #region Methods
        /// <summary>
        /// Generate the Graph by parsing the nodes of the boardgrid
        /// </summary>
        private void GenerateGraph()
        {
            Dictionary<Wire, List<GraphNode>> wiresNotAdded = new Dictionary<Wire, List<GraphNode>>(); // List the wires that have not been processed yet and the nodes to which they are connected
            Dictionary<Component, List<GraphNode>> componentsNotAdded = new Dictionary<Component, List<GraphNode>>(); // List the components that have not been processed yet and the nodes to which they are connected

            // Parse all grid nodes and create corresponding graph nodes
            for (int i = 0; i < BoardGrid.Nodes.Count; i++)
            {
                for (int j = 0; j < BoardGrid.Nodes[i].Count; j++)
                {
                    Node gridNode = BoardGrid.Nodes[i][j];
                    if (!gridNode.IsIsolated()) // Isolated nodes are discarded
                    {
                        GraphNode node = new GraphNode(gridNode);
                        Nodes.Add(node); // Create the GraphNode
                        foreach (object gridLink in gridNode.ConnectedElements.Keys) // Save all connected wires and components
                        {
                            if (gridLink is Wire wire)
                            {
                                if (wiresNotAdded.ContainsKey(wire)) wiresNotAdded[wire].Add(node);
                                else wiresNotAdded[wire] = new List<GraphNode> { node };
                            }
                            else if (gridLink is Component component)
                            {
                                if (componentsNotAdded.ContainsKey(component)) componentsNotAdded[component].Add(node);
                                else componentsNotAdded[component] = new List<GraphNode> { node };
                            }
                        }
                    }
                }
            }

            Dictionary<GraphNode, GraphNode> mergedNodes = new Dictionary<GraphNode, GraphNode>(); // List all the nodes (keys) merged into another node (values)
            // Parse all wires and merge the connected nodes
            foreach (Wire wire in wiresNotAdded.Keys)
            {
                // Ensure that the nodes to merge have not already been merged into another node and discarded
                while (mergedNodes.ContainsKey(wiresNotAdded[wire][0]))
                {
                    wiresNotAdded[wire][0] = mergedNodes[wiresNotAdded[wire][0]];
                }
                while (mergedNodes.ContainsKey(wiresNotAdded[wire][1]))
                {
                    wiresNotAdded[wire][1] = mergedNodes[wiresNotAdded[wire][1]];
                }
                if (wiresNotAdded[wire][0] != wiresNotAdded[wire][1]) // A node can't be merged with itself
                {
                    MergeNodes(wiresNotAdded[wire][0], wiresNotAdded[wire][1]); // Merge the nodes
                    mergedNodes[wiresNotAdded[wire][1]] = wiresNotAdded[wire][0]; // Save the operation
                }
            }

            // Create all links
            foreach (Component component in componentsNotAdded.Keys)
            {
                // Ensure that the nodes to use have not been merged into another node and discarded
                while (mergedNodes.ContainsKey(componentsNotAdded[component][0]))
                {
                    componentsNotAdded[component][0] = mergedNodes[componentsNotAdded[component][0]];
                }
                while (mergedNodes.ContainsKey(componentsNotAdded[component][1]))
                {
                    componentsNotAdded[component][1] = mergedNodes[componentsNotAdded[component][1]];
                }

                GraphLink link = new GraphLink(componentsNotAdded[component][0], componentsNotAdded[component][1], component); // Create the link
                componentsNotAdded[component][0].AddLink(link); // Add the link to both nodes
                componentsNotAdded[component][1].AddLink(link);
                Links.Add(link);
            }
        }

        /// <summary>
        /// Merge two nodes linked only by a wire
        /// </summary>
        private void MergeNodes(GraphNode node1, GraphNode node2)
        {
            if (node1 == node2)
            {
                return; 
            }
            node1.GridNodes.AddRange(node2.GridNodes); // Add the grid nodes from the second node to the first
            foreach (GraphLink link in node2.ConnectedLinks) // Add the links from the second node to the first
            {
                node1.AddLink(link);
            }
            node1.LinkedNodes.Remove(node2);
            Nodes.Remove(node2);
        }
        #endregion
    }
}
