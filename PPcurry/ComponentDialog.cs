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
        private List<UIElement> EditableFields; 
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
            this.EditableFields = new List<UIElement>();


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
            mainDockPanel.LastChildFill = false; // If set to true, the last element would not be placed right
            this.Content = mainDockPanel;

            // The StackPanel to stack all the controls
            StackPanel mainStackPanel = new StackPanel();
            mainDockPanel.Children.Add(mainStackPanel);
            mainStackPanel.Orientation = Orientation.Vertical;
            DockPanel.SetDock(mainStackPanel, Dock.Top);

            // To edit the name
            TextBlock textName = new TextBlock();
            textName.Text = "Nom :";
            textName.Margin = new Thickness(0, 0, 10, 0); // Space between the name and the value

            TextBox nameBox = new TextBox
            {
                Text = this.ParentComponent.GetName(),
                MinWidth = 200,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            this.EditableFields.Add(nameBox);
            DockPanel namePanel = new DockPanel();
            namePanel.Margin = new Thickness(10);
            DockPanel.SetDock(nameBox, Dock.Right);
            DockPanel.SetDock(textName, Dock.Right);
            namePanel.Children.Add(nameBox);
            namePanel.Children.Add(textName);
            mainStackPanel.Children.Add(namePanel);

            // To edit attributes
            Dictionary<string, double?> attributes = this.ParentComponent.GetAttributes(); // The components attributes
            Dictionary<string, Dictionary<string, double>> attributesUnits = this.ParentComponent.GetAttributesUnits(); // The component attributes available units
            List<TextBox> attributesValuesControls = new List<TextBox>(); // The controls to edit the attributes


            foreach (string attributeName in attributes.Keys)
            {
                TextBlock attributeNameControl = new TextBlock();
                attributeNameControl.Text = $"{attributeName} :";
                attributeNameControl.Margin = new Thickness(0, 0, 10, 0); // Space between the name and the value

                TextBox attributeValueControl = new TextBox
                {
                    MinWidth = 200,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                if (attributes[attributeName] != null) // If the attribute already has a value
                {
                    attributeValueControl.Text = ((double)attributes[attributeName]).ToString();
                }

                this.EditableFields.Add(nameBox);
                DockPanel attributeControl = new DockPanel();
                attributeControl.Margin = new Thickness(10);
                DockPanel.SetDock(attributeValueControl, Dock.Right);
                DockPanel.SetDock(attributeNameControl, Dock.Right);
                attributeControl.Children.Add(attributeValueControl);
                attributeControl.Children.Add(attributeNameControl);
                mainStackPanel.Children.Add(attributeControl);
            }

            // For the "ok" and "cancel" buttons
            StackPanel buttonsStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
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
            buttonOk.Click += ButtonOk_Click;
            buttonsStackPanel.Children.Add(buttonOk);
            Button buttonCancel = new Button
            {
                Width = 50,
                Height = 25,
                Margin = new Thickness(10),
                Content = "Annuler"
            };
            buttonCancel.Click += ButtonCancel_Click;
            buttonsStackPanel.Children.Add(buttonCancel);
        }

        /// <summary>
        /// Save modified attributes
        /// </summary>
        private void SaveValues()
        {

        }

        /// <summary>
        /// Reset modified attributes
        /// </summary>
        private void ResetValues()
        {

        }

        /// <summary>
        /// Handle the click on the "Ok" button
        /// </summary>
        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.SaveValues();
            this.Hide();
        }

        /// <summary>
        /// Handle the click on the "Cancel" button
        /// </summary>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.ResetValues();
            this.Hide();
        }

        /// <summary>
        /// Handle the closing of the window to cancel it and hide the window instead
        /// </summary>
        private void ComponentDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the closing
            this.ResetValues();
            this.Hide();
        }
    }
}
