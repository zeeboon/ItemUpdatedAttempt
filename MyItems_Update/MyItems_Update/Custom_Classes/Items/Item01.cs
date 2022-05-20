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
    class Item01 : ItemBase
    {
        public override string ItemName => "reset item";

        public override string ItemLangTokenName => "COOLDOWNRESET";

        public override string ItemPickupDesc => "Reset cooldowns at the cost of health";

        public override string ItemFullDescription => "Using an ability while it's on cooldown will reset its cooldown at the cost of your health";

        public override string ItemLore => "woowee it's lore";

        public override ItemTier Tier => ItemTier.Lunar;

        public override string ItemModelPath => "Prefabs/PickupModels/PickupMystery";

        public override string ItemIconPath => "Textures/MiscIcons/texMysteryIcon";

        private static bool TriggerItem = false;
        private static float CooldownTimer = 0f;
        public static BuffDef CooldownBuff { get; private set; }

        private static CharacterBody CharBody = null;
        private static GenericSkill CharSkill = null;

        private static List<CharacterBody> CharBodiesList = new List<CharacterBody>();
        public static ItemDef cooldownItem = ScriptableObject.CreateInstance<ItemDef>();
        //private static string pickupName;
        //private static ItemIndex pickupIndex;

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
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalcStats;

            //On.RoR2.CharacterBody.CmdOnSkillActivated += CharacterBody_CmdOnSkillActivated;
            //On.RoR2.CharacterBody.
        }


        public static void SetupAttributes()
        {

            //Item.SetupAttributes();
            CooldownBuff = ScriptableObject.CreateInstance<BuffDef>();
            CooldownBuff.buffColor = Color.blue;
            CooldownBuff.canStack = false;
            CooldownBuff.isDebuff = false;
            //CooldownBuff.name = T2Module.modInfo.shortIdentifier + "SnakeEyes";
            CooldownBuff.name = "CooldownBuff";
            CooldownBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            //BuffAPI.Add(new CustomBuff(CooldownBuff));
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            //CreateItemDisplayRules();
            CreateLang();
            SetupAttributes();
            CreateItem(cooldownItem);
            //pickupIndex = cooldownItem.itemIndex;
            Hooks();
            //pickupName = "ITEM_"  + ItemLangTokenName + "_NAME";

        }


        //////////////////////////----------------------------------------------------------------------------
        //////////////////////////----------------------------------------------------------------------------

        /*
        private void CharacterBody_CmdOnSkillActivated(On.RoR2.CharacterBody.orig_CmdOnSkillActivated orig, CharacterBody self, sbyte skillIndex)
        {
            throw new NotImplementedException();
        }*/

        private void CharacterBody_RecalcStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (!self)
                return ;
            
            if (self.inventory)
            {
                int itemCount = self.inventory.GetItemCount(cooldownItem.itemIndex);

                if (itemCount > 0 && TriggerItem == true)
                {
                    CharSkill = CharBody.skillLocator.special;
                    
                    float maxCooldown = CharSkill.skillDef.baseRechargeInterval;
                    float cooldown = maxCooldown - CharSkill.rechargeStopwatch;
                    float maxDamage = self.healthComponent.fullCombinedHealth / 2;

                    float damageFract = cooldown / maxCooldown;


                    float damage = maxDamage * damageFract;
                    float currentHP = self.healthComponent.combinedHealth;

                    /*
                    LogInfo($"recharge stopwatch {CharSkill.rechargeStopwatch}");
                    LogInfo($"base interval {CharSkill.skillDef.baseRechargeInterval}");
                    LogInfo($"cooldown {cooldown}");
                    LogInfo($"max hp {self.healthComponent.fullCombinedHealth}");
                    LogInfo($"max dmg {maxDamage}");
                    LogInfo($"dmg fract {cooldown} / {maxCooldown} = {damageFract}");
                    LogInfo($"dmg {maxDamage} * {damageFract} = {damage}");
                    */

                    /*
                    max = 50

                    cd = 3
                    maxCd = 12

                    dmg = 12,5

                    */


                    var dmg = new DamageInfo
                    {
                        attacker = self.gameObject,
                        crit = false,
                        damage = damage*2,
                        inflictor = null,
                        position = self.footPosition,
                        force = new Vector3(0, 0, 0),
                        damageType = DamageType.NonLethal | DamageType.BypassArmor,
                        rejected = false,
                        //procChainMask = default(ProcChainMask)

                    };
                    Vector3 corePosition = self.corePosition;
                    //EntityStates.VagrantMonster.ExplosionAttack.novaEffectPrefab
                    //EntityStates.GlobalSkills.LunarDetonator.Detonate.detonationEffectPrefab
                    //EntityStates.Interactables.GoldBeacon.Ready.activationEffectPrefab
                    EffectManager.SpawnEffect(EntityStates.Interactables.GoldBeacon.Ready.activationEffectPrefab, new EffectData
                    {
                        origin = corePosition,
                        scale = 20f,
                        rotation = self.coreTransform.rotation
                    }, true);

                    TriggerItem = false;
                    self.healthComponent.TakeDamage(dmg);
                    //LogInfo($"current health {self.healthComponent.combinedHealth}");
                    //LogInfo($"damage done {currentHP - self.healthComponent.combinedHealth}");



                }
            }
        }
        
        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig(self, skill);

            if (!self)
                return;
            
            if (self.inventory && self.inventory.GetItemCount(cooldownItem.itemIndex) > 0 && self.isPlayerControlled)
            {

                CharBodiesList.Add(self);
                //LogInfo($"cooldown timer {CooldownTimer}");
                LogInfo(self);

                if (skill == self.skillLocator.special)
                {
                    self.AddTimedBuff(CooldownBuff, 0.25f);
                    TriggerItem = false;
                }

                /*
                if (self.HasBuff(CooldownBuff))
                {
                    TriggerItem = false;
                    //CooldownTimer = 0.5f;
                }*/

            }
        }

        public static void Update()
        {
            //LogInfo("TESTTESTTEST");
            if (Input.GetKeyDown(KeyCode.F2))
            {
                //Get the player body to use a position:	
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //LogInfo(PickupCatalog.FindPickupIndex(cooldownItem.itemIndex));
                
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(cooldownItem.itemIndex), transform.position, transform.forward * 20f);
            }
            /*
            if (CooldownTimer > 0f)
            {
                CooldownTimer -= Time.deltaTime;
                if (CooldownTimer < 0f)
                {
                    CooldownTimer = 0f;
                }
            }*/
            
            for (int i = 0; i < CharBodiesList.Count; i++)
            {
                if (CharBodiesList[i] == null || CharBodiesList[i].inventory.GetItemCount(cooldownItem.itemIndex) <= 0)
                {
                    CharBodiesList.RemoveAt(i);
                }

                
                else
                {
                    CharBody = CharBodiesList[i];
                    //LogInfo(CharBody);
                    CharSkill = CharBody.skillLocator.special;

                    if (CharBody.inputBank.skill4.down == true && CharSkill != null && CharBody.inventory )
                    {
                        
                        int itemCount = CharBody.inventory.GetItemCount(cooldownItem.itemIndex);

                        //LogInfo($"attempt {CharSkill.CanExecute()}");

                        //LogInfo($"cooldown timer {CooldownTimer}");

                        bool notBusy = (!CharSkill.stateMachine.HasPendingState() && !CharSkill.skillDef.IsAlreadyInState(CharSkill));
                        
                        if (itemCount > 0 && !CharBody.HasBuff(CooldownBuff) && notBusy)
                        {
                            
                            //CharacterBody_RecalcStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, self);
                            TriggerItem = true;
                            CharBody.RecalculateStats(); //dmg user

                            CharSkill.AddOneStock();
                            //LogInfo("cooldown reset");
                            bool canExecute = CharSkill.CanExecute();
                            if (canExecute)
                            {
                                CharSkill.ExecuteIfReady();
                            }
                            
                            


                            CharBody.AddTimedBuff(CooldownBuff, 4f);

                        }
                    }
                }
            }
            

            /*
            if (CharBody!=null)
            {
                CharSkill = CharBody.skillLocator.special;
                if (CharBody.inputBank.skill4.down == true && CharSkill!=null)
                {
                    if (CharBody.inventory)
                    {
                        int itemCount = CharBody.inventory.GetItemCount(itemDef.itemIndex);
                        bool canActivate = CharSkill.IsReady();

                        LogInfo($"attempt {canActivate}");
                        LogInfo($"cooldown timer {CooldownTimer}");

                        if (itemCount > 0 && CooldownTimer == 0f)
                        {
                            if (CharSkill.CanExecute())
                            {
                                CooldownTimer = 0.5f;
                                TriggerItem = false;
                                LogInfo("cooldown not reset");
                            }
                            else
                            {
                                //CharacterBody_RecalcStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, self);
                                TriggerItem = true;
                                CharBody.RecalculateStats();

                                CharSkill.AddOneStock();
                                LogInfo("cooldown reset");
                                CooldownTimer = 4f;
                            }
                            //LogInfo("press button");
                        }
                    }
                }
            }
            */




        }
        

    }
}
