using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

public class DuplicateAdjacently : MonoBehaviour
{
    [MenuItem("Editor Extensions/Duplicate Adjacently #%d")]
    static void Duplicate()
    {
        GameObject duped = Instantiate(Selection.activeGameObject, Selection.activeGameObject.transform.position, Selection.activeGameObject.transform.rotation, Selection.activeTransform.transform.parent);
        duped.transform.SetSiblingIndex(Selection.activeTransform.GetSiblingIndex() + 1);
        Selection.activeGameObject = duped;
    }
}