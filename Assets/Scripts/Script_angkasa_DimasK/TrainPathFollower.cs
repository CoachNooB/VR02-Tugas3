using UnityEngine;
using System.Collections.Generic;

public class TrainPathFollower : MonoBehaviour
{
    [Header("Track Path")]
    public List<Transform> waypoints;
    public bool loop = true;

    [Header("Movement")]
    public float maxSpeed = 15f;
    public float acceleration = 8f;
    public float brakeDeceleration = 12f;

    [HideInInspector] public float currentProgress = 0f;
    [HideInInspector] public float totalPathLength = 0f;

    private List<float> segmentLengths = new List<float>();
    private float currentSpeed = 0f;

    private void Start()
    {
        InitializePath();
    }

    private void InitializePath()
    {
        if (waypoints == null || waypoints.Count < 2)
        {
            Debug.LogError("TrainPathFollower: waypoints kurang dari 2!");
            enabled = false;
            return;
        }

        segmentLengths.Clear();
        totalPathLength = 0f;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;
            float dist = Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
            segmentLengths.Add(dist);
            totalPathLength += dist;
        }
        if (loop && waypoints.Count > 1)
        {
            if (waypoints[waypoints.Count - 1] != null && waypoints[0] != null)
            {
                float dist = Vector3.Distance(waypoints[waypoints.Count - 1].position, waypoints[0].position);
                segmentLengths.Add(dist);
                totalPathLength += dist;
            }
        }

        if (totalPathLength <= 0f)
        {
            Debug.LogError("TrainPathFollower: total path length is zero!");
            enabled = false;
            return;
        }

        currentProgress = 0f;
        UpdatePositionOnPath(0f);
    }

    private void Update()
    {
        if (!enabled) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.W)) input = 1f;
        else if (Input.GetKey(KeyCode.S)) input = -1f;

        float targetSpeed = input * maxSpeed;
        if (Mathf.Abs(input) > 0.01f)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeDeceleration * Time.deltaTime);

        if (Mathf.Abs(currentSpeed) > 0.001f)
        {
            float distanceStep = currentSpeed * Time.deltaTime;
            float deltaProgress = distanceStep / totalPathLength;
            currentProgress += deltaProgress;

            if (loop)
            {
                if (currentProgress > 1f) currentProgress -= 1f;
                else if (currentProgress < 0f) currentProgress += 1f;
            }
            else
            {
                currentProgress = Mathf.Clamp01(currentProgress);
                if (currentProgress >= 1f || currentProgress <= 0f) currentSpeed = 0f;
            }

            UpdatePositionOnPath(currentProgress);
        }
    }

    private void UpdatePositionOnPath(float p)
    {
        GetPositionOnPath(p, out Vector3 pos, out Quaternion rot);
        if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
        {
            transform.position = pos;
            transform.rotation = rot;
        }
    }

    public void GetPositionOnPath(float p, out Vector3 position, out Quaternion rotation)
    {
        position = transform.position;
        rotation = transform.rotation;

        if (segmentLengths.Count == 0 || totalPathLength <= 0f)
            return;

        p = Mathf.Clamp01(p);
        float targetDist = p * totalPathLength;
        float accumulated = 0f;
        int segIndex = 0;
        for (int i = 0; i < segmentLengths.Count; i++)
        {
            if (targetDist <= accumulated + segmentLengths[i])
            {
                segIndex = i;
                break;
            }
            accumulated += segmentLengths[i];
        }
        segIndex = Mathf.Clamp(segIndex, 0, segmentLengths.Count - 1);

        float localDist = targetDist - accumulated;
        float segProgress = segmentLengths[segIndex] > 0 ? localDist / segmentLengths[segIndex] : 0f;
        segProgress = Mathf.Clamp01(segProgress);

        Vector3 p0, p1;
        if (segIndex < waypoints.Count - 1)
        {
            p0 = waypoints[segIndex] != null ? waypoints[segIndex].position : Vector3.zero;
            p1 = waypoints[segIndex + 1] != null ? waypoints[segIndex + 1].position : Vector3.zero;
        }
        else if (loop && waypoints.Count > 1)
        {
            p0 = waypoints[segIndex] != null ? waypoints[segIndex].position : Vector3.zero;
            p1 = waypoints[0] != null ? waypoints[0].position : Vector3.zero;
        }
        else
        {
            p0 = waypoints[segIndex] != null ? waypoints[segIndex].position : Vector3.zero;
            p1 = p0;
        }

        position = Vector3.Lerp(p0, p1, segProgress);
        Vector3 dir = (p1 - p0).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
        if (currentSpeed < 0) dir = -dir;
        rotation = Quaternion.LookRotation(dir);
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
        if (loop && waypoints[0] != null && waypoints[waypoints.Count - 1] != null)
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
    }
}