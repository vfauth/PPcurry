using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PPcurry
{
    public class Wire
    {
        #region Attributes

        private BoardGrid BoardGrid;
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
                Move();
            }
        }
        private List<Rectangle> Rectangles { get; set; } // The 3 rectangles composing the wire
        #endregion


        #region Constructor

        public Wire(BoardGrid boardGrid, Node originNode)
        {
            this.BoardGrid = boardGrid;
            this.Rectangles = new List<Rectangle>();
            for (int i = 0; i < 3; i++)
            {
                this.Rectangles.Add(new Rectangle() { Fill = new SolidColorBrush(Colors.Black) });
                BoardGrid.Children.Add(Rectangles[i]);
            }
            this.Nodes = new List<Node>() { originNode, originNode };
        }
        #endregion


        #region Methods

        /// <summary>
        /// Update the attributes of the rectangle associated with the wire
        /// </summary>
        public void Move()
        {
            SortNodes();

            // Positions of vertices
            Node[] vertices = new Node[4];
            vertices[0] = ExtremitiesNodes[0];
            vertices[1] = this.BoardGrid.Magnetize(new Point((ExtremitiesNodes[0].GetPosition().X + ExtremitiesNodes[1].GetPosition().X) / 2, ExtremitiesNodes[0].GetPosition().Y));
            vertices[2] = this.BoardGrid.Magnetize(new Point((ExtremitiesNodes[0].GetPosition().X + ExtremitiesNodes[1].GetPosition().X )/ 2, ExtremitiesNodes[1].GetPosition().Y));
            vertices[3] = ExtremitiesNodes[1];

            // Set the rectangles position and size
            for (int k = 0; k < 3; k++)
            {
                Canvas.SetLeft(this.Rectangles[k], vertices[k].GetPosition().X - Properties.Settings.Default.WireThickness / 2);
                Canvas.SetTop(this.Rectangles[k], vertices[k].GetPosition().Y - Properties.Settings.Default.WireThickness / 2);

                if (vertices[k].GetPosition().X == vertices[k + 1].GetPosition().X) // If the nodes are on the same column
                {
                    this.Rectangles[k].Height = Math.Abs(vertices[k+1].GetPosition().Y - vertices[k].GetPosition().Y + Properties.Settings.Default.WireThickness + Properties.Settings.Default.GridThickness); // Size
                    this.Rectangles[k].Width = Properties.Settings.Default.WireThickness;
                }
                else // If the nodes are on the same line
                {
                    this.Rectangles[k].Height = Properties.Settings.Default.WireThickness; // Size
                    this.Rectangles[k].Width = Math.Abs(vertices[k+1].GetPosition().X - vertices[k].GetPosition().X + Properties.Settings.Default.WireThickness + Properties.Settings.Default.GridThickness);
                }
                Debug.WriteLine("TEST");
                Debug.WriteLine(Rectangles[k].Width);
                Debug.WriteLine(Rectangles[k].Height);
                Debug.WriteLine(Canvas.GetLeft(Rectangles[k]));
                Debug.WriteLine(Canvas.GetTop(Rectangles[k]));
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
        #endregion
    }
}
