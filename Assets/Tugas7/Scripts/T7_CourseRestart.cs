using UnityEngine;

namespace Tugas7
{
    /// <summary>
    /// Replay loop: after finishing the course, the player can press E on the
    /// Restart Terminal to reset everything and run the gauntlet again.
    /// Configured by the scene builder.
    /// </summary>
    public sealed class T7_CourseRestart : MonoBehaviour
    {
        [SerializeField] private T7_CourseManager courseManager;
        [SerializeField] private T7_CourseInteractable restartTerminal;
        [SerializeField] private T7_CourseInteractable finishBeacon;
        [SerializeField] private T7_FinishPresentation finishPresentation;
        [SerializeField] private T7_Checkpoint[] checkpoints;
        [SerializeField] private T7_Gate startGate;
        [SerializeField] private Rigidbody crate;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Rigidbody playerBody;
        [SerializeField] private T7_PlayerHealth playerHealth;
        [SerializeField] private AudioSource sfxSource;

        private Vector3 cratePosition;
        private Quaternion crateRotation;
        private bool crateCaptured;

        public void Configure(T7_CourseManager manager, T7_CourseInteractable terminal,
            T7_CourseInteractable beacon, T7_FinishPresentation presentation,
            T7_Checkpoint[] coursePoints, T7_Gate gate, Rigidbody pushCrate,
            Transform start, Rigidbody body, T7_PlayerHealth health, AudioSource source)
        {
            if (isActiveAndEnabled && restartTerminal != null)
                restartTerminal.InteractionRequested -= HandleRestart;
            courseManager = manager;
            restartTerminal = terminal;
            finishBeacon = beacon;
            finishPresentation = presentation;
            checkpoints = coursePoints;
            startGate = gate;
            crate = pushCrate;
            startPoint = start;
            playerBody = body;
            playerHealth = health;
            sfxSource = source;
            CaptureCrateHome();
            if (isActiveAndEnabled && restartTerminal != null)
                restartTerminal.InteractionRequested += HandleRestart;
        }

        private void OnEnable()
        {
            CaptureCrateHome();
            if (restartTerminal != null) restartTerminal.InteractionRequested += HandleRestart;
        }

        private void OnDisable()
        {
            if (restartTerminal != null) restartTerminal.InteractionRequested -= HandleRestart;
        }

        // Remember where the crate starts so replay can put it back.
        private void CaptureCrateHome()
        {
            if (crateCaptured || crate == null) return;
            cratePosition = crate.position;
            crateRotation = crate.rotation;
            crateCaptured = true;
        }

        // Called by the Restart Terminal (E). Returns the status text for the HUD.
        private string HandleRestart()
        {
            if (courseManager == null || !courseManager.IsComplete)
                return "Finish the course first, then restart here";

            courseManager.ResetCourse();
            if (checkpoints != null)
                for (int i = 0; i < checkpoints.Length; i++)
                    checkpoints[i]?.ResetCheckpoint();
            finishBeacon?.Relock();
            finishPresentation?.ResetPresentation();
            startGate?.Close();
            ResetCrate();
            ResetPlayer();
            if (sfxSource != null && sfxSource.clip != null)
                sfxSource.PlayOneShot(sfxSource.clip);
            return "Course reset — head back to the Start Terminal!";
        }

        private void ResetCrate()
        {
            if (crate == null || !crateCaptured) return;
            crate.linearVelocity = Vector3.zero;
            crate.angularVelocity = Vector3.zero;
            crate.position = cratePosition;
            crate.rotation = crateRotation;
        }

        private void ResetPlayer()
        {
            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
                if (startPoint != null)
                    playerBody.transform.SetPositionAndRotation(startPoint.position, startPoint.rotation);
            }
            if (playerHealth != null)
                playerHealth.RestoreForRespawn(playerHealth.MaxHealth);
        }
    }
}
