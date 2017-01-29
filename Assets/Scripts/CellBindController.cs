using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBindController : MonoBehaviour {
    public SpriteRenderer[] spriteTints;
    public SpriteRenderer[] spriteShapes;
    public SpriteRenderer[] spriteIcons;

    public bool isPresenting { get { return mIsPresenting; } } //for phagocytes and cells
    public CellBindData data { get { return mData; } }

    private CellBindData mData;

    private bool mIsPresenting;

    public void Populate(CellBindData newData) {
        mData = newData;
        Refresh();
    }

    public void Present() {
        if(!mIsPresenting) {
            mIsPresenting = true;

            //animate?
        }
    }

    public void Deinit() {
        mIsPresenting = false;

        //reset animation?
    }

    public void Refresh() {
        if(mData == null)
            return;

        for(int i = 0; i < spriteTints.Length; i++)
            spriteTints[i].color = mData.color;

        for(int i = 0; i < spriteShapes.Length; i++)
            spriteShapes[i].sprite = mData.shape;

        for(int i = 0; i < spriteIcons.Length; i++)
            spriteIcons[i].sprite = mData.icon;
    }
}
