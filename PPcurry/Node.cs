
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
using System.Xml.Linq;
using PPcurry;

public class Node
{
    #region Attributes

    private BoardGrid Grid; // The board on which is this Node
    private Point Position; // The position of the Node
    private Component[] ConnectedComponents = new Component[4]; // The components connected (Up/North, Right/East, Down/South, Left/West)
    #endregion

    #region Accessors/Mutators

    public Point GetPosition() => this.Position;

    public Component[] GetComponents() => this.ConnectedComponents;
    public void SetPosition(Point point) => this.Position = point;

    public void SetTopComponent(Component component) => this.ConnectedComponents[0] = component;

    public void SetRightComponent(Component component) => this.ConnectedComponents[1] = component;

    public void SetBottomComponent(Component component) => this.ConnectedComponents[2] = component;


    public void SetLeftComponent(Component component) => this.ConnectedComponents[3] = component;
    #endregion

    #region Constructor

    public Node(Point position, BoardGrid boardGrid)
    {
        // Save attributes
        this.Grid = boardGrid as BoardGrid;
        this.Position = position;
    }

    #endregion
}
