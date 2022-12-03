using UnityEngine;
using SFCore.Utils;
using System.Collections;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using Modding;

namespace BuzzboRemastered
{
    internal partial class HiveKnightAlter : MonoBehaviour
    {

        public const string SN_CheckAwaken = "Check Awakened";
        public const string SN_HandleAwaken = "Make Awakened";

        private HealthManager _hm;
        private Recoil _recoil;
        private tk2dSpriteAnimator _anim;
        //private AudioSource _audio;

        private GameObject _stabHit;
        private GameObject _slash1;
        private GameObject _slash2;
        private GameObject _roarEmitter;
        private GameObject _shadowRecharge;
        private GameObject _honeySpike;
        private GameObject droppers;
        //private AudioClip _roarAudioClip;

        private PlayMakerFSM _stunControl;
        private PlayMakerFSM _control;

        private int maxHp;
        private float timeBeforeAwaken = 1f;

        private int awakenedAttackCount = 0;//9999;//
        private bool awakened;
        private int awakenedSlashChain = 3;
        private int awakenedJumpChain = 1;
        private int regenPausers = 0;
        private bool stunned = false;
        private bool spikeSpamming = false;

        public void Awake()
        {
            ModHooks.LanguageGetHook += LanguageGet;
            
            //Modding.Logger.Log("Start HiveKnightAlter Awake()");

            On.HealthManager.TakeDamage += TookDamage;
            On.InfectedEnemyEffects.RecieveHitEffect += InfectedEnemyEffects_RecieveHitEffect;

            _hm = gameObject.GetComponent<HealthManager>();
            _recoil = gameObject.GetComponent<Recoil>();
            _anim = gameObject.GetComponent<tk2dSpriteAnimator>();
            //_audio = gameObject.GetComponent<AudioSource>();
            
            _stabHit = transform.Find("Stab Hit").gameObject;
            _slash1 = transform.Find("Slash 1").gameObject;
            _slash2 = transform.Find("Slash 2").gameObject;
            _shadowRecharge = Instantiate(HeroController.instance.gameObject.transform.Find("Effects/Shadow Recharge").gameObject, this.gameObject.transform);
            _honeySpike = Instantiate(GameObject.Find("Battle Scene/Globs/Hive Knight Glob/Stingers/Stinger"));
            _honeySpike.SetActive(false);

            droppers = GameObject.Find("Battle Scene/Droppers");

            _shadowRecharge.transform.localScale *= 3;
            _shadowRecharge.LocateMyFSM("Recharge Effect").FsmVariables.GetFsmFloat("Shadow Recharge Time").Value = timeBeforeAwaken;
            
            _stunControl = gameObject.LocateMyFSM("Stun Control");
            _control = gameObject.LocateMyFSM("Control");

            //Modding.Logger.Log("End HiveKnightAlter Awake()");
        }

        private string LanguageGet(string key, string sheetTitle, string orig)
        {
            switch (key)
            {
                case "HIVE_KNIGHT_1":
                    return stunned ? "This is so sad... Alexa play Vespacito" : "Are you watching, Fonsi?";
                case "HIVE_KNIGHT_2":
                    return stunned ? "This is so sad... Alexa play Vespacito" : "Have faith in me, Luis!";
                case "HIVE_KNIGHT_3":
                    return stunned ? "This is so sad... Alexa play Vespacito" : "oof";
            }
            return orig;
        }

        public void Start()
        {
            //Modding.Logger.Log("Start HiveKnightAlter Start()");

            InitHealth();
            InitFSM();
            //_roarAudioClip = (_control.GetState("Intro").Actions[2] as AudioPlayerOneShotSingle).audioClip.Value as AudioClip;
            //_roarAudioClip = _control.GetAction<AudioPlayerOneShotSingle>("Control", 2).audioClip.Value as AudioClip;

            SetAwakened(false);//true);//

            //Modding.Logger.Log("End HiveKnightAlter Start()");
        }

        private void InitHealth()
        {
            StartCoroutine(Regeneration());

            maxHp = (_hm.hp / 2) * 3 + 200; // 850 -> 1475, 1300 -> 2150       // Below: Phase 2 / Phase 3 HP
            _control.FsmVariables.GetFsmInt("P2 HP").Value = (maxHp / 100) * 75; // 780 -> 1106.25, 780 -> 1612.5 (Overrides Asc/Rad value of 9999)
            _control.FsmVariables.GetFsmInt("P3 HP").Value = (maxHp / 10) * 5; // 550 -> 737.5, 550 -> 1075 (Overrides Asc/Rad value of 9999)
            /*if (BossSceneController.Instance)
            {
                switch (BossSceneController.Instance.BossLevel)
                {
                    case 0:
                        break;
                    case 1:
                    case 2:
                        _control.FsmVariables.GetFsmInt("P2 HP").Value = 9999; // Re-override the Asc/Rad value
                        _control.FsmVariables.GetFsmInt("P3 HP").Value = 9999; // Re-override the Asc/Rad value
                        break;
                }
            } // Re-override the value for Asc/Rad to 9999;*/

            _hm.hp = maxHp;
            gameObject.RefreshHPBar();
            _recoil.enabled = false; // Remove knockback
        }

        private void InitFSM()
        {
            
            _control.InsertMethod("Stun Start", () => { stunned = true; regenPausers += 1; }, 0);
            _control.InsertMethod("Stun Recover", () => { stunned = false; regenPausers -= 1; }, 0);


            #region Roar stun
            // Stun the Knight during the roar
            _control.InsertAction("Intro", new SendEventByName()
            {
                eventTarget = new FsmEventTarget()
                {
                    target = FsmEventTarget.EventTarget.GameObject,
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = HeroController.instance.gameObject
                    }
                },
                sendEvent = "ROAR ENTER",
                delay = 0f,
                everyFrame = false
            }, 2);     // Enter roar
            _control.AddMethod("Intro", () => {
                _roarEmitter = GameObject.Instantiate(_control.FsmVariables.GetFsmGameObject("Roar Emitter").Value);
                _roarEmitter.SetActive(false);
            });
            _control.InsertAction("Intro End", new SendEventByName()
            {
                eventTarget = new FsmEventTarget()
                {
                    target = FsmEventTarget.EventTarget.GameObject,
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = HeroController.instance.gameObject
                    }
                },
                sendEvent = "ROAR EXIT",
                delay = 0f,
                everyFrame = false
            }, 1); // Exit roar
            #endregion Roar stun

            // Changes the attack selection a bit, to not overlap identical attacks that break when repeating them immediately
            _control.ChangeTransition("Glob Recover", "FINISHED", "Idle");
            _control.ChangeTransition("Roar Cooldown", "FINISHED", "Idle");
            _control.GetAction<SendRandomEventV3>("Phase 2", 2).eventMax[2].Value = 1; // Glob cannot run twice in a row
            _control.GetAction<SendRandomEventV3>("Phase 3", 1).eventMax[2].Value = 1; // Glob cannot run twice in a row

            _control.GetAction<Wait>("Glob Recover", 1).time = 0;
            _control.GetAction<Wait>("Roar Cooldown", 1).time = 0;

            #region Awakening states
            // Check Awakened state
            _control.AddState("Check Awakened");
            _control.GetState("Idle").ChangeTransition("FINISHED", "Check Awakened");
            _control.GetState("Idle").ChangeTransition("TOOK DAMAGE", "Check Awakened");
            _control.GetState("Check Awakened").AddTransition("FINISHED", "Phase Check");

            // Handle Awaken state
            _control.AddState("Make Awakened");
            _control.GetState("Check Awakened").AddTransition("AWAKEN", "Make Awakened");
            _control.GetState("Make Awakened").AddTransition("PHASE CHECK", "Phase Check");

            _control.GetState("Check Awakened").AddMethod(() => CheckAwakening());
            _control.GetState("Make Awakened").AddMethod(() => { StartCoroutine(ManageAwakening()); });
            #endregion Awakening states

            // [N] Teleport Spikes
            _control.AddState("TeleIn Spikes");
            _control.GetState("TeleIn Spikes").AddTransition("FINISHED", "TeleIn 2");
            _control.GetState("TeleIn Spikes").AddMethod(() => { StartCoroutine(TeleportSpikes()); });

            // [N] Dash Spikes
            _control.AddState("Dash Spikes");
            _control.GetState("Dash Spikes").AddTransition("FINISHED", "Dash");
            _control.GetState("Dash Spikes").AddMethod(() => { StartCoroutine(DashSpikes()); });

            // [N] Jump Spikes
            _control.AddState("Jump Spikes");
            _control.GetState("Jump Spikes").AddTransition("FINISHED", "In Air");
            _control.GetState("Jump Spikes").AddMethod(() => { StartCoroutine(JumpSpikes()); });

            _control.AddState("Roar Spikes");
            _control.GetState("Roar Spikes").AddTransition("FINISHED", "Roar");
            _control.ChangeTransition("Bee Roar Antic", "FINISHED", "Roar Spikes");
            _control.GetState("Roar Spikes").AddMethod(() => { StartCoroutine(SpiralSpikes()); });
            _control.AddState("Glob Spikes");
            _control.GetState("Glob Spikes").AddTransition("FINISHED", "Glob Antic 2");
            _control.GetState("Glob Spikes").AddMethod(() => { StartCoroutine(SpiralSpikes()); });

            // [A] Slash Chain
            _control.AddState("Awakened Slash Chain");
            _control.GetState("Awakened Slash Chain").AddTransition("END CHAIN", "Start Fall");
            _control.GetState("Awakened Slash Chain").AddTransition("CONTINUE CHAIN", "TeleOut 2");
            _control.GetState("Awakened Slash Chain").AddMethod(() =>
            {
                awakenedSlashChain--;
                if (awakenedSlashChain <= 0)
                {
                    awakenedSlashChain = 3;
                    _control.SendEvent("END CHAIN");
                }

                GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                _control.SendEvent("CONTINUE CHAIN");
            });

            // [A] Dash-Teleport
            _control.AddState("Awakened Dash Tele");
            _control.GetState("Awakened Dash Tele").AddTransition("TELEPORT", "TeleOut 2");
            _control.GetState("Awakened Dash Tele").AddMethod(() =>
            {
                awakenedSlashChain = 1;
                _stabHit.SetActive(false);
                GetComponent<Rigidbody2D>().gravityScale = 0f;
                GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                _control.SendEvent("TELEPORT");
            });

            // [A] Jump chain (used after Scream)
            _control.AddState("Awakened Jump Chain");
            _control.GetState("Awakened Jump Chain").AddTransition("END CHAIN", "Idle");
            _control.GetState("Awakened Jump Chain").AddTransition("CONTINUE CHAIN", "Aim Jump");
            _control.GetState("Awakened Jump Chain").AddAction(new FaceObject()
            {
                objectA = _control.FsmVariables.GetFsmGameObject("Self"),
                objectB = HeroController.instance.gameObject, //_control.FsmVariables.GetFsmGameObject("Hero"),
                spriteFacesRight = false,
                playNewAnimation = false,
                newAnimationClip = "",
                resetFrame = false,
                everyFrame = false
            });
            _control.GetState("Awakened Jump Chain").AddMethod(() =>
            {
                awakenedJumpChain--;
                if (awakenedJumpChain <= 0)
                {
                    awakenedJumpChain = 1;
                    _control.SendEvent("END CHAIN");
                }

                GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                _control.SendEvent("CONTINUE CHAIN");
            });
            _control.GetState("Start Fall").AddMethod(() => { awakenedJumpChain = 0; });

            // [A] Scream-Jump chain
            _control.AddState("Awakened Scream Jump");
            _control.GetState("Awakened Scream Jump").AddTransition("FINISHED", "Awakened Jump Chain");
            _control.GetState("Awakened Scream Jump").AddMethod(() => { awakenedJumpChain = 4; });

            // [A] Spike Spam
            _control.CopyState("TeleOut 1", "Spike Spam Init");
            _control.CopyState("TeleOut 2", "Spike Spam Out");
            _control.CopyState("Tele Pos", "Spike Spam Pos");
            _control.CopyState("Aim L", "Spike Spam L");
            _control.CopyState("Aim R", "Spike Spam R");
            _control.CopyState("TeleIn 1", "Spike Spam In 1");
            _control.CopyState("Slash 1", "Spike Spam Slash");
            _control.CopyState("TeleIn 2", "Spike Spam End");

            _control.ChangeTransition("Spike Spam Init", "FINISHED", "Spike Spam Out");
            _control.ChangeTransition("Spike Spam Out", "FINISHED", "Spike Spam Pos");
            _control.AddTransition("Spike Spam Pos", "POSITIONED", "Spike Spam In 1");
            _control.RemoveAction("Spike Spam Pos", 1);
            _control.ChangeTransition("Spike Spam In 1", "FINISHED", "Spike Spam Slash");
            _control.AddTransition("Spike Spam Slash", "CONTINUE SPAM", "Spike Spam Out");
            _control.AddTransition("Spike Spam Slash", "END SPAM", "Spike Spam End");
            _control.RemoveTransition("Spike Spam Slash", "FINISHED");
            _control.ChangeTransition("Spike Spam End", "FINISHED", "Slash 2");

            _control.GetState("Spike Spam Init").AddMethod(() => { StartCoroutine(SpikeSpamTimer(7f)); });
            _control.GetState("Spike Spam Pos").AddMethod(() =>
            {
                float _distance = 0;
                float xPos = 69.2f;
                float yPos = 32;
                while (_distance < 6.5f)
                {
                    xPos = UnityEngine.Random.Range(58.3f, 79.6f);
                    yPos = UnityEngine.Random.Range(27.3f, 40);
                    _distance = Mathf.Sqrt((xPos - HeroController.instance.transform.position.x) * (xPos - HeroController.instance.transform.position.x) + (yPos - HeroController.instance.transform.position.y) * (yPos - HeroController.instance.transform.position.y));
                }
                _control.FsmVariables.GetFsmFloat("X Pos").Value = xPos;
                _control.FsmVariables.GetFsmFloat("Y Pos").Value = yPos;
                _control.SendEvent("POSITIONED");
            });
            _control.GetAction<ActivateGameObject>("Spike Spam Slash", 2).activate = false;
            _control.GetAction<SetVelocity2d>("Spike Spam Slash", 5).x = 0;
            _control.GetState("Spike Spam Slash").AddMethod(() => { StartCoroutine(SpikeSpamSlash()); });
            _control.GetState("Spike Spam End").RemoveAction(2);
            _control.GetState("Spike Spam End").RemoveAction(1);

            // Removal of idle time overrider
            _control.GetState("Phase 2").RemoveAction(0); // VERY IMPORTANT TO REMEMBER
        }

        #region Awakening
        private void CheckAwakening()
        {
            awakenedAttackCount -= 1;
            if (awakenedAttackCount <= 0) {
                if (awakened) SetAwakened(false);
                if (120 + (awakenedAttackCount * 5) < UnityEngine.Random.Range(1, 100))
                {
                    _control.SendEvent("AWAKEN");
                }
            }
        }

        IEnumerator ManageAwakening()
        {

            // Stop interactions
            this.transform.GetComponent<BoxCollider2D>().enabled = false;
            this.transform.GetComponent<Rigidbody2D>().isKinematic = true;
            yield return new WaitForSeconds(0.05f);
            if (stunned)
            {
                this.transform.GetComponent<BoxCollider2D>().enabled = true;
                this.transform.GetComponent<Rigidbody2D>().isKinematic = false;
                yield break;
            }

            _anim.Play("Intro"); // Animation start

            // Roar
            var roar = Instantiate(_roarEmitter);
            roar.transform.SetParent(this.transform);
            roar.transform.localPosition = Vector3.zero;
            roar.SetActive(true);
            //_audio.PlayOneShot(_roarAudioClip, 5f);

            _shadowRecharge.SetActive(true);

            // Awaken
            yield return new WaitForSeconds(timeBeforeAwaken);
            SetAwakened(true);
            awakenedAttackCount = UnityEngine.Random.Range(6, 9);
            yield return new WaitForSeconds(1f);
            
            // End roar, change animation, resume interactions
            roar.LocateMyFSM("emitter").SendEvent("END");
            _anim.Play("Recover");
            this.transform.GetComponent<BoxCollider2D>().enabled = true;
            this.transform.GetComponent<Rigidbody2D>().isKinematic = false;
            _control.SendEvent("PHASE CHECK");

        }

        #endregion Awakening

        #region Regeneration
        IEnumerator Regeneration()
        {
            while(true)
            {
                yield return new WaitForSeconds(0.1f);
                if (!(regenPausers > 0)) 
                {
                    _hm.hp += 1;
                    if (_hm.hp > maxHp) { _hm.hp = maxHp; }
                }
            }
        }

        private void TookDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);
            StartCoroutine(PauseRegen(0.5f));
        }

        IEnumerator PauseRegen(float t)
        {
            regenPausers += 1;
            yield return new WaitForSeconds(t);
            regenPausers -= 1;
        }
        #endregion Regeneration

        public void OnDestroy()
        {
            On.HealthManager.TakeDamage -= TookDamage;
            On.InfectedEnemyEffects.RecieveHitEffect -= InfectedEnemyEffects_RecieveHitEffect;
            SetAwakened(false);
        }

    }
}