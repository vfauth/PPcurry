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
            BoardGrid = boardGrid;
            Wire = wire;
            DraggingNode = draggingNode;
            MouseOffset = mouseOffset;

            // Search for the static node
            if (DraggingNode == Wire.Nodes[0])
            {
                StaticNode = Wire.Nodes[1];
            }
            else
            {
                StaticNode = Wire.Nodes[0];
            }

            BoardGrid.DraggingWire = true;
        }
        #endregion


        #region Methods

        /// <summary>
        /// Called when the mouse moves
        /// </summary>
        public void DragOver(Point mousePos)
        {
            Node newNode = BoardGrid.Magnetize(mousePos + MouseOffset);
            // Move the wire if the new node is inside the canvas and different from the former node
            if (!(newNode.Position.X < -BoardGrid.GridThickness || newNode.Position.Y < -BoardGrid.GridThickness || newNode.Position.X > BoardGrid.ActualWidth + BoardGrid.GridThickness || newNode.Position.Y > BoardGrid.ActualHeight + BoardGrid.GridThickness))
            {
                if (newNode != DraggingNode)
                {
                    DraggingNode = newNode;
                    Wire.Nodes = new List<Node>() { StaticNode, DraggingNode }; // Move the wire
                }
            }
        }

        /// <summary>
        /// End the drag
        /// </summary>
        public void EndDrag()
        {
            if (StaticNode == DraggingNode) // Remove the wire if it is just a point
            {
                BoardGrid.RemoveWire(Wire);
            }
            else
            {
                Wire.ConnectToNodes();
            }
        }
        #endregion
    }
}
