using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using ZeebsZitems.Base_Classes;
using System;
using System.Reflection;
using UnityEngine;
using RoR2;
using ZeebsZitems.Utils;
using static ZeebsZitems.Utils.Log;
using System.IO;
using SearchableAttribute = HG.Reflection.SearchableAttribute;



//automatically renamed based on project name.
namespace ZeebsZitems
{
    //--------------R2API dependency. This template is heavily based on the modules provided by this API, so it uses it as a dependency.--------------------
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]

    //add other mod dependencies here using the format listed above.

    //------------------Makes mod server-side. Change this only if you're sure it won't fuck something up-----------------------
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    //------------------Defines your mod's GUID, Name, and Version to BepIn. These are set in variables later.---------------------------
    [BepInPlugin(ModGUID, ModName, ModVersion)]

    //------------------This line adds R2API dependencies required. ONLY ADD THE ONES YOU NEED. If you need to know which ones you need, check the R2API documentation----------
    [R2APISubmoduleDependency(/*nameof(ResourcesAPI),*/ nameof(LanguageAPI), nameof(UnlockableAPI), nameof(ItemAPI), nameof(DotAPI), nameof(RecalculateStatsAPI))]

    //main class where the most basic stuff happens. Basically only here to activate the mod.
    public class Main : BaseUnityPlugin
    {

        //define mod ID: uses the format of "com.USERNAME.MODNAME"
        public const string ModGUID = "com.Zeeboon.ZeebsZitems";

        //define the mod name inside quotes. Can be anything.
        public const string ModName = "Zeeb's Zitems";

        //define mod version inside quotes. Follows format of "MAJORVERSION.MINORPATCH.BUGFIX". Ex: 1.2.3 is Major Release 1, Patch 2, Bug Fix 3.
        public const string ModVersion = "1.0.1";

        //Creates an asset bundle that can be easily accessed from other classes
        public static AssetBundle Assets = null;
        //public static AssetBundleResourcesProvider Provider; 


        //List other necessary variables and bits here. For example, you may need a list of all your new things to add them to the game properly.
        public static PluginInfo PInfo { get; private set; }

        //this method runs when your mod is loaded.
        public void Awake()
        {
            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            PInfo = Info;

            //loads an asset bundle if one exists. Objects will need to be called from this bundle using AssetBundle.LoadAsset(string path)
            /*using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RoR2GenericModTemplate1.mod_assets"))
            {

                if (stream != null)
                {

                    Assets = AssetBundle.LoadFromStream(stream);

                }

            }*/

            string pluginfolder = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);
            const string assetBundle = "mod_assets";


            string AssetBundlePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Main.PInfo.Location), assetBundle);
            Assets = AssetBundle.LoadFromFile(AssetBundlePath);

            

            //runs our configs
            Configs();

            //this method will instantiate everything we want to add to the game. see below
            Instantiate();

            //runs hooks that are seperate from all additions (i.e, if you need to call something when the game runs or at special times)
            Hooks();
        }

        public void Configs()
        {

            //insert configs here

        }

        public void Hooks()
        {

            //insert hooks here

        }

        //we make calls to Verify on each thing here to make our call in Awake clean
        public void Instantiate()
        {
            VerifyEquipment(new Custom_Classes.Equipment.Equipment01());
            //VerifyItems(new Custom_Classes.Items.Item01());           // temp disable cuz dotAPI doesn't work
            VerifyItems(new Custom_Classes.Items.Item02());
            VerifyItems(new Custom_Classes.Items.Item03());
            VerifyItems(new Custom_Classes.Items.Item04());
        }

        //this method will instantiate our items based on a generated config option
        public void VerifyItems(ItemBase item)
        {
            //generates a config file to turn the item on or off and get its value
            var isEnabled = Config.Bind<bool>("Items", "enable " + item.ItemName, true, "Enable this item in game?").Value;

            //checks to see if the config is enabled
            if (isEnabled)
            {
                //if the item is activated, instantiates the item
                item.Init(base.Config);
            }
        }

        public void VerifyEquipment(EquipmentBase equip)
        {

            //generates a config file to turn the item on or off and get its value
            var isEnabled = Config.Bind<bool>("Equipment", "enable " + equip.EquipmentName, true, "Enable this equipment in game?").Value;

            //checks to see if the config is enabled
            if (isEnabled)
            {

                //if the item is activated, instantiates the item
                equip.Init(base.Config);
            }
        }

        //this method will instantiate our achievements based on a generated config option
        /*public void VerifyAchievements(AchievementBase achievement)
        {

            var isEnabled = Config.Bind<bool>("Items", "enable" + achievement.AchievementNameToken, true, "Enable this achievement in game? Default: true").Value;

            if (isEnabled)
            {

                achievement.Init(base.Config);

            }

        }*/

        //place other necessary methods below

        private void Update()
        {
            Custom_Classes.Equipment.Equipment01.Update();
            Custom_Classes.Items.Item01.Update();
            Custom_Classes.Items.Item02.Update();
            Custom_Classes.Items.Item03.Update();
            Custom_Classes.Items.Item04.Update();

        }

    }

}
