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
    public class Wire
    {
        #region Attributes

        private BoardGrid BoardGrid;
        private double Thickness; // The thickness of the wires
        private bool IsSelected = false; // Whether the wire is currently selected or not
        private List<Node> ExtremitiesNodes; // Nodes connected to the wire
        public List<Node> Nodes
        {
            get
            {
                return ExtremitiesNodes;
            }
            set
            {
                ExtremitiesNodes = value;
                Redraw();
            }
        }
        public List<Rectangle> Rectangles { get; } // The 3 rectangles composing the wire
        #endregion


        #region Constructor

        public Wire(BoardGrid boardGrid, Node originNode)
        {
            this.BoardGrid = boardGrid;
            this.Thickness = Properties.Settings.Default.WireThickness; // Default thickness
            this.Rectangles = new List<Rectangle>();
            for (int i = 0; i < 3; i++)
            {
                Rectangle newRect = new Rectangle() { Fill = Brushes.Black };
                newRect.MouseLeftButtonUp += SwitchIsSelected;
                this.Rectangles.Add(newRect);
                BoardGrid.Children.Add(Rectangles[i]);
            }
            this.Nodes = new List<Node>() { originNode, originNode };
        }
        #endregion


        #region Methods

        /// <summary>
        /// Update the attributes of the rectangles associated to the wire
        /// </summary>
        public void Redraw()
        {
            SortNodes();

            // Positions of vertices
            Node[] vertices = new Node[4];
            vertices[0] = ExtremitiesNodes[0];
            if (ExtremitiesNodes[0].GetPosition().Y < ExtremitiesNodes[1].GetPosition().Y)
            {
                vertices[1] = this.BoardGrid.Magnetize(new Point((ExtremitiesNodes[0].GetPosition().X + ExtremitiesNodes[1].GetPosition().X) / 2, ExtremitiesNodes[0].GetPosition().Y));
            }
            else
            {
                vertices[1] = this.BoardGrid.Magnetize(new Point((ExtremitiesNodes[0].GetPosition().X + ExtremitiesNodes[1].GetPosition().X) / 2, ExtremitiesNodes[1].GetPosition().Y));
            }
            vertices[2] = this.BoardGrid.Magnetize(new Point((ExtremitiesNodes[0].GetPosition().X + ExtremitiesNodes[1].GetPosition().X) / 2, ExtremitiesNodes[1].GetPosition().Y));
            vertices[3] = ExtremitiesNodes[1];

            // Set the rectangles position and size
            for (int k = 0; k < 3; k++)
            {
                Canvas.SetLeft(this.Rectangles[k], vertices[k].GetPosition().X - this.Thickness / 2);
                Canvas.SetTop(this.Rectangles[k], vertices[k].GetPosition().Y - this.Thickness / 2);

                if (k == 1) // The middle rectangle is the vertical one
                {
                    this.Rectangles[k].Height = Math.Abs(vertices[0].GetPosition().Y - vertices[2].GetPosition().Y) + this.Thickness; // Size
                    this.Rectangles[k].Width = this.Thickness;
                }
                else // The two other rectangles are horizontal
                {
                    this.Rectangles[k].Height = this.Thickness; // Size
                    this.Rectangles[k].Width = Math.Abs(vertices[k+1].GetPosition().X - vertices[k].GetPosition().X) + this.Thickness;
                }
            }
        }

        /// <summary>
        /// Sort the nodes if they are not in the right order: Lowest X then lowest Y if X is equal
        /// </summary>
        private void SortNodes()
        {
            if (this.ExtremitiesNodes.Count > 1)
            {
                Point Point1 = this.ExtremitiesNodes[0].GetPosition();
                Point Point2 = this.ExtremitiesNodes[1].GetPosition();
                if (Point1.X > Point2.X)
                {
                    this.ExtremitiesNodes.Reverse();
                }
                else if (Point1.X == Point2.X && Point1.Y > Point2.Y)
                {

                    this.ExtremitiesNodes.Reverse();
                }
            }
        }

        /// <summary>
        /// Connect the wire to the nodes at its extremities
        /// </summary>
        public void ConnectToNodes()
        {
            ClearNodes(); // Clear previous connections

            if (this.ExtremitiesNodes[0].GetPosition().X == this.ExtremitiesNodes[1].GetPosition().X) // Vertical wire
            {
                if (this.ExtremitiesNodes[0].GetPosition().Y > this.ExtremitiesNodes[1].GetPosition().Y)
                {
                    this.ExtremitiesNodes[0].ConnectedComponents.Add(this, Directions.Up);
                    this.ExtremitiesNodes[1].ConnectedComponents.Add(this, Directions.Down);
                }
                else if (this.ExtremitiesNodes[0].GetPosition().Y < this.ExtremitiesNodes[1].GetPosition().Y)
                {
                    this.ExtremitiesNodes[0].ConnectedComponents.Add(this, Directions.Down);
                    this.ExtremitiesNodes[1].ConnectedComponents.Add(this, Directions.Up);
                }
            }
            else // Vertical wire
            {
                if (this.ExtremitiesNodes[0].GetPosition().X > this.ExtremitiesNodes[1].GetPosition().X)
                {
                    this.ExtremitiesNodes[0].ConnectedComponents.Add(this, Directions.Left);
                    this.ExtremitiesNodes[1].ConnectedComponents.Add(this, Directions.Right);
                }
                else if (this.ExtremitiesNodes[0].GetPosition().X < this.ExtremitiesNodes[1].GetPosition().X)
                {
                    this.ExtremitiesNodes[0].ConnectedComponents.Add(this, Directions.Right);
                    this.ExtremitiesNodes[1].ConnectedComponents.Add(this, Directions.Left);
                }
            }
        }

        /// <summary>
        /// Remove this object from all connected nodes
        /// </summary>
        public void ClearNodes()
        {
            if (this.ExtremitiesNodes[1].ConnectedComponents.ContainsKey(this))
            {
                this.ExtremitiesNodes[1].ConnectedComponents.Remove(this); // Remove the anchor from the node connected elements
            }
            if (this.ExtremitiesNodes[0].ConnectedComponents.ContainsKey(this))
            {
                this.ExtremitiesNodes[0].ConnectedComponents.Remove(this); // Remove the anchor from the node connected elements
            }
        }

        /// <summary>
        /// Select or deselect the wire
        /// </summary>
        public void SetIsSelected(bool isSelected)
        {
            if (isSelected != this.IsSelected) // Check whether the selected state has changed
            {
                this.IsSelected = isSelected;
                if (isSelected)
                {
                    this.Thickness = Properties.Settings.Default.WireThicknessSelected; // The selected wire is thicker
                    this.BoardGrid.SelectedElement = this;
                }
                else
                {
                    this.Thickness = Properties.Settings.Default.WireThickness; // The default wire thickness
                    this.BoardGrid.SelectedElement = null;
                }
                Redraw();
            }
            this.IsSelected = isSelected;
        }

        /// <summary>
        /// Change the selection status of the wire
        /// </summary>
        public void SwitchIsSelected(object sender, MouseButtonEventArgs e)
        {
            SetIsSelected(!this.IsSelected);
        }
        #endregion
    }
}
