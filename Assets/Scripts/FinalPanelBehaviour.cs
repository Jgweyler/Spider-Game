using UnityEngine;
using UnityEngine.UI;

public class FinalPanelBehaviour : MonoBehaviour
{
    // Clase que representa el panel de fin de juego
    private Text finalText;
    private Animator animator;

     void Awake()
    {
        finalText = gameObject.GetComponentInChildren<Text>();
        animator = gameObject.GetComponent<Animator>();
    }

    public void setEndPanelText( bool win)
    {
        if (win)
            finalText.text = "Misión cumplida.";
        else
            finalText.text = "Misión fallida.";
    }

    public void showEndPanel()
    {
        animator.SetTrigger("Appear");
    }

    public void hideEndPanel()
    {
        animator.SetTrigger("Reset");
    }
}
