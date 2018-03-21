
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

namespace PPcurry
{
    [Serializable()]
    public class Node
    {
        #region Attributes

        [NonSerialized()] private BoardGrid BoardGrid; // The board on which is this Node
        public Point Position { get; set; } // The position of the Node
        public Dictionary<object, Directions> ConnectedElements = new Dictionary<object, Directions>(); // The elements connected (Up, Right, Down, Left)
        #endregion


        #region Constructor

        public Node(Point position, BoardGrid boardGrid)
        {
            // Save attributes
            BoardGrid = boardGrid as BoardGrid;
            Position = position;
        }

        /// <summary>
        /// Update the BoardGrid attribute after deserialization
        /// </summary>
        public void Deserialized(BoardGrid boardGrid)
        {
            BoardGrid = boardGrid;
        }
        #endregion

        #region Methods
        public bool IsIsolated()
        {
            return !(ConnectedElements.All(element => element.Key==null));  
        }
        #endregion
    }
}
