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
        
        private int[] CurrentFlowIndicator { get; set; } //The indicator of the way of the current


        #endregion

        #region Constructor
        public TensionNode()
        {
            this.CurrentFlowIndicator = new int[1] { 0 };
        }
        #endregion
    }
}
