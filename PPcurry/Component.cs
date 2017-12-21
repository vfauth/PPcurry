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


namespace PPcurry
{
    class Component
    {
        // Attributes

        private double[] Pos; // The position of the component on the grid
		private Uri ComponentImageUri; // The path to the resource
        private Image ComponentImage; // The Image object
		private String ComponentName; // The component name


        // Accessors/Mutators

        public double[] GetPosition() => this.Pos;
        public void SetPosition(double[] position) => this.Pos = position;

        public String GetName() => this.ComponentName;
        public void SetName(String name)
        {
            this.ComponentName = name;
            this.ComponentImage.ToolTip = name; // Update the tooltip
        }


        // Constructors

        /// <summary>
        /// Add one component to the board.
        /// </summary>
        /// <param name="x">The component abscissa.</param>
        /// <param name="y">The component ordinate.</param>
        public Component(double x, double y, Uri imageUri, String name, Canvas canvas)
        {
            // Save attributes
            this.Pos = new double[2] { x , y }; 
            this.ComponentImageUri = imageUri;
            this.ComponentName = name;

            // Create the image and display it
            this.ComponentImage = new Image();
            this.ComponentImage.Width = 100; /* TO BE IMPROVED: must not be fixed (implement zoom) */
            this.ComponentImage.Height = 100;
            this.ComponentImage.Margin = new Thickness(0);
            this.ComponentImage.Source = new BitmapImage(imageUri);
            this.ComponentImage.SetValue(Canvas.LeftProperty, x); 
            this.ComponentImage.SetValue(Canvas.TopProperty, y);
            canvas.Children.Add(this.ComponentImage); 
            this.ComponentImage.ToolTip = name;
        }


        // Methods

        public static Uri GetComponentUri(String tag)
        {
            Uri ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/resistor.png"); // Without a default value to return, the code doesn't compile
            switch(tag)
            {
                case "ac_voltage_source":
                    ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/ac_voltage_source.png");
                    break;
                case "capacitor":
                    ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/capacitor.png");
                    break;
                case "current_source":
                    ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/current_source.png");
                    break;
                case "dc_voltage_source":
                    ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/dc_voltage_source.png");
                    break;
                case "diode":
                    ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/diode.png");
                    break;
                case "ground":
                    ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/ground.png");
                    break;
                case "inductor":
                    ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/inductor.png");
                    break;
                case "resistor":
                    ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/resistor.png");
                    break;
                case "spst_closed":
                    ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/spst_closed.png");
                    break;
                case "spst_open":
                    ComponentUri = new Uri("pack://siteoforigin:,,,/Resources/spst_open.png");
                    break;
            }
            return ComponentUri;
        }


        public static String GetComponentDefaultName(String tag)
        {
            String ComponentName = "";
            switch (tag)
            {
                case "ac_voltage_source":
                    ComponentName = "Source de tension alternative";
                    break;
                case "capacitor":
                    ComponentName = "Condensateur";
                    break;
                case "current_source":
                    ComponentName = "Source de courant";
                    break;
                case "dc_voltage_source":
                    ComponentName = "Source de tension continue";
                    break;
                case "diode":
                    ComponentName = "Diode";
                    break;
                case "ground":
                    ComponentName = "Terre";
                    break;
                case "inductor":
                    ComponentName = "Inductance";
                    break;
                case "resistor":
                    ComponentName = "Résistance";
                    break;
                case "spst_closed":
                    ComponentName = "Interrupteur fermé";
                    break;
                case "spst_open":
                    ComponentName = "Interrupteur ouvert";
                    break;
            }
            return ComponentName;
        }
    }
}
