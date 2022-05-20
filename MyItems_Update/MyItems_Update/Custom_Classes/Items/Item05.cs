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
using TILER2;
using UnityEngine.AddressableAssets;

namespace MyItems_Update.Custom_Classes.Items
{
    class Item05 : ItemBase
    {
        public override string ItemName => "Stinky Bomb";

        public override string ItemLangTokenName => "STINKYBOMB";

        public override string ItemPickupDesc => "ITEMDESC";

        public override string ItemFullDescription => "ITEMFULLDESC";

        public override string ItemLore => "ITEMLORE";

        public override ItemTier Tier => ItemTier.Tier1;

        /*= Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
         */
        //myItemDef.deprecatedTier = ItemTier.Tier2;

        public override string ItemModelPath => "";
        

        public override string ItemIconPath => "";

        public static ItemDef StinkyBomb = ScriptableObject.CreateInstance<ItemDef>();

        //private string BombModelPath = 
        public GameObject StinkProjectilePrefab;

        public static float ProcChance = 100f;
        public static float ProcStack = 10f;
        public static float StinkDamage = 0.8f;


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

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            //CreateItemDisplayRules();
            //StinkProjectilePrefab = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/Base/WarCryOnMultiKill/WarCryEffect.prefab").WaitForCompletion();
            LogInfo("trying to load stinky prefab");
            StinkProjectilePrefab = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/Base/MiniMushroom/SporeGrenadeProjectileDotZone.prefab").WaitForCompletion();
            LogInfo("loaded stinky prefab");
            StinkProjectilePrefab.GetComponent<ProjectileController>().procCoefficient = 0f;
            CreateLang();
            CreateItem(StinkyBomb);
            Hooks();

        }


        //////////////////////////----------------------------------------------------------------------------
        //////////////////////////----------------------------------------------------------------------------

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig.Invoke(self, damageInfo, victim);
            
            if ( victim && damageInfo.attacker && damageInfo.procCoefficient >= 0f)
            {

                //CharacterBody component = victim.GetComponent<CharacterBody>();
                //CharacterBody component2 = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody characterBody;
                HealthComponent healthComponent;

                if (victim.TryGetComponent<CharacterBody>(out characterBody) && damageInfo.attacker.TryGetComponent<CharacterBody>(out characterBody)
                    && victim.TryGetComponent<HealthComponent>(out healthComponent))
                {
                    
                    if (damageInfo.attacker.GetComponent<CharacterBody>().inventory)
                    {
                        
                        //CharacterBody attacker = damageInfo.attacker.GetComponent<CharacterBody>();
                        CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                        CharacterBody attacker = damageInfo.attacker.GetComponent<CharacterBody>();
                        float itemCount = attacker.inventory.GetItemCount(StinkyBomb);



                        if (itemCount > 0 && Util.CheckRoll(ProcChance + (ProcStack * (itemCount - 1))))
                        {
                            FireBomb(victimBody, attacker, damageInfo);
                        }
                        
                    }
                }
            }
            

        }

        private void FireBomb(CharacterBody victim, CharacterBody attacker, DamageInfo damageInfo)
        {
            

            
            bool alive = victim.healthComponent.alive;
            float num11 = 5f;
            Vector3 position = damageInfo.position;
            Vector3 forward = victim.corePosition - position;
            float magnitude = forward.magnitude;
            Quaternion rotation = (magnitude != 0f) ? Util.QuaternionSafeLookRotation(forward) : UnityEngine.Random.rotationUniform;
            float damage = Util.OnHitProcDamage(damageInfo.damage, attacker.damage, StinkDamage);
            ProjectileManager.instance.FireProjectile(StinkProjectilePrefab, position, rotation, damageInfo.attacker, damage, 0f, damageInfo.crit, DamageColorIndex.Item, null, alive ? (magnitude * num11) : -1f);
            
            

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

        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7))
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(StinkyBomb.itemIndex), transform.position, transform.forward * 20f);
            }
        }

    }
}
