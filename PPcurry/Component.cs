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

        private int[] Pos; // The position of the component on the grid
        private Image ComponentImage; // The Image object


        // Accessors/Mutators

        //public Card GetCardPaired() => cardPaired;
        //public Card SetCardPaired(Card cardPaired) => this.cardPaired = cardPaired;


        // Constructors

        /// <summary>
        /// Add one component to the board.
        /// </summary>
        /// <param name="x">The component abscissa.</param>
        /// <param name="y">The component ordinate.</param>
        public Component(int x, int y, Image image)
        {
            this.Pos = new int[2] { x , y };
            this.ComponentImage = image;
        }


        // Methods
    }
}
