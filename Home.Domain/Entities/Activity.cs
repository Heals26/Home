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

        /// <summary>
        /// Tracking of who did what
        /// </summary>
        public ICollection<Audit> Audits { get; set; }

        /// <summary>
        /// The region where the content sits
        /// </summary>
        public ICollection<ActivityRegion> Regions { get; set; }

        /// <summary>
        /// Todo, Refining, Progressing, Blocked, Testing, Done
        /// </summary>
        public ActivityState State { get; set; }

        /// <summary>
        /// Todo, In Progress, Done
        /// </summary>
        public ActivityStatus Status { get; set; }

        public User User { get; set; }

        #endregion Properties

    }

}
