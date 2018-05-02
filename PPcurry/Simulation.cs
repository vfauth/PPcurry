using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPcurry;

namespace PPcurry
{
    class Simulation
    {
        #region Attributes
        private BoardGrid BoardGrid { get; set; } //the BoardGrid associated to the graph
        private List<Graph> SimulationGraphs { get; set; } // The simulated Graphs linked to the circuit


        #endregion

        #region Constructor
        public Simulation(BoardGrid boardGrid)
        {
            BoardGrid = boardGrid;
        }
        #endregion

        #region Methods
        public void GenerateGraph()
        {
            List<Node> nodesToGoThrough = new List<Node> { };
            List<Node> nodesAlreadyGoneThrough = new List < Node > { };
            List<List<Node>> nodes = this.BoardGrid.Nodes;
            int i = nodes.Count;
            int j = nodes[0].Count;

            while (!(nodesToGoThrough.Any()) && i != 0 && j != 0)
            {
                if (!(nodes[i][j].IsIsolated()))
                {
                    nodesToGoThrough.Add(nodes[i][j]); //Get the first node to analyse
                }
            }

            while (nodesToGoThrough.Any())
            {
                Node currentNode = nodesToGoThrough[0];
                nodesAlreadyGoneThrough.Add(currentNode);
                nodesToGoThrough.RemoveAt(0);
                List<object> components = currentNode.ConnectedElements.Keys.ToList(); //components connected to current node

                foreach (Component component in components)
                {
                    if (!(nodesAlreadyGoneThrough.Any(c => c == component.ConnectedNodes[0]))) //check if node has already been gone through
                    {
                        nodesToGoThrough.Add(component.ConnectedNodes[0]);
                    }

                    if (!(nodesAlreadyGoneThrough.Any(c => c == component.ConnectedNodes[1]))) //check if node has already been gone through
                    {
                        nodesToGoThrough.Add(component.ConnectedNodes[1]);
                    }


                }
            }

        }

        #endregion
    }
}
