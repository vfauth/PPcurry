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

        private BoardGrid Grid; // The main window in which is this component
        private double[] Position; // The position of the component on the grid
		private string ComponentName; // The component name
        private bool IsDragged; // Whether thhe component is currently being dragged
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
            Uri ImageUri = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, Properties.Settings.Default.ResourcesFolder, xmlElement.Element("image").Value));
            this.Source = new BitmapImage(ImageUri); // Image to display
            this.SetValue(Canvas.LeftProperty, x); // Position
            this.SetValue(Canvas.TopProperty, y);
            this.ToolTip = this.ComponentName;
            this.MouseLeftButtonDown += Component_MouseLeftButtonDown;
            this.MouseLeftButtonUp += Component_MouseLeftButtonUp;
            this.MouseMove += Component_MouseMove;
            canvas.Children.Add(this); // Add the component
        }
        #endregion


        #region Methods

        /// <summary>
        /// When the component is left-clicked, he is dragged
        /// </summary>
        public void Component_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsDragged = true;
            Debug.WriteLine(e.GetPosition((Image)sender).X + "; " + e.GetPosition((Image)sender).Y);
            CursorDraggingPosition = e.GetPosition((Image)sender); // The position of the cursor on the image during dragging
           ((Component)sender).CaptureMouse();  // The cursor cannot quit the image
        }

        /// <summary>
        /// The dragging finishes when the component is released
        /// </summary>
        public void Component_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsDragged = false;
            ((Component)sender).ReleaseMouseCapture(); // Cancel CaptureMouse()
        }

        /// <summary>
        /// This function manages the dragging of the component and is called for each move
        /// </summary>
        public void Component_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragged) return; // This function must do nothing if the component is not being dragged

            Point MousePos = e.GetPosition(Grid); // Mouse position relative to the BoardCanvas
            // The image is aligned with the grid
            //MousePos = Grid.Magnetize(MousePos);
            Canvas.SetLeft(this, MousePos.X - this.ActualWidth/2 + CursorDraggingPosition.X);
            Canvas.SetTop(this, MousePos.Y - this.ActualHeight/2 + CursorDraggingPosition.Y);
        }
        #endregion
    }
}
