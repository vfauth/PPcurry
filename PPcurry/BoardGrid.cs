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

            this.Background = new SolidColorBrush
            {
                Opacity = 0 // Transparent background
            }; // Background to enable mouse events

            this.AllowDrop = true; // Components can be dropped on the board
            this.Drop += BoardGrid_Drop; // Event handler called when a component is dropped
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
                Rectangle Line = new Rectangle
                {
                    Height = GridThickness,
                    Width = this.ActualWidth,
                    Fill = new SolidColorBrush(Colors.Gray)
                };
                Line.SetValue(TopProperty, (double)(GridSpacing / 2 + y * (GridSpacing + GridThickness))); // Position
                Lines.Add(Line);
                this.Children.Add(Line); // Display the line
            }
            for (int x = Columns.Count(); x < this.ActualWidth / (GridSpacing + GridThickness) + 1; x++)
            {
                Rectangle Column = new Rectangle
                {
                    Height = this.ActualHeight,
                    Width = GridThickness,
                    Fill = new SolidColorBrush(Colors.Gray)
                };
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
        /// Draw the grid once the component is loaded 
        /// </summary>
        private void BoardGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
        }

        /// <summary>
        /// Event handler called when a component is dropped
        /// </summary>
        private void BoardGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat)) // If a component is dragged from the left panel, a new Component object must be created
            {
                string componentType = e.Data.GetData(DataFormats.StringFormat) as string; // The component type
                
                Point relativePosition = e.GetPosition(this); // Position of the drop relative to the board
                relativePosition.X -= (2*GridSpacing + GridThickness) / 2;
                relativePosition.Y -= (2*GridSpacing + GridThickness) / 2;
                XElement xmlElement = ((MainWindow)Application.Current.MainWindow).GetXmlComponentsList().Element(componentType); // Get the XML element with all the component data
                Component newComponent = new Component(relativePosition.X, relativePosition.Y, this, xmlElement); // Create the component and display it
                this.AddComponent(newComponent);
            }
            else if (e.Data.GetDataPresent(typeof(Component))) // If a component already present of the board is dragged
            {
                Component component = e.Data.GetData(typeof(Component)) as Component; // The dragged component

                Point relativePosition = e.GetPosition(this); // Position of the drop relative to the board
                relativePosition.X -= (2 * GridSpacing + GridThickness) / 2;
                relativePosition.Y -= (2 * GridSpacing + GridThickness) / 2;
                component.SetValue(LeftProperty, relativePosition.X); // The component is moved
                component.SetValue(TopProperty, relativePosition.Y);
            }
        }
        #endregion
    }
}
