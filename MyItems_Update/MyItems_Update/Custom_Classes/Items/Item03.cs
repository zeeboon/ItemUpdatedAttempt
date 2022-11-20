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
    class Item03 : ItemBase
    {
        public override string ItemName => "Champion Fungus";

        public override string ItemLangTokenName => "CHUNGUS";

        public override string ItemPickupDesc => "Killing an enemy heals you for a percentage of the damage done";
         
        public override string ItemFullDescription => $"<style=cIsDamage>Killing an enemy</style> heals you for <style=cIsHealing>{HealPercentage}%</style>" +
                                                        $" <style=cStack>[+{HealStackPercentage}% per stack]</style> of the health they had left.";

        public override string ItemLore => "a big cartoon mushroom";

        public override ItemTier Tier => ItemTier.Tier2;

        public override string ItemModelPath => "Assets/ItemTests/Models/Prefabs/Items/Chungus.prefab";
        public override string ItemIconPath => "Assets/ItemTests/Textures/Icons/Items/ChungusIcon.png";

        public static ItemDef KillHeal = ScriptableObject.CreateInstance<ItemDef>();

        private readonly float HealPercentage = 12f;
        private readonly float HealStackPercentage = 12f;
        //private bool IsFullHealth = false;
        //private float EnemyHealth = 0f;

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
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_takeDamage;
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            //CreateItemDisplayRules();
            CreateLang();
            CreateItem(KillHeal);
            Hooks();
            //pickupName = ItemName;

        }

        //////////////////////////----------------------------------------------------------------------------
        //////////////////////////----------------------------------------------------------------------------


        private void HealthComponent_takeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            /*
            if (damageInfo != null && self != null && damageInfo.attacker.GetComponent<CharacterBody>().inventory)
            {
                int itemCount = damageInfo.attacker.GetComponent<CharacterBody>().inventory.GetItemCount(KillHeal);

                if (itemCount > 0)
                {
                    CharacterBody victimBody = self ? self.GetComponent<CharacterBody>() : null;

                    float fullHealth = victimBody.healthComponent.fullCombinedHealth;
                    float currentHealth = victimBody.healthComponent.combinedHealth;

                    LogInfo($"{fullHealth}");
                    LogInfo($"{currentHealth}");


                    if (fullHealth == currentHealth)
                    {
                        IsFullHealth = true;
                        LogInfo("IS FULL HEALTH");
                        EnemyHealth = fullHealth;
                    }
                    else
                    {
                        IsFullHealth = false;
                        LogInfo("IS NOT FULL HEALTH");
                    }
                }
                

            }
            */


            int itemCount = 0;
            float currentHealth = 0f;
            bool killHeal = false;

            if (damageInfo != null && self != null && damageInfo.attacker != null)
            {
                if (damageInfo.attacker.GetComponent<CharacterBody>().inventory)
                {
                    itemCount = damageInfo.attacker.GetComponent<CharacterBody>().inventory.GetItemCount(KillHeal);

                    if (itemCount > 0)
                    {
                        CharacterBody victimBody = self ? self.GetComponent<CharacterBody>() : null;
                        currentHealth = victimBody.healthComponent.combinedHealth;
                        LogInfo($"KILL HEAL");
                        killHeal = true;
                    }
                }


            }

            orig(self, damageInfo);
            

            if (self.GetComponent<CharacterBody>().healthComponent.combinedHealth <= 0 && killHeal == true && damageInfo.attacker.GetComponent<CharacterBody>().inventory.GetItemCount(KillHeal) > 0)
            {
                ProcChainMask procChainMask = damageInfo.procChainMask;

                float healAmount = (currentHealth / 100f) * (HealPercentage + (HealStackPercentage * itemCount));

                damageInfo.attacker.GetComponent<CharacterBody>().healthComponent.Heal(healAmount, procChainMask, true);
                LogInfo($"HEALED FOR {healAmount}");
                killHeal = false;
            }

                /*
                if (IsFullHealth)
                {
                    int itemCount = damageInfo.attacker.GetComponent<CharacterBody>().inventory.GetItemCount(KillHeal);
                    if (self.GetComponent<CharacterBody>().healthComponent.combinedHealth <= 0 && itemCount > 0)
                    {

                        LogInfo("INSTAKILL");

                        ProcChainMask procChainMask = damageInfo.procChainMask;
                        float healAmount = (EnemyHealth / 100f) * (HealPercentage + (HealStackPercentage * itemCount));

                        damageInfo.attacker.GetComponent<CharacterBody>().healthComponent.Heal(healAmount, procChainMask, true);
                        LogInfo($"HEALED FOR {healAmount}");

                    }
                    else
                    {
                        LogInfo("NOT INSTAKILL");
                    }
                    IsFullHealth = false;
                }*/
            }

        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.F4))
            {

                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //LogInfo(PickupCatalog.FindPickupIndex(iceDeathItem.itemIndex));
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(KillHeal.itemIndex), transform.position, transform.forward * 20f);

            }
        }
    }
}
