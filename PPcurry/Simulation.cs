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
            List<List<Node>> Nodes = BoardGrid.Nodes;
            SimulationGraphs.Clear();
            Graph Graph = new Graph();
            int N = Nodes.Count();
            int M = Nodes[0].Count();

            for (int i= 0;i<N; i++) {

                for (int j=0;j<M;j++)
                {
                    if (!Nodes[i][j].IsIsolated()) //add the node to the graph if it is not isolated
                    {
                        Tuple<int, int> id = new Tuple<int, int>(i, j);
                        Graph.AddNode(new TensionNode(id));
                    }
                   
                }
            }

            for (int i = 0; i < N; i++)
            {

                for (int j = 0; j < M; j++)
                {
                    
                    if (Nodes[i][j].ConnectedElements.ContainsValue(Directions.Up)){
                        TensionNode originNode = Graph.GetNodeFromId(new Tuple<int, int>(i, j));
                        TensionNode destinationNode = Graph.GetNodeFromId(new Tuple<int, int>(i - 1, j));
                        Object component = Nodes[i][j].ConnectedElements.FirstOrDefault(x => x.Value == Directions.Up).Key;

                        if (component.GetType().Equals(typeof(Wire)))
                        {
                            TensionEdge edge = new TensionEdge(null, new Tuple<TensionNode, TensionNode>(originNode, destinationNode));
                            Graph.AddEdge(originNode, destinationNode, edge);
                        }
                        else
                        {
                            TensionEdge edge = new TensionEdge((Component) component, new Tuple<TensionNode, TensionNode>(originNode, destinationNode));
                            Graph.AddEdge(originNode, destinationNode, edge);
                        }

                    }

                }
            }

        }

        #endregion
    }
}
