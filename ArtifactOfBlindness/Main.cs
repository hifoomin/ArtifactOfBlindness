using BepInEx;
using UnityEngine;
using System.Reflection;
using ArtifactOfBlindness.Artifact;
using System.Linq;
using System;
using R2API;
using R2API.ContentManagement;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using BepInEx.Logging;

namespace ArtifactOfBlindness
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2APIContentManager.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "HIFU";
        public const string PluginName = "ArtifactOfBlindness";
        public const string PluginVersion = "1.1.0";

        public static AssetBundle artifactofblindness;

        public static ManualLogSource ABLogger;

        public static ConfigEntry<float> moveSpeedBuff;
        public static ConfigEntry<float> attackSpeedBuff;
        public static ConfigEntry<float> cooldownReductionBuff;
        public static ConfigEntry<float> percentRegenBuff;
        public static ConfigEntry<float> flatRegenBuff;
        public static ConfigEntry<float> mithrixMultiplier;

        public void Awake()
        {
            ABLogger = base.Logger;
            artifactofblindness = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("ArtifactOfBlindness.dll", "artifactofblindness"));

            moveSpeedBuff = Config.Bind("Stats", "Movement Speed Buff", 1.5f, "Decimal. Default is 1.5");
            attackSpeedBuff = Config.Bind("Stats", "Attack Speed Buff", 0.35f, "Decimal. Default is 0.35");
            cooldownReductionBuff = Config.Bind("Stats", "Cooldown Reduction Buff", 0.35f, "Decimal. Default is 0.35");
            percentRegenBuff = Config.Bind("Stats", "Percent Regen Buff", 0.01f, "Decimal. Default is 0.01");
            flatRegenBuff = Config.Bind("Stats", "Flat Regen Buff", 5f, "Default is 5");
            mithrixMultiplier = Config.Bind("Stats", "Mithrix Stats Multiplier", 0.5f, "Decimal. Default is 0.5");

            ModSettingsManager.SetModIcon(artifactofblindness.LoadAsset<Sprite>("Assets/ArtifactOfBlindness/texArtifactOfBlindnessEnabled.png"));
            ModSettingsManager.AddOption(new StepSliderOption(moveSpeedBuff, new StepSliderConfig() { min = 0, max = 3f, increment = 0.1f, restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(attackSpeedBuff, new StepSliderConfig() { min = 0, max = 2f, increment = 0.05f, restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(cooldownReductionBuff, new StepSliderConfig() { min = 0, max = 1f, increment = 0.05f, restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(percentRegenBuff, new StepSliderConfig() { min = 0, max = 0.1f, increment = 0.01f, restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(flatRegenBuff, new StepSliderConfig() { min = 0, max = 30f, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(mithrixMultiplier, new StepSliderConfig() { min = 0, max = 2f, increment = 0.05f, restartRequired = true }));

            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                artifact.Init(Config);
            }
        }
    }
}