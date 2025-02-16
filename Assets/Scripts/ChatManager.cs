using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    public TMP_Text chatDisplay;  // Texto donde se muestra el historial del chat
    public TMP_InputField inputField; // Input donde el usuario escribe
    public Button sendButton; // Botón para enviar mensajes
    public ScrollRect scrollRect; // Scroll View para manejar el desplazamiento

    private Dictionary<string, string> userColors; // Diccionario de colores para cada usuario
    private string[] users = { "Iván", "Jorge", "Sergio", "Empar", "Toni" }; // Lista de usuarios

    void Start()
    {
        sendButton.onClick.AddListener(SendMessage);
        inputField.onSubmit.AddListener(delegate { SendMessage(); });

        //Limpiar el chatDisplay
        chatDisplay.text = "";


        // Configurar colores para cada usuario
        userColors = new Dictionary<string, string>
        {
            { "Iván", "#FF5733" },  // Naranja
            { "Jorge", "#33FF57" }, // Verde
            { "Sergio", "#3357FF" }, // Azul
            { "Empar", "#F5B041" }, // Naranja
            { "Toni", "#9B59B6" }  // Morado
        };

        // Dar foco automático al input al iniciar
        inputField.Select();
        inputField.ActivateInputField();
    }

    public void SendMessage()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            // Elegir usuario aleatorio
            string randomUser = users[Random.Range(0, users.Length)];
            string userColor = userColors[randomUser];

            // Formatear el mensaje con el color del usuario
            string formattedMessage = $"<color={userColor}><b>{randomUser}:</b></color> {inputField.text}";

            // Agregar el mensaje al historial del chat
            chatDisplay.text += "\n" + formattedMessage;

            // Limpiar input y mantener el foco
            inputField.text = "";
            inputField.ActivateInputField();

            // Forzar actualización del Layout para el Scroll
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatDisplay.rectTransform);

            // Hacer que el Scroll se desplace hasta el final
            ScrollToBottom();
        }
    }

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
