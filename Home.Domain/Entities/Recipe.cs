using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Domain.Entities
{

    public class Recipe
    {

        #region Properties
	        
	    public long RecipeID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public ICollection<Audit> Audits { get; set; }
        public ICollection<RecipeIngredient> Ingredients { get; set; }
        public ICollection<RecipeNote> Notes { get; set; }
        public ICollection<RecipeStep> Steps { get; set; }
	        
	    #endregion Properties

    }

}
