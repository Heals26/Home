namespace Home.Domain.Entities
{

    public class Activity
    {

        #region Properties

        public long ActivityID { get; set; }

        public ICollection<Audit> Audits { get; set; }
        public DateTime? CompletedDateUTC { get; set; }
        public DateTime DueDateUTC { get; set; }
        public string Title { get; set; }

        public ActivityStatus Status { get; set; }
        public ICollection<ActivityRegion> Regions { get; set; }
        public User User { get; set; }

        #endregion Properties

    }

}
