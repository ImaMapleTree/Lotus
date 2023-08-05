using UnityEngine;

namespace Lotus.Utilities;

public static class GameObjectUtils
{
    public static GameObject CreateGameObject(string name, Transform parent, Vector3 position = default, Vector3? scale = null)
    {
        GameObject gameObject = new(name);
        gameObject.transform.SetParent(parent);
        gameObject.transform.localScale = scale ?? new Vector3(1f, 1f, 1f);
        gameObject.transform.localPosition = position;
        return gameObject;
    }

    public static GameObject CreateChild(this GameObject gameObject, string name, Vector3 position = default, Vector3? scale = null)
    {
        return CreateGameObject(name, gameObject.transform, position, scale);
    }

    public static T QuickComponent<T>(this GameObject gameObject, string name, Vector3 position = default, Vector3? scale = null) where T : Component
    {
        return gameObject.CreateChild(name, position, scale).AddComponent<T>();
    }
}