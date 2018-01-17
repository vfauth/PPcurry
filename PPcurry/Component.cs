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
    public class Component : Image
    {
        #region Attributes

        private BoardGrid Grid; // The board on which is this component
        private double[] Position; // The position of the component on the grid
		private string ComponentName; // The component name
        private Point CursorDraggingPosition; // The position of the cursor on the image during dragging
        #endregion


        #region Accessors/Mutators

        public double[] GetPosition() => this.Position;
        public void SetPosition(double[] position) => this.Position = position;

        public string GetName() => this.ComponentName;
        public void SetName(string name)
        {
            this.ComponentName = name;
            this.ToolTip = name; // Update the tooltip
        }
        #endregion


        #region Constructor

        /// <summary>
        /// Add one component to the board
        /// </summary>
        /// <param name="x">The component abscissa</param>
        /// <param name="y">The component ordinate</param>
        /// <param name="canvas">The canvas on which to display the component</param>
        /// <param name="xmlElement">The XML Element with the component data</param>
        public Component(double x, double y, Canvas canvas, XElement xmlElement)
        {
            // Save attributes
            this.Position = new double[2] { x , y };
            this.ComponentName = xmlElement.Element("name").Value;
            this.Grid = canvas as BoardGrid;

            // Set the image attributes and display it
            this.Width = 2*Grid.GetGridSpacing() + Grid.GetGridThickness(); // The component covers 2 grid cells
            this.Height = 2*Grid.GetGridSpacing() + Grid.GetGridThickness(); // The component covers 2 grid cells
            Uri imageUri = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, Properties.Settings.Default.ResourcesFolder, xmlElement.Element("image").Value));
            this.Source = new BitmapImage(imageUri); // Image to display
            this.SetValue(Canvas.LeftProperty, x); // Position
            this.SetValue(Canvas.TopProperty, y);
            this.ToolTip = this.ComponentName;
            this.MouseMove += Component_MouseMove;
            canvas.Children.Add(this); // Add the component
        }
        #endregion


        #region Methods

        /// <summary>
        /// If the mouse moves over a component on the board and the left mouse button is pressed, the component is dragged
        /// </summary>
        public void Component_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) // Drag only if the left button is pressed
            {
                DragDrop.DoDragDrop((Component)sender, (Component)sender, DragDropEffects.Move); // Begin the drag&drop
            }
        }
        #endregion
    }
}
