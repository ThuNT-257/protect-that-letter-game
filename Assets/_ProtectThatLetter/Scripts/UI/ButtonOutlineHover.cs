using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOutlineHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private Image buttonImage;
    [SerializeField] private Material outlineMaterial;

    private Material defaultMaterial;

    private void Awake() {
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        if (buttonImage != null) {
            defaultMaterial = buttonImage.material;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (buttonImage != null && outlineMaterial != null) {
            buttonImage.material = outlineMaterial; 
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (buttonImage != null) {
            buttonImage.material = defaultMaterial;
        }
    }
}