using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QM_EnemyCountIndicator_Continued
{
    internal class CameraMover
    {
        public void MoveCameraNextMonster(Creature creature, State state, float speed)
        {
            MoveCamera(creature, state.Get<GameCamera>(), speed);
        }

        // Thanks NBK_RedSpy

        /// <summary>
        /// Moves the camera to the obstacle if the location has been explored.
        /// </summary>
        /// <param name="obstacle"></param>
        public void MoveCamera(Creature creature, GameCamera camera, float speed)
        {
            camera.SetCameraMode(CameraMode.BorderMove);
            var pos = creature.Creature3dView._meshRenderer.transform.position;
            camera.MoveCameraToPosition(new Vector3(pos.x, pos.y), .25f / speed);
        }
    }
}
