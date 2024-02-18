namespace CashrewardsOffers.Domain.Entities
{
    public class Experiment
    {
        public Experiment(string featureToggle, string enrolFeatureToggle, long maxEnrolmentCount)
        {
            FeatureToggle = featureToggle;
            EnrolFeatureToggle = enrolFeatureToggle;
            MaxEnrolmentCount = maxEnrolmentCount;
        }

        public string FeatureToggle { get; }
        public string EnrolFeatureToggle { get;  }
        public long MaxEnrolmentCount { get; }
    }
}
