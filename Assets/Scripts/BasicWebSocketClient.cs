using UnityEngine;
using WebSocketSharp;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class ChatClient : MonoBehaviour
{
    private WebSocket ws;
    public TMP_InputField inputField;
    public TMP_Text chatDisplay;
    public TMP_Text horasActivoText; // Texto para mostrar horas activas
    public ScrollRect scrollRect;
    public Button sendButton;

    private List<string> receivedMessages = new List<string>();
    private DateTime connectionStartTime; // Hora de conexión del cliente

    void Start()
    {
        chatDisplay.text = "";
        StartCoroutine(ConnectToServer());

        sendButton.onClick.AddListener(SendMessageToServer);
        inputField.onEndEdit.AddListener(HandleEnterKey);
    }

    private void HandleEnterKey(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessageToServer();
        }
    }

    IEnumerator ConnectToServer()
    {
        while (ws == null || ws.ReadyState != WebSocketState.Open)
        {
            ws = new WebSocket("ws://127.0.0.1:7777/");

            ws.OnOpen += (sender, e) =>
            {
                Debug.Log("WebSocket conectado correctamente.");
                connectionStartTime = DateTime.Now; // Registra la hora de conexión
                InvokeRepeating("ActualizarHorasActivas", 0, 60); // Actualiza cada minuto
            };

            ws.OnMessage += (sender, e) =>
            {
                receivedMessages.Add(e.Data);
            };

            ws.OnError += (sender, e) =>
            {
                Debug.LogError("Error en el WebSocket: " + e.Message);
            };

            ws.OnClose += (sender, e) =>
            {
                Debug.Log("WebSocket cerrado. Código: " + e.Code + ", Razón: " + e.Reason);
                CancelInvoke("ActualizarHorasActivas"); // Detiene el contador al cerrar la conexión
            };

            ws.ConnectAsync();
            yield return new WaitForSeconds(1);
        }
    }

    void Update()
    {
        if (receivedMessages.Count > 0)
        {
            foreach (var message in receivedMessages)
            {
                chatDisplay.text += message + "\n";
            }
            receivedMessages.Clear();

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void ActualizarHorasActivas()
    {
        if (horasActivoText != null)
        {
            TimeSpan tiempoConectado = DateTime.Now - connectionStartTime;
            horasActivoText.text = $"Horas activas: {tiempoConectado.TotalHours:F1}";
        }
    }

    public void SendMessageToServer()
    {
        if (ws == null || ws.ReadyState != WebSocketState.Open || string.IsNullOrWhiteSpace(inputField.text))
        {
            Debug.LogError("No se puede enviar el mensaje. La conexión no está abierta o el mensaje está vacío.");
            return;
        }

        string message = inputField.text;
        ws.Send(message);
        inputField.text = "";
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }
}
