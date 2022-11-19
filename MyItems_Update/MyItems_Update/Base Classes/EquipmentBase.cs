using BepInEx.Configuration;
using R2API;
using RoR2;
using MyItems_Update.Utils;
using UnityEngine;
using static MyItems_Update.Utils.Log;

namespace MyItems_Update.Base_Classes
{
    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }

        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }

        public static GameObject EquipmentBodyModelPrefab;

        public virtual bool AppearsInSinglePlayer { get; } = true;

        public virtual bool AppearsInMultiPlayer { get; } = true;

        public virtual bool CanDrop { get; } = true;

        public virtual float Cooldown { get; } = 60f;

        public virtual bool EnigmaCompatible { get; } = true;

        public virtual bool IsBoss { get; } = false;

        public virtual bool IsLunar { get; } = false;

        public static EquipmentDef EquipmentDef;

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        public abstract void Init(ConfigFile config);

        protected virtual void CreateConfig(ConfigFile config){}


        protected virtual void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }

        protected void CreateEquipment(EquipmentDef equipDef)
        {
            equipDef.name = "EQUIPMENT_" + EquipmentLangTokenName;
            equipDef.nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME";
            equipDef.pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP";
            equipDef.descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION";
            equipDef.loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE";


            //equipDef.pickupIconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            //equipDef.pickupModelPrefab = Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

            //----------    
            if (EquipmentModelPath == "")
            {

                equipDef.pickupModelPrefab = Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
            }
            else
            {
                equipDef.pickupModelPrefab = Main.Assets.LoadAsset<GameObject>(EquipmentModelPath);
                //string prefab = "Assets/ItemTests/Models/prefabs/Item/item1/Item_1.prefab";

            }
            if (EquipmentIconPath == "")
            {
                equipDef.pickupIconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            }
            else
            {
                equipDef.pickupIconSprite = Main.Assets.LoadAsset<Sprite>(EquipmentIconPath);
            }

            //---------- 

            equipDef.appearsInSinglePlayer = AppearsInSinglePlayer;
            equipDef.appearsInMultiPlayer = AppearsInMultiPlayer;
            equipDef.canDrop = CanDrop;
            equipDef.cooldown = Cooldown;
            equipDef.enigmaCompatible = EnigmaCompatible;
            equipDef.isBoss = IsBoss;
            equipDef.isLunar = IsLunar;

            var displayRules = new ItemDisplayRuleDict(null);

            EquipmentDef = equipDef;

            ItemAPI.Add(new CustomEquipment(equipDef, displayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }

        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EquipmentDef)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        protected abstract bool ActivateEquipment(EquipmentSlot slot);

        public virtual void Hooks() { }
    }
}
