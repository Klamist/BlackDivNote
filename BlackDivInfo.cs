using BepInEx;
using EFT;
using EFT.Communications;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using Comfort.Common;

namespace BlackDivInfo
{
    [BepInPlugin("ciallo.blackdivinfo", "Black Div Notifier", "1.1.0")]
    public class BlackDivInfoPlugin : BaseUnityPlugin
    {
        public static bool blackDivSpawned = false;
        public static bool wedgeSpawned = false;

        private void Awake()
        {
            new CustomBossPatch().Enable();
            new NewGamePatch().Enable();
        }

        private void Update()
        {
            if (blackDivSpawned && UnityInput.Current.GetKeyDown(KeyCode.O))
            {
                ShowBlackDivMessage();
            }
            if (wedgeSpawned && UnityInput.Current.GetKeyDown(KeyCode.O))
            {
                ShowWedgeMessage();
            }
        }

        public static void ShowBlackDivMessage()
        {
            NotificationManagerClass.DisplayMessageNotification(
                "Black Division have arrived",
                ENotificationDurationType.Long
            );
        }

        public static void ShowWedgeMessage()
        {
            NotificationManagerClass.DisplayMessageNotification(
                "BOSS Wedge is here!",
                ENotificationDurationType.Long
            );
        }
    }

    internal class CustomBossPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(BotBoss).GetConstructors()[0];

        [PatchPostfix]
        private static void PatchPostfix(BotBoss __instance)
        {
            WildSpawnType role = __instance.Owner.Profile.Info.Settings.Role;

            // 检测自定义 boss ID
            if (role == (WildSpawnType)848420 ||
                role == (WildSpawnType)848421 ||
                role == (WildSpawnType)848422 ||
                role == (WildSpawnType)848423)
            {
                BlackDivInfoPlugin.blackDivSpawned = true;
                BlackDivInfoPlugin.ShowBlackDivMessage();
            }

            if (role == (WildSpawnType)848424)
            {
                BlackDivInfoPlugin.wedgeSpawned = true;
                BlackDivInfoPlugin.ShowWedgeMessage();
            }
        }
    }

    internal class NewGamePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod("OnGameStarted");

        [PatchPrefix]
        public static void PatchPrefix()
        {
            BlackDivInfoPlugin.blackDivSpawned = false;
            BlackDivInfoPlugin.wedgeSpawned = false;

            if (Singleton<GameWorld>.Instantiated)
            {
                Singleton<GameWorld>.Instance.GetOrAddComponent<BlackDivNoteMono>();
            }
        }
    }

    // MonoBehaviour 用于在 raid 结束时清理状态
    public class BlackDivNoteMono : MonoBehaviour
    {
        public void OnDestroy()
        {
            BlackDivInfoPlugin.blackDivSpawned = false;
            BlackDivInfoPlugin.wedgeSpawned = false;
        }
    }
}
