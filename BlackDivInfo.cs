using BepInEx;
using EFT;
using EFT.Communications;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using Comfort.Common;

namespace BlackDivInfo
{
    [BepInPlugin("ciallo.blackdivinfo", "Black Div Notifier", "1.4.0")]
    public class BlackDivInfoPlugin : BaseUnityPlugin
    {
        public static bool blackDivSpawned = false;
        public static bool wedgeSpawned = false;
        public static bool blackRaiderSpawned = false;

        private void Awake()
        {
            new NewGamePatch().Enable();
        }

        private void Update()
        {
            if (!UnityInput.Current.GetKeyDown(KeyCode.O))
                return;

            if (blackDivSpawned)
                ShowBlackDivMessage();

            if (wedgeSpawned)
                ShowWedgeMessage();

            if (blackRaiderSpawned)
                ShowBlackRaiderMessage();
        }

        public static void ShowBlackDivMessage()
        {
            NotificationManager.DisplayMessageNotification(
                "Black Division have arrived",
                ENotificationDurationType.Long);
        }

        public static void ShowWedgeMessage()
        {
            NotificationManager.DisplayMessageNotification(
                "Boss Wedge is here!",
                ENotificationDurationType.Long);
        }

        public static void ShowBlackRaiderMessage()
        {
            NotificationManager.DisplayMessageNotification(
                "Black Raider is coming with Wedge",
                ENotificationDurationType.Long);
        }
    }

    internal class NewGamePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod("OnGameStarted");
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            BlackDivInfoPlugin.blackDivSpawned = false;
            BlackDivInfoPlugin.wedgeSpawned = false;
            BlackDivInfoPlugin.blackRaiderSpawned = false;

            if (Singleton<GameWorld>.Instantiated)
            {
                Singleton<GameWorld>.Instance
                    .GetOrAddComponent<BlackDivNoteMono>();
            }
        }
    }

    public class BlackDivNoteMono : MonoBehaviour
    {
        private GameWorld gameWorld;

        private void Start()
        {
            gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null)
                return;

            gameWorld.OnPersonAdd += OnPersonAdd;

            ScanExistingPlayers();
        }

        private void ScanExistingPlayers()
        {
            if (gameWorld?.AllAlivePlayersList == null)
                return;

            foreach (var player in gameWorld.AllAlivePlayersList)
            {
                if (player == null || player.IsYourPlayer)
                    continue;

                ProcessPlayer(player);
            }
        }

        private void OnPersonAdd(IPlayer iPlayer)
        {
            if (iPlayer is Player player)
            {
                ProcessPlayer(player);
            }
        }

        private void ProcessPlayer(Player player)
        {
            if (player?.Profile?.Info?.Settings == null)
                return;

            int roleId = (int)player.Profile.Info.Settings.Role;

            switch (roleId)
            {
                // Black Division 主队
                case 848420:
                case 848421:
                case 848422:
                case 848423:

                    if (!BlackDivInfoPlugin.blackDivSpawned)
                    {
                        BlackDivInfoPlugin.blackDivSpawned = true;
                        BlackDivInfoPlugin.ShowBlackDivMessage();
                    }
                    break;

                // Wedge
                case 848424:

                    if (!BlackDivInfoPlugin.wedgeSpawned)
                    {
                        BlackDivInfoPlugin.wedgeSpawned = true;
                        BlackDivInfoPlugin.ShowWedgeMessage();
                    }
                    break;

                // Black Raider
                case 848426:

                    if (!BlackDivInfoPlugin.blackRaiderSpawned)
                    {
                        BlackDivInfoPlugin.blackRaiderSpawned = true;
                        BlackDivInfoPlugin.ShowBlackRaiderMessage();
                    }
                    break;
            }
        }

        private void OnDestroy()
        {
            if (gameWorld != null)
            {
                gameWorld.OnPersonAdd -= OnPersonAdd;
            }

            BlackDivInfoPlugin.blackDivSpawned = false;
            BlackDivInfoPlugin.wedgeSpawned = false;
            BlackDivInfoPlugin.blackRaiderSpawned = false;
        }
    }
}