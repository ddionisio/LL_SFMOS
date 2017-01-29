using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "cellBind", menuName = "Stats/Cell Bind")]
public class CellBindData : ScriptableObject {
    public enum Type {
        None,
        Epitope, //part of antigen
        MHC1, //antigen presentation of cells
        MHC2 //antigen presentation of phagocytes
    }

    public int id;

    [SerializeField]
    Color _color;

    [SerializeField]
    Sprite _shape;

    [SerializeField]
    Sprite _icon;

    public Color color { get { return _color; } }
    public Sprite shape { get { return _shape; } }
    public Sprite icon { get { return _icon; } }
}
