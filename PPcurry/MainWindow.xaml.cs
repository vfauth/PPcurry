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
using System.Xml.Linq;

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


        #region Accessors/Mutators

        public XElement GetXmlComponentsList() => this.XmlComponentsList;
        public void SetXmlComponentsList(XElement xmlComponentsList) => this.XmlComponentsList = xmlComponentsList;
        #endregion


        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this; // Application.Current.MainWindow can return null sometimes, so we prevent it
            this.BoardGrid = new BoardGrid(39, 1);
            MainPanel.Children.Add(BoardGrid);
            LoadComponents();
        }
        #endregion


        #region Methods

        /// <summary>
        /// Load the components then display them in the panel at the left
        /// </summary>
        private void LoadComponents()
        { 
            this.XmlComponentsList = XElement.Load(@"Components.xml"); // Load the XML file
            foreach (XElement element in XmlComponentsList.Elements())
            {
                Image newComponent = new Image
                {
                    Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, Properties.Settings.Default.ResourcesFolder, element.Element("image").Value))), // The image to display
                    Margin = new Thickness(10, 5, 5, 10), // The thickness around the image
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 2 * BoardGrid.GetGridSpacing() + BoardGrid.GetGridThickness(), // The component covers 2 grid cells
                    Height = 2 * BoardGrid.GetGridSpacing() + BoardGrid.GetGridThickness() // The component covers 2 grid cells
                };
                newComponent.MouseMove += ComponentInLeftPanel_MouseMove; // Event handler for component selection
                newComponent.ToolTip = element.Element("name").Value; // The name of the component appears on the tooltip
                newComponent.Tag = element.Element("type").Value; // The component is identified by the image tag
                
                ComponentsPanel.Children.Add(newComponent); // The component is added to the left panel
            }
        }

        /// <summary>
        /// If the mouse moves over a component in the left panel and the left mouse button is pressed, the component is dragged
        /// </summary>
        private void ComponentInLeftPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) // Drag only if the left button is pressed
            {

                Point relativePosition = e.GetPosition(this); // Position of the drop relative to the board
                relativePosition.X -= (2 * BoardGrid.GetGridSpacing() + BoardGrid.GetGridThickness()) / 2;
                relativePosition.Y -= (2 * BoardGrid.GetGridSpacing() + BoardGrid.GetGridThickness()) / 2;
                XElement xmlElement = XmlComponentsList.Element((string)((Image)sender).Tag); // Get the XML element with all the component data

                Component newComponent = new Component(relativePosition, BoardGrid, xmlElement)
                {
                    Opacity = 0 // The component is only displayed once on the board
                }; // Create the component and display it
                
                DragDrop.DoDragDrop(newComponent, newComponent, DragDropEffects.Move); // Begin the drag&drop
            }
        }
        #endregion
    }
}