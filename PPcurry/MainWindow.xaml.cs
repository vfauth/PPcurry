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

namespace PPcurry
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Attributes

        private List<Component> ComponentsList; // The list of components on the board
        public BoardGrid BoardGrid; // The grid on the board
        #endregion


        #region Accessors/Mutators

        public void AddComponent(Component component)
        {
            if (component != null)
            {
                this.ComponentsList.Add(component);
            }
        }
        #endregion


        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            ComponentsList = new List<Component>();
            this.BoardGrid = new BoardGrid(39, 1);
            BoardCanvas.Children.Add(BoardGrid);
        }
        #endregion


        #region Methods

        /// <summary>
        /// When a component is selected in the left panel, a new Component is created then dragged
        /// </summary>
        private void ComponentsPanel_ComponentSelected(object sender, MouseButtonEventArgs e)
        {
            Image ComponentSelected = (Image)sender;
            String ComponentToAddTag = (String)ComponentSelected.Tag; // To identify the component selected

            // Create the component at the image location 
            Point RelativePosition = ComponentSelected.TransformToAncestor(ComponentsPanel).Transform(new Point(0, 0)); // Position of the selected component relative to the panel
            RelativePosition.X -= ComponentSelected.ActualWidth/2;
            RelativePosition.Y -= ComponentSelected.ActualHeight/2;
            Uri ImageUri = Component.GetComponentUri(ComponentToAddTag); // Get the default URI to the image
            String Name = Component.GetComponentDefaultName(ComponentToAddTag); // Get the default component name
            Component NewComponent = new Component(RelativePosition.X, RelativePosition.Y, ImageUri, Name, BoardCanvas); // Create the component and display it
            this.AddComponent(NewComponent);

            NewComponent.Component_MouseLeftButtonDown(NewComponent, e); // Begin the drag
        }
        #endregion
    }
}