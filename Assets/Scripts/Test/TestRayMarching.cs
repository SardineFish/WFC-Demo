using System;
using SardineFish.Utils;
using UnityEngine;

namespace WFC.Test
{
    [ExecuteInEditMode]
    public class TestRayMarching : MonoBehaviour
    {
        public Transform From;
        public Transform To;

        private void Update()
        {
        }

        private void OnDrawGizmos()
        {
            if(!From || !To)
                return;

            var dir = To.position - From.position;
            var ray = new Ray(From.position, dir);
            var distance = Mathf.Abs(Mathf.CeilToInt(dir.x)) + Math.Abs(Mathf.CeilToInt(dir.y)) +
                           Mathf.Abs(Mathf.CeilToInt(dir.z));

            Gizmos.DrawLine(From.position, To.position);
            Gizmos.color = Color.cyan.WithAlpha(0.3f);
            foreach (var (pos, normal) in Utility.VoxelRayMarching(ray, distance)
            )
            {
                Gizmos.DrawCube(pos + (Vector3.one / 2), Vector3.one);
                Gizmos.DrawRay(pos + (Vector3.one / 2), normal);
            }
        }
    }
}