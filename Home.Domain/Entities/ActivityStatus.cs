namespace Home.Domain.Entities
{

    public class ActivityStatus
    {

        #region Properties

        public long ActivityStatusID { get; set; }
        public string Name { get; set; }

        public ICollection<Activity> Activities { get; set; }

        #endregion Properties

    }

}
