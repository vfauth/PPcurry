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
using System.Globalization;

namespace PPcurry
{
    public class ComponentDialog : Window
    {
        #region Attributes

        public Component ComponentEdited { get; set; } 
        private Dictionary<string, TextBox> EditableFields; // The fields and the name of the associated attribute
        #endregion


        #region Constructor

        /// <summary>
        /// Create controls to allow editing of components attributes
        /// </summary>
        public ComponentDialog()
        {
            this.Owner = Application.Current.MainWindow;

            this.SizeToContent = SizeToContent.WidthAndHeight; // The dialog size is that of its children
            this.WindowStyle = WindowStyle.ToolWindow; // To have only the close button
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner; // The dialog is positionned in the center of the main window

            // Event handlers
            this.Closing += ComponentDialog_Closing;
        }
        #endregion


        #region Methods

        /// <summary>
        /// Display the dialog to edit the attributes of the component given as a parameter
        /// </summary>
        public void Display(Component component)
        {
            this.ComponentEdited = component;
            this.Content = null; // Delete all previous elements
            this.Title = "Edit attributes";
            this.FillDialog();
            this.Show();
        }

        /// <summary>
        /// Create controls inside the dialog
        /// </summary>
        private void FillDialog()
        {
            this.EditableFields = new Dictionary<string, TextBox>();
            DockPanel mainDockPanel = new DockPanel
            {
                LastChildFill = false // If set to true, the last element would not be placed right
            };
            this.Content = mainDockPanel;

            // The StackPanel to stack all the controls
            StackPanel mainStackPanel = new StackPanel();
            mainDockPanel.Children.Add(mainStackPanel);
            mainStackPanel.Orientation = Orientation.Vertical;
            DockPanel.SetDock(mainStackPanel, Dock.Top);

            // To edit the name
            TextBlock textName = new TextBlock
            {
                Text = "Nom :",
                Margin = new Thickness(0, 0, 10, 0) // Space between the name and the value
            };

            TextBox nameBox = new TextBox
            {
                Text = this.ComponentEdited.GetName(),
                MinWidth = 200,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            this.EditableFields.Add("name", nameBox);
            DockPanel namePanel = new DockPanel
            {
                Margin = new Thickness(10)
            };
            DockPanel.SetDock(nameBox, Dock.Right);
            DockPanel.SetDock(textName, Dock.Right);
            namePanel.Children.Add(nameBox);
            namePanel.Children.Add(textName);
            mainStackPanel.Children.Add(namePanel);

            // To edit attributes
            Dictionary<string, double?> attributes = this.ComponentEdited.Attributes; // The components attributes
            Dictionary<string, Dictionary<string, double>> attributesUnits = this.ComponentEdited.AttributesUnits; // The component attributes available units
            List<TextBox> attributesValuesControls = new List<TextBox>(); // The controls to edit the attributes
            
            foreach (string attributeName in attributes.Keys)
            {
                TextBlock attributeNameControl = new TextBlock
                {
                    Text = $"{attributeName} :",
                    Margin = new Thickness(0, 0, 10, 0) // Space between the name and the value
                };

                TextBox attributeValueControl = new TextBox
                {
                    MinWidth = 200,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                if (attributes[attributeName] != null) // If the attribute already has a value
                {
                    attributeValueControl.Text = ((double)attributes[attributeName]).ToString();
                }

                this.EditableFields.Add(attributeName, attributeValueControl);
                DockPanel attributeControl = new DockPanel
                {
                    Margin = new Thickness(10)
                };
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
            // Names
            this.ComponentEdited.SetName(EditableFields["name"].Text);

            // Attributes
            Dictionary<string, double?> attributes = new Dictionary<string, double?>(this.ComponentEdited.Attributes); // The components attributes
            Dictionary<string, Dictionary<string, double>> attributesUnits = this.ComponentEdited.AttributesUnits; // The component attributes available units

            foreach (string attribute in EditableFields.Keys)
            {
                if (EditableFields[attribute].Text == "")
                {
                    attributes[attribute] = null;
                }
                else if (attribute != "name")
                {
                    attributes[attribute] = double.Parse(EditableFields[attribute].Text, CultureInfo.InvariantCulture); // Parse the string while supporting decimal points and commas 
                }
            }

            this.ComponentEdited.Attributes = attributes;
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
        #endregion
    }
}
