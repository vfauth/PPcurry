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
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;

namespace PPcurry
{
    public class ComponentDialog : StackPanel
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
            this.Orientation = Orientation.Vertical;
            this.KeyDown += ComponentDialog_KeyUp;
        }

        private void ComponentDialog_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ButtonOk_Click(sender, e);
                    break;
                case Key.Escape:
                    ButtonCancel_Click(sender, e);
                    break;
            }
        }
        #endregion


        #region Methods

        /// <summary>
        /// Display the dialog to edit the attributes of the component given as a parameter
        /// </summary>
        public void Display(Component component)
        {
            this.ComponentEdited = component;
            this.Children.Clear(); // Delete all previous elements
            this.FillDialog();
            ((MainWindow)Application.Current.MainWindow).AttributesDialogHost.IsOpen = true; // Display the dialog
        }

        /// <summary>
        /// Create controls inside the dialog
        /// </summary>
        private void FillDialog()
        {
            this.EditableFields = new Dictionary<string, TextBox>();

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
            DockPanel.SetDock(textName, Dock.Left);
            DockPanel.SetDock(nameBox, Dock.Right);
            namePanel.Children.Add(nameBox);
            namePanel.Children.Add(textName);
            this.Children.Add(namePanel);

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
                DockPanel.SetDock(attributeNameControl, Dock.Left);
                DockPanel.SetDock(attributeValueControl, Dock.Right);
                attributeControl.Children.Add(attributeValueControl);
                attributeControl.Children.Add(attributeNameControl);
                this.Children.Add(attributeControl);
            }

            // For the "ok" and "cancel" buttons
            StackPanel buttonsStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            this.Children.Add(buttonsStackPanel);
            Button buttonOk = new Button
            {
                Margin = new Thickness(10),
                Content = "Ok",
                ToolTip = "Close the dialog and save the attributes"
            };
            buttonOk.Click += ButtonOk_Click;
            buttonsStackPanel.Children.Add(buttonOk);
            Button buttonCancel = new Button
            {
                Margin = new Thickness(10),
                Content = "Cancel",
                ToolTip = "Close the dialog but doesn't save the attributes"
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
                    attributes[attribute] = double.Parse(EditableFields[attribute].Text.Replace('.', ',')); // Parse the string while supporting decimal points and commas 
                }
            }

            this.ComponentEdited.Attributes = attributes;
        }

        /// <summary>
        /// Handler called when the "Ok" button is clicked or the Enter key is pressed
        /// </summary>
        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.SaveValues();
            ((MainWindow)Application.Current.MainWindow).AttributesDialogHost.IsOpen = false; // Close the dialog
        }

        /// <summary>
        /// Handler called when the "Cancel" button is clicked or the Esc key is pressed
        /// </summary>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).AttributesDialogHost.IsOpen = false; // Close the dialog
        }
        #endregion
    }
}
