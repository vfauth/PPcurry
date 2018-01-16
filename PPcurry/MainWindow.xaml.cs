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
        private List<Component> ComponentsOnBoard; // The list of components on the board
        private XElement XmlComponentsList; // The XML Element containing all the available components data
        #endregion


        #region Accessors/Mutators

        public void AddComponent(Component component)
        {
            if (component != null)
            {
                this.ComponentsOnBoard.Add(component);
            }
        }
        #endregion


        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            ComponentsOnBoard = new List<Component>();
            this.BoardGrid = new BoardGrid(39, 1);
            BoardCanvas.Children.Add(BoardGrid);
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
                Image NewComponent = new Image();
                NewComponent.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, Properties.Settings.Default.ResourcesFolder, element.Element("image").Value))); // The image to display
                NewComponent.Margin = new Thickness(10, 5, 5, 10); // The thickness around the image
                NewComponent.VerticalAlignment = VerticalAlignment.Top;
                NewComponent.Width = 2*(BoardGrid.GetGridSpacing()+BoardGrid.GetGridThickness()); // The component covers 2 grid cells
                NewComponent.Height = 2 * (BoardGrid.GetGridSpacing() + BoardGrid.GetGridThickness()); // The component covers 2 grid cells
                NewComponent.MouseLeftButtonUp += ComponentsPanel_ComponentSelected; // Event handler for component selection
                NewComponent.ToolTip = element.Element("name").Value; // The name of the component appears on the tooltip
                NewComponent.Tag = element.Element("type").Value; // The component is identified by the image tag

                ComponentsPanel.Children.Add(NewComponent); // The component is added to the left panel
            }
        }

        /// <summary>
        /// When a component is selected in the left panel, a new Component is created then dragged
        /// </summary>
        private void ComponentsPanel_ComponentSelected(object sender, MouseButtonEventArgs e)
        {
            Image ComponentSelected = (Image)sender;
            String ComponentType = (String)ComponentSelected.Tag; // The component type

            // Create the component at the image location 
            Point RelativePosition = ComponentSelected.TransformToAncestor(ComponentsPanel).Transform(new Point(0, 0)); // Position of the selected component relative to the panel
            RelativePosition.X -= ComponentSelected.ActualWidth/2;
            RelativePosition.Y -= ComponentSelected.ActualHeight/2;
            XElement XmlElement = this.XmlComponentsList.Element(ComponentType); // Get the XML element with all the component data
            Component NewComponent = new Component(RelativePosition.X, RelativePosition.Y, BoardGrid, XmlElement); // Create the component and display it
            this.AddComponent(NewComponent);

            NewComponent.Component_MouseLeftButtonDown(NewComponent, e); // Begin the drag
        }
        #endregion
    }
}