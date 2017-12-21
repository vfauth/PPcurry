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
        // Attributes

        private String ComponentToAddTag;
        private List<Component> ComponentsList;


        // Accessors/Mutators


        // Constructors

        public MainWindow()
        {
            InitializeComponent();
            ComponentToAddTag = "";
            ComponentsList = new List<Component>();
        }


        // Methods

        private void ComponentSelectedFromPanel(object sender, MouseButtonEventArgs e)
        {
            this.ComponentToAddTag = (String)((Image)sender).Tag;
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ComponentToAddTag != "")
            {
                double X = (e.GetPosition(MainCanvas)).X; // Place the component at the click position, relative to the canvas
                double Y = (e.GetPosition(MainCanvas)).Y;
                Uri ImageUri = Component.GetComponentUri(ComponentToAddTag); // Get the default URI to the image
                String Name = Component.GetComponentDefaultName(ComponentToAddTag); // Get the default component name
                Component NewComponent = new Component(X, Y, ImageUri, Name, MainCanvas); // Create the component and display it
                this.ComponentsList.Add(NewComponent);

                this.ComponentToAddTag = ""; // TO BE IMPROVED // Reinitialize the chosen component; must be tied to configuration (for serial insertion)
            }
        }
    }
}
