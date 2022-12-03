using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BuzzboRemastered
{
    public class TextureStrings
    {
        // Logic copied off ToT, thanks SFG

        public const string BuzzboNormalKey = "BuzzboNormal";
        private const string BuzzboNormalFile = "BuzzboRemastered.Resources.BuzzboNormal.png";
        public const string BuzzboAwakenedKey = "BuzzboAwakened";
        private const string BuzzboAwakenedFile = "BuzzboRemastered.Resources.BuzzboAwakened.png";

        private readonly Dictionary<string, Sprite> _dict;

        public TextureStrings()
        {

            Modding.Logger.Log("Start TextureStrings constructor");

            Assembly asm = Assembly.GetExecutingAssembly();
            _dict = new Dictionary<string, Sprite>();
            var tmpTextures = new Dictionary<string, string>();
            tmpTextures.Add(BuzzboNormalKey, BuzzboNormalFile);
            tmpTextures.Add(BuzzboAwakenedKey, BuzzboAwakenedFile);

            foreach (var pair in tmpTextures)
            {
                using (Stream s = asm.GetManifestResourceStream(pair.Value))
                {
                    //Modding.Logger.Log("Key: " + pair.Key);
                    //Modding.Logger.Log("Value: " + pair.Key);
                    if (s != null)
                    {
                        //Modding.Logger.Log("Stream is not null");

                        byte[] buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        s.Dispose();

                        //Create texture from bytes
                        var tex = new Texture2D(2, 2);

                        tex.LoadImage(buffer, true);

                        // Create sprite from texture
                        _dict.Add(pair.Key, Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
                    }
                }
            }
        }

        public Sprite Get(string key)
        {
            return _dict[key];
        }
    }
}
