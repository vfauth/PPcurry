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
    public class Component : Image
    {
        #region Attributes

        private double[] Position; // The position of the component on the grid
		private String ComponentName; // The component name
        private bool IsDragged; // Whether thhe component is currently being dragged
        private MainWindow MainWindow; // The main window in which is this component
        #endregion


        #region Accessors/Mutators

        public double[] GetPosition() => this.Position;
        public void SetPosition(double[] position) => this.Position = position;

        public String GetName() => this.ComponentName;
        public void SetName(String name)
        {
            this.ComponentName = name;
            this.ToolTip = name; // Update the tooltip
        }
        #endregion


        #region Constructor

        /// <summary>
        /// Add one component to the board
        /// </summary>
        /// <param name="x">The component abscissa</param>
        /// <param name="y">The component ordinate</param>
        /// <param name="imageUri">The URI to the image to display</param>
        /// <param name="name">The component name</param>
        /// <param name="canvas">The canvas on which to display the component</param>
        public Component(double x, double y, Uri imageUri, String name, Canvas canvas)
        {
            // Save attributes
            this.Position = new double[2] { x , y };
            this.ComponentName = name;
            this.MainWindow = (MainWindow)Window.GetWindow(canvas);
            Debug.WriteLine(MainWindow);

            // Set the image attributes and display it
            this.Width = 100; // Size /* TO BE IMPROVED: must not be fixed (implement zoom) */
            this.Height = 100;
            this.Source = new BitmapImage(imageUri); // Image to display
            this.SetValue(Canvas.LeftProperty, x); // Position
            this.SetValue(Canvas.TopProperty, y);
            this.ToolTip = name;
            this.MouseLeftButtonDown += Component_MouseLeftButtonDown;
            this.MouseLeftButtonUp += Component_MouseLeftButtonUp;
            this.MouseMove += Component_MouseMove;
            canvas.Children.Add(this); // Add the component
        }
        #endregion


        #region Methods

        /// <summary>
        /// When the component is left-clicked, he is dragged
        /// </summary>
        public void Component_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsDragged = true;
            ((Component)sender).CaptureMouse();  // The cursor cannot quit the image
        }

        /// <summary>
        /// The dragging finishes when the component is released
        /// </summary>
        public void Component_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsDragged = false;
            ((Component)sender).ReleaseMouseCapture(); // Cancel CaptureMouse()
        }

        /// <summary>
        /// This function manages the dragging of the component and is called for each move
        /// </summary>
        public void Component_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragged) return; // This function must do nothing if the component is not being dragged

            Canvas MainCanvas = (Canvas)MainWindow.FindName("MainCanvas");

            Point MousePos = e.GetPosition(MainCanvas); // Mouse position relative to the MainCanvas

            // The image is centered on the mouse position
            double Left = MousePos.X - (this.ActualWidth / 2);
            double Top = MousePos.Y - (this.ActualHeight / 2);
            Canvas.SetLeft(this, Left);
            Canvas.SetTop(this, Top);
        }

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
        #endregion
    }
}
