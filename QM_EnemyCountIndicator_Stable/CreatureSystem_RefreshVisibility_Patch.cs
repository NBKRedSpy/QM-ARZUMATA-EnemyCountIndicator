using HarmonyLib;
using MGSC;
using System;

namespace QM_EnemyCountIndicator_Continued_Stable
{
    public class EnemyIndicator_Stable
    {
        [HarmonyPatch(typeof(VisibilitySystem), "UpdateVisibility", new Type[] { typeof(ItemsOnFloor), typeof(Creatures), typeof(MapObstacles), typeof(MapRenderer), typeof(MapGrid), typeof(MapEntities), typeof(FireController), typeof(Visibilities) })]
        public static class VisibilitySystem_UpdateVisibility_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(ref ItemsOnFloor itemsOnFloor, ref Creatures creatures, ref MapObstacles mapObstacles, ref MapRenderer mapRenderer, ref MapGrid mapGrid, ref MapEntities mapEntities, ref FireController fireController, ref Visibilities visibilities)
            {
                QM_EnemyCountIndicator_Continued.EnemyIndicator.monsterCountSeen = QM_EnemyCountIndicator_Continued.EnemyIndicator.CountSeenMonsters(creatures.Monsters, out int total);
                QM_EnemyCountIndicator_Continued.EnemyIndicator.monsterCountTotal = total;
                QM_EnemyCountIndicator_Continued.EnemyIndicator.player = creatures.Player;
            }
        }
    }
}
