using BepInEx.Configuration;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using RoR2;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.AddressableAssets;
using R2API;
using System.Collections.Generic;

namespace ArtifactOfBlindness.Artifact
{
    internal class ArtifactOfBlindness : ArtifactBase<ArtifactOfBlindness>
    {
        public override string ArtifactName => "Artifact of Blindness";

        public override string ArtifactLangTokenName => "HIFU_ArtifactOfBlindness";

        public override string ArtifactDescription => "Severely reduce ally vision. Enemies gain increased attack speed, movement speed, regeneration and decreased cooldowns outside one ally's vision.";

        public override Sprite ArtifactEnabledIcon => Main.artifactofblindness.LoadAsset<Sprite>("Assets/ArtifactOfBlindness/texArtifactOfBlindnessEnabled.png");

        public override Sprite ArtifactDisabledIcon => Main.artifactofblindness.LoadAsset<Sprite>("Assets/ArtifactOfBlindness/texArtifactOfBlindnessDisabled.png");

        public static RampFog fog;
        public static ChromaticAberration ab;
        public static DepthOfField dof;
        public static Grain grain;

        // public static LensDistortion ld;
        public static Vignette vn;

        public static GameObject ppHolder;
        public static BuffDef regenBuff;
        public static BuffDef speedBuff;
        public static BuffDef aspdBuff;
        private static readonly string[] blacklistedScenes = { "artifactworld", "crystalworld", "eclipseworld", "infinitetowerworld", "intro", "loadingbasic", "lobby", "logbook", "mysteryspace", "outro", "PromoRailGunner", "PromoVoidSurvivor", "splash", "title", "voidoutro" };

        public override void Init(ConfigFile config)
        {
            ppHolder = new("HIFU_ArtifactOfBlindnessPP");
            Object.DontDestroyOnLoad(ppHolder);
            ppHolder.layer = LayerIndex.postProcess.intVal;
            ppHolder.AddComponent<HIFU_ArtifactOfBlindnessPostProcessingController>();
            PostProcessVolume pp = ppHolder.AddComponent<PostProcessVolume>();
            Object.DontDestroyOnLoad(pp);
            pp.isGlobal = true;
            pp.weight = 0f;
            pp.priority = float.MaxValue - 0.5f;
            PostProcessProfile ppProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
            Object.DontDestroyOnLoad(ppProfile);
            ppProfile.name = "HIFU_ArtifactOfBlindness";

            fog = ppProfile.AddSettings<RampFog>();
            fog.SetAllOverridesTo(true);
            fog.fogColorStart.value = new Color32(45, 45, 53, 165);
            fog.fogColorMid.value = new Color32(44, 44, 56, 255);
            fog.fogColorEnd.value = new Color32(44, 44, 56, 255);
            fog.skyboxStrength.value = 0.02f;
            fog.fogPower.value = 0.35f;
            fog.fogIntensity.value = 0.99f;
            fog.fogZero.value = 0f;
            fog.fogOne.value = 0.05f;

            ab = ppProfile.AddSettings<ChromaticAberration>();
            ab.SetAllOverridesTo(true);
            ab.intensity.value = 0.15f;
            ab.fastMode.value = false;

            dof = ppProfile.AddSettings<DepthOfField>();
            dof.SetAllOverridesTo(true);
            dof.aperture.value = 5f;
            dof.focalLength.value = 68.31f;
            dof.focusDistance.value = 5f;

            grain = ppProfile.AddSettings<Grain>();
            grain.SetAllOverridesTo(true);
            grain.intensity.value = 0.09f;
            grain.size.value = 4.57f;
            grain.lumContrib.value = 5.86f;
            grain.colored.value = true;

            vn = ppProfile.AddSettings<Vignette>();
            vn.SetAllOverridesTo(true);
            vn.intensity.value = 0.15f;
            vn.roundness.value = 1f;
            vn.smoothness.value = 0.2f;
            vn.rounded.value = false;
            vn.color.value = new Color32(255, 255, 255, 255);

            pp.sharedProfile = ppProfile;
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public override void Hooks()
        {
            CreateBuff();
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (Instance.ArtifactEnabled)
            {
                if (body.teamComponent.teamIndex == TeamIndex.Player)
                {
                    var fogSphere = body.GetComponent<HIFU_ArtifactOfBlindnessFogSphereController>();
                    if (fogSphere == null)
                    {
                        var fogSphereInstance = body.gameObject.AddComponent<HIFU_ArtifactOfBlindnessFogSphereController>();
                    }
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.HasBuff(regenBuff) || sender.HasBuff(speedBuff) || sender.HasBuff(aspdBuff))
            {
                if (sender.name == "BrotherBody(Clone)" || sender.name == "BrotherHurtBody(Clone)")
                {
                    args.baseAttackSpeedAdd += Main.attackSpeedBuff.Value * Main.mithrixMultiplier.Value;
                    args.cooldownMultAdd += Main.cooldownReductionBuff.Value * Main.mithrixMultiplier.Value;
                    args.moveSpeedMultAdd += Main.moveSpeedBuff.Value * Main.mithrixMultiplier.Value;
                    args.baseRegenAdd += (sender.healthComponent.combinedHealth * Main.percentRegenBuff.Value * Main.mithrixMultiplier.Value) + Main.flatRegenBuff.Value * Main.mithrixMultiplier.Value + (Main.flatRegenBuff.Value * Main.mithrixMultiplier.Value) / 5f * (sender.level - 1);
                }
                else
                {
                    args.baseAttackSpeedAdd += Main.attackSpeedBuff.Value;
                    args.cooldownMultAdd += Main.cooldownReductionBuff.Value;
                    args.moveSpeedMultAdd += Main.moveSpeedBuff.Value;
                    args.baseRegenAdd += (sender.healthComponent.combinedHealth * Main.percentRegenBuff.Value) + Main.flatRegenBuff.Value + Main.flatRegenBuff.Value / 5f * (sender.level - 1);
                }
            }
        }

        private void CreateBuff()
        {
            regenBuff = ScriptableObject.CreateInstance<BuffDef>();
            speedBuff = ScriptableObject.CreateInstance<BuffDef>();
            aspdBuff = ScriptableObject.CreateInstance<BuffDef>();

            var regenIconTexture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Croco/texBuffRegenBoostIcon.tif").WaitForCompletion();
            regenBuff.canStack = false;
            regenBuff.isDebuff = false;
            regenBuff.name = "Darkness";
            regenBuff.iconSprite = Sprite.Create(regenIconTexture, new Rect(0f, 0f, (float)regenIconTexture.width, (float)regenIconTexture.height), new Vector2(0f, 0f));
            regenBuff.buffColor = new Color32(63, 63, 130, 255);

            var speedIconTexture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/texMovespeedBuffIcon.tif").WaitForCompletion();
            speedBuff.canStack = false;
            speedBuff.isDebuff = false;
            speedBuff.name = "Darkness";
            speedBuff.iconSprite = Sprite.Create(speedIconTexture, new Rect(0f, 0f, (float)speedIconTexture.width, (float)speedIconTexture.height), new Vector2(0f, 0f));
            speedBuff.buffColor = new Color32(63, 63, 130, 255);

            var aspdIconTexture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/AttackSpeedOnCrit/texBuffAttackSpeedOnCritIcon.tif").WaitForCompletion();
            aspdBuff.canStack = false;
            aspdBuff.isDebuff = false;
            aspdBuff.name = "Darkness";
            aspdBuff.iconSprite = Sprite.Create(aspdIconTexture, new Rect(0f, 0f, (float)aspdIconTexture.width, (float)aspdIconTexture.height), new Vector2(0f, 0f));
            aspdBuff.buffColor = new Color32(63, 63, 130, 255);

            ContentAddition.AddBuffDef(regenBuff);
            ContentAddition.AddBuffDef(speedBuff);
            ContentAddition.AddBuffDef(aspdBuff);
        }

        private void Run_onRunDestroyGlobal(Run obj)
        {
            var ppVolume = ppHolder.GetComponent<PostProcessVolume>();
            var sceneName = SceneManager.GetActiveScene().name;
            if (!blacklistedScenes.Contains(sceneName))
            {
                ppVolume.weight = 0f;
            }
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            var ppVolume = ppHolder.GetComponent<PostProcessVolume>();

            var sceneName = SceneManager.GetActiveScene().name;
            if (Instance.ArtifactEnabled && !blacklistedScenes.Contains(sceneName))
            {
                ppVolume.weight = 1f;
            }
            else
            {
                ppVolume.weight = 0f;
            }
            orig(self);
        }
    }

    public class HIFU_ArtifactOfBlindnessPostProcessingController : MonoBehaviour
    {
        public PostProcessVolume volume;

        public void Start()
        {
            volume = GetComponent<PostProcessVolume>();
        }
    }

    public class HIFU_ArtifactOfBlindnessFogSphereController : MonoBehaviour
    {
        public GameObject body;
        public CharacterBody bodyComponent;
        public float checkInterval = 1f; // change to 0.08f when it works
        public float timer;
        public float radius = 30f;
        public static List<HIFU_ArtifactOfBlindnessFogSphereController> fogList = new();
        public bool anyEnemiesOutside = false;
        public Vector3 myPosition;
        public Vector3 enemyPosition;

        public void Awake()
        {
            fogList.Add(this);
        }

        public void OnDestroy()
        {
            fogList.Remove(this);
        }

        public void Start()
        {
            body = gameObject;
            bodyComponent = body.GetComponent<CharacterBody>();
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer >= checkInterval)
            {
                timer = 0f;
                for (int i = 0; i < CharacterBody.instancesList.Count; i++)
                {
                    var cachedBody = CharacterBody.instancesList[i];
                    if (cachedBody && cachedBody.teamComponent.teamIndex != TeamIndex.Player)
                    {
                        enemyPosition = cachedBody.transform.position;
                        foreach (HIFU_ArtifactOfBlindnessFogSphereController controller in fogList)
                        {
                            // todo: remove useless fucks like birdsharks, pots and the like from getting checked (idk how)
                            // and I think players check each other on multiplayer (?)
                            myPosition = controller.bodyComponent.transform.position;
                            // both positions are 0, 0, 0 in multiplayer so enemies dont get buffs AAAAAAAA
                            if (Vector3.Distance(enemyPosition, myPosition) >= radius)
                            {
                                anyEnemiesOutside = true;
                                Main.ABLogger.LogInfo("enemyPosition is" + enemyPosition);
                                Main.ABLogger.LogInfo("controller body component transform position is" + myPosition);
                            }
                            else
                            {
                                anyEnemiesOutside = false;
                            }

                            // the idea is to group all spheres and run them on the server
                        }

                        if (anyEnemiesOutside)
                        {
                            AddBuffs(cachedBody);
                        }
                        else
                        {
                            RemoveBuffs(cachedBody);
                        }
                    }
                }
            }
        }

        private void AddBuffs(CharacterBody body)
        {
            bool hasAnyBuff = body.HasBuff(ArtifactOfBlindness.regenBuff) || body.HasBuff(ArtifactOfBlindness.speedBuff) || body.HasBuff(ArtifactOfBlindness.aspdBuff);
            if (!hasAnyBuff)
            {
                body.AddBuff(ArtifactOfBlindness.regenBuff);
                body.AddBuff(ArtifactOfBlindness.speedBuff);
                body.AddBuff(ArtifactOfBlindness.aspdBuff);
            }
        }

        private void RemoveBuffs(CharacterBody body)
        {
            bool hasAnyBuff = body.HasBuff(ArtifactOfBlindness.regenBuff) || body.HasBuff(ArtifactOfBlindness.speedBuff) || body.HasBuff(ArtifactOfBlindness.aspdBuff);
            if (hasAnyBuff)
            {
                body.RemoveBuff(ArtifactOfBlindness.regenBuff);
                body.RemoveBuff(ArtifactOfBlindness.speedBuff);
                body.RemoveBuff(ArtifactOfBlindness.aspdBuff);
            }
        }
    }
}