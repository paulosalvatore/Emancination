using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Json : MonoBehaviour
{
	internal static string urlBase = "http://192.168.227.62/ControleUnityJson/";

	private void Start()
	{
		string url = urlBase;
		WWW www = new WWW(url);
		StartCoroutine(WaitForRequest(www));
	}

	private IEnumerator WaitForRequest(WWW www)
	{
		yield return www;

		if (www.error == null)
		{
			string json = LoadResourceTextfile("perguntas");

			Estagios estagios = JsonUtility.FromJson<Estagios>(json);

			foreach (Perguntas perguntas in estagios.estagios)
			{
				foreach (Pergunta pergunta in perguntas.perguntas)
				{
					foreach (Escolha escolha in pergunta.escolhas)
					{
						Debug.Log(escolha.economia);
						Debug.Log(escolha.educacao);
						Debug.Log(escolha.emprego);
						Debug.Log(escolha.felicidade);
						Debug.Log(escolha.evento);
						Debug.Log(escolha.proximaPergunta.estagio);
						Debug.Log(escolha.proximaPergunta.pergunta);
					}
				}
			}
		}
		else
		{
			Debug.Log("WWW Error: " + www.error);
		}
	}

	public static string LoadResourceTextfile(string path)
	{
		string filePath = path.Replace(".json", "");

		TextAsset targetFile = Resources.Load<TextAsset>(filePath);

		return targetFile.text;
	}
}

[Serializable]
public class Estagios
{
	public List<Perguntas> estagios;
}

[Serializable]
public class Perguntas
{
	public List<Pergunta> perguntas;
}

[Serializable]
public class Pergunta
{
	public List<Escolha> escolhas;
}

[Serializable]
public class Escolha
{
	public ProximaPergunta proximaPergunta;
	public string economia;
	public int emprego;
	public int felicidade;
	public int educacao;
	public int saude;
	public string evento;
	public int jornal;
	public int escolha;
}

[Serializable]
public class ProximaPergunta
{
	public int estagio;
	public int pergunta;
}

[System.Serializable]
public class Retorno
{
	public int retorno;
}

[System.Serializable]
public class Votacao
{
	public int votacaoId;
	public int estagio;
	public int duracao;
	public Escolha escolha;
}

[System.Serializable]
public class Status
{
	public int economia;
	public int emprego;
	public int felicidade;
	public int educacao;
	public int saude;
	public string evento;
	public int jornal;
}
