using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
using System.Xml.Linq;
using System.IO;
using System.Windows.Markup;
using System.Xml;

namespace PPcurry
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Attributes

        private BoardGrid BoardGrid; // The grid on the board
        private XElement XmlComponentsList; // The XML Element containing all the available components data
        #endregion


        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this; // Application.Current.MainWindow can return null sometimes, so we prevent it
        }
        #endregion


        #region Methods

        /// <summary>
        /// Load the components then display them in the panel at the left
        /// </summary>
        private void LoadComponents()
        {
            XmlComponentsList = XElement.Load(@"./Data/Components.xml"); // Load the XML file
            foreach (XElement element in XmlComponentsList.Elements())
            {
                Image newComponent = new Image
                {
                    Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, Properties.Settings.Default.ResourcesFolder, element.Element("image").Value))), // The image to display
                    Margin = new Thickness(10, 5, 5, 10), // The thickness around the image
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 2 * BoardGrid.GridSpacing + 2 * BoardGrid.GridThickness, // The component covers 2 grid cells
                    Height = 2 * BoardGrid.GridSpacing + 2 * BoardGrid.GridThickness // The component covers 2 grid cells
                };
                newComponent.MouseMove += ComponentInLeftPanel_MouseMove; // Event handler for component selection
                newComponent.ToolTip = element.Element("name").Value; // The name of the component appears on the tooltip
                newComponent.Tag = element.Element("type").Value; // The component is identified by the image tag
                
                ComponentsPanel.Children.Add(newComponent); // The component is added to the left panel
            }
        }

        /// <summary>
        /// Create the board once the windows is loaded to avoid some issues 
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BoardGrid = new BoardGrid();
            CanvasController.Content = BoardGrid;
            LoadComponents();
        }

        /// <summary>
        /// Write an error message to the log file and terminate the application
        /// </summary>
        public void LogError(System.Exception exception)
        {
            using (StreamWriter logFile = new StreamWriter("log.txt", true)) // The file to which append the text
            {
                logFile.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff ") + exception.Message);
            }
            Application.Current.Shutdown(); // Close the window
            Environment.Exit(0); // Terminate the process
        }

        /// <summary>
        /// If the mouse moves over a component in the left panel and the left mouse button is pressed, the component is dragged
        /// </summary>
        private void ComponentInLeftPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) // Drag only if the left button is pressed
            {

                Point relativePosition = e.GetPosition(this); // Position of the drop relative to the board
                relativePosition.X -= (2 * BoardGrid.GridSpacing + BoardGrid.GridThickness) / 2;
                relativePosition.Y -= (2 * BoardGrid.GridSpacing + BoardGrid.GridThickness) / 2;
                XElement xmlElement = XmlComponentsList.Element((string)((Image)sender).Tag); // Get the XML element with all the component data

                Component newComponent = new Component(relativePosition, BoardGrid, xmlElement); // Create the component
                newComponent.GraphicalComponent.Opacity = 0; // The component is only displayed once on the board
                
                DragDrop.DoDragDrop(newComponent.GraphicalComponent, newComponent, DragDropEffects.Move); // Begin the drag&drop
            }
        }

        /// <summary>
        /// Handler called when a key is released
        /// </summary>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.E:
                    RotateLeftButton_Click(sender, e);
                    break;
                case Key.R:
                    RotateRightButton_Click(sender, e);
                    break;
                case Key.W:
                    WireModeButton_Click(sender, e);
                    break;
                case Key.Delete:
                    DeleteButton_Click(sender, e);
                    break;
            }
        }

        /// <summary>
        /// Handler called when the new circuit button is clicked
        /// </summary>
        private void NewCircuitButton_Click(object sender, RoutedEventArgs e)
        {
            BoardGrid = new BoardGrid();
            CanvasController.Content = BoardGrid;

            // Reinitialize the buttons
            RotateLeftButton.IsEnabled = false;
            RotateRightButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            WireModeButton.IsChecked = false;
            MultipleWiresModeCheckBox.IsEnabled = false;
            MultipleWiresModeTextBlock.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handler called when the load button is clicked
        /// </summary>
        private void LoadCircuitButton_Click(object sender, RoutedEventArgs e)
        {
            // Open file "data.xml" and deserialize the object from it
            Stream stream = File.Open("data.xml", FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();

            NewCircuitButton_Click(sender, e); // Reset the circuit and the buttons
            SavedCircuit savedCircuit = ((SavedCircuit)formatter.Deserialize(stream)); // Load the circuit
            stream.Close();

            // Add the saved elements to the board
            BoardGrid.Nodes = savedCircuit.Nodes;
            BoardGrid.ComponentsOnBoard = savedCircuit.Components;
            BoardGrid.WiresOnBoard = savedCircuit.Wires;
            foreach (List<Node> line in savedCircuit.Nodes)
            {
                foreach (Node node in line)
                {
                    node.Deserialized(BoardGrid);
                }
            }
            foreach (Component component in savedCircuit.Components)
            {
                component.Deserialized(BoardGrid);
            }
            foreach (Wire wire in savedCircuit.Wires)
            {
                wire.Deserialized(BoardGrid);
            }
        }

        /// <summary>
        /// Handler called when the save button is clicked
        /// </summary>
        private void SaveCircuitButton_Click(object sender, RoutedEventArgs e)
        {
            // CHOOSE FILE THERE

            // Unselected every element before serialization
            foreach (Component component in BoardGrid.ComponentsOnBoard)
            {
                component.SetIsSelected(false);
            }
            foreach (Wire wire in BoardGrid.WiresOnBoard)
            {
                wire.SetIsSelected(false);
            }

            // File to which save the circuit
            Stream stream = File.Open("data.xml", FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, new SavedCircuit(BoardGrid)); // Save the BoardGrid
            stream.Close();
        }

        /// <summary>
        /// Handler called when the left rotation button is clicked or when the E key is pressed
        /// </summary>
        private void RotateLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (BoardGrid.SelectedElement != null && BoardGrid.SelectedElement is Component)
            {
                ((Component)BoardGrid.SelectedElement).RotateLeft();
            }
        }

        /// <summary>
        /// Handler called when the right rotation button is clicked or when the R key is pressed
        /// </summary>
        private void RotateRightButton_Click(object sender, RoutedEventArgs e)
        {
            if (BoardGrid.SelectedElement != null && BoardGrid.SelectedElement is Component)
            {
                ((Component)BoardGrid.SelectedElement).RotateRight();
            }
        }

        /// <summary>
        /// Handler called when the delete button is clicked or when the Del key is pressed
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            BoardGrid.DeleteSelected();
        }

        /// <summary>
        /// Handler called when the wire button is clicked or when the W key is pressed
        /// </summary>
        private void WireModeButton_Click(object sender, RoutedEventArgs e)
        {
            BoardGrid.AddingWire = (bool)WireModeButton.IsChecked; // Enable or disable "wire mode"
            if (!(bool)WireModeButton.IsChecked && BoardGrid.DraggingWire) // Dragging is interrupted if the wire button is unselected 
            {
                BoardGrid.DraggingWire = false;
            }
            MultipleWiresModeCheckBox.IsEnabled = (bool)WireModeButton.IsChecked; // Enable or disable the multiple wires mod checker
        }

        /// <summary>
        /// Handler called when the multiple wires mode checker is clicked
        /// </summary>
        private void MultipleWiresModeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            BoardGrid.AddingMultipleWires = ((bool)MultipleWiresModeCheckBox.IsChecked);
        }

        /// <summary>
        /// Display or hide the textblock right to the multiple wires mode checker when it is enabled or disabled
        /// </summary>
        private void MultipleWiresModeCheckBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MultipleWiresModeTextBlock != null) // To prevent a bug where this method is called before the TextBlock is initialized
            {
                if (MultipleWiresModeCheckBox.IsEnabled)
                {
                    MultipleWiresModeTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    MultipleWiresModeTextBlock.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Hide the left drawer when the mouse leaves
        /// </summary>
        private void ComponentsPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            Drawer.IsLeftDrawerOpen = false;
        }

        /// <summary>
        /// Check whether the drawer must be closed
        /// </summary>
        private void Drawer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.GetPosition(this).X > ComponentsPanel.DesiredSize.Width)
            {
                Drawer.IsLeftDrawerOpen = false;
            }
        }
        #endregion
    }
}