using HarmonyLib;
using MGSC;
using QM_EnemyCountIndicator;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QM_ARZUMATA_EnemyCountIndicator
{
    internal class EnemyIndicator
    {
        public static bool debugLog = true;
        public static Transform enemyIndicatorInstance = null;
        public static int monsterCount = 0;

        public static TextMeshProUGUI indicatorTextComponentTextMesh;
        public static TextMeshProUGUI indicatorCounterTextComponentTextMesh;
        public static Image indicatorCounterBackgroundImage;
        public static Image indicatorContainerBorderImage;
        
        public static Color IndicatorCounterBackgroundDefault;
        
        // Number of times to increase/decrease brightness before reversing direction.
        private static int brightnessStepCount = 5;

        private static bool isIncreasingBrightness = true;
        private static int currentStepIndex = 0;
        private static float currentStepTime = 0f;

        // How many times per second the color changes (in this case, 35 times per sec).
        private static float stepDuration = 1f / 35f; 

        [Hook(ModHookType.DungeonStarted)]
        public static void EnemyCountIndicatorButton(IModContext context)
        {
            stepDuration = 1f / Plugin.Config.BlinkIntensity;

            // Find DungeonHudScreen
            var dungeonHud = GameObject.FindObjectOfType<DungeonHudScreen>(true);

            // Find the UpperRight panel in the hierarchy
            var upperRight = dungeonHud.transform.Find("UpperRight");

            // We need this for coordinates offset
            var mapButton = upperRight.transform.Find("MapButton");

            // Find the LowerRight panel in the hierarchy
            var lowerRight = dungeonHud.transform.Find("LowerRight");

            // Find the QmorphosState prefab
            var qmorphosStatePrefab = lowerRight.transform.Find("QmorphosState"); 

            // We can rename even more but not required for now.

            /* In Unity Editor we have following properties:
             * Skull Button Pos:    Vector3(0,   0,  0);
             * Skull Button Size:   Vector3(101, 13, 0);
             * QmorphosState Pos:   Vector3(0,   15, 0);
             * QmorphosState Size:  Vector3(101, 21, 0);
             * Each time its +2 px space
            */

            var offsetPx = 2;

            if (Plugin.Config.PositionUpperRight == true)
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
            var indicatorContainerBorder = enemyIndicatorInstance.GetChild(0);
            var indicatorText = indicatorContainerBorder.GetChild(0);
            var imageindicatorCounterBackground = indicatorContainerBorder.GetChild(1);
            var imageindicatorCounterText = imageindicatorCounterBackground.GetChild(0);

            indicatorContainerBorderImage = indicatorContainerBorder.GetComponent<Image>();
            indicatorCounterBackgroundImage = imageindicatorCounterBackground.GetComponent<Image>();

            IndicatorCounterBackgroundDefault = indicatorCounterBackgroundImage.color; // Just making sure we keep original color.
            indicatorCounterBackgroundImage.color = Plugin.Config.IndicatorBackgroundColor;

            if (indicatorTextComponentTextMesh == null)
            {
                indicatorTextComponentTextMesh = indicatorText.GetComponent<TextMeshProUGUI>();
                indicatorTextComponentTextMesh.text = "ENEMIES";
            }

            if (indicatorCounterTextComponentTextMesh == null)
            {
                indicatorCounterTextComponentTextMesh = imageindicatorCounterText.GetComponent<TextMeshProUGUI>();
                indicatorCounterTextComponentTextMesh.text = "0";
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
                // Show the enemy indicator
                enemyIndicatorInstance.gameObject.SetActive(true); 
                indicatorCounterTextComponentTextMesh.text = monsterCount.ToString();

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
                            // Adjust this factor to control the amount of increase
                            float brightnessIncreaseFactor = 1.2f; 
                            Color brighterColor = new Color(
                            Mathf.Clamp(indicatorCounterBackgroundImage.color.r * brightnessIncreaseFactor, 0, 1),
                            Mathf.Clamp(indicatorCounterBackgroundImage.color.g * brightnessIncreaseFactor, 0, 1),
                            Mathf.Clamp(indicatorCounterBackgroundImage.color.b * brightnessIncreaseFactor, 0, 1),
                            indicatorCounterBackgroundImage.color.a
                            );

                            // Apply the brighter color to the material.
                            indicatorCounterBackgroundImage.color = brighterColor;
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
                            // Adjust this factor to control the amount of decrease
                            float brightnessDecreaseFactor = 1f / 1.2f;
                            Color dimmerColor = new Color(
                                Mathf.Clamp(indicatorCounterBackgroundImage.color.r * brightnessDecreaseFactor, 0, 1),
                                Mathf.Clamp(indicatorCounterBackgroundImage.color.g * brightnessDecreaseFactor, 0, 1),
                                Mathf.Clamp(indicatorCounterBackgroundImage.color.b * brightnessDecreaseFactor, 0, 1),
                                indicatorCounterBackgroundImage.color.a
                            );

                            // Apply the dimmer color to the material.
                            indicatorCounterBackgroundImage.color = dimmerColor;
                            currentStepIndex--;

                            // Reset time for next step
                            currentStepTime = 0f; 
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
                // Hide the enemy indicator
                enemyIndicatorInstance.gameObject.SetActive(false);
                indicatorCounterTextComponentTextMesh.text = "0";
            }
        }
    }
}
