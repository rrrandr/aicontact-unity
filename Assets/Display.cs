using UnityEngine;
using UnityEngine.EventSystems;

public class Display : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]private Animator animator;

    private void Awake()
    {
        animator ??= GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool("Display", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("Display", false);
    }
}
