using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class LoLSceneAutoLoad : MonoBehaviour {
        [SerializeField]
        string _scene = "";

        [SerializeField]
        float _delay = 0f;

        [SerializeField]
        bool _destroyAfter = false;

        IEnumerator Start() {
            if(_delay > 0f)
                yield return new WaitForSeconds(_delay);

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_scene, UnityEngine.SceneManagement.LoadSceneMode.Single);

            if(_destroyAfter)
                Destroy(gameObject);
        }
    }
}