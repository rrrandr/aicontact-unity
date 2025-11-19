using UnityEngine;

public class Colors : MonoBehaviour
{
    public Color grey;
    public Color darkGrey;
    public Color yellow;
    public Color darkYellow;

    public static Colors _instance;
    public static Colors Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }
}
