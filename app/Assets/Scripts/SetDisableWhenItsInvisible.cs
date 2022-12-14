using UnityEngine;

public class SetDisableWhenItsInvisible: MonoBehaviour
{
    private void OnBecameInvisible()
    {
        Debug.Log("invisible");
        this.gameObject.SetActive(false);
    }
}
