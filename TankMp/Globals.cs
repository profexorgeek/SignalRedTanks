namespace TankMp
{
    public static class Globals
    {
        public static float Network_EntityUpdateSeconds = 0.7f;


        public static float Tank_SecondsToRespawn = 5f;
        public static float Tank_Speed = 200f;
        public static float Tank_Drag = 1.5f;
        public static float Tank_Accel = Tank_Speed / Tank_Drag;
        public static float Tank_LerpSeconds = 0.2f;
        public static float Tank_ReloadSeconds = 0.2f;
        public static float Tank_AnimationSpeedScale = 10f;
        public static float Tank_Health = 100f;

        public static float Bullet_Damage = 10f;
        public static float Bullet_LifeSeconds = 4f;
        public static float Bullet_Speed = 300f;
        public static float Bullet_LerpSeconds = 0.5f;
    }
}
