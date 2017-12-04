using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using WebSocketSharp;
using OneSignalPush.MiniJSON;
using System.Collections.Generic;
using UnityEngine.Networking;

public class StatusCallManager : MonoBehaviour
{
    [Header("Screen Status Ocurrence")]
    [SerializeField] private TextMeshProUGUI m_TextStatusCurrent;
    [SerializeField] private GameObject m_ButtonStartCall;
    [SerializeField] private GameObject m_ButtonFinishCall;
    [SerializeField] private GameObject m_ButtonCheckin;
    [SerializeField] private GameObject m_ButtonCheckout;

    [Header("Buttons top")]
    [SerializeField] private GameObject buttonInformationOcurrence;
    [SerializeField] private GameObject buttonCallEmergency;
    [SerializeField] private GameObject buttonChat;

    [Header("Screen Information Ocurrence")]
    [SerializeField] private InputField labelPhone;
    [SerializeField] private InputField labelNameRequester;
    [SerializeField] private InputField labelCounty;
    [SerializeField] private InputField labelAddress;
    [SerializeField] private InputField labelNumberAddress;
    [SerializeField] private InputField labelNeighborhood;
    [SerializeField] private InputField labelReference;
    [SerializeField] private InputField labelPatient;
    [SerializeField] private InputField labelSex;
    [SerializeField] private InputField labelAge;
    [SerializeField] private InputField labelProblemPatient;
    [SerializeField] private InputField labelMoreInformation;
    [SerializeField] private GameObject m_IconEmergency;

    [Header("Screen Emergency medical")]
    [SerializeField] private InputField labelPression;
    [SerializeField] private InputField labelHeartRate;
    [SerializeField] private InputField labelOxygenSaturation;
    [SerializeField] private InputField labelTemperature;

    private Occurrence occurrence;
    public static string getAddressComplete;

    private string api = "http://172.10.100.75:8080/";

    private string urlWebSocket = "";

    private WebSocket webSocket;

    void Start()
    {
        urlWebSocket = "ws://172.10.100.75:2325/zap/" + SystemInfo.deviceUniqueIdentifier;

        webSocket = new WebSocket(urlWebSocket);
        webSocket.OnMessage += ReceiveDataWebSocket;
        webSocket.OnError += ErrorWebSocket;
        webSocket.Connect();

        getAddressComplete = "";
        occurrence = new Occurrence();

        StartCoroutine(SendLocalization_Coroutine());
    }

    private void ReceiveDataWebSocket(object sender, MessageEventArgs dataReceived)
    {
        print("Data WebSocket:: " + dataReceived.Data);

        VerifyInformationReceivedWebSocket(dataReceived.Data);
    }

    private void ErrorWebSocket(object sender, ErrorEventArgs dataReceived)
    {
        print("Error WebSocket:: " + dataReceived.Message);

        m_TextStatusCurrent.text = "Houve um erro. Reinicie aplicativo... \n " + dataReceived.Message;
    }

    private void VerifyInformationReceivedWebSocket(string dataAsJson)
    {
        occurrence = JsonUtility.FromJson<Occurrence>(dataAsJson);

        var addressComplete = string.Format("{0} {1} {2} {3}", occurrence.endereco, occurrence.numero, occurrence.bairro, occurrence.municipio);

        getAddressComplete = addressComplete;

        m_TextStatusCurrent.text = "Recebendo uma ocorrência...";
        m_ButtonStartCall.SetActive(true);
    }

    #region Buttons

    public void GetOcurrence()
    {
        m_ButtonStartCall.SetActive(false);
        m_ButtonFinishCall.SetActive(true);

        m_TextStatusCurrent.text = "Em uma ocorrência...";
        occurrence.status = 'B';

        m_ButtonCheckin.SetActive(true);

        if (!occurrence.chamadaMedica)
            m_IconEmergency.SetActive(false);
        else
            m_IconEmergency.SetActive(true);

        buttonInformationOcurrence.SetActive(true);
        buttonCallEmergency.SetActive(true);
        buttonChat.SetActive(true);

        SetInformationsOccurrenceCurrent();
    }

    public void FinishOcurrence()
    {
        m_ButtonCheckout.SetActive(false);
        m_ButtonFinishCall.SetActive(false);
        occurrence.status = 'C';
        m_TextStatusCurrent.text = "Nenhum ocorrência no momento...";

        buttonInformationOcurrence.SetActive(false);
        buttonCallEmergency.SetActive(false);
        buttonChat.SetActive(false);
    }

    private void SetInformationsOccurrenceCurrent()
    {
        labelPhone.text = occurrence.telefone;
        labelNameRequester.text = occurrence.nome;
        labelCounty.text = occurrence.municipio;
        labelAddress.text = occurrence.endereco;
        labelNumberAddress.text = occurrence.numero;
        labelNeighborhood.text = occurrence.bairro;
        labelReference.text = occurrence.referencia;
        labelPatient.text = occurrence.paciente_nome;
        labelSex.text = occurrence.sexo.ToString();
        labelAge.text = occurrence.paciente_idade.ToString();
        labelProblemPatient.text = occurrence.queixa;
        labelMoreInformation.text = occurrence.obervacoes;
    }

    public void OnButtonCheckinCliked()
    {
        m_ButtonCheckin.SetActive(false);
        m_ButtonCheckout.SetActive(true);
    }

    public void OnButtonCheckoutCliked()
    {
        m_ButtonCheckout.SetActive(false);
    }

    public void OnButtonSubmitDataLifeClicked()
    {
        StartCoroutine(SubmitDataLife_Coroutine());
    }

    #endregion

    private IEnumerator SubmitDataLife_Coroutine()
    {
        var emergencyMedical = new EmergencyMedical();

        emergencyMedical.heartRate = labelHeartRate.text;
        emergencyMedical.oxygenSaturation = labelOxygenSaturation.text;
        emergencyMedical.pression = labelPression.text;
        emergencyMedical.temperature = labelTemperature.text;

        UnityWebRequest request = UnityWebRequest.Post("url", JsonUtility.ToJson(emergencyMedical));

        yield return request.Send();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Send data Emergency medical");

            labelHeartRate.text = string.Empty;
            labelOxygenSaturation.text = string.Empty;
            labelPression.text = string.Empty;
            labelTemperature.text = string.Empty;
        }
    }

    private IEnumerator SendLocalization_Coroutine()
    {
        yield return new WaitForSeconds(30f);

        Input.location.Start();
        var lastPosition = new LocationService();

        var positionOccurrence = new PositionOccurrence();

        positionOccurrence.idUser = SystemInfo.deviceUniqueIdentifier;
        positionOccurrence.latitude = lastPosition.lastData.latitude.ToString();
        positionOccurrence.longitude = lastPosition.lastData.longitude.ToString();

        var dataAsJson = JsonUtility.ToJson(positionOccurrence);
        webSocket.Send(dataAsJson);

        print("Enviou localização");
        StartCoroutine(SendLocalization_Coroutine());
    }

    private IEnumerator UpdateOccurrence()
    {
        UnityWebRequest request = UnityWebRequest.Put("url", JsonUtility.ToJson(occurrence));

        yield return request.Send();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Update Occurrence");
        }
    }
}

#region Models

[Serializable]
public class PositionOccurrence
{
    public string idUser;
    public string latitude;
    public string longitude;
}

[Serializable]
public class Occurrence
{
    public int id;
    public string nome;
    public string telefone;
    public string municipio;
    public string endereco;
    public string numero;
    public string bairro;
    public string referencia;
    public string paciente_nome;
    public int paciente_idade;
    public char sexo;
    public string queixa;
    public string obervacoes;
    public bool chamadaMedica;
    public DateTime inicio;
    public DateTime fim;
    public char status;
}

[Serializable]
public class EmergencyMedical
{
    public string pression;
    public string heartRate;
    public string oxygenSaturation;
    public string temperature;
}

#endregion
