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
using TILER2;

namespace MyItems_Update.Custom_Classes.Items
{
    class Item04 : ItemBase
    {
        public override string ItemName => "Deaths Doorhandle";

        public override string ItemLangTokenName => "DOORHANDLE";

        public override string ItemPickupDesc => "Falling below half health increases movement speed, attack speed and chance to crit";

        public override string ItemFullDescription => $"<style=cDeath>Being hit while below {HealthPercentage}% health</style> will <style=cIsUtility>increase your movement speed and" +
                                                        $"attack speed by {MoveSpeed * 100}%</style> and <style=cIsDamage> chance to crit by {CritChance}%</style> " +
                                                        $"<style=cStack>[+{CritStack}% per stack]</style> for {BuffDuration} seconds <style=cStack>[+{DurationStack} per stack.]</style>";
                                                        

        public override string ItemLore => "you are the one who knocks";

        public override ItemTier Tier => ItemTier.Tier2;

        public override string ItemModelPath => "";

        public override string ItemIconPath => "";

        public static ItemDef DeathItem = ScriptableObject.CreateInstance<ItemDef>();
        public static BuffDef DeathItemBuff { get; private set; }

        public float CritChance = 50f;
        public float CritStack = 10f;
        public float MoveSpeed = 0.5f;
        public float AttackSpeed = 0.5f;
        public float HealthPercentage = 50f;
        public float BuffDuration = 6f;
        public float DurationStack = 3f;

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
            //On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;

            RecalculateStatsAPI.GetStatCoefficients += RecalcStats_GetStatCoefficients;
            
        }

        public static void SetupAttributes()
        {

            //Item.SetupAttributes();
            DeathItemBuff = ScriptableObject.CreateInstance<BuffDef>();
            DeathItemBuff.buffColor = Color.red;
            DeathItemBuff.canStack = false;
            DeathItemBuff.isDebuff = false;
            //DeathItemBuff.name = T2Module.modInfo.shortIdentifier + "SnakeEyes";
            DeathItemBuff.name = "DeathItemBuff";
            DeathItemBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            //BuffAPI.Add(new CustomBuff(DeathItemBuff));
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            //CreateItemDisplayRules();
            CreateLang();
            SetupAttributes();
            CreateItem(DeathItem);
            Hooks();
            //pickupName = ItemName;
        }

        //////////////////////////----------------------------------------------------------------------------
        //////////////////////////----------------------------------------------------------------------------

        private void RecalcStats_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            
            if (sender.inventory)
            {
                int itemCount = sender.inventory.GetItemCount(DeathItem);

                if (sender.HasBuff(DeathItemBuff) && itemCount > 0)
                {
                    args.critAdd += CritChance + (CritStack * (itemCount-1));
                    args.moveSpeedMultAdd += MoveSpeed;
                    args.attackSpeedMultAdd += AttackSpeed;

                    LogInfo("HAS BUFF");
                }
            }
        }

        /*
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            
            if (self.inventory)
            {
                int itemCount = self.inventory.GetItemCount(DeathItem);

                if (self.HasBuff(DeathItemBuff) && itemCount > 0)
                {
                    RecalculateStatsAPI.StatHookEventArgs character = self;


                    character.critAdd += CritChance;
                    character.moveSpeed *= MoveSpeed;
                    character.attackSpeed *= AttackSpeed;

                    
                }
            }
            LogInfo("NORMAL RECALC");
        }
        */
        // maybe change to RecalculateStats if I want to have buff active as long as you're under the threshold, instead of only timed
        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);

            if (self.GetComponent<CharacterBody>().inventory)
            {
                CharacterBody victim = self.GetComponent<CharacterBody>();
                float itemCount = victim.GetComponent<CharacterBody>().inventory.GetItemCount(DeathItem);

                if (victim.inventory.GetItemCount(DeathItem) > 0 && victim.healthComponent.health <= (victim.healthComponent.fullHealth/100f) * HealthPercentage)
                {
                    victim.AddTimedBuff(DeathItemBuff, BuffDuration + (DurationStack * (itemCount-1)));
                }
            }
        }

        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //LogInfo(PickupCatalog.FindPickupIndex(iceDeathItem.itemIndex));
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(DeathItem.itemIndex), transform.position, transform.forward * 20f);
            }
        }

    }
}
