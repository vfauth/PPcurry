using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPcurry;
using System.Diagnostics;

namespace PPcurry
{
    class Simulation
    {
        #region Attributes
        public BoardGrid BoardGrid { get; } // The BoardGrid associated to the simulation
        public Graph SimulatedGraph { get; } // The Graph representing the simulated circuit
        #endregion


        #region Constructor
        public Simulation(BoardGrid boardGrid)
        {
            BoardGrid = boardGrid;
            SimulatedGraph = new Graph(boardGrid); // Generate the graph representing the circuit
        }
        #endregion


        #region Methods

        #endregion
    }
}
