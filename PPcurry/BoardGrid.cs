using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace PPcurry
{
    public class BoardGrid : Canvas
    {
        #region Attributes
        
        private int GridSpacing; // The distance between two lines or columns
        private int GridThickness; // The lines thickness
        //private Rectangle Background; // The background receiving mouse events
        private List<Rectangle> Lines; // The lines of the grid
        private List<Rectangle> Columns; // The columns of the grid
        private List<Component> ComponentsOnBoard; // The list of components on the board

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

            this.Background = new SolidColorBrush(); // Background to enable mouse events
            this.Background.Opacity = 0; // Transparent background

            this.AllowDrop = true; // Components can be dropped on the board
            this.Drop += BoardGrid_ComponentDropped; // Event handler for component dropping
            this.DragOver += BoardGrid_DragOver; // Event handler for component dragging
        }
        #endregion


        #region Methods
        /// <summary>
        /// Update the grid and draw it
        /// </summary>
        public void DrawGrid()
        {
            // Generation of new lines and columns if the board has extended 
            for (int y = Lines.Count(); y < this.ActualHeight / (GridSpacing + GridThickness) + 1; y++)
            {
                Rectangle Line = new Rectangle();
                Line.Height = GridThickness;
                Line.Width =this.ActualWidth;
                Line.Fill = new SolidColorBrush(Colors.Gray);
                Line.SetValue(TopProperty, (double)(GridSpacing / 2 + y * (GridSpacing + GridThickness))); // Position
                Lines.Add(Line);
                this.Children.Add(Line); // Display the line
            }
            for (int x = Columns.Count(); x < this.ActualWidth / (GridSpacing + GridThickness) + 1; x++)
            {
                Rectangle Column = new Rectangle();
                Column.Height = this.ActualHeight;
                Column.Width = GridThickness;
                Column.Fill = new SolidColorBrush(Colors.Gray);
                Column.SetValue(LeftProperty, (double)(GridSpacing / 2 + x * (GridSpacing + GridThickness))); // Position
                Columns.Add(Column);
                this.Children.Add(Column); // Display the column
            }
        }

        /// <summary>
        /// Returns the nearest grid node (as a Point) of given Point
        /// </summary>
        public Point Magnetize(Point point)
        {
            double X = point.X;
            double Y = point.Y;
            Point Nearest = new Point();
            if (Math.Abs(X - GridSpacing * (int)(X/GridSpacing)) < Math.Abs(X - GridSpacing * ((int)(X / GridSpacing) + 1)))
            {
                Nearest.X = GridSpacing * (int)(X / GridSpacing);
            }
            else
            {
                Nearest.X = GridSpacing * ((int)(X / GridSpacing) + 1);
            }
            if (Math.Abs(Y - GridSpacing * (int)(Y / GridSpacing)) < Math.Abs(Y - GridSpacing * ((int)(Y / GridSpacing) + 1)))
            {
                Nearest.Y = GridSpacing * (int)(Y / GridSpacing);
            }
            else
            {
                Nearest.Y = GridSpacing * ((int)(Y / GridSpacing) + 1);
            }
            return Nearest;
        }

        /// <summary>
        /// Draw the grid when the component is loaded 
        /// </summary>
        private void BoardGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
        }

        /// <summary>
        /// Handle a drop on the board
        /// </summary>
        private void BoardGrid_ComponentDropped(object sender, DragEventArgs e)
        {
            if (true) // If a component is dragged from the left panel, a new Component object must be created
            {
                string ComponentType = e.Data.GetData(DataFormats.StringFormat) as string; // The component type
                
                Point RelativePosition = e.GetPosition(this); // Position of the drop relative to the board
                RelativePosition.X -= (2*GridSpacing + GridThickness) / 2;
                RelativePosition.Y -= (2*GridSpacing + GridThickness) / 2;
                XElement XmlElement = ((MainWindow)Application.Current.MainWindow).GetXmlComponentsList().Element(ComponentType); // Get the XML element with all the component data
                Component NewComponent = new Component(RelativePosition.X, RelativePosition.Y, this, XmlElement); // Create the component and display it
                Debug.WriteLine("TEST2");
                this.AddComponent(NewComponent);
            }
        }

        private void BoardGrid_DragOver(object sender, DragEventArgs e)
        {
            Debug.WriteLine("DRAGOVER");
        }
        #endregion
    }
}
