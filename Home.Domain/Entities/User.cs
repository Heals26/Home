namespace Home.Domain.Entities
{

    public class User
    {

        #region Properties

        public long UserID { get; set; }
        public ICollection<Audit> Audits { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        public ICollection<Activity> AssignedActivities { get; set; }

        #endregion Properties

    }

}
