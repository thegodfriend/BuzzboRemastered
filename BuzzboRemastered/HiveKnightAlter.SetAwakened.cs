using HutongGames.PlayMaker.Actions;
using Modding;
using SFCore.Utils;
using System.Collections;
using UnityEngine;

namespace BuzzboRemastered
{
    internal partial class HiveKnightAlter
    {
        
        public void SetAwakened(bool toAwakened)
        {

            SetAwakenedVisuals(toAwakened);

            if (toAwakened)
            {
                // Awaken

                // Damage
                this.gameObject.GetComponent<DamageHero>().damageDealt = 2;
                _stabHit.GetComponent<DamageHero>().damageDealt = 2;
                _slash1.GetComponent<DamageHero>().damageDealt = 2;
                _slash2.GetComponent<DamageHero>().damageDealt = 2;

                // Attack Selection
                _control.FsmVariables.GetFsmFloat("Idle Time").Value = 0f;
                _control.GetAction<BoolTest>("Phase 1", 0).boolVariable = false;
                _control.GetAction<SendRandomEventV3>("Phase 1", 1).weights[2].Value = 0f; // 1/5 -> 0 (Alt: 1/5) 
                _control.GetAction<BoolTest>("Phase 2", 0).boolVariable = false;
                _control.GetAction<SendRandomEventV3>("Phase 2", 1).weights[2].Value = 0.1f; // 2 in 10 -> 1 in 9 
                _control.GetAction<BoolTest>("Phase 3", 0).boolVariable = false;
                _control.GetAction<SendRandomEventV3>("Phase 3", 1).weights[0].Value = 0.4f; // 25% -> 40% (Alt: 30%)
                _control.GetAction<SendRandomEventV3>("Phase 3", 1).weights[1].Value = 0.3f; // 25% -> 30% (Alt: 40%)
                _control.GetAction<SendRandomEventV3>("Phase 3", 1).weights[2].Value = 0.1f; // 25% -> 10% (Alt: 20%)
                _control.GetAction<SendRandomEventV3>("Phase 3", 1).weights[3].Value = 0.2f; // 25% -> 20% (Alt: 10%)

                // Slash
                _control.GetAction<Wait>("Tele Pause", 0).time = 0.05f;
                _control.ChangeTransition("TeleIn 1", "FINISHED", "TeleIn 2");
                _control.GetAction<Wait>("TeleIn 2", 1).time = 0.3f;
                _control.FsmVariables.GetFsmFloat("Slash Speed").Value = -54f;
                _control.GetAction<DecelerateXY>("Slash Recover", 2).decelerationX.Value = 0.8f;
                _control.ChangeTransition("Slash Recover", "FINISHED", "Awakened Slash Chain");

                // Dash
                transform.GetComponent<tk2dSpriteAnimator>().Library.GetClipByName("Stab Antic").fps = 20;
                _control.ChangeTransition("Dash Antic", "FINISHED", "Dash");
                _control.FsmVariables.GetFsmFloat("Dash Speed").Value = -75f;
                _control.GetAction<DecelerateXY>("Dash Recover", 1).decelerationX.Value = 0.7f; // Actually irrelevant, but nevermind
                _control.ChangeTransition("Dash", "FINISHED", "Awakened Dash Tele");

                // Jump
                _control.ChangeTransition("Jump", "FINISHED", "In Air");
                _control.ChangeTransition("Land", "FINISHED", "Awakened Jump Chain");

                // Bee Scream
                _control.ChangeTransition("Phase 3", "BEE ROAR", "Roar");
                _control.GetAction<ActivateGameObject>("Roar", 6).activate = false;
                _control.ChangeTransition("Roar Recover", "FINISHED", "Awakened Scream Jump");

                // "Honey Spikes"
                _control.ChangeTransition("Glob Antic 1", "FINISHED", "Glob Antic 2");
                _control.ChangeTransition("Glob Antic 2", "FINISHED", "Spike Spam Init");
            }
            else
            {
                // Asleepn

                // Damage
                this.gameObject.GetComponent<DamageHero>().damageDealt = 1;
                _stabHit.GetComponent<DamageHero>().damageDealt = 1;
                _slash1.GetComponent<DamageHero>().damageDealt = 1;
                _slash2.GetComponent<DamageHero>().damageDealt = 1;

                // Attack Selection
                _control.FsmVariables.GetFsmFloat("Idle Time").Value = 0.25f;
                _control.GetAction<BoolTest>("Phase 1", 0).boolVariable = _control.FsmVariables.GetFsmBool("Will Jump");
                _control.GetAction<SendRandomEventV3>("Phase 1", 1).weights[2].Value = 0.2f; // 1/5 -> 1/5 (Alt: 0)
                _control.GetAction<BoolTest>("Phase 2", 0).boolVariable = _control.FsmVariables.GetFsmBool("Will Jump");
                _control.GetAction<SendRandomEventV3>("Phase 2", 1).weights[2].Value = 0.1f; // 2 in 10 -> 1 in 9 
                _control.GetAction<BoolTest>("Phase 3", 0).boolVariable = _control.FsmVariables.GetFsmBool("Will Jump");
                _control.GetAction<SendRandomEventV3>("Phase 3", 1).weights[0].Value = 0.3f; // 25% -> 30% (Alt: 40%)
                _control.GetAction<SendRandomEventV3>("Phase 3", 1).weights[1].Value = 0.4f; // 25% -> 40% (Alt: 30%)
                _control.GetAction<SendRandomEventV3>("Phase 3", 1).weights[2].Value = 0.2f; // 25% -> 20% (Alt: 10%)
                _control.GetAction<SendRandomEventV3>("Phase 3", 1).weights[3].Value = 0.1f; // 25% -> 10% (Alt: 20%)


                // Slash
                _control.GetAction<Wait>("Tele Pause", 0).time = 0.15f;
                _control.ChangeTransition("TeleIn 1", "FINISHED", "TeleIn Spikes");
                _control.GetAction<Wait>("TeleIn 2", 1).time = 0.6f;
                _control.FsmVariables.GetFsmFloat("Slash Speed").Value = -45f;
                _control.GetAction<DecelerateXY>("Slash Recover", 2).decelerationX.Value = 0.85f;
                _control.ChangeTransition("Slash Recover", "FINISHED", "Start Fall");

                // Dash
                transform.GetComponent<tk2dSpriteAnimator>().Library.GetClipByName("Stab Antic").fps = 15;
                _control.ChangeTransition("Dash Antic", "FINISHED", "Dash Spikes");
                _control.FsmVariables.GetFsmFloat("Dash Speed").Value = -65f;
                _control.GetAction<DecelerateXY>("Dash Recover", 1).decelerationX.Value = 0.8f;
                _control.ChangeTransition("Dash", "FINISHED", "Dash Recover");

                // Jump
                _control.ChangeTransition("Jump", "FINISHED", "Jump Spikes");
                _control.ChangeTransition("Land", "FINISHED", "Idle");

                // Bee Scream
                _control.ChangeTransition("Phase 3", "BEE ROAR", "Bee Roar Antic");
                _control.GetAction<ActivateGameObject>("Roar", 6).activate = true;
                _control.ChangeTransition("Roar Recover", "FINISHED", "Roar Cooldown");

                // "Honey Spikes"
                _control.ChangeTransition("Glob Antic 1", "FINISHED", "Glob Spikes");
                _control.ChangeTransition("Glob Antic 2", "FINISHED", "Glob Strike");

            }

            SetDropperDir(toAwakened);
            awakened = toAwakened;
        }

        private GameObject SpawnHoneySpike(Vector3 pos, float rot)
        {
            GameObject Spike = Instantiate(_honeySpike);
            Spike.SetActive(true);
            Spike.transform.localPosition = pos;
            Spike.transform.localRotation = Quaternion.Euler(0, 0, rot);
            Spike.GetComponent<HiveKnightStinger>().direction = rot;

            return Spike;
        }

        private GameObject SpawnTargetedHoneySpike(Vector3 pos, Vector3 target)
        {
            float rot = Mathf.Atan2(target.y - pos.y, target.x - pos.x) * (180 / Mathf.PI);
            return SpawnHoneySpike(pos, rot);

        }

        IEnumerator TeleportSpikes()
        {
            yield return new WaitForSeconds(0.2f);
            
            GameObject[] Spikes = new GameObject[7];

            for (int i = 0; i < 7; i++)
            {
                Vector3 spikePos;
                float spikeRot = 0;

                switch (gameObject.transform.localScale.x)
                {
                    case -1: // Facing right
                        spikeRot = 45 - i * 22.5f;
                        break;
                    case 1: // Facing left
                        spikeRot = 135 + i * 22.5f;
                        break;
                }
                spikePos = this.transform.position;// + new Vector3(Mathf.Cos(spikeRot * 0.0174532924f), Mathf.Sin(spikeRot * 0.0174532924f))*4;

                Spikes[i] = SpawnHoneySpike(spikePos, spikeRot);

                ReflectionHelper.SetField<HiveKnightStinger, float>(Spikes[i].GetComponent<HiveKnightStinger>(), "speed", 0.80f*25f);
            }

            yield return new WaitForSeconds(0.2f);
            
            for (int i = 0; i < 7; i++)
            {
                ReflectionHelper.SetField<HiveKnightStinger, float>(Spikes[i].GetComponent<HiveKnightStinger>(), "speed", 0f);
            }

            yield return new WaitForSeconds(0.2f);

            for (int i = 0; i < 7; i++)
            {
                ReflectionHelper.SetField<HiveKnightStinger, float>(Spikes[i].GetComponent<HiveKnightStinger>(), "speed", 30f);
            }

        }

        IEnumerator DashSpikes()
        {
            //float start = Time.time;
            bool odd = true;
            while (true) //(Time.time - start < 0.18f)
            {
                if (odd)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3 spikePos;
                        float spikeRot = 0;

                        switch (gameObject.transform.localScale.x)
                        {
                            case 1: // Facing left
                                spikeRot = 45 - i * 22.5f;
                                break;
                            case -1: // Facing right
                                spikeRot = 135 + i * 22.5f;
                                break;
                        }
                        spikePos = this.transform.position;

                        GameObject spike = SpawnHoneySpike(spikePos, spikeRot);

                        ReflectionHelper.SetField<HiveKnightStinger, float>(spike.GetComponent<HiveKnightStinger>(), "speed", 35f);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 spikePos;
                        float spikeRot = 0;

                        switch (gameObject.transform.localScale.x)
                        {
                            case 1: // Facing left
                                spikeRot = 33.75f - i * 22.5f;
                                break;
                            case -1: // Facing right
                                spikeRot = 146.25f + i * 22.5f;
                                break;
                        }
                        spikePos = this.transform.position;

                        GameObject spike = SpawnHoneySpike(spikePos, spikeRot);

                        ReflectionHelper.SetField<HiveKnightStinger, float>(spike.GetComponent<HiveKnightStinger>(), "speed", 35f);
                    }
                }
                odd = !odd;
                yield return new WaitForSeconds(0.04f);
                if (!(_control.ActiveStateName == "Dash Spikes" || _control.ActiveStateName == "Dash")) break;
            }
        }

        IEnumerator JumpSpikes()
        {
            //float start = Time.time;
            while (true)//(Time.time - start < 0.2f)
            {
                GameObject spike = SpawnTargetedHoneySpike(transform.position, HeroController.instance.transform.position);
                ReflectionHelper.SetField<HiveKnightStinger, float>(spike.GetComponent<HiveKnightStinger>(), "speed", 35f);
                yield return new WaitForSeconds(0.08f);
                if (!(_control.ActiveStateName == "Jump Spikes" ||
                    //_control.ActiveStateName == "Jump" ||
                    _control.ActiveStateName == "In Air")) break;
            }
        }

        IEnumerator SpiralSpikes()
        {
            float rot = UnityEngine.Random.Range(0, 360);
            float mod = UnityEngine.Random.Range(13, 37);
            while (true)
            {
                GameObject spike = SpawnHoneySpike(_control.transform.position, rot);
                StartCoroutine(DelaySpecificSpike(spike, 0.2f, 0.2f, mod+15));
                yield return new WaitForSeconds(0.02f);//0.04f);//
                if ( !(_control.ActiveStateName == "Bee Roar Antic"
                    || _control.ActiveStateName == "Roar"
                    || _control.ActiveStateName == "Glob Antic 1"
                    || _control.ActiveStateName == "Glob Antic 2"
                    || _control.ActiveStateName == "Glob Strike")) break;
                rot += mod;
                rot %= 360;
            }
        }

        IEnumerator DelaySpecificSpike(GameObject spike, float headstart, float delay, float speed)
        {
            yield return new WaitForSeconds(headstart);
            ReflectionHelper.SetField<HiveKnightStinger, float>(spike.GetComponent<HiveKnightStinger>(), "speed", 0f);
            yield return new WaitForSeconds(delay);
            ReflectionHelper.SetField<HiveKnightStinger, float>(spike.GetComponent<HiveKnightStinger>(), "speed", speed);
        }

        private void SetDropperDir (bool inverted)
        {
            foreach(Transform dropper in droppers.transform)
            {
                if (inverted)
                {
                    dropper.position = new Vector3(79.9f, 22.0f, 0.0f);
                    PlayMakerFSM _dControl = dropper.gameObject.LocateMyFSM("Control");
                    _dControl.FsmVariables.GetFsmFloat("Start Y").Value = 22f;
                    _dControl.GetAction<RandomFloat>("Swarm Start", 5).min = 19.5f;//26f;//
                    _dControl.GetAction<RandomFloat>("Swarm Start", 5).max = 19.5f;//26f;//
                    _dControl.GetAction<FloatCompare>("Swarm", 3).float2 = 42f;
                    _dControl.GetAction<FloatCompare>("Swarm", 3).lessThan = null;
                    _dControl.GetAction<FloatCompare>("Swarm", 3).greaterThan = HutongGames.PlayMaker.FsmEvent.GetFsmEvent("END");
                }
                else
                {
                    dropper.position = new Vector3(79.9f, 42.0f, 0.0f);
                    PlayMakerFSM _dControl = dropper.gameObject.LocateMyFSM("Control");
                    _dControl.FsmVariables.GetFsmFloat("Start Y").Value = 42f;
                    _dControl.GetAction<RandomFloat>("Swarm Start", 5).min = -19.5f;
                    _dControl.GetAction<RandomFloat>("Swarm Start", 5).max = -19.5f;
                    _dControl.GetAction<FloatCompare>("Swarm", 3).float2 = 22f;
                    _dControl.GetAction<FloatCompare>("Swarm", 3).lessThan = HutongGames.PlayMaker.FsmEvent.GetFsmEvent("END");
                    _dControl.GetAction<FloatCompare>("Swarm", 3).greaterThan = null;
                }
            }
        }

        IEnumerator SpikeSpamTimer(float t)
        {
            StartCoroutine(PauseRegen(t));
            spikeSpamming = true;
            yield return new WaitForSeconds(t);
            spikeSpamming = false;
        }

        IEnumerator SpikeSpamSlash()
        {
            yield return new WaitForSeconds(0.02f);
            
            GameObject Spike = SpawnTargetedHoneySpike(this.transform.position, HeroController.instance.transform.position);

            yield return new WaitForSeconds(0.03f);
            if (spikeSpamming) _control.SendEvent("CONTINUE SPAM");
            else
            {
                awakenedSlashChain = 0;
                _control.SendEvent("END SPAM");
            }
        }

    }
}
