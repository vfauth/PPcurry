using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPcurry;

namespace PPcurry
{
    class TensionNode
    {
        #region Attributes
        
        private List<int> CurrentFlowIndicator { get; set; } //The indicator of the way of the current
        public Tuple<int,int> Id { get; set; } //The Id of a Node : a Tuple to associate it easily with a node of the Board




        #endregion

        #region Constructor
        public TensionNode(Tuple<int,int> id)
        {
            Id = id;
        }
        #endregion
    }
}
