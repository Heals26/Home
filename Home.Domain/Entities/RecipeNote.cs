using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Domain.Entities
{

    public class RecipeNote
    {

        #region Properties
	        
	    public long RecipeNoteID { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOnUTC { get; set; }

        public Recipe Recipe { get; set; }
	        
	    #endregion Properties

    }

}
