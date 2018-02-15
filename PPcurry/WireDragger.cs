using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Xml.Linq;

namespace PPcurry
{
    public class WireDragger
    {
        #region Attributes

        private BoardGrid BoardGrid;
        private Wire Wire; // The wire to drag
        private Node StaticNode; // The node that doesn't move
        private Node CurrentDraggingNode; // The wire end to drag
        #endregion


        #region Constructor

        public WireDragger(BoardGrid boardGrid, Wire wire, Point mousePos)
        {
            this.BoardGrid = boardGrid;
            this.Wire = wire;
            this.CurrentDraggingNode = this.BoardGrid.Magnetize(mousePos);
            // Search the static node
            if (CurrentDraggingNode == Wire.Nodes[0])
            {
                this.StaticNode = Wire.Nodes[1];
            }
            else
            {
                this.StaticNode = Wire.Nodes[0];
            }

            this.BoardGrid.IsAddingWire = true;
        }
        #endregion


        #region Methods

        /// <summary>
        /// Called when the mouse moves
        /// </summary>
        public void Dragging(Node nearestNode)
        {
            if (nearestNode != CurrentDraggingNode)
            {
                this.CurrentDraggingNode = nearestNode;
                this.Wire.Nodes = new List<Node>() { StaticNode, CurrentDraggingNode }; // Moves the wire
            }
        }

        /// <summary>
        /// End the drag
        /// </summary>
        public void EndDrag()
        {
            this.BoardGrid.IsAddingWire = false;
        }
        #endregion
    }
}
