using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class APIManagement : MonoBehaviour
{
    private string baseURL = "https://my-json-server.typicode.com/JuanGonzalezAr/api-simulador-cartas";
    private string thirdPartyURL = "https://dummyjson.com/products/";

    [Header("Referencias de UI en Escena")]
    public TextMeshProUGUI textoUsuarioActivo;
    public Transform contenedorBaraja;
    public GameObject prefabCarta;
    public Button botonCambiarUsuario;

    private int usuarioActualId = 1;

    void Start()
    {
        if (botonCambiarUsuario != null) botonCambiarUsuario.onClick.AddListener(CambiarUsuario);
        CargarDatosUsuario(usuarioActualId);
    }

    public void CargarDatosUsuario(int idUsuario)
    {
        Debug.Log("Cargando usuario ID: " + idUsuario);
        StartCoroutine(GetUsuarioInfo(idUsuario));
        StartCoroutine(GetBarajaPorUsuario(idUsuario));
    }

    IEnumerator GetUsuarioInfo(int usuarioId)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(baseURL + "/usuarios"))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = "{\"usuarios\":" + webRequest.downloadHandler.text + "}";
                ListaUsuarios lista = JsonUtility.FromJson<ListaUsuarios>(jsonResult);

                foreach (var u in lista.usuarios)
                {
                    if (u.id == usuarioId)
                    {
                        textoUsuarioActivo.text = "Jugador: " + u.nombre;
                        break;
                    }
                }
            }
        }
    }

    IEnumerator GetBarajaPorUsuario(int usuarioId)
    {
        foreach (Transform hijo in contenedorBaraja) Destroy(hijo.gameObject);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(baseURL + "/barajas"))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = "{\"barajas\":" + webRequest.downloadHandler.text + "}";
                ListaBarajas lista = JsonUtility.FromJson<ListaBarajas>(jsonResult);

                foreach (var b in lista.barajas)
                {
                    if (b.usuarioId == usuarioId)
                    {
                        foreach (int idCarta in b.cartasIds)
                        {
                            StartCoroutine(GetDetalleCartaDummy(idCarta));
                        }
                    }
                }
            }
        }
    }

    IEnumerator GetDetalleCartaDummy(int cartaId)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(thirdPartyURL + cartaId))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                CartaDummy carta = JsonUtility.FromJson<CartaDummy>(webRequest.downloadHandler.text);

                // Usamos exclusivamente la imagen segura en JPG basada en el ID (evitando el .webp)
                string imagenSeguraJPG = $"https://picsum.photos/seed/{cartaId}/200/300.jpg";

                StartCoroutine(CargarImagenDeURL(imagenSeguraJPG, texture => {
                    if (prefabCarta != null && contenedorBaraja != null)
                    {
                        GameObject nuevaCarta = Instantiate(prefabCarta, contenedorBaraja);
                        CardView view = nuevaCarta.GetComponent<CardView>();

                        if (view != null)
                        {
                            Sprite spriteFinal = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            view.ConfigurarCarta(carta.title, spriteFinal);
                        }
                    }
                }));
            }
        }
    }

    IEnumerator CargarImagenDeURL(string url, System.Action<Texture2D> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                callback(texture);
            }
            else
            {
                Debug.LogError("Error descargando imagen de: " + url + " | Error: " + webRequest.error);
            }
        }
    }

    void CambiarUsuario()
    {
        usuarioActualId = (usuarioActualId == 1) ? 2 : 1;
        CargarDatosUsuario(usuarioActualId);
    }
}

[System.Serializable] public class Usuario { public int id; public string nombre; }
[System.Serializable] public class Baraja { public int usuarioId; public List<int> cartasIds; }
[System.Serializable] public class ListaUsuarios { public List<Usuario> usuarios; }
[System.Serializable] public class ListaBarajas { public List<Baraja> barajas; }
[System.Serializable] public class CartaDummy { public int id; public string title; public string thumbnail; }