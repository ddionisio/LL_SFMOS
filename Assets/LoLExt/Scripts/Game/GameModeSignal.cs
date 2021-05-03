using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    [CreateAssetMenu(fileName = "gameModeSignal", menuName = "Game/Mode Signal")]
    public class GameModeSignal : M8.SignalParam<GameMode> {
        public override void Invoke(GameMode parm) {
            if(parm != null)
                parm.SetAsCurrent();
            else
                GameMode.ClearCurrent();

            base.Invoke(parm);
        }
    }
}