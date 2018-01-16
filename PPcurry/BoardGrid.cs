using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace PPcurry
{
    public class BoardGrid : Canvas
    {
        #region Attributes

        private Point Origin; // The position of the origin
        private int GridSpacing; // The distance between two lines or columns
        private int GridThickness; // The lines thickness
        private double BoardWidth; // The canvas width
        private double BoardHeight; // The canvas height
        private List<Rectangle> Lines; // The lines of the grid
        private List<Rectangle> Columns; // The columns of the grid

        #endregion

        #region Accessors/Mutators
        
        public int GetGridSpacing() => this.GridSpacing;
        public void SetGridSpacing(int spacing) => this.GridSpacing = spacing;

        public int GetGridThickness() => this.GridThickness;
        public void SetGridThickness(int thickness) => this.GridThickness = thickness;
        #endregion


        #region Constructor
        public BoardGrid(int spacing, int thickness)
        {
            this.Loaded += BoardGrid_Loaded; // Draw the grid a first time after initialization

            // Initialization of attributes
            this.Origin = new Point(spacing/2, spacing/2);
            this.GridSpacing = spacing;
            this.GridThickness = thickness;
            this.Lines = new List<Rectangle>();
            this.Columns = new List<Rectangle>();
        }
        #endregion


        #region Methods
        /// <summary>
        /// Update the grid and draw it
        /// </summary>
        public void DrawGrid()
        {
            this.BoardWidth = ((StackPanel)(((Canvas)(this.Parent)).Parent)).ActualWidth;
            this.BoardHeight = ((StackPanel)(((Canvas)(this.Parent)).Parent)).ActualHeight;

            // Generation of new lines and columns if the board has extended 
            for (int y = Lines.Count(); y < (int)((BoardHeight + Origin.Y) / GridSpacing) + 1; y++)
            {
                Rectangle Line = new Rectangle();
                Line.Height = GridThickness;
                Line.Width = BoardWidth;
                Line.Fill = new SolidColorBrush(Colors.Gray);
                Line.SetValue(TopProperty, Origin.Y + y * GridSpacing); // Position
                Lines.Add(Line);
                this.Children.Add(Line); // Display the line
            }
            for (int x = Columns.Count(); x < (int)((BoardWidth + Origin.X) / GridSpacing) + 1; x++)
            {
                Rectangle Column = new Rectangle();
                Column.Height = BoardHeight;
                Column.Width = GridThickness;
                Column.Fill = new SolidColorBrush(Colors.Gray);
                Column.SetValue(LeftProperty, Origin.X + x * GridSpacing); // Position
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
        #endregion
    }
}
