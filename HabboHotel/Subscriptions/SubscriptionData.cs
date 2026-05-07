namespace Plus.HabboHotel.Subscriptions
{
    public class SubscriptionData
    {
        public int Id { get; }
        public string Name { get; }
        public string Badge { get; }
        public int Credits { get; }
        public int Duckets { get; }
        public int Respects { get; }
        public int StaticAggression { get; }

        public SubscriptionData(int id, string name, string badge, int credits, int duckets, int respects, int staticAggression)
        {
            Id = id;
            Name = name;
            Badge = badge;
            Credits = credits;
            Duckets = duckets;
            Respects = respects;
            StaticAggression = staticAggression;
        }
    }
}