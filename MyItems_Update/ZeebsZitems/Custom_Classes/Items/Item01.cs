using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using ZeebsZitems.Base_Classes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ZeebsZitems.Utils.ItemHelper;
using static ZeebsZitems.Utils.Log;
using UnityEngine.AddressableAssets;

namespace ZeebsZitems.Custom_Classes.Items
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

        public override ItemTag[] ItemTags { get; } = { ItemTag.Damage };

        public override bool CanRemove { get; } = true;

        public static BuffDef StinkBombBuff { get; private set; }
        public static DotController.DotIndex StinkDot { get; private set; }

        public GameObject StinkProjectilePrefab;
        public GameObject StinkEffectPrefab;
        private EffectData StinkEffect;
        public static ConfigEntry<bool> alwaysShowFX { get; set; }

        public static float ProcChance = 10f; 
        public static float ProcStack = 10f;
        public static float StinkDamage = 1.7f;
        public static float StinkDamageStack = 1f;
        public static float StinkDuration = 4f;
        public static float StinkRadius = 10f;


        public override void CreateConfig(ConfigFile config)
        {
            //alwaysShowFX = config.Bind<bool>("Items", "Always show " + ItemName + " VFX", true, "Might not appear sometimes when multiple enemies and effects are flying around otherwise. \nSet to false if this ends up causing lag or FPS issues.");
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
            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManager_ProcessHitEnemy;
        }

        public static void SetupAttributes()
        {
            StinkBombBuff = ScriptableObject.CreateInstance<BuffDef>();
            StinkBombBuff.buffColor = Color.yellow;
            StinkBombBuff.canStack = true;
            StinkBombBuff.isDebuff = true;
            StinkBombBuff.name = "StinkyBombBuff";
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

            StinkEffectPrefab = Main.Assets.LoadAsset<GameObject>("Assets/ItemTests/Models/Prefabs/VFX/stinkPoof.prefab");
            StinkEffectPrefab.AddComponent<EffectComponent>();
            StinkEffect = new EffectData
            {
                scale = (StinkRadius * 0.6f)
            };
            StinkEffectPrefab.GetComponent<EffectComponent>().applyScale = true;
            StinkEffectPrefab.AddComponent<VFXAttributes>();
            /*if (alwaysShowFX.Value == true)
            {
                StinkEffectPrefab.GetComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            }
            else
            {
                StinkEffectPrefab.GetComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Medium;
            }*/
            StinkEffectPrefab.GetComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Medium;
            StinkEffectPrefab.GetComponent<VFXAttributes>().vfxIntensity = VFXAttributes.VFXIntensity.Low;
            ContentAddition.AddEffect(StinkEffectPrefab);

            CreateLang();
            SetupAttributes();
            CreateItem(StinkyBomb);
            Hooks();
        }


        //////////////////////////----------------------------------------------------------------------------
        //////////////////////////----------------------------------------------------------------------------

        private void GlobalEventManager_ProcessHitEnemy(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig.Invoke(self, damageInfo, victim);

            if (victim && damageInfo.attacker && damageInfo.procCoefficient > 0f)
            {

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
                            SpawnStink(victimBody, attacker, damageInfo, StinkBombBuff, itemCount);
                        }

                    }
                }
            }
        }

        private void SpawnStink(CharacterBody victimBody, CharacterBody attackerBody, DamageInfo damageInfo, BuffDef stinkBombBuff, int itemCount)
        {
            float radius = StinkRadius;
            Vector3 hitPos = damageInfo.position;
            SphereSearch sphereSearch = new SphereSearch();
            List<HurtBox> targets = new List<HurtBox>();

            LogWarning($"stink pos: {hitPos}");
            LogWarning($"targets: {targets}");

            sphereSearch.origin = hitPos;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.radius = radius;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(attackerBody.teamComponent.teamIndex));
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.GetHurtBoxes(targets);
            sphereSearch.ClearCandidates();

            for (int i = 0; i < targets.Count; i++)
            {
                LogWarning($"checking stink for target {i+1}!");
                HurtBox hurtBox = targets[i];
                HealthComponent healthComp = hurtBox.healthComponent;
                if (healthComp != null && itemCount > 0)
                {
                    LogWarning($"has body!: {hurtBox.healthComponent.gameObject}");
                    float dmgValue = (1.7f + (itemCount - 1)) * damageInfo.damage;
                    InflictDotInfo inflictDotInfo = new InflictDotInfo
                    {
                        victimObject = hurtBox.healthComponent.gameObject,
                        attackerObject = damageInfo.attacker,
                        
                        dotIndex = StinkDot,
                        damageMultiplier = (damageInfo.damage / attackerBody.damage)    //get total dmg
                                                * (1.7f + (itemCount - 1)),             //dmg coefficient 170% + 100% /stack
                        duration = StinkDuration
                    };
                    LogWarning($"dotAttacker: {damageInfo.attacker}");
                    LogWarning($"dotIndex: {StinkDot}");
                    LogWarning($"durationDamageMultiplier: {inflictDotInfo.damageMultiplier}");
                    LogWarning($"dotDuration: {inflictDotInfo.duration}");

                    DotController.InflictDot(ref inflictDotInfo);
                    LogWarning($"get stanked on idiot!!!");
                }
            }

            StinkEffect.origin = hitPos;

            EffectManager.SpawnEffect(StinkEffectPrefab, StinkEffect, true);
        }

        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))   //drop pickup 4 testing, disable on release
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(StinkyBomb.itemIndex), transform.position, transform.forward * 20f);
            }
        }

    }
}
