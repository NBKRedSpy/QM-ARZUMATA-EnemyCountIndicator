using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_EnemyCountIndicator_Stable
{
    public class EnemyIndicator_Stable
    {
        [HarmonyPatch(typeof(CreatureSystem), "RefreshVisibility", new Type[] {
            typeof(Creatures), })]
        public static class CreatureSystem_RefreshVisibility_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(ref Creatures creatures)
            {
                QM_EnemyCountIndicator.EnemyIndicator.monsterCountSeen = QM_EnemyCountIndicator.EnemyIndicator.CountSeenMonsters(creatures.Monsters, out int total);
                QM_EnemyCountIndicator.EnemyIndicator.monsterCountTotal = total;
                QM_EnemyCountIndicator.EnemyIndicator.player = creatures.Player;
            }
        }
    }
}
