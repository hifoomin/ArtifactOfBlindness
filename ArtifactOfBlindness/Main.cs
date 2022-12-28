using BepInEx;
using UnityEngine;
using System.Reflection;
using ArtifactOfBlindness.Artifact;
using System.Linq;
using System;
using R2API;
using R2API.ContentManagement;
using BepInEx.Configuration;

namespace ArtifactOfBlindness
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2APIContentManager.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "HIFU";
        public const string PluginName = "ArtifactOfBlindness";
        public const string PluginVersion = "1.0.1";
        public static AssetBundle artifactofblindness;

        public void Awake()
        {
            artifactofblindness = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("ArtifactOfBlindness.dll", "artifactofblindness"));

            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                artifact.Init(Config);
            }
        }
    }
}