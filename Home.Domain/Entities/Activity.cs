using Home.Domain.Enumerations;

namespace Home.Domain.Entities
{

    public class Activity
    {
        #region Fields

        private readonly ICollection<Audit> m_Audits = [];

        #endregion Fields

        #region Properties

        public long ActivityID { get; set; }

        public DateTime? CompletedDateUTC { get; set; }
        public DateTime DueDateUTC { get; set; }
        public string Title { get; set; }

        public ICollection<Audit> Audits
        {
            get => this.m_Audits.Where(a => a.Entity == ResourceTypeSE.Activity).ToHashSet();
        }
        public ICollection<ActivityRegion> Regions { get; set; }
        public ActivityStatus Status { get; set; }
        public User User { get; set; }

        #endregion Properties

    }

}
