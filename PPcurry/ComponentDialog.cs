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
using System.Windows.Controls.Primitives;
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
    public class ComponentDialog : Window
    {
        #region Attributes

        private Component ParentComponent;
        #endregion


        #region Constructor

        /// <summary>
        /// Create controls to fill to allow editing of the component attributes
        /// </summary>
        /// <param name="component">The component whose attributes this dialog must display</param>
        public ComponentDialog(Component component)
        {
            this.ParentComponent = component;
            this.Owner = Application.Current.MainWindow;
            
            this.SizeToContent = SizeToContent.WidthAndHeight; // The dialog size is that of its children
            this.WindowStyle = WindowStyle.ToolWindow; // To have only the close button
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner; // The dialog is positionned in the center of the main window

            // Event handlers
            this.Closing += ComponentDialog_Closing;
            
            FillDialog();
        }
        #endregion

        /// <summary>
        /// Create controls inside the dialog
        /// </summary>
        private void FillDialog()
        {
            DockPanel mainDockPanel = new DockPanel();
            this.Content = mainDockPanel;

            // The StackPanel to stack all the controls
            StackPanel mainStackPanel = new StackPanel();
            mainDockPanel.Children.Add(mainStackPanel);
            mainStackPanel.Orientation = Orientation.Vertical;
            DockPanel.SetDock(mainStackPanel, Dock.Top);

            // For the "ok" and "cancel" buttons
            StackPanel buttonsStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            mainStackPanel.Children.Add(buttonsStackPanel);
            DockPanel.SetDock(buttonsStackPanel, Dock.Bottom);
            Button buttonOk = new Button
            {
                Width = 50,
                Height = 25,
                Margin = new Thickness(10),
                Content = "Ok"
            };
            buttonOk.Click += buttonOk_Click;
            buttonsStackPanel.Children.Add(buttonOk);
            Button buttonCancel = new Button
            {
                Width = 50,
                Height = 25,
                Margin = new Thickness(10),
                Content = "Annuler"
            };
            buttonCancel.Click += buttonCancel_Click;
            buttonsStackPanel.Children.Add(buttonCancel);
        }

        /// <summary>
        /// Handle the click on the "Ok" button
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// Handle the click on the "Cancel" button
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// Handle the closing of the window to cancel it and hide the window instead
        /// </summary>
        private void ComponentDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the closing
            this.Hide();
        }
    }
}
