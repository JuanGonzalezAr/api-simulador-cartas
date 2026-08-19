using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    public Image imagenPersonaje;
    public TextMeshProUGUI textoInfo;

    public void ConfigurarCarta(string nombre, Sprite spriteImagen)
    {
        textoInfo.text = nombre;
        imagenPersonaje.sprite = spriteImagen;
    }
}