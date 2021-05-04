using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.LL_SFMOS {
    [CreateAssetMenu(fileName = "card", menuName = "Game/Card")]
    public class CardData : ScriptableObject {
        public Sprite icon;
        [M8.Localize]
        public string nameRef;
    }
}