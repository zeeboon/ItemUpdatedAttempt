using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using MyItems_Update.Base_Classes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static MyItems_Update.Utils.ItemHelper;
using static MyItems_Update.Utils.Log;
using UnityEngine.AddressableAssets;

namespace MyItems_Update.Custom_Classes.Items
{
    class Item01 : ItemBase
    {
        public override string ItemName => "Stinky Bomb";

        public override string ItemLangTokenName => "STINKYBOMB";

        public override string ItemPickupDesc => "Chance on hit to poison surrounding enemies.";

        public override string ItemFullDescription => $"<style=cIsDamage>{ProcChance}%</style> chance on hit to <style=cIsDamage>poison</style> " +
                                                            $"all enemies in {StinkRadius} meters for <style=cIsDamage>{StinkDamage * 100}%</style> <style=cStack>[+ {StinkDamageStack * 100}% per stack]</style>" +
                                                            $" TOTAL damage over 4 seconds.";

        public override string ItemLore => "the peak of biological warfare";

        public override ItemTier Tier => ItemTier.Tier1;

        public override string ItemModelPath => "Assets/ItemTests/Models/Prefabs/Items/StinkyBomb.prefab";
        public override string ItemIconPath => "Assets/ItemTests/Textures/Icons/Items/StinkyBombIcon.png";

        public static ItemDef StinkyBomb = ScriptableObject.CreateInstance<ItemDef>();

        public static BuffDef StinkBombBuff { get; private set; }
        public static DotController.DotIndex StinkDot { get; private set; }

        public GameObject StinkProjectilePrefab;
        public GameObject StinkEffectPrefab1;
        public GameObject StinkEffectPrefab2;
        private EffectData StinkEffect;

        public static float ProcChance = 10f;  //15
        public static float ProcStack = 10f;
        public static float StinkDamage = 1.7f;
        public static float StinkDamageStack = 1f;
        public static float StinkDuration = 4f;
        public static float StinkRadius = 10f;


        public override void CreateConfig(ConfigFile config)
        {

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = Main.Assets.LoadAsset<GameObject>(ItemModelPath);

            var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {

                new RoR2.ItemDisplayRule
               {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }

            });

            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }


        public static void SetupAttributes()
        {

            //Item.SetupAttributes();
            StinkBombBuff = ScriptableObject.CreateInstance<BuffDef>();
            StinkBombBuff.buffColor = Color.yellow;
            StinkBombBuff.canStack = true;
            StinkBombBuff.isDebuff = true;
            StinkBombBuff.name = "StinkyBombBuff";
            //StinkBombBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            StinkBombBuff.iconSprite = Main.Assets.LoadAsset<Sprite>("Assets/ItemTests/Textures/Icons/Buffs/StinkyIcon.png");
            ContentAddition.AddBuffDef(StinkBombBuff);
            DotController.DotDef dotDef = new DotController.DotDef
            {
                interval = 1f,
                damageCoefficient = 1f / StinkDuration,
                damageColorIndex = DamageColorIndex.Poison,
                associatedBuff = StinkBombBuff
            };
            StinkDot = DotAPI.RegisterDotDef(dotDef, null, null);
            
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            //CreateItemDisplayRules();
            StinkProjectilePrefab = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/Base/MiniMushroom/SporeGrenadeProjectileDotZone.prefab").WaitForCompletion();
            StinkProjectilePrefab.GetComponent<ProjectileController>().procCoefficient = 0f;
            //StinkEffectPrefab1 = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/Base/MiniMushroom/SporeGrenadeGasImpact.prefab").WaitForCompletion();
            StinkEffectPrefab1 = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/Junk/Bandit/SmokescreenEffect.prefab").WaitForCompletion();
            //StinkEffectPrefab2 = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/Base/Common/VFX/MuzzleflashSmokeRing.prefab").WaitForCompletion();

            StinkEffectPrefab2 = Main.Assets.LoadAsset<GameObject>("Assets/ItemTests/Models/Prefabs/VFX/stinkPoof.prefab");
            StinkEffectPrefab2.AddComponent<EffectComponent>();
            StinkEffect = new EffectData
            {
                scale = (StinkRadius * 0.6f)
            };
            StinkEffectPrefab2.GetComponent<EffectComponent>().applyScale = true;
            StinkEffectPrefab2.AddComponent<VFXAttributes>();
            ContentAddition.AddEffect(StinkEffectPrefab2);

            //StinkEffectPrefab2.AddComponent<NetworkIdentity>();
            CreateLang();
            SetupAttributes();
            CreateItem(StinkyBomb);
            Hooks();
            
        }


        //////////////////////////----------------------------------------------------------------------------
        //////////////////////////----------------------------------------------------------------------------

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig.Invoke(self, damageInfo, victim);
            
            if ( victim && damageInfo.attacker && damageInfo.procCoefficient > 0f)
            {

                //CharacterBody component = victim.GetComponent<CharacterBody>();
                //CharacterBody component2 = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody characterBody;
                CharacterBody characterBody2;
                HealthComponent healthComponent;

                if (victim.TryGetComponent<CharacterBody>(out characterBody) && damageInfo.attacker.TryGetComponent<CharacterBody>(out characterBody2)
                    && victim.TryGetComponent<HealthComponent>(out healthComponent))
                {
                    
                    if (damageInfo.attacker.GetComponent<CharacterBody>().inventory)
                    {
                        CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                        CharacterBody attacker = damageInfo.attacker.GetComponent<CharacterBody>();
                        int itemCount = attacker.inventory.GetItemCount(StinkyBomb);

                        if (itemCount > 0 && Util.CheckRoll(ProcChance * damageInfo.procCoefficient))
                        {
                            //FireBomb(victimBody, attacker, damageInfo);
                            SpawnStink(victimBody, attacker, damageInfo, StinkBombBuff, itemCount);
                        }
                        
                    }
                }
            }
        }

        private void FireBomb(CharacterBody victim, CharacterBody attacker, DamageInfo damageInfo)
        {
            /*
            GameObject stinkBomb = null;
            GameObject gameObject = Resources.Load<GameObject>("prefabs/projectiles/EngiMine");
            stinkBomb = PrefabAPI.InstantiateClone(gameObject, "FootMine", true);
            UnityEngine.Object.Destroy(stinkBomb.GetComponent<ProjectileDeployToOwner>());
            */


            //bool alive = victim.healthComponent.alive;
            //float num11 = 5f;
            Vector3 position = damageInfo.position;
            Vector3 forward = victim.corePosition - position;
            float magnitude = forward.magnitude;
            Quaternion rotation = (magnitude != 0f) ? Util.QuaternionSafeLookRotation(forward) : UnityEngine.Random.rotationUniform;
            float damage = Util.OnHitProcDamage(damageInfo.damage, attacker.damage, StinkDamage);
            StinkProjectilePrefab.GetComponent<ProjectileController>().procCoefficient = 0f;
            ProjectileManager.instance.FireProjectile(StinkProjectilePrefab, position, rotation, damageInfo.attacker, damage, 0f, damageInfo.crit, DamageColorIndex.Item, null);
            //ProjectileManager.instance.FireProjectile(StinkProjectilePrefab, position, rotation, damageInfo.attacker, damage, 0f, damageInfo.crit, DamageColorIndex.Item, null, alive ? (magnitude * num11) : -1f);



            /*
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = BombModelPath,
                position = position,
                rotation = Util.QuaternionSafeLookRotation(this.DetermineFacing(mNum)),
                procChainMask = procChainMask2,
                target = victim,
                owner = gameObject,
                damage = damage,
                crit = damageInfo.crit,
                force = 200f,
                damageColorIndex = DamageColorIndex.Item
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);*/

            /*
            FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
            fireProjectileInfo.projectilePrefab = StinkProjectilePrefab;
            fireProjectileInfo.rotation = Quaternion.identity;
            fireProjectileInfo.owner = attacker.gameObject;
            fireProjectileInfo.damage = attacker.damage * FistDamageMult;
            fireProjectileInfo.force = FistForce;
            fireProjectileInfo.crit = owner.RollCrit();
            fireProjectileInfo.fuseOverride = delay;
            fireProjectileInfo.procChainMask = default(ProcChainMask);

            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            */
        }

        private void SpawnStink(CharacterBody victimBody, CharacterBody attackerBody, DamageInfo damageInfo, BuffDef stinkBombBuff, int itemCount)
        {
            float radius = StinkRadius;
            Vector3 hitPos = damageInfo.position;
            SphereSearch sphereSearch = new SphereSearch();
            List<HurtBox> targets = new List<HurtBox>();

            sphereSearch.origin = hitPos;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.radius = radius;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(attackerBody.teamComponent.teamIndex));
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            //sphereSearch.OrderCandidatesByDistance();
            sphereSearch.GetHurtBoxes(targets);
            sphereSearch.ClearCandidates();


            for (int i = 0; i < targets.Count; i++)
            {
                HurtBox hurtBox = targets[i];
                HealthComponent healthComp = hurtBox.healthComponent;
                if (healthComp != null && itemCount > 0)
                {
                    //float dmgValue = (float)itemCount * 1f * attackerBody.damage;
                    float dmgValue = (1.7f + (itemCount - 1)) * damageInfo.damage;
                    InflictDotInfo inflictDotInfo = new InflictDotInfo
                    {
                        victimObject = hurtBox.healthComponent.gameObject,
                        attackerObject = damageInfo.attacker,
                        //totalDamage = new float?(dmgValue),
                        
                        dotIndex = StinkDot,
                        damageMultiplier = (damageInfo.damage / attackerBody.damage)    //get total dmg
                                                * (1.7f + (itemCount - 1)),             //dmg coefficient
                        duration = StinkDuration
                    };
                    LogInfo("damageinfo.damage" + damageInfo.damage);
                    LogInfo("attackerBody.damage" + attackerBody.damage);
                    //DotController.InflictDot(healthComp.gameObject, damageInfo.attacker, StinkDot, StinkDuration, 1f, null);
                    //damageInfo.procChainMask.AddProc(ProcType.BleedOnHit);
                    DotController.InflictDot(ref inflictDotInfo);
                }
            }

            /*
            EffectManager.SpawnEffect(StinkEffectPrefab1, new EffectData
            {
                origin = corePosition,
                scale = radius - 1f,
                //rotation = Util.QuaternionSafeLookRotation(report.damageInfo.force)
            }, true);*/


            StinkEffect.origin = hitPos;


            //foreach (GameObject child in StinkEffectPrefab2.transform)
            //{
            //    child.AddComponent<EffectComponent>();
            //    child.GetComponent<EffectComponent>().applyScale = true;
            //}

            //EffectManager.SpawnEffect(StinkEffectPrefab2, new EffectData
            //{
            //    origin = hitPos,
            //    scale = radius * 4,
            //    //rotation = Util.QuaternionSafeLookRotation(report.damageInfo.force)
            //}, true);

            EffectManager.SpawnEffect(StinkEffectPrefab2, StinkEffect, true);


            LogInfo("particle position: " + hitPos);
        }

        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(StinkyBomb.itemIndex), transform.position, transform.forward * 20f);
            }
        }

    }
}
