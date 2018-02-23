using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        
        private double GridSpacing; // The distance between two lines or columns
        public double GridThickness { get; set; } // The lines thickness
        private List<Rectangle> Lines; // The lines of the grid
        private List<Rectangle> Columns; // The columns of the grid
        private List<Component> ComponentsOnBoard; // The list of components on the board
        private List<Wire> WiresOnBoard = new List<Wire>(); // The list of wires on the board
        private List<List<Node>> Nodes;
        private object _SelectedElement; // The element currently selected
        public ComponentDialog Dialog { get; } // The dialog to edit a component attributes
        public List<WireDragger> CurrentWireDraggers { get; set; } = new List<WireDragger>(); // The WireDraggers to drag every wire connected to a component along with it

        public bool AddingWire { get; set; } = false; // Whether we are in "adding wire mode"
        public bool AddingMultipleWires { get; set; } = false; // Whether we are in "adding multiple wires mode"
        public bool DraggingWire { get; set; } = false; // Whether we are dragging wires
        #endregion


        #region Accessors/Mutators

        public double GetGridSpacing() => this.GridSpacing;
        public void SetGridSpacing(int spacing) => this.GridSpacing = spacing;

        public double GetGridThickness() => this.GridThickness;
        public void SetGridThickness(int thickness) => this.GridThickness = thickness;

        public List<List<Node>> GetNodes() => this.Nodes;

        public object SelectedElement
        {
            get
            {
                return _SelectedElement;
            }
            set
            {
                if (_SelectedElement == null) // If no element was previously selected
                {
                    if (value != null)
                    {
                        ((MainWindow)Application.Current.MainWindow).DeleteButton.IsEnabled = true; // Enable the delete button
                    }
                    if (value is Component)
                    {
                        ((MainWindow)Application.Current.MainWindow).RotateLeftButton.IsEnabled = true; // Enable both rotation buttons
                        ((MainWindow)Application.Current.MainWindow).RotateRightButton.IsEnabled = true;
                    }
                }
                else if (_SelectedElement is Component) // If a component was previsouly selected
                {
                    ((Component)_SelectedElement).SetIsSelected(false);
                    if (value == null)
                    {
                        ((MainWindow)Application.Current.MainWindow).RotateLeftButton.IsEnabled = false; // Disable both rotation buttons
                        ((MainWindow)Application.Current.MainWindow).RotateRightButton.IsEnabled = false;
                        ((MainWindow)Application.Current.MainWindow).DeleteButton.IsEnabled = false; // Disable the delete button

                    }
                    else if (value is Wire)
                    {
                        ((MainWindow)Application.Current.MainWindow).RotateLeftButton.IsEnabled = false; // Disable both rotation buttons
                        ((MainWindow)Application.Current.MainWindow).RotateRightButton.IsEnabled = false;
                    }
                }
                else if (_SelectedElement is Wire) // If a wire was previsouly selected
                {
                    ((Wire)_SelectedElement).SetIsSelected(false);
                    if (value == null)
                    {
                        ((MainWindow)Application.Current.MainWindow).DeleteButton.IsEnabled = false; // Disable the delete button

                    }
                    else if (value is Component)
                    {
                        ((MainWindow)Application.Current.MainWindow).RotateLeftButton.IsEnabled = true; // Enable both rotation buttons
                        ((MainWindow)Application.Current.MainWindow).RotateRightButton.IsEnabled = true;
                    }
                }

                // Notify to the new selected element that it is selected
                if (value is Component)
                {
                    ((Component)value).SetIsSelected(true);
                }
                else if (value is Wire)
                {
                    ((Wire)value).SetIsSelected(true);
                }

                _SelectedElement = value;
            }
        }
        #endregion


        #region Constructor

        /// <summary>
        /// The canvas on which components are displayed
        /// </summary>
        public BoardGrid()
        {
            this.Loaded += BoardGrid_Loaded; // Draw the grid a first time after initialization

            // Initialization of attributes
            this.GridSpacing = Properties.Settings.Default.GridSpacing;
            this.GridThickness = Properties.Settings.Default.GridThickness;
            this.Lines = new List<Rectangle>();
            this.Columns = new List<Rectangle>();
            this.ComponentsOnBoard = new List<Component>();
            this.Nodes = new List<List<Node>>();
            this.Dialog = new ComponentDialog();

            this.Background = new SolidColorBrush { Opacity = 0 }; // A background is required to enable mouse events
            this.ClipToBounds = true; // To have the (0, 0) point in the top left corner even with components with negative positions

            // Enable drag&drop
            this.AllowDrop = true; // Components can be dropped on the board
            this.DragEnter += BoardGrid_DragEnter; // Event handler called when a dragged component enters the board
            this.DragOver += BoardGrid_DragOver; // Event handler continusously called while dragging
            this.Drop += BoardGrid_Drop; // Event handler called when a component is dropped
            this.DragLeave += BoardGrid_DragLeave; // Event handler called when a dragged component leaves the board

            // Event handlers
            this.MouseLeftButtonDown += BoardGrid_MouseLeftButtonDown; // Event handler called when left-clicking
            this.MouseLeftButtonUp += BoardGrid_MouseLeftButtonUp; // Event handler called when releasing the mouse left button
            this.MouseMove += BoardGrid_MouseMove; // Event handler called when the mouse moves
            this.SizeChanged += BoardGrid_SizeChanged; // Event handler called when the board is resized
        }
        #endregion


        #region Methods

        /// <summary>
        /// Update the grid and draw it
        /// </summary>
        public void DrawGrid()
        {
            double gridTotalSpacing = GridSpacing + GridThickness;

            // Generation of new nodes if the board has extended 
            for (int y = 0; y < this.ActualHeight / gridTotalSpacing + 2; y++)
            { 
                if (Nodes.Count < y + 1) // Add a line if necessary
                {
                    Nodes.Add(new List<Node>());
                }
                for (int x = Nodes[y].Count; x < this.ActualWidth / gridTotalSpacing + 1; x++)
                {
                    // Create the nodes on that line
                    Point nodePosition = new Point(x * gridTotalSpacing, y * gridTotalSpacing);
                    Node tmp = new Node(nodePosition, this);
                    Nodes[y].Add(tmp);
                }
            }

            foreach (Rectangle child in this.Lines) // Clear the former grid
            {
                this.Children.Remove(child);
            }
            foreach (Rectangle child in this.Columns) // Clear the former grid
            {
                this.Children.Remove(child);
            }
            Lines.Clear();
            Columns.Clear();

            // Generation of new lines and columns
            for (int y = 0; y < this.ActualHeight / gridTotalSpacing + 2; y++)
            {
                Rectangle line = new Rectangle
                {
                    Height = GridThickness,
                    Width = this.ActualWidth,
                    Fill = new SolidColorBrush(Colors.Gray)
                };
                Canvas.SetTop(line, y * gridTotalSpacing - GridThickness / 2); // Position
                Canvas.SetZIndex(line, -99); // The grid must always be in the background
                Lines.Add(line);
                this.Children.Add(line); // Display the line
            }
            for (int x = 0; x < this.ActualWidth / gridTotalSpacing + 2; x++)
            {
                Rectangle column = new Rectangle
                {
                    Height = this.ActualHeight,
                    Width = GridThickness,
                    Fill = new SolidColorBrush(Colors.Gray)
                };
                Canvas.SetLeft(column, x * gridTotalSpacing - GridThickness / 2); // Position
                Canvas.SetZIndex(column, -99); // The grid must always be in the background
                Columns.Add(column);
                this.Children.Add(column); // Display the column
            }
        }

        /// <summary>
        /// Returns the nearest grid node of the given Point
        /// </summary>
        public Node Magnetize(Point point)
        {
            int X = (int)point.X;
            int Y = (int)point.Y;
            double gridTotalSpacing = GridSpacing + GridThickness;
            int nearestX = 0; // X index of the nearest node
            int nearestY = 0; // Y index of the nearest node
            if (Math.Abs(X % gridTotalSpacing) < Math.Abs(gridTotalSpacing - X % gridTotalSpacing)) // If the nearest line is the upper one
            {
                nearestX = (int)(X / gridTotalSpacing);
            }
            else // If the nearest line is the lower one
            {
                nearestX = ((int)(X / gridTotalSpacing) + 1);
            }
            if (Math.Abs(Y % gridTotalSpacing) < Math.Abs(gridTotalSpacing - Y % gridTotalSpacing)) // If the nearest column is the left one
            {
                nearestY = (int)(Y / gridTotalSpacing);
            }
            else // If the nearest column is the right one
            {
                nearestY = ((int)(Y / gridTotalSpacing) + 1);
            }
            return Nodes[nearestY][nearestX];
        }

        /// <summary>
        /// Add a component on the board
        /// </summary>
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

        /// <summary>
        /// Delete the currently selected object
        /// </summary>
        public void DeleteSelected()
        {
            // Delete the selected component
            if (SelectedElement != null)
            {
                if (SelectedElement is Component)
                {
                    ((Component)SelectedElement).ClearNodes();
                    this.ComponentsOnBoard.Remove((Component)SelectedElement);
                    this.Children.Remove((Component)SelectedElement);
                }
                else if (SelectedElement is Wire)
                {
                    RemoveWire((Wire)SelectedElement);
                }
                SelectedElement = null;
            }
        }

        /// <summary>
        /// Remove a wire of the board
        /// </summary>
        public void RemoveWire(Wire wire)
        {
            wire.ClearNodes();
            this.WiresOnBoard.Remove(wire);
            foreach (Rectangle rectangle in wire.Rectangles)
            {
                this.Children.Remove(rectangle);
            }
        }

        /// <summary>
        /// Draw the grid once the component is loaded 
        /// </summary>
        private void BoardGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
        }

        /// <summary>
        /// Event handler called when the board is resized
        /// </summary>
        private void BoardGrid_SizeChanged(object sender, SizeChangedEventArgs e)
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
            Point mousePos = e.GetPosition(this); // Position of the mouse relative to the board
            Vector firstAnchor = component.GetAnchors()[0]; // One anchor must be superposed with a node
            Node gridNode = Magnetize(mousePos - component.GetImageSize() / 2 + firstAnchor); // The nearest grid node from the anchor
            Point componentNewPos = gridNode.GetPosition() - firstAnchor; // New position of the image relative to the board

            // Check whether every anchor is inside the canvas; it must not be farther from the border than GridThickness
            bool anchorsInsideCanvas = true;
            foreach (Vector anchor in component.GetAnchors())
            {
                if(componentNewPos.X + anchor.X < -GridThickness || componentNewPos.Y + anchor.Y < -GridThickness || componentNewPos.X + anchor.X > this.ActualWidth + GridThickness || componentNewPos.Y + anchor.Y > this.ActualHeight + GridThickness)
                {
                    anchorsInsideCanvas = false;
                    break;
                }
            }

            // If the new position is valid, move the component
            if (anchorsInsideCanvas)
            {
                Vector thickness = new Vector(component.BorderThickness.Left, component.BorderThickness.Top);
                component.SetComponentPosition(componentNewPos - thickness);
            }

            // Dragging connected wires
            if (DraggingWire)
            {
                foreach (WireDragger dragger in CurrentWireDraggers)
                {
                    dragger.DragOver(e.GetPosition(this));
                }
            }
        }

        /// <summary>
        /// Drag all connected wires along with the component
        /// </summary>
        public void DragConnectedWires()
        {
            if (!DraggingWire) // If the dragging begins
            {
                DraggingWire = true;
            }
        }

        /// <summary>
        /// Event handler called when a component is dropped
        /// </summary>
        private void BoardGrid_Drop(object sender, DragEventArgs e)
        {
            Component component = e.Data.GetData(typeof(Component)) as Component; // The dragged component
            Point mousePos = e.GetPosition(this); // Position of the mouse relative to the board
            Vector firstAnchor = component.GetAnchors()[0]; // One anchor must be superposed with a node
            Node gridNode = Magnetize(mousePos - component.GetImageSize() / 2 + firstAnchor); // The nearest grid node from the anchor
            Point componentNewPos = gridNode.GetPosition() - firstAnchor; // New position of the image relative to the board

            // Check whether every anchor is inside the canvas
            bool anchorsInsideCanvas = true;
            foreach (Vector anchor in component.GetAnchors())
            {
                if (componentNewPos.X + anchor.X < 0 || componentNewPos.Y + anchor.Y < 0 || componentNewPos.X + anchor.X > this.ActualWidth || componentNewPos.Y + anchor.Y > this.ActualHeight)
                {
                    anchorsInsideCanvas = false;
                    break;
                }
            }

            // If the new position is valid, move the component
            if (anchorsInsideCanvas)
            {
                Vector thickness = new Vector(component.BorderThickness.Left, component.BorderThickness.Top);
                Vector gridThickness = new Vector(GridThickness, GridThickness);
                component.SetComponentPosition(componentNewPos - thickness);
            }
            // Stopping to drag the connected wires
            if (DraggingWire)
            {
                Wire_EndDrag();
            }
        }

        /// <summary>
        /// Event handler called when a dragged component leaves the board
        /// </summary>
        private void BoardGrid_DragLeave(object sender, DragEventArgs e)
        {
            Component component = e.Data.GetData(typeof(Component)) as Component; // The dragged component
        }

        /// <summary>
        /// Event handler called when the mouse left button is pressed
        /// </summary>
        private void BoardGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.AddingWire)
            {
                Wire wire = new Wire(this, Magnetize(e.GetPosition(this)));
                this.WiresOnBoard.Add(wire);
                this.DraggingWire = true;
                if (!AddingMultipleWires)
                {
                    this.AddingWire = false;
                    ((MainWindow)Application.Current.MainWindow).WireModeButton.IsChecked = false;
                    ((MainWindow)Application.Current.MainWindow).MultipleWiresModeCheckBox.IsEnabled = false;
                }
                this.CurrentWireDraggers.Add(new WireDragger(this, wire, Magnetize(e.GetPosition(this)), new Vector(0, 0))); // New WireDragger centered on the mouse
            }
        }

        /// <summary>
        /// Event handler called when the mouse left button is released
        /// </summary>
        private void BoardGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DraggingWire)
            {
                Wire_EndDrag(); // End the dragging
            }
        }

        /// <summary>
        /// Event handler called when the mouse left button is released
        /// </summary>
        private void BoardGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.DraggingWire)
            {
                foreach (WireDragger wireDragger in this.CurrentWireDraggers)
                {
                    wireDragger.DragOver(e.GetPosition(this));
                }
            }
            // Open the left drawer to select components when the mouse is on the left, otherwise close it
            else
            {
                ((MainWindow)Application.Current.MainWindow).Drawer.IsLeftDrawerOpen = (e.GetPosition(this).X < ((MainWindow)Application.Current.MainWindow).ComponentsPanel.DesiredSize.Width);
            }
        }

        /// <summary>
        /// Event handler called when a wire dragging is finished
        /// </summary>
        private void Wire_EndDrag()
        {
            foreach (WireDragger wireDragger in this.CurrentWireDraggers)
            {
                wireDragger.EndDrag();
            }
            CurrentWireDraggers.Clear();
            if (!this.AddingMultipleWires)
            {
                this.DraggingWire = false;
            }
        }
        #endregion
    }
}
