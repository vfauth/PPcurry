using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPcurry
{
    class Graph
    {
        #region Attributes
        private List<List<Tuple<TensionNode,TensionEdge>>> Adjacence { get; set; } //The Adjacence lists : Adjacence[i] contains the list of neighbours of node i and the associated edge
        private List<TensionNode> Nodes { get; set; } //The Nodes of the graph

        #endregion

        #region Constructor
        public Graph()
        {

        }

        public Graph(List<List<Tuple<TensionNode, TensionEdge>>> adjacence, List<TensionNode> nodes)
        {
            Adjacence = adjacence;
            Nodes = nodes;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Remove a Node from the graph : remove it from the list of nodes and remove every link to it from the adjacence list
        /// </summary>
        /// <param name="k"></param>
        public void RemoveNode(int k)
        {
            this.Adjacence.RemoveAt(k);
            TensionNode nodeToRemove = this.Nodes.ElementAt(k);
            foreach (List<Tuple<TensionNode, TensionEdge>> sublist in Adjacence)
            {
                foreach (Tuple<TensionNode, TensionEdge> link in sublist)
                {
                    if (link.Item1 == nodeToRemove)
                    {
                        sublist.Remove(link);
                    }
                }
            }
        }

        /// <summary>
        /// Add a node to the graph
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(TensionNode node)
        {
            this.Nodes.Add(node);
        }

        /// <summary>
        /// add an edge to the graph 
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <param name="edge"></param>
        public void AddEdge(TensionNode node1, TensionNode node2, TensionEdge edge)
        {
            int k = this.Nodes.FindIndex(node => node == node1);
            if (k != null && node2 != node1) 
            {
                this.Adjacence[k].Add(new Tuple<TensionNode, TensionEdge>(node2, edge));
            }
            
            AddEdge(node2, node1, edge); //Do the symmetrical operation
        }

        public void RemoveEdge(TensionNode node1, TensionNode node2)
        {
            throw new NotImplementedException();
        }

        public void MergeNodes(TensionNode node1,TensionNode node2)
        {
            if (this.Nodes.FindIndex(node => node == node1) != null & this.Nodes.FindIndex(node => node == node2) != null)
            {
                if (this.Nodes.FindIndex(node => node == node1) > this.Nodes.FindIndex(node => node == node2) && node1 != node2)
                {
                    MergeNodes(node2, node1);
                }

                int k = this.Nodes.FindIndex(node => node == node1);
                int l = this.Nodes.FindIndex(node => node == node2);

                foreach (Tuple<TensionNode, TensionEdge> link in Adjacence[k]) //Remove the links between node1 and node2
                {
                    if (link.Item1 == node2)
                    {

                        Adjacence[k].Remove(link);
                    }
                }


                foreach (Tuple<TensionNode, TensionEdge> link in Adjacence[l])
                {
                    if (link.Item1 == node1)
                    {

                        Adjacence[l].Remove(link);
                    }
                }

                Adjacence[k].AddRange(Adjacence[l]); //Add all the links from Node2 to Node1

                foreach (List<Tuple<TensionNode, TensionEdge>> sublist in Adjacence) //move all the links leading to node2 to node1
                {
                    foreach (Tuple<TensionNode, TensionEdge> link in sublist)
                    {
                        if (link.Item1 == node2)
                        {
                            sublist.Add(new Tuple<TensionNode, TensionEdge>(node1, link.Item2));
                            sublist.Remove(link);
                        }
                    }
                }

                Adjacence.RemoveAt(l); //Remove the Adjacence list of Node2

            }
        }
        #endregion
    }
}
