using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Domain.Entities
{

    public class RecipeStep
    {

        #region Properties

        public long RecipeStepID { get; set; }

        public int Sequence { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
	        
	    #endregion Properties

    }

}
