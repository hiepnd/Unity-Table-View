using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TableViewCell : MonoBehaviour
{
    public string reuseIdentifier;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private RectTransform rect;
    public RectTransform Rect {
        get {
            if (rect == null) {
                rect = GetComponent<RectTransform>();
            }

            return rect;
        }
    }

    public int Section {
        get;
        set;
    }

    public int Row {
        get;
        set;
    }

    public void SetIndex (int section, int row)
    {
        Section = section;
        Row = row;
    }
}
