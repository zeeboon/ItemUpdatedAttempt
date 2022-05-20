using BepInEx.Configuration;
using R2API;
using RoR2;
using MyItems_Update.Utils;
using UnityEngine;
using static MyItems_Update.Utils.Log;
using UnityEngine.AddressableAssets;

namespace MyItems_Update.Base_Classes
{

    public abstract class ItemBase
    {
        //sets item name
        public abstract string ItemName { get; }
        //sets lang token
        public abstract string ItemLangTokenName { get; }
        //sets the pickup description
        public abstract string ItemPickupDesc { get; }
        //sets the full description
        public abstract string ItemFullDescription { get; }
        //sets logbook lore
        public abstract string ItemLore { get; }

        //sets item tier
        public abstract ItemTier Tier { get; }

        //public abstract myItemDef.deprecatedTier = ItemTier.Tier2;
        

        //sets item tags. See ItemTag in a tool like dnSpy to learn more.
        public virtual ItemTag[] ItemTags { get; } = { };

        //sets paths for model and icons
        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }

        //determines whether item can be removed
        public virtual bool CanRemove { get; }
        //determines if item is hidden from the game
        public virtual bool Hidden { get; }

        //determines whether or not an unlockable is required
        public virtual bool HasUnlockable { get; }

        //creates an unlockable requirement
        public virtual UnlockableDef ItemPreReq { get; }

        //creates necessary GameObject field for display rules
        public static GameObject ItemBodyModelPrefab;

        //initializes the item
        public abstract void Init(ConfigFile config);

        public abstract void CreateConfig(ConfigFile config);

        //actually creates instance of item
        public static ItemDef itemDef;

        //sets the lang tokens for in game use
        protected void CreateLang()
        {

            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);

        }

        //sets display rules
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        //actually defines the item
        protected void CreateItem(ItemDef itemDef)
        {
            
            itemDef.name = "ITEM_" + ItemLangTokenName;
            itemDef.nameToken = "ITEM_" + ItemLangTokenName + "_NAME";
            itemDef.pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP";
            itemDef.descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION";
            itemDef.loreToken = "ITEM_" + ItemLangTokenName + "_LORE";

            //itemDef.pickupModelPrefab = Main.Assets.LoadAsset<GameObject>(ItemModelPath);
            //itemDef.pickupIconSprite = Main.Assets.LoadAsset<Sprite>(ItemIconPath);


            //----------    
            if (ItemModelPath == "")
            {
                
                itemDef.pickupModelPrefab = Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
            }
            else
            {
                itemDef.pickupModelPrefab = Main.Assets.LoadAsset<GameObject>(ItemModelPath);
                //string prefab = "Assets/ItemTests/Models/prefabs/Item/item1/Item_1.prefab";
                
            }
            if (ItemIconPath == "")
            {
                itemDef.pickupIconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            }
            else
            {
                itemDef.pickupIconSprite = Main.Assets.LoadAsset<Sprite>(ItemIconPath);
            }

            //----------   
            
            

            itemDef.hidden = Hidden;
            itemDef.tags = ItemTags;
            itemDef.canRemove = CanRemove;
            itemDef.tier = Tier;
            //itemDef.deprecatedTier = Tier;
            //itemDef.deprecatedTier = ItemTier.Tier2;
            /*
#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001
            */
            itemDef.unlockableDef = ItemPreReq;

            //var itemDisplayRuleDict = CreateItemDisplayRules();
            var displayRules = new ItemDisplayRuleDict(null);

            ItemAPI.Add(new CustomItem(itemDef, displayRules));
        }

        //where hooks go
        public abstract void Hooks();

        //gets count of item from CharacterBody or CharacterMaster
        public int GetCount(CharacterBody body)
        {

            if (!body || !body.inventory)
            {

                return 0;

            }

            return body.inventory.GetItemCount(itemDef);

        }

        public int GetCount(CharacterMaster master)
        {

            if (!master || !master.inventory)
            {

                return 0;

            }

            return master.inventory.GetItemCount(itemDef);

        }

    }

}