using UnityEngine;

namespace Tugas7
{
    public static class T7_GroundProbe
    {
        public static bool IsGrounded(Transform player, CapsuleCollider capsule,
            float distance, LayerMask mask)
        {
            if (player == null || capsule == null || distance <= 0f) return false;
            float horizontalScale = Mathf.Max(Mathf.Abs(player.lossyScale.x), Mathf.Abs(player.lossyScale.z));
            float radius = capsule.radius * horizontalScale * 0.9f;
            Vector3 origin = player.TransformPoint(capsule.center);
            RaycastHit[] hits = Physics.SphereCastAll(origin, radius, Vector3.down,
                distance, mask, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null || hit.collider.transform.IsChildOf(player)) continue;
                if (hit.normal.y >= 0.35f) return true;
            }
            return false;
        }
    }
}
