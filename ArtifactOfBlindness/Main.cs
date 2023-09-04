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
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "HIFU";
        public const string PluginName = "ArtifactOfBlindness";
        public const string PluginVersion = "1.2.0";

        public static AssetBundle artifactofblindness;

        public static ManualLogSource ABLogger;

        public static ConfigEntry<bool> disclaimer1;
        public static ConfigEntry<bool> disclaimer2;

        public static ConfigEntry<float> moveSpeedBuff;
        public static ConfigEntry<float> attackSpeedBuff;
        public static ConfigEntry<float> cooldownReductionBuff;
        public static ConfigEntry<float> percentRegenBuff;
        public static ConfigEntry<float> flatRegenBuff;
        public static ConfigEntry<float> mithrixMultiplier;

        public static ConfigEntry<bool> enableFog;
        public static ConfigEntry<bool> enableDoF;
        public static ConfigEntry<bool> enableVignette;
        public static ConfigEntry<bool> enableAberration;
        public static ConfigEntry<bool> enableGrain;

        public static ConfigEntry<float> fogRadius;
        public static ConfigEntry<bool> fogIndicator;
        public static ConfigEntry<Color> fogIndicatorColor;
        public static ConfigEntry<Color> fogColorStart;
        public static ConfigEntry<Color> fogColorMid;
        public static ConfigEntry<Color> fogColorEnd;
        public static ConfigEntry<float> fogIntensity;
        public static ConfigEntry<bool> everyoneGlow;
        public static ConfigEntry<Color> glowColor;
        public static ConfigEntry<float> glowIntensity;
        public static ConfigEntry<float> glowRadius;
        public static ConfigEntry<float> depthOfFieldStrength;

        public void Awake()
        {
            ABLogger = base.Logger;
            artifactofblindness = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("ArtifactOfBlindness.dll", "artifactofblindness"));

            disclaimer1 = Config.Bind("Stats", "_Important!", true, "Each player should have the same config to minimize bugs.");

            moveSpeedBuff = Config.Bind("Stats", "Movement Speed Buff", 1.5f, "Decimal. Default is 1.5");
            attackSpeedBuff = Config.Bind("Stats", "Attack Speed Buff", 0.35f, "Decimal. Default is 0.35");
            cooldownReductionBuff = Config.Bind("Stats", "Cooldown Reduction Buff", 0.35f, "Decimal. Default is 0.35");
            percentRegenBuff = Config.Bind("Stats", "Percent Regen Buff", 0.01f, "Decimal. Default is 0.01");
            flatRegenBuff = Config.Bind("Stats", "Flat Regen Buff", 5f, "Default is 5");
            mithrixMultiplier = Config.Bind("Stats", "Mithrix Stats Multiplier", 0.5f, "Decimal. Default is 0.5");
            fogRadius = Config.Bind("Stats", "Fog Radius", 30f, "Default is 30");

            disclaimer2 = Config.Bind("Visual", "_Important!", true, "Each player should have the same config to minimize bugs.");

            enableFog = Config.Bind("Visual", "Enable Fog?", true, "Default is true");
            enableDoF = Config.Bind("Visual", "Enable Depth of Field?", true, "Default is true");
            enableVignette = Config.Bind("Visual", "Enable Vignette?", true, "Default is true");
            enableAberration = Config.Bind("Visual", "Enable Chromatic Aberration?", true, "Default is true");
            enableGrain = Config.Bind("Visual", "Enable Grain?", true, "Default is true");

            fogIndicator = Config.Bind("Visual", "Show Fog Radius Indicator?", true, "Default is false");
            fogIndicatorColor = Config.Bind("Visual", "Fog Radius Indicator Color", new Color(0.3137254902f, 0.2470588235f, 0.2392156863f, 0.2431372549f), "RGBA / 255. Default is 0.3137254902, 0.2470588235, 0.2392156863, 0.2431372549 or RGBA 80, 63, 61, 62 in Risk of Options.");
            fogColorStart = Config.Bind("Visual", "Fog Color Start", new Color(0.17647058823f, 0.17647058823f, 0.20784313725f, 0.64705882352f), "The color close to your camera in RGBA / 255. Default is 0.17647058823, 0.17647058823, 0.20784313725, 0.64705882352 or RGBA 45, 45, 53, 165 in Risk of Options.");
            fogColorMid = Config.Bind("Visual", "Fog Color Mid", new Color(0.1725490196f, 0.1725490196f, 0.21960784313f, 1f), "The color between the close and far points from your camera in RGBA / 255. Default is 0.1725490196, 0.1725490196, 0.21960784313, 1 or RGBA 44, 44, 56, 255 in Risk of Options.");
            fogColorEnd = Config.Bind("Visual", "Fog Color End", new Color(0.1725490196f, 0.1725490196f, 0.21960784313f, 1f), "The color far from your camera in RGBA / 255. Default is 0.1725490196, 0.1725490196, 0.21960784313, 1 or RGBA 44, 44, 56, 255 in Risk of Options.");
            fogIntensity = Config.Bind("Visual", "Fog Intensity", 0.99f, "Default is 0.99");
            everyoneGlow = Config.Bind("Visual", "Make all allies glow?", true, "Default is false");
            glowColor = Config.Bind("Visual", "Glow Color", new Color(1f, 1f, 1f, 1f), "RGBA / 255. Default is 1, 1, 1, 1 or RGBA 255, 255, 255, 255 in Risk of Options.");
            glowIntensity = Config.Bind("Visual", "Glow Intensity", 0.2f, "Default is 0.3");
            glowRadius = Config.Bind("Visual", "Glow Radius", 13f, "Default is 13");
            depthOfFieldStrength = Config.Bind("Visual", "Depth of Field Strength", 69f, "Default is 69");

            ModSettingsManager.SetModIcon(artifactofblindness.LoadAsset<Sprite>("Assets/ArtifactOfBlindness/texArtifactOfBlindnessEnabled.png"));
            ModSettingsManager.AddOption(new CheckBoxOption(disclaimer1));
            ModSettingsManager.AddOption(new StepSliderOption(moveSpeedBuff, new StepSliderConfig() { min = 0, max = 3f, increment = 0.1f, restartRequired = false }));
            ModSettingsManager.AddOption(new StepSliderOption(attackSpeedBuff, new StepSliderConfig() { min = 0, max = 2f, increment = 0.05f, restartRequired = false }));
            ModSettingsManager.AddOption(new StepSliderOption(cooldownReductionBuff, new StepSliderConfig() { min = 0, max = 1f, increment = 0.05f, restartRequired = false }));
            ModSettingsManager.AddOption(new StepSliderOption(percentRegenBuff, new StepSliderConfig() { min = 0, max = 0.1f, increment = 0.01f, restartRequired = false }));
            ModSettingsManager.AddOption(new StepSliderOption(flatRegenBuff, new StepSliderConfig() { min = 0, max = 30f, increment = 1f, restartRequired = false }));
            ModSettingsManager.AddOption(new StepSliderOption(mithrixMultiplier, new StepSliderConfig() { min = 0, max = 2f, increment = 0.05f, restartRequired = false }));

            ModSettingsManager.AddOption(new CheckBoxOption(disclaimer2));
            ModSettingsManager.AddOption(new CheckBoxOption(enableFog));
            ModSettingsManager.AddOption(new CheckBoxOption(enableDoF));
            ModSettingsManager.AddOption(new CheckBoxOption(enableVignette));
            ModSettingsManager.AddOption(new CheckBoxOption(enableAberration));
            ModSettingsManager.AddOption(new CheckBoxOption(enableGrain));
            ModSettingsManager.AddOption(new StepSliderOption(fogRadius, new StepSliderConfig() { min = 0, max = 100f, increment = 1f, restartRequired = false }));
            ModSettingsManager.AddOption(new CheckBoxOption(fogIndicator, new CheckBoxConfig() { restartRequired = false }));
            ModSettingsManager.AddOption(new ColorOption(fogIndicatorColor, new ColorOptionConfig() { restartRequired = false }));
            ModSettingsManager.AddOption(new ColorOption(fogColorStart, new ColorOptionConfig() { restartRequired = false }));
            ModSettingsManager.AddOption(new ColorOption(fogColorMid, new ColorOptionConfig() { restartRequired = false }));
            ModSettingsManager.AddOption(new ColorOption(fogColorEnd, new ColorOptionConfig() { restartRequired = false }));
            ModSettingsManager.AddOption(new StepSliderOption(fogIntensity, new StepSliderConfig() { min = 0, max = 1f, increment = 0.01f, restartRequired = false }));
            ModSettingsManager.AddOption(new CheckBoxOption(everyoneGlow, new CheckBoxConfig() { restartRequired = false }));
            ModSettingsManager.AddOption(new ColorOption(glowColor, new ColorOptionConfig() { restartRequired = false }));
            ModSettingsManager.AddOption(new StepSliderOption(glowIntensity, new StepSliderConfig() { min = 0, max = 5f, increment = 0.1f, restartRequired = false }));
            ModSettingsManager.AddOption(new StepSliderOption(glowRadius, new StepSliderConfig() { min = 0, max = 100f, increment = 1f, restartRequired = false }));
            ModSettingsManager.AddOption(new StepSliderOption(depthOfFieldStrength, new StepSliderConfig() { min = 0, max = 200f, increment = 1f, restartRequired = false }));

            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                artifact.Init(Config);
            }
        }
    }
}