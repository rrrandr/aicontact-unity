using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject text;
    public GameObject loader;

    public void ToggleLoader(bool toggle)
    {
        if (text != null)
            text.SetActive(!toggle);

        loader.SetActive(toggle);
    }
}
