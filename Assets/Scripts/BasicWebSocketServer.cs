using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System;
using System.IO;
using System.Collections.Generic;

public class BasicWebSocketServer : MonoBehaviour
{
    private WebSocketServer wss;
    private List<string> colors; // Lista para almacenar colores aleatorios
    private static List<string> historialMensajes = new List<string>(); // Historial de mensajes
    private static bool hostExists = false;
    private bool isHost = false;

    public TMPro.TMP_InputField inputField; // Campo de entrada para mostrar mensajes

    void Start()
    {

        // Genera 100 colores aleatorios
        colors = GenerateRandomColors(100);

        if (!hostExists)
        {
            isHost = true;
            hostExists = true;
            Debug.Log("Esta instancia es el host.");
            try
            {
                wss = new WebSocketServer(7777);
                ChatBehavior.SetColors(colors); // Asigna colores al comportamiento del chat
                wss.AddWebSocketService<ChatBehavior>("/");
                wss.Start();
                Debug.Log("Servidor WebSocket iniciado en ws://127.0.0.1:7777/");

                if (inputField != null)
                {
                    inputField.text = "Servidor iniciado: Este cliente es el servidor principal.";
                    ClearInputFieldAfterDelay(3); // Borra el mensaje después de 3 segundos
                }

                // Inicia la actualización del texto de horas activas
                InvokeRepeating("ActualizarHorasActivas", 0, 60); // Actualiza cada minuto
            }
            catch (Exception ex)
            {
                Debug.LogError("Error al iniciar el servidor WebSocket: " + ex.Message);
                isHost = false;
                hostExists = false;
            }
        }
        else
        {
            Debug.Log("Esta instancia no es el host.");
        }
    }

    void OnDestroy()
    {
        if (isHost && wss != null)
        {
            string fechaActual = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            System.IO.File.WriteAllLines("chatlog" + fechaActual + ".txt", historialMensajes); // Guarda el historial al cerrar
            wss.WebSocketServices["/"].Sessions.Broadcast("Se cerro la sala");
            wss.Stop();
            wss = null;
            hostExists = false;

            if (inputField != null)
            {
                inputField.text = "El servidor se ha cerrado.";
                ClearInputFieldAfterDelay(5); // Borra el mensaje después de 5 segundos
            }

            Debug.Log("Servidor WebSocket detenido.");
        }
    }

    // Genera colores aleatorios
    private List<string> GenerateRandomColors(int cantidad)
    {
        List<string> randomColors = new List<string>();
        System.Random random = new System.Random();

        for (int i = 0; i < cantidad; i++)
        {
            int red = random.Next(0, 256);
            int green = random.Next(0, 256);
            int blue = random.Next(0, 256);

            // Formato hexadecimal del color
            string hexColor = $"#{red:X2}{green:X2}{blue:X2}";
            randomColors.Add(hexColor);
        }

        return randomColors;
    }

    // Limpia el texto del campo de entrada después de un retraso
    private void ClearInputFieldAfterDelay(float delay)
    {
        if (inputField != null)
        {
            StartCoroutine(ClearInputFieldCoroutine(delay));
        }
    }

    private IEnumerator ClearInputFieldCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        inputField.text = "";
    }

    public class ChatBehavior : WebSocketBehavior
    {
        private string clienteId;
        private string clienteColor;
        private static List<string> colors;
        private static System.Random random = new System.Random();

        // Configura la lista de colores
        public static void SetColors(List<string> colorList)
        {
            colors = colorList;
        }

        protected override void OnOpen()
        {
            clienteId = "Usuario" + Sessions.Count;
            clienteColor = colors != null && colors.Count > 0 ? colors[random.Next(colors.Count)] : "#FFFFFF";

            if (isFirstConnection())
            {
                string serverMessage = "Servidor iniciado.";
                Debug.Log(serverMessage);
                Sessions.Broadcast(serverMessage);
                historialMensajes.Add(serverMessage);
            }

            string userMessage = $"<color={clienteColor}>{clienteId}</color> se ha conectado.";
            Debug.Log(userMessage);
            Sessions.Broadcast(userMessage);
            historialMensajes.Add($"{clienteId} se ha conectado.");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            string message = $"<color={clienteColor}>{clienteId}</color>: {e.Data}";
            string messageFormated = $"{clienteId}: {e.Data}";
            Debug.Log("Mensaje recibido: " + message);
            Sessions.Broadcast(message);
            historialMensajes.Add(messageFormated);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            string userMessage = $"<color={clienteColor}>{clienteId}</color> se ha desconectado.";
            Debug.Log(userMessage);
            Sessions.Broadcast(userMessage);
            historialMensajes.Add($"{clienteId} se ha desconectado.");

            if (isHostDisconnection())
            {
                string serverMessage = "El servidor se ha cerrado.";
                Debug.Log(serverMessage);
                Sessions.Broadcast(serverMessage);
                historialMensajes.Add(serverMessage);
            }
        }

        private bool isFirstConnection()
        {
            return Sessions.Count == 1;
        }

        private bool isHostDisconnection()
        {
            return Sessions.Count == 0;
        }
    }
}
