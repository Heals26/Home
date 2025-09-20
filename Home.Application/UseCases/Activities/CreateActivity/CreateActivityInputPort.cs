using Home.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Application.UseCases.Activities.CreateActivity;

public class CreateActivityInputPort
{

	#region Properties

	public DateTime? DueDateUTC { get; set; }
	public ICollection<ActivityRegion> Regions { get; set; }
	public string? State { get; set; }
	public ActivityStatus? Status { get; set; }
	public string Title { get; set; }

	#endregion Properties

}
