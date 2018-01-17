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
    public class Component : Border
    {
        #region Attributes

        private Image ComponentImage;
        private BoardGrid BoardGrid; // The board on which is this component
        private Point Position; // The position of the component on the grid
        private Vector Size; // The component displayed size as a Vector
        private Vector Anchor; // The vector between the image origin and one of the component anchors
        private double Scale; // The scaling factor applied to the image
        private string ComponentName; // The component name

        private bool IsSelected; // Is true when the component is selected
        private int LastMouseLeftButtonDown; // Timestamp of last MouseLeftButtonDown event; 0 if already handled
        private int LastMouseLeftButtonUp; // Timestamp of last MouseLeftButtonUp event; 0 if already handled
        #endregion


        #region Accessors/Mutators

        public Vector GetAnchor() => this.Anchor * this.Scale;

        public Vector GetSize() => this.Size;

        public Point GetPosition() => this.Position;
        public void SetPosition(Point position) => this.Position = position;

        public string GetName() => this.ComponentName;
        public void SetName(string name)
        {
            this.ComponentName = name;
            this.ToolTip = name; // Update the tooltip
        }

        public void SetIsSelected(bool isSelected)
        {
            if (isSelected != this.IsSelected)
            {
                this.IsSelected = isSelected;
                if (isSelected)
                {
                    this.BoardGrid.SetSelectedComponent(this);
                    this.BorderThickness = new Thickness(Properties.Settings.Default.ComponentBorderThickness);

                    // Adjust the component size and position to avoid image resizing
                    double thickness = Properties.Settings.Default.ComponentBorderThickness;
                    this.Width += thickness*2;
                    this.Height += thickness*2;
                    this.SetValue(Canvas.LeftProperty, (double)this.GetValue(Canvas.LeftProperty) - thickness);
                    this.SetValue(Canvas.TopProperty, (double)this.GetValue(Canvas.TopProperty) - thickness);
                }
                else
                {
                    this.BoardGrid.SetSelectedComponent(null);
                    this.BorderThickness = new Thickness(0);

                    // Adjust the component size and position to avoid image resizing
                    double thickness = Properties.Settings.Default.ComponentBorderThickness;
                    this.Width -= thickness * 2;
                    this.Height -= thickness * 2;
                    this.SetValue(Canvas.LeftProperty, (double)this.GetValue(Canvas.LeftProperty) + thickness); 
                    this.SetValue(Canvas.TopProperty, (double)this.GetValue(Canvas.TopProperty) + thickness);
                }
            }
        }
        public void SwitchIsSelected()
        {
            SetIsSelected(!this.IsSelected);
        }
        #endregion


        #region Constructor

        /// <summary>
        /// Add one component to the board
        /// </summary>
        /// <param name="x">The component abscissa</param>
        /// <param name="y">The component ordinate</param>
        /// <param name="boardGrid">The canvas on which to display the component</param>
        /// <param name="xmlElement">The XML Element with the component data</param>
        public Component(Point position, BoardGrid boardGrid, XElement xmlElement)
        {
            // Save attributes
            this.BoardGrid = boardGrid as BoardGrid;
            this.Position = position;
            this.ComponentName = xmlElement.Element("name").Value;
            this.Anchor.X = (double)xmlElement.Element("anchors").Element("anchor").Element("posX");
            this.Anchor.Y = (double)xmlElement.Element("anchors").Element("anchor").Element("posY");

            // Set the image attributes and display it
            this.Width = 2*BoardGrid.GetGridSpacing() + 3*BoardGrid.GetGridThickness(); // The component covers 2 grid cells
            this.Height = 2*BoardGrid.GetGridSpacing() + 3*BoardGrid.GetGridThickness(); // The component covers 2 grid cells
            this.Size = new Vector(this.Width, this.Height);

            this.Scale = this.Width / (double)xmlElement.Element("width");
            Uri imageUri = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, Properties.Settings.Default.ResourcesFolder, xmlElement.Element("image").Value));
            this.ComponentImage = new Image();
            this.ComponentImage.Source = new BitmapImage(imageUri); // Image to display
            this.Child = ComponentImage;

            this.BorderBrush = new SolidColorBrush(Colors.Black);
            this.BorderThickness = new Thickness(0);
            this.SetValue(Canvas.LeftProperty, Position.X); // Position
            this.SetValue(Canvas.TopProperty, Position.Y);
            this.ToolTip = this.ComponentName; // Tooltip
            boardGrid.AddComponent(this); // Add the component

            this.MouseLeftButtonDown += Component_MouseLeftButtonDown; // Event handler to trigger selection or properties editing
            this.MouseLeftButtonUp += Component_MouseLeftButtonUp; // Event handler to trigger selection or properties editing
            this.MouseMove += Component_MouseMove; // Event handler to trigger drag&drop
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

        /// <summary>
        /// Event handler to trigger selection (single-click) or properties editing (double-click)
        /// </summary>
        private void Component_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.LastMouseLeftButtonDown = e.Timestamp; // Save the time of the event
            this.LastMouseLeftButtonUp = 0; // Clear the time of the last MouseLeftButtonUp event
        }

        /// <summary>
        /// Event handler to trigger selection (single-click) or properties editing (double-click)
        /// </summary>
        private void Component_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.LastMouseLeftButtonUp = e.Timestamp; // Save the time of the event
            if (LastMouseLeftButtonUp - LastMouseLeftButtonDown < Properties.Settings.Default.SingleClickMaxDuration) // Single-click
            {
                SwitchIsSelected();
            }
            this.LastMouseLeftButtonDown = 0; // Clear the time of the last MouseLeftButtonDown event
        }
        #endregion
    }
}
