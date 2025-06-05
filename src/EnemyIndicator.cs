using HarmonyLib;
using MGSC;
using QM_EnemyCountIndicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QM_ARZUMATA_EnemyCountIndicator
{
    internal class EnemyIndicator
    {
        public enum Location
        {
            Top, 
            Bottom,
        }

        public static bool debugLog = true;
        public static Transform enemyIndicatorInstance = null;
        public static int monsterCount = 0;

        public static TextMeshProUGUI textmeshComponentStage;
        public static TextMeshProUGUI textmeshComponentCurrent;
        public static Image imageComponentImage;
        public static Location location = Location.Top;
        public static string ComponentStageColorDefault = "#8D1131FF"; // Default color, but we update it in code just in case.
        public string ComponentCurrentColorDefault = "#FBE343FF";

        private static int brightnessStepCount = 5; // Number of times to increase/decrease brightness before reversing direction.
        private static bool isIncreasingBrightness = true;
        private static int currentStepIndex = 0;
        private static float currentStepTime = 0f;
        private const float stepDuration = 1f / 35f; // How many times per second the color changes (in this case, 5 times per sec)

        // Convert colors to hex and log them
        public static string FaceColorToHex(Color color) => $"#{color.r:F0}{color.g:F0}{color.b:F0}";
        public static string AlphaAwareColorToHex(Color color) => $"#{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}{(int)(color.a * 255):X2}";

        public static Color HexStringToUnityColor(string hex)
        {
            if (string.IsNullOrEmpty(hex) || !hex.StartsWith("#") || hex.Length != 9)
            {
                throw new ArgumentException("Invalid color format", nameof(hex));
            }

            // Parse the R, G, B, and A values from the hex string
            int r = int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            int a = int.Parse(hex.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);

            // Convert to normalized float values
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        [Hook(ModHookType.DungeonStarted)]
        public static void EnemyCountIndicatorButton(IModContext context)
        {
            // Find DungeonHudScreen
            var dungeonHud = GameObject.FindObjectOfType<DungeonHudScreen>(true);

            if (debugLog)
            {
                Plugin.Logger.Log($"dungeonHud: {dungeonHud.name}");
            }

            // UpperRight Part

            // Find the UpperRight panel in the hierarchy
            var upperRight = dungeonHud.transform.Find("UpperRight");

            // We need this for coordinates offset
            var mapButton = upperRight.transform.Find("MapButton");

            if (debugLog)
            {
                Plugin.Logger.Log($"upperRight: {upperRight.name}");
            }

            // Lower Right Part

            // Find the LowerRight panel in the hierarchy
            var lowerRight = dungeonHud.transform.Find("LowerRight");

            if (debugLog)
            {
                Plugin.Logger.Log($"lowerRight: {lowerRight.name}");
            }

            // Find the QmorphosState prefab
            var qmorphosStatePrefab = lowerRight.transform.Find("QmorphosState"); 

            if (debugLog)
            {
                Plugin.Logger.Log($"qmorphosStatePrefab: {qmorphosStatePrefab.name}");
                Plugin.Logger.Log($"qmorphosStatePrefab position: {qmorphosStatePrefab.transform.position.ToString()}");

                foreach (Transform child in qmorphosStatePrefab.transform)
                {
                    Plugin.Logger.Log($"Child: {child.name}");
                }
            }

            // We can rename even more but not required for now.

            /* In Unity Editor we have following properties:
             * Skull Button Pos:    Vector3(0,   0,  0);
             * Skull Button Size:   Vector3(101, 13, 0);
             * QmorphosState Pos:   Vector3(0,   15, 0);
             * QmorphosState Size:  Vector3(101, 21, 0);
             * Each time its +2 px space
             * */

            var offsetPx = 2;

            if (location == Location.Top)
            {

                // Instantiate the QmorphosState prefab under the UpperPanel as enemyIndicatorInstance
                enemyIndicatorInstance = GameObject.Instantiate(qmorphosStatePrefab, upperRight);
                var enemyIndicatorRect = enemyIndicatorInstance.GetComponent<RectTransform>();

                // Get the size (width and height) of qmorphosStatePrefab using RectTransform
                var coordinateOffset = mapButton.GetComponent<RectTransform>();
                var coordinateOffsetHintLabel = mapButton.GetChild(0).GetComponent<RectTransform>();

                // Set new position based on the position and size of qmorphosStatePrefab.
                var result = coordinateOffset.localPosition.y - coordinateOffset.sizeDelta.y - 2 - coordinateOffsetHintLabel.sizeDelta.y - 2;

                enemyIndicatorInstance.transform.localPosition = new Vector3(0, 
                    coordinateOffset.localPosition.y - 
                    coordinateOffset.sizeDelta.y - 
                    coordinateOffsetHintLabel.sizeDelta.y -
                    offsetPx -
                    enemyIndicatorRect.sizeDelta.y, 
                    0);

                Plugin.Logger.Log($"coordinateOffset.localPosition.y: {coordinateOffset.localPosition.y.ToString()}");
                Plugin.Logger.Log($"coordinateOffset.sizeDelta.y: {coordinateOffset.sizeDelta.y.ToString()}");
                Plugin.Logger.Log($"coordinateOffsetHintLabel.sizeDelta.y: {coordinateOffsetHintLabel.sizeDelta.y.ToString()}");
                Plugin.Logger.Log($"result: {result.ToString()}");
            }
            else
            {
                // Instantiate the QmorphosState prefab under the LowerPanel as enemyIndicatorInstance
                enemyIndicatorInstance = GameObject.Instantiate(qmorphosStatePrefab, lowerRight);

                // Get the size (width and height) of qmorphosStatePrefab using RectTransform
                var coordinateOffset = qmorphosStatePrefab.GetComponent<RectTransform>();

                // Set new position based on the position and size of qmorphosStatePrefab.
                enemyIndicatorInstance.transform.localPosition = new Vector3(0, coordinateOffset.localPosition.y + coordinateOffset.sizeDelta.y + 2, 0);
            }

            enemyIndicatorInstance.name = "EnemyIndicator";

            // Adjust text of enemy indicator.
            var panel = enemyIndicatorInstance.GetChild(0);
            var stage = panel.GetChild(0);
            var image = panel.GetChild(1);
            var current = image.GetChild(0);

            imageComponentImage = image.GetComponent<Image>();
            ComponentStageColorDefault = AlphaAwareColorToHex(imageComponentImage.color);

            if (debugLog)
            {

                // Iterate through each component on this child
                foreach (Component comp in image.GetComponents(typeof(Component)))
                {
                    Plugin.Logger.Log($"\tComponent: {comp.GetType().Name}");
                }
            }

            if (textmeshComponentStage == null)
            {
                textmeshComponentStage = stage.GetComponent<TextMeshProUGUI>();
                textmeshComponentStage.text = "ENEMIES";
            }

            if (textmeshComponentCurrent == null)
            {
                textmeshComponentCurrent = current.GetComponent<TextMeshProUGUI>();
                textmeshComponentCurrent.text = "0";
            }
        }

        // Another way to do that, but UpdateVisibility is better as it's more generic.

        // [HarmonyPatch(typeof(Creature), "ChangeDirection", new Type[] { typeof(CellPosition), typeof(bool), typeof(bool), typeof(bool) })]
        // public static class Creature_ChangeDirection_CellPosition_Patch
        // {
        //     // Prefix method (if needed)
        //     [HarmonyPrefix]
        //     public static bool Prefix(ref MGSC.CellPosition dirPos, ref bool playAnim, ref bool updateLos, ref bool markActionFlag)
        //     {
        //         // Your prefix logic here
        //             
        //         return true;  // Return false to cancel the original method
        //     }
        // 
        //     // Postfix method (with correct signature)
        //     [HarmonyPostfix]
        //     public static void Postfix(MGSC.Creature __instance, ref MGSC.CellPosition dirPos, bool playAnim, bool updateLos, bool markActionFlag, ref bool __result)
        //     {
        //         // Your post-execution logic here
        //         // monsterCount = __instance._creatures.Player._visibleCreatures.Count;
        //         monsterCount = 0;
        //         foreach (Creature creature in __instance._creatures.Monsters)
        //         {
        //             var monster = (Monster)creature;
        //             MapCell cell = monster._mapGrid.GetCell(monster.CreatureData.Position, false);
        // 
        //             //if (creature.IsSeenByPlayer)
        //             if (cell.isSeen)
        //             {
        //                 monsterCount++;
        //             }
        //         }
        //         // Ensure you have an 'out' parameter for the return value of the original method
        //         __result = true;  // Or whatever logic is needed to determine the result
        // 
        //         Plugin.Logger.Log("ChangeDirection(CellPosition) completed with result: " + __result);
        //     }
        // }

        [HarmonyPatch(typeof(VisibilitySystem), "UpdateVisibility", new Type[] { typeof(ItemsOnFloor), typeof(Creatures), typeof(MapObstacles), typeof(MapRenderer), typeof(MapGrid), typeof(MapEntities), typeof(FireController), typeof(Visibilities) })]
        public static class UpdateVisibility_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(ref ItemsOnFloor itemsOnFloor, ref Creatures creatures, ref MapObstacles mapObstacles, ref MapRenderer mapRenderer, ref MapGrid mapGrid, ref MapEntities mapEntities, ref FireController fireController, ref Visibilities visibilities)
            {
                monsterCount = CountSeenMonsters(creatures.Monsters);
            }
        }

        // This one is needed too if enemy moves from view when you skip turn for example
        [HarmonyPatch(typeof(CreatureSystem), "IsSeeMonsters", new Type[] { typeof(Creatures), typeof(MapGrid)})]
        internal class IsSeeMonsters_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(Creatures creatures, MapGrid mapGrid, ref bool __result)
            {
                monsterCount = CountSeenMonsters(creatures.Monsters);
            }
        }

        private static int CountSeenMonsters(List<Creature> creatures)
        {
            int monsterCount = 0;
            foreach (Creature creature in creatures)
            {
                var monster = (Monster)creature;
                MapCell cell = monster._mapGrid.GetCell(monster.CreatureData.Position, false);

                if (cell.isSeen)
                {
                    monsterCount++;
                }
            }

            return monsterCount;
        }


        [Hook(ModHookType.DungeonUpdateBeforeGameLoop)]
        public static void DungeonUpdateBeforeGameLoop(IModContext context)
        {
            // Constant dungeon loop update called every frame
            if (monsterCount > 0)
            {
                enemyIndicatorInstance.gameObject.SetActive(true); // Hide the enemy indicator
                textmeshComponentCurrent.text = monsterCount.ToString();

                if (isIncreasingBrightness)
                {
                    if (currentStepIndex >= brightnessStepCount)
                    {
                        // Reached maximum brightness, start decreasing
                        isIncreasingBrightness = false;
                    }
                    else
                    {
                        currentStepTime += Time.deltaTime;
                        if (currentStepTime >= stepDuration)
                        {
                            float brightnessIncreaseFactor = 1.2f; // Adjust this factor to control the amount of increase
                            Color brighterColor = new Color(
                            Mathf.Clamp(imageComponentImage.color.r * brightnessIncreaseFactor, 0, 1),
                            Mathf.Clamp(imageComponentImage.color.g * brightnessIncreaseFactor, 0, 1),
                            Mathf.Clamp(imageComponentImage.color.b * brightnessIncreaseFactor, 0, 1),
                            imageComponentImage.color.a
                        );

                            // Apply the brighter color to the material.
                            imageComponentImage.color = brighterColor;
                            currentStepIndex++;
                            currentStepTime = 0f; // Reset time for next step
                        }
                    
                    }
                }
                else
                {
                    if (currentStepIndex <= -brightnessStepCount)
                    {
                        // Reached minimum brightness, start increasing again
                        isIncreasingBrightness = true;
                    }
                    else
                    {
                        currentStepTime += Time.deltaTime;
                        if (currentStepTime >= stepDuration)
                        {
                            float brightnessDecreaseFactor = 1f / 1.2f; // Adjust this factor to control the amount of decrease
                            Color dimmerColor = new Color(
                                Mathf.Clamp(imageComponentImage.color.r * brightnessDecreaseFactor, 0, 1),
                                Mathf.Clamp(imageComponentImage.color.g * brightnessDecreaseFactor, 0, 1),
                                Mathf.Clamp(imageComponentImage.color.b * brightnessDecreaseFactor, 0, 1),
                                imageComponentImage.color.a
                            );

                            // Apply the dimmer color to the material.
                            imageComponentImage.color = dimmerColor;
                            currentStepIndex--;
                            currentStepTime = 0f; // Reset time for next step
                        }
                        
                    }
                }

                if (isIncreasingBrightness)
                {
                    currentStepIndex++;
                }
                else
                {
                    currentStepIndex--;
                }



            }
            else
            {
                imageComponentImage.color = HexStringToUnityColor(ComponentStageColorDefault);
                textmeshComponentCurrent.text = "0";
                enemyIndicatorInstance.gameObject.SetActive(false); // Hide the enemy indicator
            }
        }
    }
}
