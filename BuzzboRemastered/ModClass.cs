using Modding;
using System.Collections.Generic;
using UnityEngine;

namespace BuzzboRemastered
{
    public class BuzzboRemastered : Mod
    {
        internal static BuzzboRemastered Instance;

        public TextureStrings SpriteDict { get; private set; }

        public static Sprite GetSprite(string name) => Instance.SpriteDict.Get(name);

        public override string GetVersion() => "0.0.4.89";

        //public override List<ValueTuple<string, string>> GetPreloadNames()
        //{
        //    return new List<ValueTuple<string, string>>
        //    {
        //        new ValueTuple<string, string>("White_Palace_18", "White Palace Fly")
        //    };
        //}

        public BuzzboRemastered() : base("Hallow Knight: Buzzbo [TEST BUILD]")
        {
            Instance = this;

            SpriteDict = new TextureStrings();

        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            //Instance = this;

            //Log("Initializing Buzzbo test mod version " + GetVersion());
            ModHooks.OnEnableEnemyHook += EnemyEnabled;
            ModHooks.LanguageGetHook += LanguageGet;

            Log("Initialized");
        }

        private bool EnemyEnabled(GameObject enemy, bool isAlreadyDead)
        {
            if (enemy.name == "Hive Knight" && enemy.GetComponent<HiveKnightAlter>() == null)
                enemy.AddComponent<HiveKnightAlter>();
            return isAlreadyDead;
        }

        private string LanguageGet(string key, string sheetTitle, string orig)
        {
            //string text = orig;
            //Log("Key: " + key);
            //Log("Text: " + text);
            //return text;

            switch (key) {
                case "HIVE_KNIGHT_SUPER":
                    return "Dual Blood";
                case "HIVE_KNIGHT_MAIN":
                    return "Buzzbo";
                case "HIVE_KNIGHT_SUB":
                    return "";
                case "NAME_HIVE_KNIGHT":
                    return "Buzzbo";
                case "GG_S_HIVEKNIGHT":
                    return "Unyielding god of self-enhancement";
                case "DESC_HIVE_KNIGHT":
                    return "Greatest warrior among his people. Eternal and unyielding.";
                case "NOTE_HIVE_KNIGHT":
                    return "This creature...it fires out its own spines even as it grows them back. It unnerves me, to an extent.";
            }

            return orig;
        }
    }
}