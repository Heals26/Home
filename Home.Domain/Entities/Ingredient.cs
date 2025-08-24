using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Domain.Entities
{

    public  class Ingredient
    {

        #region Properties
	        
	    public long IngredientID { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Volumne { get; set; }
        public decimal Weight { get; set; }
	        
        public ICollection<Recipe> Recipes { get; set; }

	    #endregion Properties

    }

}
