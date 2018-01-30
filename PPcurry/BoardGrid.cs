using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace PPcurry
{
    public class BoardGrid : Canvas
    {
        #region Attributes
        
        private int GridSpacing; // The distance between two lines or columns
        private int GridThickness; // The lines thickness
        private List<Rectangle> Lines; // The lines of the grid
        private List<Rectangle> Columns; // The columns of the grid
        private List<Component> ComponentsOnBoard; // The list of components on the board
        private List<List<Node>> Nodes;

        #endregion


        #region Accessors/Mutators

        public int GetGridSpacing() => this.GridSpacing;
        public void SetGridSpacing(int spacing) => this.GridSpacing = spacing;

        public int GetGridThickness() => this.GridThickness;
        public void SetGridThickness(int thickness) => this.GridThickness = thickness;

        public void AddComponent(Component component)
        {
            if (component != null)
            {
                this.ComponentsOnBoard.Add(component);
                if (component.Parent != null)
                {
                    ((Panel)component.Parent).Children.Remove(component);
                }
                this.Children.Add(component);
            }
        }
        public void AddNode(Node Node)
        {
            if (Node.GetPosition() != null)
            {
                //this.ComponentsOnBoard.Add(component);
                //if (component.Parent != null)
                //{
                //    ((Panel)component.Parent).Children.Remove(component);
                //}
                //this.Children.Add(component);
            }
        }
        #endregion


        #region Constructor
        public BoardGrid(int spacing, int thickness)
        {
            this.Loaded += BoardGrid_Loaded; // Draw the grid a first time after initialization

            // Initialization of attributes
            this.GridSpacing = spacing;
            this.GridThickness = thickness;
            this.Lines = new List<Rectangle>();
            this.Columns = new List<Rectangle>();
            ComponentsOnBoard = new List<Component>();
            Nodes = new List<List<Node>>();

            this.Background = new SolidColorBrush
            {
                Opacity = 0 // Transparent background
            }; // Background to enable mouse events

            this.AllowDrop = true; // Components can be dropped on the board
            this.DragEnter += BoardGrid_DragEnter; // Event handler called when a dragged component enters the board
            this.DragOver += BoardGrid_DragOver; // Event handler continusously called while dragging
            this.Drop += BoardGrid_Drop; // Event handler called when a component is dropped
            this.DragLeave += BoardGrid_DragLeave; // Event handler called when a dragged component leaves the board
        }
        #endregion


        #region Methods
        /// <summary>
        /// Update the grid and draw it
        /// </summary>
        public void DrawGrid()
        {
            double gridTotalSpacing = GridSpacing + GridThickness;

            // Generation of new lines and columns if the board has extended 
            for (int y = Lines.Count(); y < this.ActualHeight / gridTotalSpacing + 1; y++)
            {
                Rectangle Line = new Rectangle
                {
                    Height = GridThickness,
                    Width = this.ActualWidth,
                    Fill = new SolidColorBrush(Colors.Gray)
                };
                Line.SetValue(TopProperty, (double)(y * gridTotalSpacing)); // Position
                Lines.Add(Line);
                this.Children.Add(Line); // Display the line

                Nodes.Add(new List<Node>()); // Add a list of nodes for that line
                for (int x = 0; x < this.ActualWidth / gridTotalSpacing + 1; x++)
                {
                    // Create the nodes on that line
                    Point nodePosition = new Point(x * gridTotalSpacing, y * gridTotalSpacing);
                    Nodes[y].Add(new Node(nodePosition, this));
                }
            }
            for (int x = Columns.Count(); x < this.ActualWidth / gridTotalSpacing + 1; x++)
            {
                Rectangle Column = new Rectangle
                {
                    Height = this.ActualHeight,
                    Width = GridThickness,
                    Fill = new SolidColorBrush(Colors.Gray)
                };
                Column.SetValue(LeftProperty, (double)(x * gridTotalSpacing)); // Position
                Columns.Add(Column);
                this.Children.Add(Column); // Display the column

                // Add nodes for the new columns
                for(int y = 0; y < Nodes.Count; y++)
                {
                    Point nodePosition = new Point(x * gridTotalSpacing, y * gridTotalSpacing);
                    Nodes[y].Add(new Node(nodePosition, this));
                }
            }
        }

        /// <summary>
        /// Returns the nearest grid node (as a Point) of the given Point
        /// </summary>
        private Point Magnetize(Point point)
        {
            double X = point.X;
            double Y = point.Y;
            double gridTotalSpacing = GridSpacing + GridThickness;
            Point Nearest = new Point();
            if (Math.Abs(X % gridTotalSpacing) < Math.Abs(gridTotalSpacing - X % gridTotalSpacing))
            {
                Nearest.X = gridTotalSpacing * (int)(X / gridTotalSpacing);
            }
            else
            {
                Nearest.X = gridTotalSpacing * ((int)(X / gridTotalSpacing) + 1);
            }
            if (Math.Abs(Y % gridTotalSpacing) < Math.Abs(gridTotalSpacing - Y % gridTotalSpacing))
            {
                Nearest.Y = gridTotalSpacing * (int)(Y / gridTotalSpacing);
            }
            else
            {
                Nearest.Y = gridTotalSpacing * ((int)(Y / gridTotalSpacing) + 1);
            }
            return Nearest;
        }

        /// <summary>
        /// Draw the grid once the component is loaded 
        /// </summary>
        private void BoardGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
        }



        /// <summary>
        /// Event handler called when a dragged component enters the board
        /// </summary>
        private void BoardGrid_DragEnter(object sender, DragEventArgs e)
        {
            Component component = e.Data.GetData(typeof(Component)) as Component; // The dragged component

            component.Opacity = 100; // Display the component
        }

        /// <summary>
        /// Event handler continuously called while dragging
        /// </summary>
        private void BoardGrid_DragOver(object sender, DragEventArgs e)
        {
            Component component = e.Data.GetData(typeof(Component)) as Component; // The dragged component

            Point relativePosition = e.GetPosition(this); // Position of the mouse relative to the board
            Point gridNode = Magnetize(relativePosition - component.GetSize()/2 + component.GetAnchor()); // The nearest grid node from the anchor
            component.SetValue(LeftProperty, gridNode.X - component.GetAnchor().X); // The component is moved
            component.SetValue(TopProperty, gridNode.Y - component.GetAnchor().Y);

            // If the component is dragged outside of the board, it is hidden
            if (relativePosition.X < 0 || relativePosition.X > this.ActualWidth || relativePosition.Y < 0 || relativePosition.Y > this.ActualHeight)
            {
                component.Opacity = 0;
            }
            else
            {
                component.Opacity = 100;
            }
        }

        /// <summary>
        /// Event handler called when a component is dropped
        /// </summary>
        private void BoardGrid_Drop(object sender, DragEventArgs e)
        {
            Component component = e.Data.GetData(typeof(Component)) as Component; // The dragged component

            Point relativePosition = e.GetPosition(this); // Position of the mouse relative to the board
            Point gridNode = Magnetize(relativePosition - component.GetSize() / 2 + component.GetAnchor()); // The nearest grid node from the anchor
            component.SetValue(LeftProperty, gridNode.X - component.GetAnchor().X); // The component is moved
            component.SetValue(TopProperty, gridNode.Y - component.GetAnchor().Y);
        }

        /// <summary>
        /// Event handler called when a dragged component leaves the board
        /// </summary>
        private void BoardGrid_DragLeave(object sender, DragEventArgs e)
        {
            Component component = e.Data.GetData(typeof(Component)) as Component; // The dragged component

            component.Opacity = 0; // Do not display the component
        }
        #endregion
    }
}
