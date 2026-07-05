using UnityEngine;

public class TrainCarriage : MonoBehaviour
{
    public TrainPathFollower train;
    public float distanceFromFront = 0f;

    private float progressOffset;

    private void Start()
    {
        if (train == null)
            train = GetComponentInParent<TrainPathFollower>();

        if (train != null && train.totalPathLength > 0)
            progressOffset = distanceFromFront / train.totalPathLength;
        else
            progressOffset = 0f;
    }

    private void Update()
    {
        if (train == null || !train.enabled || train.totalPathLength <= 0 || train.waypoints == null || train.waypoints.Count < 2)
            return;

        float myProgress = train.currentProgress - progressOffset;
        if (train.loop)
        {
            if (myProgress < 0) myProgress += 1f;
            if (myProgress > 1) myProgress -= 1f;
        }
        else
        {
            myProgress = Mathf.Clamp01(myProgress);
        }

        train.GetPositionOnPath(myProgress, out Vector3 pos, out Quaternion rot);
        if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z) &&
            !float.IsInfinity(pos.x) && !float.IsInfinity(pos.y) && !float.IsInfinity(pos.z))
        {
            transform.position = pos;
            transform.rotation = rot;
        }
    }
}