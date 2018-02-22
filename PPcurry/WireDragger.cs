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
        private Node DraggingNode; // The node to drag
        private Vector MouseOffset; // The position of the node to drag relative to the mouse
        #endregion


        #region Constructor

        public WireDragger(BoardGrid boardGrid, Wire wire, Node draggingNode, Vector mouseOffset)
        {
            this.BoardGrid = boardGrid;
            this.Wire = wire;
            this.DraggingNode = draggingNode;
            this.MouseOffset = mouseOffset;

            // Search for the static node
            if (DraggingNode == Wire.Nodes[0])
            {
                this.StaticNode = Wire.Nodes[1];
            }
            else
            {
                this.StaticNode = Wire.Nodes[0];
            }

            this.BoardGrid.DraggingWire = true;
        }
        #endregion


        #region Methods

        /// <summary>
        /// Called when the mouse moves
        /// </summary>
        public void DragOver(Point mousePos)
        {
            Node newNode = this.BoardGrid.Magnetize(mousePos + MouseOffset);
            // Move the wire if the new node is inside the canvas and different from the former node
            if (!(newNode.GetPosition().X < -this.BoardGrid.GridThickness || newNode.GetPosition().Y < -this.BoardGrid.GridThickness || newNode.GetPosition().X > this.BoardGrid.ActualWidth + this.BoardGrid.GridThickness || newNode.GetPosition().Y > this.BoardGrid.ActualHeight + this.BoardGrid.GridThickness))
            {
                if (newNode != DraggingNode)
                {
                    this.DraggingNode = newNode;
                    this.Wire.Nodes = new List<Node>() { StaticNode, DraggingNode }; // Move the wire
                }
            }
        }

        /// <summary>
        /// End the drag
        /// </summary>
        public void EndDrag()
        {
            if (this.StaticNode == this.DraggingNode) // Remove the wire if it is just a point
            {
                BoardGrid.RemoveWire(this.Wire);
            }
            else
            {
                this.Wire.ConnectToNodes();
            }
        }
        #endregion
    }
}
