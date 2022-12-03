using System;
using MonoMod.ModInterop;
using UnityEngine;

namespace BuzzboRemastered
{
    internal static class EnemyHPBar
    {

        [ModImportName(nameof(EnemyHPBar))]
        public static class EnemyHPBarImport
        {
            public static Action<GameObject>? RefreshHPBar = null!;
        }

        static EnemyHPBar() => typeof(EnemyHPBarImport).ModInterop();

        internal static void RefreshHPBar(this GameObject go)
        {
            EnemyHPBarImport.RefreshHPBar?.Invoke(go);
        }

    }
}
