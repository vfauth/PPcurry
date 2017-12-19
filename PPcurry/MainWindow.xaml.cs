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

        private String PathToComponentToAdd;
        private List<Component> ComponentsList;


        // Accessors/Mutators


        // Constructors

        public MainWindow()
        {
            InitializeComponent();
            PathToComponentToAdd = "";
            ComponentsList = new List<Component>();
        }


        // Methods

        private void ComponentSelectedFromPanel(object sender, MouseButtonEventArgs e)
        {
            this.PathToComponentToAdd = (String)((Image)sender).Tag;
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PathToComponentToAdd != "")
            {
                Image NewImage = new Image();
                NewImage.Source = new BitmapImage(new Uri($"pack://siteoforigin:,,,/Resources/{PathToComponentToAdd}.png"));
                NewImage.Height = 100; // TO BE IMPROVED: must not be fixed
                NewImage.Width = 100;
                NewImage.SetValue(Canvas.LeftProperty, (e.GetPosition(MainCanvas)).X); // Place the component at the click position, relative to the canvas
                NewImage.SetValue(Canvas.TopProperty, (e.GetPosition(MainCanvas)).Y);
                MainCanvas.Children.Add(NewImage);

                this.PathToComponentToAdd = ""; // TO BE IMPROVED // Reinitialize the chosen component; must be tied to configuration (for serial insertion)
            }
        }
    }
}
