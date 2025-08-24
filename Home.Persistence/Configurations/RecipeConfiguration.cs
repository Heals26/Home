using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Persistence.Configurations
{

    public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
    {

        #region Methods
	        
	    public void Configure(EntityTypeBuilder<Recipe> entity)
        {
            entity.ToTable(nameof(Recipe), DomainValues.Schema);

            entity.HasKey(e => e.RecipeID;
        }
	        
	        #endregion Methods

    }

}
