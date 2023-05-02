using UnityEngine;

namespace TOHTOR.Utilities;

public static class GameObjectUtils
{
    public static GameObject CreateGameObject(string name, Transform parent, Vector3 position = default)
    {
        GameObject gameObject = new(name);
        gameObject.transform.SetParent(parent);
        gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        gameObject.transform.localPosition = position;
        return gameObject;
    }

    public static GameObject CreateChild(this GameObject gameObject, string name, Vector3 position = default)
    {
        return CreateGameObject(name, gameObject.transform, position);
    }
}