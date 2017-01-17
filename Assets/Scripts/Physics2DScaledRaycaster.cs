using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class Physics2DScaledRaycaster : PhysicsRaycaster {
    public Vector2 targetSize;

    protected Physics2DScaledRaycaster() { }

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList) {
        if(eventCamera == null)
            return;

        var pos = eventData.position;

        pos.x /= Screen.width;
        pos.y /= Screen.height;

        pos.x *= targetSize.x;
        pos.y *= targetSize.y;

        var ray = eventCamera.ScreenPointToRay(pos);

        float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;
        
        var hits = Physics2D.GetRayIntersectionAll(ray, dist, finalEventMask);

        if(hits.Length != 0) {
            for(int b = 0, bmax = hits.Length; b < bmax; ++b) {
                var sr = hits[b].collider.gameObject.GetComponent<SpriteRenderer>();

                var result = new RaycastResult {
                    gameObject = hits[b].collider.gameObject,
                    module = this,
                    distance = Vector3.Distance(eventCamera.transform.position, hits[b].point),
                    worldPosition = hits[b].point,
                    worldNormal = hits[b].normal,
                    screenPosition = pos,
                    index = resultAppendList.Count,
                    sortingLayer =  sr != null ? sr.sortingLayerID : 0,
                    sortingOrder = sr != null ? sr.sortingOrder : 0
                };
                resultAppendList.Add(result);
            }
        }
    }
}
