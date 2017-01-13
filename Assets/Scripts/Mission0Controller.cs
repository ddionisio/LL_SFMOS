using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission0Controller : MissionController {
    
    public MucusGatherInputField mucusGatherInput;
    public MucusGather mucusGather;

    public Transform pointer;
    public GameObject pointerGO;

    private bool mIsPointerActive;
    
    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        if(mucusGatherInput) {
            mucusGatherInput.pointerDownCallback -= OnMucusFieldInputDown;
            mucusGatherInput.pointerDragCallback -= OnMucusFieldInputDrag;
            mucusGatherInput.pointerUpCallback -= OnMucusFieldInputUp;
        }
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        mucusGatherInput.pointerDownCallback += OnMucusFieldInputDown;
        mucusGatherInput.pointerDragCallback += OnMucusFieldInputDrag;
        mucusGatherInput.pointerUpCallback += OnMucusFieldInputUp;

        mIsPointerActive = false;

        if(pointerGO)
            pointerGO.SetActive(false);

        if(pointer)
            pointer.gameObject.SetActive(false);
    }

    void SetPointerActive(bool active) {
        if(mIsPointerActive != active) {
            mIsPointerActive = active;

            if(pointerGO)
                pointerGO.SetActive(mIsPointerActive);

            if(pointer)
                pointer.gameObject.SetActive(mIsPointerActive);
        }
    }

    void OnMucusFieldInputDown(MucusGatherInputField input) {
        if(input.currentAreaType == MucusGatherInputField.AreaType.Bottom) {
            mucusGather.transform.position = new Vector3(input.originPosition.x, input.originPosition.y, 0f);
            mucusGather.Activate();
        }
    }

    void OnMucusFieldInputDrag(MucusGatherInputField input) {
        if(mucusGather.isActive) {
            var pos = input.currentPosition;

            bool pointerActive = !mucusGather.Contains(pos) && input.currentAreaType == MucusGatherInputField.AreaType.Top;

            SetPointerActive(pointerActive);

            if(pointerActive) {
                pointer.position = new Vector3(pos.x, pos.y, pointer.position.z);
            }
        }
    }

    void OnMucusFieldInputUp(MucusGatherInputField input) {
        SetPointerActive(false);

        if(mucusGather.isActive) {
            var pos = input.currentPosition;

            bool pointerActive = !mucusGather.Contains(pos) && input.currentAreaType == MucusGatherInputField.AreaType.Top;

            if(pointerActive) {
                var dir = new Vector2(0f, 0f);
                var dist = 0f;

                mucusGather.Release(dir, dist);
            }
            else {
                mucusGather.Cancel();
            }
        }
    }
}
