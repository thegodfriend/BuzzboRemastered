using Modding;
using UnityEngine;

namespace BuzzboRemastered
{
    internal partial class HiveKnightAlter
    {

        private Color color_hiveblood = new Color(0.957f, 0.608f, 0.212f);//1f, 0.784f, 0f);
        private Color color_lifeblood = new Color(0f, 0.584f, 1f);

        
        private void InfectedEnemyEffects_RecieveHitEffect(On.InfectedEnemyEffects.orig_RecieveHitEffect orig, InfectedEnemyEffects self, float attackDirection)
        {
            GameObject enemy = self.gameObject;
            if (enemy == this.gameObject)
            {
                if (ReflectionHelper.GetField<InfectedEnemyEffects, bool>(self, "didFireThisFrame"))
                {
                    return;
                }

                SpriteFlash spriteFlash = ReflectionHelper.GetField<InfectedEnemyEffects, SpriteFlash>(self, "spriteFlash");
                AudioEvent impactAudio = ReflectionHelper.GetField<InfectedEnemyEffects, AudioEvent>(self, "impactAudio");
                AudioSource audioSourcePrefab = ReflectionHelper.GetField<InfectedEnemyEffects, AudioSource>(self, "audioSourcePrefab");
                Vector3 effectOrigin = ReflectionHelper.GetField<InfectedEnemyEffects, Vector3>(self, "effectOrigin");

                Color bloodColor = (awakened ? color_lifeblood : color_hiveblood);

                if (spriteFlash != null)
                {
                    spriteFlash.flash(bloodColor, 0.9f, 0.01f, 0.01f, 0.25f);
                }
                //FSMUtility.SendEventToGameObject(base.gameObject, "DAMAGE FLASH", true);
                impactAudio.SpawnAndPlayOneShot(audioSourcePrefab, base.transform.position);
                //this.hitFlashOrangePrefab.Spawn(base.transform.TransformPoint(this.effectOrigin));
                switch (DirectionUtils.GetCardinalDirection(attackDirection))
                {
                    case 0:
                        GlobalPrefabDefaults.Instance.SpawnBlood(base.transform.position + effectOrigin, 3, 4, 10f, 15f, 120f, 150f, bloodColor);
                        GlobalPrefabDefaults.Instance.SpawnBlood(base.transform.position + effectOrigin, 8, 15, 10f, 25f, 30f, 60f, bloodColor);
                        break;
                    case 1:
                        GlobalPrefabDefaults.Instance.SpawnBlood(base.transform.position + effectOrigin, 8, 10, 20f, 30f, 80f, 100f, bloodColor);
                        break;
                    case 2:
                        GlobalPrefabDefaults.Instance.SpawnBlood(base.transform.position + effectOrigin, 3, 4, 10f, 15f, 30f, 60f, bloodColor);
                        GlobalPrefabDefaults.Instance.SpawnBlood(base.transform.position + effectOrigin, 8, 10, 15f, 25f, 120f, 150f, bloodColor);
                        break;
                    case 3:
                        GlobalPrefabDefaults.Instance.SpawnBlood(base.transform.position + effectOrigin, 4, 5, 15f, 25f, 140f, 180f, bloodColor);
                        GlobalPrefabDefaults.Instance.SpawnBlood(base.transform.position + effectOrigin, 4, 5, 15f, 25f, 360f, 400f, bloodColor);
                        break;
                }
                ReflectionHelper.SetField<InfectedEnemyEffects, bool>(self, "didFireThisFrame", true);

                return;
            }

            orig(self, attackDirection);
        }

        private void SetAwakenedVisuals (bool awakened)
        {
            if (awakened)
            {
                this.gameObject.GetComponent<tk2dSprite>().GetCurrentSpriteDef().material.mainTexture = BuzzboRemastered.GetSprite(TextureStrings.BuzzboAwakenedKey).texture;
            }
            else
            {
                this.gameObject.GetComponent<tk2dSprite>().GetCurrentSpriteDef().material.mainTexture = BuzzboRemastered.GetSprite(TextureStrings.BuzzboNormalKey).texture;
            }
        }

    }
}
