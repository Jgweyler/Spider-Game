using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	#region SINGLETON
	protected static UIManager _instance = null;
	public static UIManager instance { get { return _instance; } }
	void Awake() { _instance = this; }
	#endregion

	// Menu principal
	public GameObject mainMenu			= null;	// Panel del menu principal (Primera pantalla en mostrarse)

	// Sub-menus durante el juego
	public FinalPanelBehaviour endPanel	= null;	// Panel de fin de juego (Dentro de la interfaz del juego)
	public Text scoreText				= null;	// Puntuacion del juego

    //HP de la araña
    public GameObject HPContainer;
    public Image spiderHPImage;
    private List<Image> spiderHPList;



	public void showMainMenu()
	{
        CanvasGroup mainMenuCanvasGroup = mainMenu.GetComponent<CanvasGroup>();
        // Mostrar objeto mainMenu
        mainMenuCanvasGroup.alpha = 1;
        mainMenuCanvasGroup.interactable = true;
        mainMenuCanvasGroup.blocksRaycasts = true;

        // Ocultar endPanel
        hideEndPanel();
    }

	public void hideMainMenu()
	{
        CanvasGroup mainMenuCanvasGroup = mainMenu.GetComponent<CanvasGroup>();
        // Ocultar objeto mainMenu
        mainMenuCanvasGroup.alpha = 0;
        mainMenuCanvasGroup.interactable = false;
        mainMenuCanvasGroup.blocksRaycasts = false;
    }


	public void showEndPanel(bool win)
	{
        // Mostrar panel fin de juego (ganar o perder)
        endPanel.setEndPanelText(win);
        endPanel.showEndPanel();
	}

	public void hideEndPanel()
	{
        // Ocultar el panel
        endPanel.hideEndPanel();
	}

	public void updateScore(int score)
	{
        // Actualizar el 'UI text' con la puntuacion 
        scoreText.text = score.ToString();
	}
    
    public void updateSpiderHPinHUD(int hp)
    {
        Destroy(spiderHPList[hp]);
        spiderHPList.RemoveAt(hp);
    }
    //Inicializa el número de vidas que habrá en el HUD

    public void setupHPListHUD( int maxHP)
    {
        int startPosition = -75;
        Image HPImage;

        //Si no hay una lista Inicializada la creamos. En el caso de que exista eliminamos todos las imagenes existentes para inicializarla de nuevo.
        if(spiderHPList == null)
        {
            spiderHPList = new List<Image>();
        }
        else
        {
            spiderHPList.Clear();
        }

        for (int i = 0; i < maxHP; i++)
        {
           HPImage=  Instantiate(spiderHPImage, HPContainer.transform);
            HPImage.transform.position = new Vector3(HPImage.transform.position.x + startPosition, HPImage.transform.position.y, HPImage.transform.position.z);
            startPosition += 75;
            spiderHPList.Add(HPImage);
        }
    }

}
