namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 3차원 좌표를 표현합니다.
    /// </summary>
    public readonly struct Point3D
    {
        private static Point3D zeroVector = new(0f, 0f, 0f);

        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static float DistanceSquared(in Point3D a, in Point3D b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            var dz = a.Z - b.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static Point3D zero
        {
            get
            {
                return zeroVector;
            }
        }
    }
}
