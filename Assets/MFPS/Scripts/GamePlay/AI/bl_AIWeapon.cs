using UnityEngine;

namespace MFPS.Runtime.AI
{
    public class bl_AIWeapon : MonoBehaviour
    {
        [Header("Info")]
        [GunID] public int GunID;
        [Range(1, 60)] public int Bullets = 30;
        [Range(1, 6)] public int bulletsPerShot = 1;
        public int maxFollowingShots = 5;
        public string BulletName = "bullet";
        [Header("References")]
        public Transform FirePoint;
        public ParticleSystem MuzzleFlash;
        public AudioClip fireSound;
        public AudioClip[] reloadSounds;

        /// <summary>
        /// 
        /// </summary>
        public void Initialize(bl_AIShooterAttackBase shooterWeapon)
        {

        }

        private bl_GunInfo m_info;
        public bl_GunInfo Info
        {
            get
            {
                if (m_info == null)
                {
                    m_info = bl_GameData.Instance.GetWeapon(GunID); ;
                }
                return m_info;
            }
        }
    }
}