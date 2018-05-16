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
    [Serializable()]
    public class Wire
    {
        #region Attributes

        [NonSerialized()] private BoardGrid BoardGrid;
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
        [NonSerialized()] private List<Rectangle> Rectangles; // The 3 rectangles composing the wire
        #endregion


        #region Constructor

        public Wire(BoardGrid boardGrid, Node originNode)
        {
            BoardGrid = boardGrid;
            Thickness = Properties.Settings.Default.WireThickness; // Default thickness
            Rectangles = new List<Rectangle>();
            for (int i = 0; i < 3; i++)
            {
                Rectangle newRect = new Rectangle() { Fill = Brushes.Black };
                newRect.MouseLeftButtonUp += SwitchIsSelected;
                Rectangles.Add(newRect);
                BoardGrid.Children.Add(Rectangles[i]);
            }
            Nodes = new List<Node>() { originNode, originNode };
        }
        #endregion


        #region Methods

        /// <summary>
        /// Update the attributes of the rectangles associated to the wire
        /// </summary>
        private void Redraw()
        {
            SortNodes();

            // Positions of vertices
            Node[] vertices = new Node[4];
            vertices[0] = ExtremitiesNodes[0];
            if (ExtremitiesNodes[0].Position.Y < ExtremitiesNodes[1].Position.Y)
            {
                vertices[1] = BoardGrid.Magnetize(new Point((ExtremitiesNodes[0].Position.X + ExtremitiesNodes[1].Position.X) / 2, ExtremitiesNodes[0].Position.Y));
            }
            else
            {
                vertices[1] = BoardGrid.Magnetize(new Point((ExtremitiesNodes[0].Position.X + ExtremitiesNodes[1].Position.X) / 2, ExtremitiesNodes[1].Position.Y));
            }
            vertices[2] = BoardGrid.Magnetize(new Point((ExtremitiesNodes[0].Position.X + ExtremitiesNodes[1].Position.X) / 2, ExtremitiesNodes[1].Position.Y));
            vertices[3] = ExtremitiesNodes[1];

            // Set the rectangles position and size
            for (int k = 0; k < 3; k++)
            {
                Canvas.SetLeft(Rectangles[k], vertices[k].Position.X - Thickness / 2);
                Canvas.SetTop(Rectangles[k], vertices[k].Position.Y - Thickness / 2);

                if (k == 1) // The middle rectangle is the vertical one
                {
                    Rectangles[k].Height = Math.Abs(vertices[0].Position.Y - vertices[2].Position.Y) + Thickness; // Size
                    Rectangles[k].Width = Thickness;
                }
                else // The two other rectangles are horizontal
                {
                    Rectangles[k].Height = Thickness; // Size
                    Rectangles[k].Width = Math.Abs(vertices[k+1].Position.X - vertices[k].Position.X) + Thickness;
                }
            }
        }

        /// <summary>
        /// Sort the nodes if they are not in the right order: Lowest X then lowest Y if X is equal
        /// </summary>
        private void SortNodes()
        {
            if (ExtremitiesNodes.Count > 1)
            {
                Point Point1 = ExtremitiesNodes[0].Position;
                Point Point2 = ExtremitiesNodes[1].Position;
                if (Point1.X > Point2.X)
                {
                    ExtremitiesNodes.Reverse();
                }
                else if (Point1.X == Point2.X && Point1.Y > Point2.Y)
                {

                    ExtremitiesNodes.Reverse();
                }
            }
        }

        /// <summary>
        /// Connect the wire to the nodes at its extremities
        /// </summary>
        public void ConnectToNodes()
        {
            ClearNodes(); // Clear previous connections

            if (ExtremitiesNodes[0].Position.X == ExtremitiesNodes[1].Position.X) // Vertical wire
            {
                if (ExtremitiesNodes[0].Position.Y > ExtremitiesNodes[1].Position.Y)
                {
                    ExtremitiesNodes[0].ConnectedElements.Add(this, Directions.Up);
                    ExtremitiesNodes[1].ConnectedElements.Add(this, Directions.Down);
                }
                else if (ExtremitiesNodes[0].Position.Y < ExtremitiesNodes[1].Position.Y)
                {
                    ExtremitiesNodes[0].ConnectedElements.Add(this, Directions.Down);
                    ExtremitiesNodes[1].ConnectedElements.Add(this, Directions.Up);
                }
            }
            else // Vertical wire
            {
                if (ExtremitiesNodes[0].Position.X > ExtremitiesNodes[1].Position.X)
                {
                    ExtremitiesNodes[0].ConnectedElements.Add(this, Directions.Left);
                    ExtremitiesNodes[1].ConnectedElements.Add(this, Directions.Right);
                }
                else if (ExtremitiesNodes[0].Position.X < ExtremitiesNodes[1].Position.X)
                {
                    ExtremitiesNodes[0].ConnectedElements.Add(this, Directions.Right);
                    ExtremitiesNodes[1].ConnectedElements.Add(this, Directions.Left);
                }
            }
        }

        /// <summary>
        /// Remove this object from all connected nodes
        /// </summary>
        private void ClearNodes()
        {
            if (ExtremitiesNodes[1].ConnectedElements.ContainsKey(this))
            {
                ExtremitiesNodes[1].ConnectedElements.Remove(this); // Remove the anchor from the node connected elements
            }
            if (ExtremitiesNodes[0].ConnectedElements.ContainsKey(this))
            {
                ExtremitiesNodes[0].ConnectedElements.Remove(this); // Remove the anchor from the node connected elements
            }
        }

        /// <summary>
        /// Select or deselect the wire
        /// </summary>
        public void SetIsSelected(bool isSelected)
        {
            if (isSelected != IsSelected) // Check whether the selected state has changed
            {
                IsSelected = isSelected;
                if (isSelected)
                {
                    Thickness = Properties.Settings.Default.WireThicknessSelected; // The selected wire is thicker
                }
                else
                {
                    Thickness = Properties.Settings.Default.WireThickness; // The default wire thickness
                }
                Redraw();
            }
        }

        /// <summary>
        /// Change the selection status of the wire
        /// </summary>
        private void SwitchIsSelected(object sender, MouseButtonEventArgs e)
        {
            if (IsSelected) // The wire is deselected
            {
                BoardGrid.SelectedElement = null;
            }
            else // The wire is selected
            {
                BoardGrid.SelectedElement = this;
            }
        }

        /// <summary>
        /// Remove the wire from the board by removing the rectangles and the connections to the nodes
        /// </summary>
        public void RemoveFromBoard()
        {
            foreach (Rectangle rectangle in Rectangles)
            {
                BoardGrid.Children.Remove(rectangle);
            }
            ClearNodes();
        }

        /// <summary>
        /// Add to the new BoardGrid and recreate the rectangles after deserialization
        /// </summary>
        public void Deserialized(BoardGrid boardGrid)
        {
            BoardGrid = boardGrid;

            Rectangles = new List<Rectangle>();
            for (int i = 0; i < 3; i++)
            {
                Rectangle newRect = new Rectangle() { Fill = Brushes.Black };
                newRect.MouseLeftButtonUp += SwitchIsSelected;
                Rectangles.Add(newRect);
                BoardGrid.Children.Add(Rectangles[i]);
            }
            Redraw();
        }
        #endregion
    }
}
