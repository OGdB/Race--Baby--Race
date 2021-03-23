using UnityEngine;

public class RaceSpeed : MonoBehaviour
{
    [Range(0.1f, 2.0f)]
    [SerializeField] private float timeSpeed = 1f;

    [SerializeField] private float overrideSpeed;
    private void OnValidate()
    {
        Time.timeScale = timeSpeed;
        if (overrideSpeed > timeSpeed)
        {
            Time.timeScale = overrideSpeed;
        }
    }
}
