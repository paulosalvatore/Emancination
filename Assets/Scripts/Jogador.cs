using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jogador : MonoBehaviour
{
	public float intervaloConexao;

	internal int id;

	internal static Jogador instancia;

	private void Awake()
	{
		instancia = this;

		InvokeRepeating("PegarId", 0, intervaloConexao);
	}

	private void PegarId()
	{
		id = PlayerPrefs.GetInt("jogadorId");

		string url = Json.urlBase + "jogadores/registrar/";

		if (id == 0)
		{
			StartCoroutine(DeterminarId(url));
		}
		else
		{
			StartCoroutine(RegistrarPresenca(url + id));
		}
	}

	private IEnumerator DeterminarId(string url)
	{
		WWW www = new WWW(url);

		yield return www;

		if (www.error == null)
		{
			Retorno retorno = JsonUtility.FromJson<Retorno>(www.text);

			id = retorno.retorno;

			PlayerPrefs.SetInt("jogadorId", id);

			Debug.Log("ID Definida: " + id);
		}
		else
		{
			Debug.Log("WWW Error: " + www.error);
		}
	}

	private IEnumerator RegistrarPresenca(string url)
	{
		WWW www = new WWW(url);

		while (true)
		{
			yield return www;

			if (www.error == null)
			{
				Retorno retorno = JsonUtility.FromJson<Retorno>(www.text);

				id = retorno.retorno;

				PlayerPrefs.SetInt("jogadorId", id);

				Debug.Log("Presença Registrada: " + id);

				break;
			}
			else
			{
				Debug.Log("WWW Error: " + www.error);
			}
		}
	}
}
