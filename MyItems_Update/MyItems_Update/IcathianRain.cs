using EntityStates;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HG;
using UnityEngine.Networking;

namespace KaisaMod.SkillStates
{
    public class IcathianRain : BaseSkillState
    {
        /*
        private int totalMissiles = 10;
        private float missileTimer;
        private int remainingMissiles;
        private float duration;
        public List<HurtBox> targets;
        public List<HurtBox> missleTargets;
        private int missleIndex;

        protected SphereSearch sphereSearch;
        private float radius = 10000000.0f;

        public static float baseDuration = 1.0f;

        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();

            RoR2.Console.print("ELP1");

            this.duration = IcathianRain.baseDuration / base.attackSpeedStat;
            this.remainingMissiles = this.totalMissiles;
            this.missleIndex = 0;

            this.animator = base.GetModelAnimator();

            RoR2.Console.print("ELP2");

            sphereSearch = new SphereSearch();
            this.SearchForTargets(targets);

            int count = 0;

            RoR2.Console.print("ELP3");

            if (this.targets.Count != 0) count = this.targets.Count - 1;
            int index = 0;

            RoR2.Console.print("ELP4");

            for (int i = totalMissiles; i < totalMissiles; i++)
            {
                if (index > count) index = 0;

                this.missleTargets[i] = this.targets[index];
                index++;
            }

            RoR2.Console.print("ELP5");

            RoR2.Console.print("Full Target List:" + this.targets);
            RoR2.Console.print("Missle Target List:" + this.missleTargets);

            base.PlayCrossfade("Fullbody, Override", "IcathianRain", "IcathianRain.playbackRate", duration, 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.missileTimer > 0f)
            {
                this.missileTimer = Mathf.Max(this.missileTimer - Time.fixedDeltaTime, 0f);
            }
            if (this.missileTimer == 0f && this.remainingMissiles > 0)
            {
                this.remainingMissiles--;
                this.missileTimer = this.duration / this.totalMissiles;
                this.FireMissile(missleTargets[missleIndex].gameObject);
                RoR2.Console.print("Fired A Missile at:" + missleTargets[missleIndex].gameObject);
                missleIndex++;
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        protected void SearchForTargets(List<HurtBox> dest)
        {
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.origin = transform.position;
            sphereSearch.radius = radius;
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(TeamIndex.Player));
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            RoR2.Console.print(":(");
            sphereSearch.GetHurtBoxes(dest);
            RoR2.Console.print(dest);
            sphereSearch.ClearCandidates();
        }

        private void FireMissile(GameObject target)
        {
            GameObject projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MissileVoidProjectile");
            float num = Modules.StaticValues.icathianRainDamageCoefficient;
            bool isCrit = Util.CheckRoll(characterBody.crit, characterBody.master);
            MissileUtils.FireMissile(characterBody.corePosition, characterBody, default(ProcChainMask), null, characterBody.damage * num, isCrit, projectilePrefab, DamageColorIndex.Item);
        }*/
    }
}