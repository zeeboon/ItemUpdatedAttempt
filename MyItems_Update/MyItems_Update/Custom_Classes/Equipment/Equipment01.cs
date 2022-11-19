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

namespace MyItems_Update.Custom_Classes.Equipment
{
    class Equipment01 : EquipmentBase
    {  
        public override string EquipmentName => "Stone Gauntlet";

        public override string EquipmentLangTokenName => "STONEGAUNTLET";

        public override string EquipmentPickupDesc => "Unleash titanic wrath";

        public override string EquipmentFullDescription => $"Summon 8 titan fists in a circle around you, each dealing <style=cIsDamage>{FistDamageMult * 100}%</style> damage";

        public override string EquipmentLore => "catch these hands";

        public override string EquipmentModelPath => "Assets/ItemTests/Models/Prefabs/Equipment/StoneGauntlet.prefab";

        public override string EquipmentIconPath => "";

        public override float Cooldown => 60f;  

        public static EquipmentDef EquipItemDef = ScriptableObject.CreateInstance<EquipmentDef>();

        //private string FistModelPath = "Prefabs/Effects/TitanFistEffect";
        private string FistModelPath = "prefabs/projectiles/TitanPreFistProjectile";

        public GameObject FistProjectilePrefab;
        public float FistForce = 6000f;
        public float FistDamageMult = 20f;
        public float FistDelay = 0.5f;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            EquipmentBodyModelPrefab = Main.Assets.LoadAsset<GameObject>(EquipmentModelPath);

            var itemDisplay = EquipmentBodyModelPrefab.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(EquipmentBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {

                new RoR2.ItemDisplayRule
               {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }

            });

            return rules;
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            //CreateItemDisplayRules();
            CreateLang();
            FistProjectilePrefab = Resources.Load<GameObject>(FistModelPath);
            CreateEquipment(EquipItemDef);
            //AddToDroplist();
            //pickupIndex = cooldownItem.itemIndex;
            Hooks();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            //LogInfo("ACTIVATE EQUIP");
            
            CharacterBody player = slot.characterBody;
            var transform = player.transform;

            FistPunch(transform.position, FistDelay, player);
            return true;

        }

        private void FistPunch(Vector3 position, float delay, CharacterBody owner) 
        {
            float spacing = (owner.radius / 2) + 8;
            FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
            fireProjectileInfo.projectilePrefab = FistProjectilePrefab;
            fireProjectileInfo.rotation = Quaternion.identity;
            fireProjectileInfo.owner = owner.gameObject;
            fireProjectileInfo.damage = owner.damage * FistDamageMult;
            fireProjectileInfo.force = FistForce;
            fireProjectileInfo.crit = owner.RollCrit();
            fireProjectileInfo.fuseOverride = delay;
            fireProjectileInfo.procChainMask = default(ProcChainMask);

            float raycastDistance = 11f;
            Vector3 collision;
            RaycastHit hit;

            /*
            for (int i = 0; i < num; i++)
            {
                float radians = 2 * Mathf.PI / num * i;
                float xAxis = Mathf.Sin(radians);
                float zAxis = Mathf.Cos(radians);
                Vector3 spawnDir = new Vector3(xAxis, 0, zAxis);
                Vector3 spawnPos = position + spawnDir * spacing;
                spawnPos.y += raycastDistance / 2;

                bool didHit = false;
                var ray = new Ray(spawnPos, Vector3.down);
                if (Physics.Raycast(ray, out hit, raycastDistance + 3, LayerIndex.world.mask))
                {
                    didHit = true;
                }
                if (didHit == false)
                {
                    if (Physics.Raycast(ray, out hit, raycastDistance + 3, LayerIndex.debris.mask))
                    {
                        didHit = true;
                    }
                }
                if (didHit == true)
                {
                    collision = hit.point;
                    fireProjectileInfo.position = collision;
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }*/

            var numberOfObjects = 8;
            var totalDegrees = 360f;
            var playerForward = owner.inputBank.aimDirection;
            //Get the rotation per step by dividing the total degrees by the number of objects to be placed, we want this rotation to be around the Body.up axis or Vector3.up if the Body.up axis produces some unexpected changes in rotation if its value isn't locked
            var rotation = Quaternion.AngleAxis(totalDegrees / numberOfObjects, Vector3.up);
            //rotate the playerForward vector

            LogInfo($"{position}");
            for (int i = 0; i < numberOfObjects; i++)
            {
                //multiply the current playerForward variable by the rotation, returning a vector rotated by the amount above
                //store the value back in playerForward so that the next time you multiply the rotation by the playerForward its rotation accumulates
                playerForward = rotation * playerForward;
                var placementPosition = position + playerForward * spacing;
                placementPosition.y += raycastDistance / 2;
                // spawn stuff etc
                bool didHit = false;
                var ray = new Ray(placementPosition, Vector3.down);
                if (Physics.Raycast(ray, out hit, raycastDistance + 3, LayerIndex.world.mask))
                {
                    didHit = true;
                    LogInfo("WORLD HIT");
                }
                if (didHit == false)
                {
                    if (Physics.Raycast(ray, out hit, raycastDistance + 3, LayerIndex.debris.mask))
                    {
                        didHit = true;
                        LogInfo("DEBRIS HIT");
                    }
                }
                if (didHit == true)
                {
                    collision = hit.point;
                    fireProjectileInfo.position = collision;
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    LogInfo("FIST SPAWNED");
                }
            }
        }

        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                LogInfo($"itemname: {EquipItemDef.nameToken}");
                LogInfo($"item index: {PickupCatalog.FindPickupIndex(EquipItemDef.equipmentIndex)}");
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(EquipItemDef.equipmentIndex), transform.position, transform.forward * 20f);
            }
        }
        
        //private void AddToDroplist()
        //{
            
        //    CharacterBody droppingBodyBodyComponent = Resources.Load<GameObject>("Prefabs/CharacterBodies/TitanBody").GetComponent<CharacterBody>();

        //    GameObject gameObject = droppingBodyBodyComponent.gameObject;
        //    //gameObject.bos
        //    DeathRewards component = gameObject.GetComponent<DeathRewards>();
        //    PickupIndex pickupIndex = (PickupIndex)component.bossPickup;
        //    if (component)
        //    {

        //        if (component.bossPickup.pickupName == string.Empty)
        //        {

        //        }
        //        pickupIndex = PickupCatalog.FindPickupIndex(EquipItemDef.equipmentIndex);
        //        PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
        //        if (pickupDef != null)
        //        {
        //            component.bossPickup.pickupName = pickupDef.internalName;
        //        }

        //    }
        //    //RoR2.BossGroup.bos
        //}
        
    }
}
