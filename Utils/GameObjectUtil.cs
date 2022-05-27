using System.Collections.Generic;
using UnityEngine;

namespace Nukadelic.Utils
{
    public static partial class GameObjectUtil
    {
        public static List<Transform> Children(this GameObject target, bool excludeDisabled = true)
        {
            var list = new List<Transform>();

            for (var i = 0; i < target.transform.childCount; ++i)
            {
                var child = target.transform.GetChild(i);

                if (excludeDisabled && !child.gameObject.activeSelf) continue;

                list.Add(child);
            }

            return list;
        }

        public static List<T> GetComponentsAll<T>(this GameObject target)
        {
            List<T> list = new List<T>();

            list.AddRange(target.GetComponentsInChildren<T>());

            list.AddRange(target.GetComponents<T>());

            return list;
        }

        public static Bounds GetColliderBounds(this GameObject target)
        {
            var bounds = new Bounds();

            foreach (var collider in GetComponentsAll<Collider>(target))

                if (EmptyBounds.Equals(bounds))

                    bounds = collider.bounds;

                else bounds.Encapsulate(collider.bounds);

            return bounds;
        }

        static readonly Bounds EmptyBounds = new Bounds();

        public static Bounds GetMeshBounds(this GameObject target)
        {
            var bounds = new Bounds();

            foreach (var mesh_renderer in GetComponentsAll<MeshRenderer>(target))

                if (EmptyBounds.Equals(bounds))

                    bounds = mesh_renderer.bounds;

                else bounds.Encapsulate(mesh_renderer.bounds);

            return bounds;
        }

        public static float GetMeshVolume(this GameObject target) => target.GetMeshBounds().extents.magnitude;

        public static float GetColliderVolume(this GameObject target) => target.GetColliderBounds().extents.magnitude;
    }

    public static partial class GameObjectUtil
    {
        public static List<Transform> Children(this Component target, bool excludeDisabled = true) => target.gameObject.Children(excludeDisabled);
        public static List<T> GetComponentsAll<T>(this Component target) => GetComponentsAll<T>(target.gameObject);
        public static Bounds GetMeshBounds(this Component target) => GetMeshBounds(target.gameObject);
        public static float GetMeshVolume(this Component target) => target.gameObject.GetMeshVolume();
        public static Bounds GetColliderBounds(this Component target) => target.gameObject.GetColliderBounds();
        public static float GetColliderVolume(this Component target) => target.gameObject.GetColliderVolume();
    }
}
