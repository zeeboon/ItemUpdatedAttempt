using BepInEx.Configuration;
using R2API;
using RoR2;
using MyItems_Update.Base_Classes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static MyItems_Update.Utils.ItemHelper;
using static MyItems_Update.Utils.Log;

namespace MyItems_Update.Custom_Classes.Items
{
    class Item02 : ItemBase
    {
        public override string ItemName => "Liquid Nitrogen";

        public override string ItemLangTokenName => "LIQUID_NITROGEN";

        public override string ItemPickupDesc => "Killing an enemy slows surrounding enemies, with a chance to cause a freezing blast instead";

        public override string ItemFullDescription => $"<style=cIsDamage>Killing an enemy</style> causes surrounding enemies to be <style=cIsUtility>slowed</style> by 50% for {SlowDuration} <style=cStack>[+ {SlowDuration / 2} per stack]</style> seconds" +
                                                        $"\nIn addition, enemies have a {BlastChance}% <style=cStack>[+ {BlastChance / 2}% per stack]</style> chance of <style=cIsDamage>exploding in ice</style>, dealing <style=cIsDamage>{BlastDamageMult * 100}% </style> <style=cStack>[+ {BlastDamageStack * 100} / stack]</style> TOTAL damage and <style=cIsUtility>freezing</style> surrounding enemies.";
        //                                                        +"\n<style=cSub>Enemies killed by the blast always explode.</style>"
        public override string ItemLore => "here's frost in your eye";

        public override ItemTier Tier => ItemTier.Tier2;

        public override string ItemModelPath => "Assets/ItemTests/Models/Prefabs/Items/Nitrogen.prefab";
        public override string ItemIconPath => "Assets/ItemTests/Textures/Icons/Items/NitrogenIcon.png";

        public static ItemDef iceDeathItem = ScriptableObject.CreateInstance<ItemDef>();

        private string ExplosionPath = "Prefabs/Effects/ImpactEffects/IgniteExplosionVFX";
        public GameObject SlowParticlePrefab;
        private EffectData SlowEffect;

        public static float SlowDuration = 3f;
        public static float SlowRadius = 14f; 
        public static float BlastRadius = 10f;
        public static float BlastDamageMult = 2.5f;
        public static float BlastDamageStack = 0.5f;
        public static float BlastChance = 10f;
        public static float BlastStackChance = 5f;

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
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            //CreateItemDisplayRules();
            CreateLang();
            //SlowParticlePrefab = Resources.Load<GameObject>(ExplosionPath);

            SlowParticlePrefab = Main.Assets.LoadAsset<GameObject>("Assets/ItemTests/Models/Prefabs/VFX/nitrogenPoof.prefab");
            SlowParticlePrefab.AddComponent<EffectComponent>();
            SlowEffect = new EffectData
            {
                scale = (SlowRadius * 0.75f)
            };
            SlowParticlePrefab.GetComponent<EffectComponent>().applyScale = true;
            SlowParticlePrefab.AddComponent<VFXAttributes>();
            ContentAddition.AddEffect(SlowParticlePrefab);

            CreateItem(iceDeathItem);
            Hooks();
            //pickupName = ItemName;
            
        }

        //////////////////////////----------------------------------------------------------------------------
        //////////////////////////----------------------------------------------------------------------------


        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            if (!report.attacker || !report.attackerBody)
                return;


            //We need an inventory to check for our item
            if (report.attackerBody.inventory || report.damageInfo.damageType == DamageType.Freeze2s)
            {
                CharacterBody victimBody = report.victimBody;
                CharacterBody attackerBody = report.attackerBody;
                float damage = report.damageDealt;

                int itemCount = attackerBody.inventory.GetItemCount(iceDeathItem.itemIndex);

                if (itemCount > 0 &&
                    Util.CheckRoll(BlastChance + (float)((itemCount-1) * BlastStackChance), attackerBody.master))
                {
                    IceBlast(victimBody, attackerBody, itemCount, damage);
                }
                else if (itemCount > 0)
                {
                    SlowOnDeath(victimBody, attackerBody, itemCount, report);
                }


            }
        }

        private void SlowOnDeath(CharacterBody victimBody, CharacterBody attackerBody, int itemCount, DamageReport report)
        {
            
            
            /*
            GlobalEventManager.igniteOnKillSphereSearch.origin = corePosition;
            GlobalEventManager.igniteOnKillSphereSearch.mask = LayerIndex.entityPrecise.mask;
            GlobalEventManager.igniteOnKillSphereSearch.radius = radius;
            GlobalEventManager.igniteOnKillSphereSearch.RefreshCandidates();
            GlobalEventManager.igniteOnKillSphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(attackerBody.teamComponent.teamIndex));
            GlobalEventManager.igniteOnKillSphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            GlobalEventManager.igniteOnKillSphereSearch.OrderCandidatesByDistance();
            GlobalEventManager.igniteOnKillSphereSearch.GetHurtBoxes(GlobalEventManager.igniteOnKillHurtBoxBuffer);
            GlobalEventManager.igniteOnKillSphereSearch.ClearCandidates();

            for (int i = 0; i < GlobalEventManager.igniteOnKillHurtBoxBuffer.Count; i++)
            {
                HurtBox hurtBox = GlobalEventManager.igniteOnKillHurtBoxBuffer[i];
                if (hurtBox.healthComponent)
                {
                    hurtBox.healthComponent.body.AddTimedBuff(RoR2Content.Buffs.Slow50, (SlowDuration / 2)  + (SlowDuration / 2) * (float)itemCount);
                }
            }
            
            GlobalEventManager.igniteOnKillHurtBoxBuffer.Clear();
            /*new BlastAttack
            {
                radius = num2,
                baseDamage = baseDamage,
                procCoefficient = 0f,
                crit = Util.CheckRoll(attackerBody.crit, attackerBody.master),
                damageColorIndex = DamageColorIndex.Item,
                attackerFiltering = AttackerFiltering.Default,
                falloffModel = BlastAttack.FalloffModel.None,
                attacker = attackerBody.gameObject,
                teamIndex = attackerBody.teamComponent.teamIndex,
                position = corePosition
            }.Fire();*/

            /*
            EffectManager.SpawnEffect(EntityStates.Bandit2.StealthMode.smokeBombEffectPrefab, new EffectData
            {
                origin = corePosition,
                scale = radius+20f,
                rotation = Util.QuaternionSafeLookRotation(report.damageInfo.force)
            }, true);
            EffectManager.SpawnEffect(SlowParticlePrefab, new EffectData
            {
                origin = corePosition,
                scale = radius,
                rotation = Util.QuaternionSafeLookRotation(report.damageInfo.force)
            }, true);
            */
            
            float radius = victimBody.radius + SlowRadius;
            Vector3 corePosition = victimBody.corePosition;
            SphereSearch sphereSearch = new SphereSearch();
            List<HurtBox> targets = new List<HurtBox>();

            sphereSearch.origin = corePosition;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.radius = radius;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(attackerBody.teamComponent.teamIndex));
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.GetHurtBoxes(targets);
            sphereSearch.ClearCandidates();
            

            for (int i = 0; i < targets.Count; i++)
            {
                HurtBox hurtBox = targets[i];
                if (hurtBox.healthComponent)
                {
                    hurtBox.healthComponent.body.AddTimedBuff(RoR2Content.Buffs.Slow50, (SlowDuration / 2) + (SlowDuration / 2) * (float)itemCount);
                }
            }

            
            //EffectManager.SpawnEffect(EntityStates.Bandit2.StealthMode.smokeBombEffectPrefab, new EffectData
            //{
            //    origin = corePosition,
            //    scale = radius+20f,
            //    rotation = Util.QuaternionSafeLookRotation(report.damageInfo.force)
            //}, true);
            //EffectManager.SpawnEffect(SlowParticlePrefab, new EffectData
            //{
            //    origin = corePosition,
            //    scale = radius,
            //    rotation = Util.QuaternionSafeLookRotation(report.damageInfo.force)
            //}, true);
            SlowEffect.origin = corePosition;
            EffectManager.SpawnEffect(SlowParticlePrefab, SlowEffect, true);

        }

        private void IceBlast(CharacterBody victimBody, CharacterBody attackerBody, int itemCount, float damage)
        {
            Vector3 corePosition = Util.GetCorePosition(victimBody.gameObject);
            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/GenericDelayBlast"), corePosition, Quaternion.identity);
            float radius = BlastRadius + victimBody.radius;
            gameObject2.transform.localScale = new Vector3(radius, radius, radius);

            float damageCoefficient = 2.5f * itemCount;
            float newDamage = Util.OnHitProcDamage(damage, attackerBody.damage, damageCoefficient);

            DelayBlast component = gameObject2.GetComponent<DelayBlast>();
            component.position = corePosition;
            //component.baseDamage = attackerBody.damage * BlastDamageMult + (BlastDamageStack * (float)(itemCount -1 ));
            component.baseDamage = newDamage;
            component.baseForce = 2300f;
            component.attacker = attackerBody.gameObject;
            component.radius = radius;
            component.crit = Util.CheckRoll(victimBody.crit, victimBody.master);
            component.procCoefficient = 0.75f;
            component.maxTimer = 0.2f;
            component.falloffModel = BlastAttack.FalloffModel.None;
            component.explosionEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/AffixWhiteExplosion");
            //component.delayEffect = Resources.Load<GameObject>("Prefabs/Effects/AffixWhiteDelayEffect");
            component.damageType = DamageType.Freeze2s;
            gameObject2.GetComponent<TeamFilter>().teamIndex = attackerBody.teamComponent.teamIndex;
        }

        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                	
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //LogInfo(PickupCatalog.FindPickupIndex(iceDeathItem.itemIndex));
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(iceDeathItem.itemIndex), transform.position, transform.forward * 20f);
                
            }
        }
    }
}
