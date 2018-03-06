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
    public class Component
    {
        #region Attributes

        [NonSerialized()] private BoardGrid BoardGrid; // The board on which is this component
        [NonSerialized()] private Image ComponentImage;
        private Uri ImageSource; // The Uri to the image source must be saved to reload the image after deserialization
        [NonSerialized()] private Border _GraphicalComponent; // The Border used to mark selection and which contains the Image accessed with the GraphicalComponent property
        private Point _ComponentPosition; // The position of the component with its border on the grid, accessed through the ComponentPosition property
        public Vector ImageSize { get; set; } // The displayed image size as a Vector
        private double Scale; // The scaling factor applied to the image
        public List<Vector> Anchors { get; } = new List<Vector>(); // The vectors between the image origin and the component anchors
        public List<Node> ConnectedNodes { get; set; } = new List<Node>(); // The nodes to which that component is connected

        private string _Name; // The component name
        public Dictionary<string, double?> Attributes { get; set; } // The components attributes; the value is always in SI units and is nullable
        public Dictionary<string, Dictionary<string, double>> AttributesUnits { get; } // The available units for the components attributes; each unit is a couple symbol:multiplier 

        [NonSerialized()] private RotateTransform Rotation; // The rotation operation to apply
        private double _RotationAngle; // The angle of the rotation, necessary for deserialization, accessible through the RotationAngle property

        private bool IsSelected = false; // Is true when the component is selected
        private int LastMouseLeftButtonDown = 0; // Timestamp of last MouseLeftButtonDown event; 0 if already handled
        private int LastClick = 0; // Timestamp of the last full click
        #endregion


        #region Accessors/Mutators

        public Border GraphicalComponent { get { return _GraphicalComponent; } set { _GraphicalComponent = value; } }

        /// <summary>
        /// Return the point at the upper left of the image, border not included
        /// </summary>
        public Point ImagePosition 
        {
            get
            {
                return Position + new Vector(GraphicalComponent.BorderThickness.Left, GraphicalComponent.BorderThickness.Top);
            }
        }

        public Point Position
        {
            get
            {
                return _ComponentPosition;
            }
            set
            {
                Canvas.SetLeft(GraphicalComponent, value.X); // Position
                Canvas.SetTop(GraphicalComponent, value.Y);
                _ComponentPosition = value;
                ConnectToNodes();
            }
        }

        public double RotationAngle
        {
            get
            {
                return _RotationAngle;
            }
            set
            {
                Rotation.Angle = value;
                if (Rotation.Angle < 0) // Angles are 360-periodic
                {
                    Rotation.Angle += 360;
                }
                else if (Rotation.Angle > 360)
                {
                    Rotation.Angle -= 360;
                }
                Rotation.CenterX = ImageSize.X / 2 + GraphicalComponent.BorderThickness.Left; // To make the component rotate around its center
                Rotation.CenterY = ImageSize.Y / 2 + GraphicalComponent.BorderThickness.Top;

                TransformAnchorsAfterRotation(value - _RotationAngle); // Apply the rotation to the anchors
                _RotationAngle = value;
                ConnectToNodes(); // Reconnect anchors
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                GraphicalComponent.ToolTip = value; // Update the tooltip
            }
        }
        #endregion


        #region Constructor

        /// <summary>
        /// Add one component to the board
        /// </summary>
        /// <param name="position">The component position</param>
        /// <param name="boardGrid">The canvas on which to display the component</param>
        /// <param name="xmlElement">The XML Element with the component data</param>
        public Component(Point position, BoardGrid boardGrid, XElement xmlElement)
        {
            // Save parameters
            BoardGrid = boardGrid as BoardGrid;
            _Name = xmlElement.Element("name").Value;

            // Display the image
            ImageSource = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, Properties.Settings.Default.ResourcesFolder, xmlElement.Element("image").Value));
            ComponentImage = new Image
            {
                Source = new BitmapImage(ImageSource) // Image to display
            };

            GraphicalComponent = new Border
            {
                Width = 2 * BoardGrid.GridSpacing + 2 * BoardGrid.GridThickness, // The component covers 2 grid cells
                Height = 2 * BoardGrid.GridSpacing + 2 * BoardGrid.GridThickness,
                Child = ComponentImage,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0),
                ToolTip = Name
            };
            ImageSize = new Vector(GraphicalComponent.Width, GraphicalComponent.Height);
            Scale = GraphicalComponent.Width / (double)xmlElement.Element("width");

            Position = position;
            // Add the component to the BoardGrid
            BoardGrid.ComponentsOnBoard.Add(this);
            BoardGrid.Children.Add(GraphicalComponent);

            // Rotation
            Rotation = new RotateTransform(0);
            GraphicalComponent.RenderTransform = Rotation;

            // Anchors
            IEnumerable<XElement> xmlAnchors = xmlElement.Element("anchors").Elements("anchor"); // Get all the anchors present in the XML
            foreach (XElement xmlAnchor in xmlAnchors)
            {
                // Parse each anchor's position
                Vector anchor = new Vector
                {
                    X = (double)xmlAnchor.Element("posX") * Scale + GraphicalComponent.BorderThickness.Left,
                    Y = (double)xmlAnchor.Element("posY") * Scale + GraphicalComponent.BorderThickness.Top
                };
                anchor.X = Math.Round(anchor.X, 0); // Round the anchor composants
                anchor.Y = Math.Round(anchor.Y, 0);
                Anchors.Add(anchor);
            }

            // Attributes
            Attributes = new Dictionary<string, double?>();
            AttributesUnits = new Dictionary<string, Dictionary<string, double>>();
            if (xmlElement.Element("attributes") != null) // Check if there are attributes
            {
                IEnumerable<XElement> xmlAttributes = xmlElement.Element("attributes").Elements("attribute"); // Get all the attributes present in the XML
                foreach (XElement xmlAttribute in xmlAttributes)// Parse each attribute's name and available units
                {
                    Attributes.Add((string)xmlAttribute.Element("name"), null); // The value is undefined for now
                    AttributesUnits.Add((string)xmlAttribute.Element("name"), new Dictionary<string, double>());

                    foreach (XElement xmlUnit in xmlAttribute.Element("units").Elements("unit"))
                    {
                        AttributesUnits[(string)xmlAttribute.Element("name")].Add((string)xmlUnit.Element("symbol"), (double)xmlUnit.Element("value"));
                    }
                }
            }

            // Event handlers
            GraphicalComponent.MouseLeftButtonDown += Component_MouseLeftButtonDown; // Event handler to trigger selection or properties editing
            GraphicalComponent.MouseLeftButtonUp += Component_MouseLeftButtonUp; // Event handler to trigger selection or properties editing
            GraphicalComponent.MouseMove += Component_MouseMove; // Event handler to trigger drag&drop
        }
        #endregion


        #region Methods

        /// <summary>
        /// Rotate the component by 90 degrees counterclockwise and reconnects anchors to nodes
        /// </summary>
        public void RotateLeft()
        {
            RotationAngle -= 90;
        }

        /// <summary>
        /// Rotate the component by 90 degrees clockwise and reconnects anchors to nodes
        /// </summary>
        public void RotateRight()
        {
            RotationAngle += 90;
        }

        /// <summary>
        /// Compute new values for anchor vectors after a rotation of a given angle
        /// </summary>
        private void TransformAnchorsAfterRotation(double rotation)
        {
            rotation *= Math.PI / 180; // Convert the angle to radians
            Vector center = ImageSize / 2;
            Vector tmpVector = new Vector();

            for (int i = 0; i < Anchors.Count(); i++) // Apply the transformation to each anchor
            {
                Vector vectorToRotate = Anchors[i] - center; // Rotation around the center, so we change the origin
                
                // Apply the rotation matrix
                tmpVector.X = Math.Cos(rotation) * vectorToRotate.X - Math.Sin(rotation) * vectorToRotate.Y;
                tmpVector.Y = Math.Sin(rotation) * vectorToRotate.X + Math.Cos(rotation) * vectorToRotate.Y;
                tmpVector += center; // Change origin again
                tmpVector.X = Math.Round(tmpVector.X, 0);
                tmpVector.Y = Math.Round(tmpVector.Y, 0);
                Anchors[i] = tmpVector;
            }
        }

        /// <summary>
        /// Update the rotation after the component has been resized
        /// </summary>
        public void SizeUpdated()
        {
            // Reapply the rotation with the new center coordinates
            Rotation.CenterX = ImageSize.X / 2 + GraphicalComponent.BorderThickness.Left;
            Rotation.CenterY = ImageSize.Y / 2 + GraphicalComponent.BorderThickness.Top;
        }

        /// <summary>
        /// If the mouse moves over a component on the board and the left mouse button is pressed, the component is dragged
        /// </summary>
        private void Component_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && BoardGrid.DraggingWire == false) // Drag only if the left button is pressed and the user is not dragging a wire
            {
                List<Wire> connectedWires = new List<Wire>(); // All the wires connected to this component
                List<Node> connectedNodes = new List<Node>(); // The node by which every wire is connected to this component
                List<Vector> connectedVectorsOffsets = new List<Vector>(); // The vectors between the image center and the  node to which every wire is connected
                Vector imageCenter = (Vector)ImagePosition + ImageSize / 2; // The center of the image
                foreach (Node node in ConnectedNodes)
                {
                    foreach (object element in node.ConnectedElements.Keys)
                    {
                        if (element is Wire)
                        {
                            connectedWires.Add((Wire)element);
                            connectedNodes.Add(node);
                            connectedVectorsOffsets.Add((Vector)node.Position - imageCenter);
                        }
                    }
                }

                // Initializes the WireDraggers to drag all connected wires
                BoardGrid.CurrentWireDraggers = new List<WireDragger>();
                for (int i = 0; i < connectedWires.Count; i++)
                {
                    BoardGrid.CurrentWireDraggers.Add(new WireDragger(BoardGrid, connectedWires[i], connectedNodes[i], connectedVectorsOffsets[i]));
                }
                BoardGrid.DragConnectedWires();

                ClearNodes(); // Disconnect all anchors from their node
                DragDrop.DoDragDrop((Border)sender, (Component)this, DragDropEffects.Move); // Begin the drag&drop
            }
        }

        /// <summary>
        /// Connect all anchors to the nearest nodes
        /// </summary>
        private void ConnectToNodes()
        {
            ClearNodes(); // Clear previous connections
            foreach (Vector anchor in Anchors)
            {
                Node node = BoardGrid.Magnetize(ImagePosition + anchor); // The nearest node

                Vector nodeRelativePosition = node.Position - ImagePosition; // Node position relative to the image
                Directions direction = new Directions(); // Direction of the component relative to the node
                try
                {
                    if (Math.Abs(ImageSize.X - nodeRelativePosition.X) < Properties.Settings.Default.GridThickness) // The grid thickness is used as an error threshold
                    {
                        direction = Directions.Left;
                    }
                    else if (Math.Abs(ImageSize.Y - nodeRelativePosition.Y) < Properties.Settings.Default.GridThickness)
                    {
                        direction = Directions.Up;
                    }
                    else if (Math.Abs(nodeRelativePosition.Y) < Properties.Settings.Default.GridThickness)
                    {
                        direction = Directions.Down;
                    }
                    else if (Math.Abs(nodeRelativePosition.X) < Properties.Settings.Default.GridThickness)
                    {
                        direction = Directions.Right;
                    }
                    else
                    {
                        throw new System.ApplicationException($"Can't determine anchor position relatively to the node. Position relative to the canvas : {node.Position}. Canvas size : {BoardGrid.ActualWidth};{BoardGrid.ActualHeight}.");
                    }
                }
                catch (System.ApplicationException e)
                {
                    ((MainWindow)Application.Current.MainWindow).LogError(e); // Write error to log and close the processus
                }
                ConnectedNodes.Add(node);
                node.ConnectedElements.Add(this, direction);
            }
        }

        /// <summary>
        /// Remove this object from all connected anchors
        /// </summary>
        public void ClearNodes()
        {
            foreach (Node node in ConnectedNodes)
            {
                node.ConnectedElements.Remove(this); // Remove the anchor from the node
            }
            ConnectedNodes.Clear();
        }

        /// <summary>
        /// Event handler to trigger selection (single-click) or properties editing (double-click)
        /// </summary>
        private void Component_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LastMouseLeftButtonDown = e.Timestamp; // Save the time of the event
        }

        /// <summary>
        /// Event handler to trigger selection (single-click) or properties editing (double-click)
        /// </summary>
        private void Component_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Timestamp - LastClick < Properties.Settings.Default.DoubleClickMaxDuration && e.Timestamp - LastClick > Properties.Settings.Default.ClicksMinimumInterval) // Double-click
            {
                BoardGrid.DialogContent.Display(this); // A double-click opens the attributes dialog
                SwitchIsSelected(); // Revert the action triggered by the first click
                LastClick = e.Timestamp; // The click timestamp is saved
            }
            else if (e.Timestamp - LastMouseLeftButtonDown < Properties.Settings.Default.SingleClickMaxDuration) // Single-click
            {
                SwitchIsSelected();
                LastClick = e.Timestamp; // The click timestamp is saved
            }
        }

        /// <summary>
        /// Select or deselect the component
        /// </summary>
        public void SetIsSelected(bool isSelected)
        {
            if (isSelected != IsSelected) // Check whether the selected state has changed
            {
                IsSelected = isSelected;
                double thickness = Properties.Settings.Default.ComponentBorderThickness;
                if (isSelected) // The component is selected
                {
                    GraphicalComponent.BorderThickness = new Thickness(thickness);

                    // Adjust the component size and position to avoid image resizing
                    GraphicalComponent.Width += thickness * 2;
                    GraphicalComponent.Height += thickness * 2;
                    Position = new Point(Position.X - thickness, Position.Y - thickness);
                }
                else // The component is deselected
                {
                    GraphicalComponent.BorderThickness = new Thickness(0);

                    // Adjust the component size and position to avoid image resizing
                    GraphicalComponent.Width -= thickness * 2;
                    GraphicalComponent.Height -= thickness * 2;
                    Position = new Point(Position.X + thickness, Position.Y + thickness);
                }
                SizeUpdated();
            }
        }

        /// <summary>
        /// Change the selection status of the component
        /// </summary>
        public void SwitchIsSelected()
        {
            if (IsSelected) // The component is deselected
            {
                BoardGrid.SelectedElement = null;
            }
            else // The component is selected
            {
                BoardGrid.SelectedElement = this;
            }
        }

        /// <summary>
        /// Recreate non-serializable elements after deserialization
        /// </summary>
        public void Deserialized(BoardGrid boardGrid)
        {
            BoardGrid = boardGrid;

            ComponentImage = new Image
            {
                Source = new BitmapImage(ImageSource) // Image to display
            };

            GraphicalComponent = new Border
            {
                Width = 2 * BoardGrid.GridSpacing + 2 * BoardGrid.GridThickness, // The component covers 2 grid cells
                Height = 2 * BoardGrid.GridSpacing + 2 * BoardGrid.GridThickness,
                Child = ComponentImage,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0),
                ToolTip = Name
            };

            Position = Position; // Set the position of the graphical element and connect it to nodes

            // Add the component to the BoardGrid
            BoardGrid.Children.Add(GraphicalComponent);

            // Rotation
            Rotation = new RotateTransform();
            GraphicalComponent.RenderTransform = Rotation;
            RotationAngle = RotationAngle; // To compute the rotation center
        }
        #endregion
    }
}
