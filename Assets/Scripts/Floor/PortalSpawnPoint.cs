using UnityEngine;

public class PortalSpawnPoint : MonoBehaviour
{
    [Tooltip("ScenePortal의 SpawnID와 동일하게 맞춰야 함")]
    [SerializeField] private string spawnID = "default";

    public string SpawnID => spawnID;


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.4f, $"Spawn: {spawnID}");
    }
#endif
}
