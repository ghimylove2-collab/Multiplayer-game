namespace Airox.Client.BattleRoyale
{
    public readonly struct SafeZoneSnapshot
    {
        public double centerX { get; }
        public double centerZ { get; }
        public double radius { get; }
        public int damagePerSecond { get; }

        public SafeZoneSnapshot(double centerX, double centerZ, double radius, int damagePerSecond)
        {
            this.centerX = centerX;
            this.centerZ = centerZ;
            this.radius = radius;
            this.damagePerSecond = damagePerSecond;
        }
    }
}
