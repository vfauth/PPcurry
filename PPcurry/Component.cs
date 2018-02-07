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
        private Point ImagePosition; // The position of the image on the grid
        private Point ComponentPosition; // The position of the component with its border on the grid
        private Vector ImageSize; // The displayed image size as a Vector
        private Vector ComponentSize; // The size of the component and its border as a Vector
        private List<Vector> Anchors = new List<Vector>(); // The vectors between the image origin and the component anchors
        private double Scale; // The scaling factor applied to the image
        private string ComponentName; // The component name

        private RotateTransform Rotation; // To rotation operation to apply

        private bool IsSelected; // Is true when the component is selected
        private int LastMouseLeftButtonDown; // Timestamp of last MouseLeftButtonDown event; 0 if already handled
        private int LastMouseLeftButtonUp; // Timestamp of last MouseLeftButtonUp event; 0 if already handled
        #endregion


        #region Accessors/Mutators

        public List<Vector> GetAnchors() => this.Anchors;

        public Vector GetImageSize()
        {
            UpdateSize();
            return this.ImageSize;
        }

        public Vector GetComponentSize()
        {
            UpdateSize();
            return this.ComponentSize;
        }

        public Point GetImagePosition()
        {
            UpdatePosition();
            return this.ImagePosition;
        }
        public void SetComponentPosition(Point position)
        {
            Canvas.SetLeft(this, position.X); // Position
            Canvas.SetTop(this, position.Y);

            UpdatePosition(); // Update the image position
        }

        public Point GetComponentPosition()
        {
            UpdatePosition();
            return this.ComponentPosition;
        }

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
                double thickness = Properties.Settings.Default.ComponentBorderThickness;
                if (isSelected)
                {
                    this.BoardGrid.SetSelectedComponent(this);
                    this.BorderThickness = new Thickness(thickness);

                    // Adjust the component size and position to avoid image resizing
                    this.Width += thickness * 2;
                    this.Height += thickness * 2;
                    this.SetComponentPosition(new Point(ComponentPosition.X - thickness, ComponentPosition.Y - thickness));
                }
                else
                {
                    this.BoardGrid.SetSelectedComponent(null);
                    this.BorderThickness = new Thickness(0);

                    // Adjust the component size and position to avoid image resizing
                    this.Width -= thickness * 2;
                    this.Height -= thickness * 2;
                    this.SetComponentPosition(new Point(ComponentPosition.X + thickness, ComponentPosition.Y + thickness));
                }
                UpdateSize();
                UpdatePosition();
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
            this.ImagePosition = position;
            this.ComponentName = xmlElement.Element("name").Value;

            // Size
            this.Width = 2 * BoardGrid.GetGridSpacing() + 3 * BoardGrid.GetGridThickness(); // The component covers 2 grid cells
            this.Height = 2 * BoardGrid.GetGridSpacing() + 3 * BoardGrid.GetGridThickness(); // The component covers 2 grid cells
            this.ImageSize = new Vector(this.Width, this.Height);
            this.ComponentSize = new Vector(this.Width, this.Height);
            this.Scale = this.Width / (double)xmlElement.Element("width");

            // Display the image
            Uri imageUri = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, Properties.Settings.Default.ResourcesFolder, xmlElement.Element("image").Value));
            this.ComponentImage = new Image
            {
                Source = new BitmapImage(imageUri) // Image to display
            };
            this.Child = ComponentImage;

            this.BorderBrush = new SolidColorBrush(Colors.Black);
            this.BorderThickness = new Thickness(0);
            this.ImagePosition = position;
            this.ComponentPosition = position;
            this.ToolTip = this.ComponentName; // Tooltip
            boardGrid.AddComponent(this); // Add the component

            // Rotation
            Rotation = new RotateTransform(0)
            {
                CenterX = this.Width / 2 + Properties.Settings.Default.ComponentBorderThickness, // To make the component rotate around its center
                CenterY = this.Height / 2 + Properties.Settings.Default.ComponentBorderThickness
            };
            this.RenderTransform = Rotation;

            // Anchors
            IEnumerable<XElement> xmlAnchors = xmlElement.Element("anchors").Elements("anchor"); // Get all the anchors present in the XML
            foreach (XElement xmlAnchor in xmlAnchors)
            {
                // Parse each anchor's position
                Vector anchor = new Vector
                {
                    X = (double)xmlAnchor.Element("posX") * this.Scale + this.BorderThickness.Left,
                    Y = (double)xmlAnchor.Element("posY") * this.Scale + this.BorderThickness.Top
                };
                anchor.X = Math.Round(anchor.X, 0); // Round the anchor composants
                anchor.Y = Math.Round(anchor.Y, 0);
                this.Anchors.Add(anchor);
            }

            this.MouseLeftButtonDown += Component_MouseLeftButtonDown; // Event handler to trigger selection or properties editing
            this.MouseLeftButtonUp += Component_MouseLeftButtonUp; // Event handler to trigger selection or properties editing
            this.MouseMove += Component_MouseMove; // Event handler to trigger drag&drop
        }
        #endregion


        #region Methods

        /// <summary>
        /// Rotate the component by 90 degrees counterclockwise and reconnects anchors to nodes
        /// </summary>
        public void RotateLeft()
        {
            this.Rotation.Angle -= 90;
            if (this.Rotation.Angle < 0) // Angles are 360-periodic
            {
                this.Rotation.Angle += 360;
            }
            this.RenderTransform = this.Rotation;
            TransformAnchorsAfterRotation(-90);
            ConnectAnchors(); // Reconnect anchors
        }

        /// <summary>
        /// Rotate the component by 90 degrees clockwise and reconnects anchors to nodes
        /// </summary>
        public void RotateRight()
        {
            this.Rotation.Angle += 90;
            if (this.Rotation.Angle >= 360) // Angles are 360-periodic
            {
                this.Rotation.Angle -= 360;
            }
            this.RenderTransform = this.Rotation;
            TransformAnchorsAfterRotation(90);
            ConnectAnchors(); // Reconnect anchors
        }

        /// <summary>
        /// Compute new values for anchor vectors after a rotation of a given angle
        /// </summary>
        private void TransformAnchorsAfterRotation(double rotation)
        {
            rotation *= Math.PI / 180; // Convert the angle to radians
            Vector center = this.ImageSize / 2;
            Vector tmpVector = new Vector();

            for (int i = 0; i < this.Anchors.Count(); i++) // Apply the transformation to each anchor
            {
                Vector vectorToRotate = Anchors[i] - center; // Rotation around the center, so we change the origin
                
                // Apply the rotation matrix
                tmpVector.X = Math.Cos(rotation) * vectorToRotate.X - Math.Sin(rotation) * vectorToRotate.Y;
                tmpVector.Y = Math.Sin(rotation) * vectorToRotate.X + Math.Cos(rotation) * vectorToRotate.Y;
                tmpVector += center; // Change origin again
                tmpVector.X = Math.Round(tmpVector.X, 0);
                tmpVector.Y = Math.Round(tmpVector.Y, 0);
                this.Anchors[i] = tmpVector;
            }
        }

        /// <summary>
        /// Updates the position attribute after the component has been moved
        /// </summary>
        public void UpdatePosition()
        {
            this.ImagePosition.X = (double)Canvas.GetLeft(this) + this.BorderThickness.Left;
            this.ImagePosition.Y = (double)Canvas.GetTop(this) + this.BorderThickness.Top;
            this.ComponentPosition.X = (double)Canvas.GetLeft(this);
            this.ComponentPosition.Y = (double)Canvas.GetTop(this);
        }

        /// <summary>
        /// Updates the size attribute after the component has been resized
        /// </summary>
        public void UpdateSize()
        {
            this.ImageSize.X = this.Width - this.BorderThickness.Left - this.BorderThickness.Right;
            this.ImageSize.Y = this.Height - this.BorderThickness.Top - this.BorderThickness.Bottom;
            this.ComponentSize.X = this.Width;
            this.ComponentSize.Y = this.Height;
        }

        /// <summary>
        /// If the mouse moves over a component on the board and the left mouse button is pressed, the component is dragged
        /// </summary>
        public void Component_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) // Drag only if the left button is pressed
            {
                // Disconnect all anchors from their node
                foreach (Vector anchor in this.Anchors)
                {
                    this.BoardGrid.Magnetize(this.ImagePosition + anchor).ConnectedComponents.Remove(this);
                }
                DragDrop.DoDragDrop((Component)sender, (Component)sender, DragDropEffects.Move); // Begin the drag&drop
            }
        }

        /// <summary>
        /// Connect all anchors to the nearest nodes
        /// </summary>
        public void ConnectAnchors()
        {
            ClearAnchors(); // Clear previous connections
            foreach (Vector anchor in this.Anchors)
            {
                Node node = this.BoardGrid.Magnetize(this.ImagePosition + anchor); // The nearest node

                Vector nodeRelativePosition = node.GetPosition() - this.ImagePosition; // Node position relative to the image
                Directions direction = new Directions(); // Direction of the component relative to the node

                try
                {
                    if (Math.Abs(this.ImageSize.X - nodeRelativePosition.X) < Properties.Settings.Default.GridSpacing / 10) // The grid spacing is used as an error threshold
                    {
                        direction = Directions.Left;
                    }
                    else if (Math.Abs(this.ImageSize.Y - nodeRelativePosition.Y) < Properties.Settings.Default.GridSpacing / 10)
                    {
                        direction = Directions.Up;
                    }
                    else if (Math.Abs(nodeRelativePosition.Y) < Properties.Settings.Default.GridSpacing / 10)
                    {
                        direction = Directions.Down;
                    }
                    else if (Math.Abs(nodeRelativePosition.X) < Properties.Settings.Default.GridSpacing / 10)
                    {
                        direction = Directions.Right;
                    }
                    else
                    {
                        throw new System.ApplicationException("Can't determine anchor position relatively to the node.");
                    }
                }
                catch (System.ApplicationException e)
                {
                    ((MainWindow)Application.Current.MainWindow).LogError(e); // Write error to log
                }
                node.ConnectedComponents.Add(this, direction);
            }
        }

        /// <summary>
        /// Remove this object from all connected anchors
        /// </summary>
        public void ClearAnchors()
        {
            foreach (List<Node> lineNode in this.BoardGrid.GetNodes())
            {
                foreach (Node node in lineNode)
                {
                    if (node.ConnectedComponents.ContainsKey(this))
                    {
                        node.ConnectedComponents.Remove(this); // Remove the anchor from the node connected elements
                    }
                }
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
