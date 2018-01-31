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
        private Point Position; // The position of the component on the grid
        private Vector Size; // The component displayed size as a Vector
        private List<Vector> Anchors = new List<Vector>(); // The vectors between the image origin and the component anchors
        double Scale; // The scaling factor applied to the image
        private string Name; // The component name
        #endregion


        #region Accessors/Mutators

        public List<Vector> GetAnchors() => this.Anchors;

        public Vector GetSize() => this.Size;

        public Point GetPosition() => this.Position;
        public void SetPosition(Point position) => this.Position = position;

        public string GetName() => this.Name;
        public void SetName(string name)
        {
            this.Name = name;
            this.ToolTip = name; // Update the tooltip
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
            this.Grid = boardGrid as BoardGrid;
            this.Position = position;
            this.Name = xmlElement.Element("name").Value;

            // Size
            this.Width = 2*Grid.GetGridSpacing() + 3*Grid.GetGridThickness(); // The component covers 2 grid cells
            this.Height = 2*Grid.GetGridSpacing() + 3*Grid.GetGridThickness(); // The component covers 2 grid cells
            this.Size = new Vector(this.Width, this.Height);
            this.Scale = this.Width / (double)xmlElement.Element("width");

            // Anchors
            IEnumerable<XElement> xmlAnchors = xmlElement.Element("anchors").Elements("anchor"); // Get all the anchors present in the XML
            foreach (XElement xmlAnchor in xmlAnchors)
            {
                // Parse each anchor's position
                Vector anchor = new Vector
                {
                    X = (double)xmlElement.Element("anchors").Element("anchor").Element("posX") * this.Scale,
                    Y = (double)xmlElement.Element("anchors").Element("anchor").Element("posY") * this.Scale
                };
                this.Anchors.Add(anchor);
            }

            // Display the image
            Uri imageUri = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, Properties.Settings.Default.ResourcesFolder, xmlElement.Element("image").Value));
            this.Source = new BitmapImage(imageUri); // Image to display
            this.SetValue(Canvas.LeftProperty, Position.X); // Position
            this.SetValue(Canvas.TopProperty, Position.Y);
            this.ToolTip = this.Name; // Tooltip
            boardGrid.AddComponent(this); // Add the component

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
                // Disconnect all anchors from their node
                foreach (Vector anchor in this.Anchors)
                {
                    this.Grid.Magnetize(this.Position + anchor).ConnectedComponents.Remove(this);
                }
                DragDrop.DoDragDrop((Component)sender, (Component)sender, DragDropEffects.Move); // Begin the drag&drop
            }
        }

        /// <summary>
        /// Connect all anchors to the nearest nodes
        /// </summary>
        public void ConnectAnchors()
        {
            foreach (Vector anchor in this.Anchors)
            {
                Node node = this.Grid.Magnetize(this.Position + anchor); // The nearest node
                Directions direction = new Directions(); // Direction of the component relative to the node 

                Debug.WriteLine("TEST1");
                Debug.WriteLine(node.GetPosition());
                if ((this.Position + anchor + this.Size).X == node.GetPosition().X)
                {
                    direction = Directions.Left;
                }
                else if ((this.Position + anchor + this.Size).Y == node.GetPosition().Y)
                {
                    direction = Directions.Up;
                }
                else if ((this.Position + anchor).Y == node.GetPosition().Y)
                {
                    direction = Directions.Down;
                }
                else if ((this.Position + anchor).X == node.GetPosition().X)
                {
                    direction = Directions.Right;
                }
                Debug.WriteLine("TEST");
                Debug.WriteLine(direction);
                node.ConnectedComponents.Add(this, direction);
            }
        }
        #endregion
    }


}
