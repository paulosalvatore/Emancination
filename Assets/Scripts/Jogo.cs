using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Jogo : MonoBehaviour
{
	[Header("Perguntas")]
	public GameObject confirmarPerguntaGameObject;
	public Image fundoPerguntaImage;
	public GameObject fundoAnimarPergunta;
	public Text perguntaText;
	public PerguntaBotao botaoSim;
	public PerguntaBotao botaoNao;
	public Image fecharJornalImage;
	public GameObject novaVotacaoGameObject;
	public GameObject tempoRestanteVotacaoGameObject;
	public Text tempoRestanteVotacaoText;
	public RectTransform cronometroBarraRectTransform;
	private int opcaoSelecionada = 0;

	private string[] perguntas = new string[5]
	{
		"PROJETO DE CONSTRUÇÃO DE UMA USINA NUCLEAR.",
		"A POPULAÇÃO ESTÁ ENVELHECENDO RÁPIDO. DEVEMOS AJUDAR OS IDOSOS?",
		"IMIGRANTES CHEGAM NA CIDADE. ACEITÁ-LOS?",
		"A METEOROLOGIA PREVIU UMA SÉRIE DE TEMPESTADES. INVESTIR EM INFRA ESTRUTURA?",
		"A POPULAÇÃO APROVA O TRANSPORTE PÚBLICO, MAS ELE PRECISA DE MANUTENÇÃO. INVESTIR?"
	};

	[Header("Votação")]
	public float duracaoVotacao;
	public float duracaoExibirVotacaoEncerrada;
	private bool votacaoAberta = false;
	private int votacaoIdAtualizada = 0;
	private int votacaoIdAberta = -1;

	[Header("Confirmação de Voto")]
	public float shakeDetectionThreshold;
	private bool confirmacaoVotoLiberada = false;
	private float accelerometerUpdateInterval = 1f / 60f;
	private float lowPassKernelWidthInSeconds = 1f;
	private float lowPassFilterFactor;
	private Vector3 lowPassValue;

	[Header("Câmera")]
	public float velocidadeSwipeCamera;
	public float velocidadeZoomCamera;
	public Range cameraSize;
	public Range cameraPositionX;
	public Range cameraPositionY;
	private bool swipeCameraHabilitado = true;

	[Header("Jornal")]
	public List<Sprite> jornais;
	public Image jornalImage;
	public Animator jornalAnimator;

	[Header("Jogadores")]
	public Text pessoasOnlineText;
	private int online = 1;

	[Header("Cidade")]
	public RectTransform economiaBarra;
	public RectTransform empregoBarra;
	public RectTransform felicidadeBarra;
	public RectTransform educacaoBarra;
	public RectTransform saudeBarra;
	public RectTransform economiaBarraFinal;
	public RectTransform empregoBarraFinal;
	public RectTransform felicidadeBarraFinal;
	public RectTransform educacaoBarraFinal;
	public RectTransform saudeBarraFinal;
	public GameObject telaFinal;
	private bool atualizarBarraFinal = false;
	private int economia;
	private int emprego;
	private int felicidade;
	private int educacao;
	private int saude;
	private string evento;
	private int jornal;

	[Header("Eventos")]
	public List<GameObject> atualizarNuclear;
	public List<GameObject> atualizarNuclearTrees;
	public List<GameObject> atualizarPredios;
	public List<GameObject> atualizarPrediosTrees;
	public List<GameObject> atualizarEstrada;
	public List<GameObject> atualizarEstradaTrees;

	[Header("Poder de Emancipação")]
	public Text poderEmancipacaoText;
	private int poderEmancipacao = 60;

	// Métodos Estáticos

	public static Jogo instancia;

	// Métodos de Inicialização

	private void Awake()
	{
		instancia = this;

		Screen.SetResolution(720, 1280, true);

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
		lowPassValue = Input.acceleration;

		InvokeRepeating("AtualizarJogadoresOnline", 0.1f, 1f);

		StartCoroutine(AtualizarStatusCidade());

		StartCoroutine(IniciarChecagemVotacaoAberta());
	}

	private void Update()
	{
		// Checamos se a Confirmação de Voto Liberada
		ChecarConfirmacaoVoto();

		// Movimentar a câmera baseado no swipe
		SwipeCamera();

		AtualizarTextoJogadores();

		AtualizarBarrasStatus();

		AtualizarBarrasFinaisStatus();

		AtualizarPoderEmancipacao();

		if (Input.GetKeyDown(KeyCode.W))
		{
			InserirEvento("nuclear");
		}
		else if (Input.GetKeyDown(KeyCode.E))
		{
			InserirEvento("predio");
		}
		else if (Input.GetKeyDown(KeyCode.R))
		{
			InserirEvento("estrada");
		}
	}

	private void AtualizarPoderEmancipacao()
	{
		poderEmancipacaoText.text = string.Format("{0:00}%", poderEmancipacao);
	}

	// Câmera

	private void SwipeCamera()
	{
		if (!swipeCameraHabilitado)
			return;

		if (Input.touchCount == 1)
		{
			Touch touch = Input.GetTouch(0);

			if (touch.phase == TouchPhase.Moved)
			{
				if (touch.position.x >= cameraPositionX.min &&
					touch.position.x <= cameraPositionX.max &&
					touch.position.y >= cameraPositionY.min &&
					touch.position.y <= cameraPositionY.max)
				{
					Vector2 touchDeltaPosition = touch.deltaPosition;

					Camera.main.transform.Translate(
						-touchDeltaPosition.x * velocidadeSwipeCamera * Time.smoothDeltaTime,
						-touchDeltaPosition.y * velocidadeSwipeCamera * Time.smoothDeltaTime,
						0
					);
				}
			}
		}
		else if (Input.touchCount == 2)
		{
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

			Camera.main.orthographicSize += deltaMagnitudeDiff * velocidadeZoomCamera;

			Camera.main.orthographicSize =
				Mathf.Clamp(
					Camera.main.orthographicSize,
					cameraSize.min,
					cameraSize.max
				);
		}
	}

	// Jornal

	private void AtualizarJornal(int jornalId)
	{
		if (jornalId == 0)
			return;

		jornalImage.sprite = jornais[jornalId - 1];

		ExibirJornal();
	}

	private void ExibirJornal()
	{
		AlterarJornalAnimator(true);

		OcultarBotaoResultado();

		Invoke("ExibirBotaoFecharJornal", 1f);

		Debug.Log("Exibir Jornal");
	}

	public void OcultarJornal(bool ativarChecagemBancoDados = false)
	{
		AlterarJornalAnimator(false);

		OcultarBotaoFecharJornal();

		Debug.Log("Ocultar Jornal");
	}

	private void AlterarJornalAnimator(bool estado)
	{
		jornalAnimator.SetBool("Exibir", estado);
	}

	private void ExibirBotaoFecharJornal()
	{
		fecharJornalImage.gameObject.SetActive(true);
	}

	private void OcultarBotaoFecharJornal()
	{
		fecharJornalImage.gameObject.SetActive(false);
	}

	// Votação

	private void IniciarVotacao(int estagioRecebido, int duracao)
	{
		if (votacaoIdAberta == estagioRecebido)
			return;

		votacaoAberta = true;

		duracaoVotacao = duracao;

		AtualizarPergunta(estagioRecebido);

		OcultarJornal();

		ExibirNovaVotacao();

		IniciarCronometroVotacao();

		ExibirBarraCronometroVotacao();

		StartCoroutine(AtualizarCronometroVotacao());

		votacaoIdAberta = estagioRecebido;

		Debug.Log("Exibir Nova Votacao");
	}

	private void EncerrarVotacao()
	{
		votacaoAberta = false;

		OcultarNovaVotacao();

		OcultarPergunta();
		OcultarFundoConfirmarVoto();

		ExibirCronometroVotacao();

		perguntaText.text = "VOTAÇÃO ENCERRADA!";

		ExibirTextoPergunta();

		Invoke("OcultarCronometroVotacao", duracaoExibirVotacaoEncerrada);
		Invoke("OcultarTextoPergunta", duracaoExibirVotacaoEncerrada);

		StartCoroutine(ChecarEventoVotacao());

		Debug.Log("Encerrar Votação");
	}

	private void ExibirNovaVotacao()
	{
		novaVotacaoGameObject.SetActive(true);
	}

	private void OcultarNovaVotacao()
	{
		novaVotacaoGameObject.SetActive(false);
	}

	private IEnumerator ChecarEventoVotacao()
	{
		yield return new WaitForSeconds(duracaoExibirVotacaoEncerrada);

		bool eventoChecado = false;

		while (!eventoChecado)
		{
			WWW www = new WWW(Json.urlBase + "votacoes/evento");

			yield return www;

			if (www.error == null)
			{
				Votacao votacao = JsonUtility.FromJson<Votacao>(www.text);

				if (votacao.votacaoId > 0 && votacao.votacaoId != votacaoIdAtualizada)
				{
					eventoChecado = true;

					AtualizarJornal(votacao.escolha.jornal);

					InserirEvento(votacao.escolha.evento);

					ExibirBotaoResultado(votacao.escolha.escolha);

					if (votacao.escolha.escolha == opcaoSelecionada)
					{
						poderEmancipacao += 10;
					}
					else
					{
						poderEmancipacao -= 10;
					}

					votacaoIdAtualizada = votacao.votacaoId;

					if (votacaoIdAberta == 5)
					{
						ExibirTelaFinal();
					}
				}
			}
			else
			{
				Debug.Log("WWW Error: " + www.error);
			}

			yield return new WaitForSeconds(0.2f);
		}
	}

	public Image resultadoVotacaoImage;
	public Sprite votacaoAprovada;
	public Sprite votacaoDeclinada;

	private void ExibirBotaoResultado(int escolha)
	{
		resultadoVotacaoImage.gameObject.SetActive(true);

		resultadoVotacaoImage.sprite = escolha == 0 ? votacaoAprovada : votacaoDeclinada;

		Invoke("OcultarBotaoResultado", 4f);
	}

	private void OcultarBotaoResultado()
	{
		resultadoVotacaoImage.gameObject.SetActive(false);
	}

	// Eventos

	private void InserirEvento(string evento)
	{
		if (evento == "")
			return;

		switch (evento)
		{
			case "nuclear":
				AlterarGameObjects(atualizarNuclear, true);
				AlterarGameObjects(atualizarNuclearTrees, false);
				break;

			case "predio":
				AlterarGameObjects(atualizarPredios, true);
				AlterarGameObjects(atualizarPrediosTrees, false);
				break;

			case "estrada":
				AlterarGameObjects(atualizarEstrada, true);
				AlterarGameObjects(atualizarEstradaTrees, false);
				break;
		}

		Debug.Log("Inserir evento: " + evento);
	}

	private void AlterarGameObjects(List<GameObject> lista, bool estado)
	{
		foreach (GameObject objeto in lista)
		{
			objeto.SetActive(estado);
		}
	}

	// Perguntas

	private void AtualizarPergunta(int estagio)
	{
		string pergunta = perguntas[estagio - 1];

		perguntaText.text = pergunta;

		Debug.Log("Atualizar Pergunta: " + pergunta);
	}

	public void IniciarExibicaoPergunta()
	{
		OcultarNovaVotacao();

		AlterarExibicaoPergunta(true);

		Debug.Log("Inicia Exibição da Pergunta");
	}

	private void ExibirPergunta()
	{
		AlterarExibicaoPergunta(true);

		Debug.Log("Exibir Pergunta");
	}

	private void OcultarPergunta()
	{
		AlterarExibicaoPergunta(false);

		Debug.Log("Ocultar Pergunta");
	}

	private void AlterarExibicaoPergunta(bool estado)
	{
		AlterarExibicaoTextoPergunta(true);

		botaoSim.gameObject.SetActive(estado);
		botaoNao.gameObject.SetActive(estado);

		Debug.Log("Alterar exibição da pergunta");
	}

	private void ExibirTextoPergunta()
	{
		AlterarExibicaoTextoPergunta(true);
	}

	private void OcultarTextoPergunta()
	{
		AlterarExibicaoTextoPergunta(false);

		Debug.Log("Ocultar Texto Pergunta");
	}

	private void AlterarExibicaoTextoPergunta(bool estado)
	{
		perguntaText.gameObject.SetActive(estado);

		Debug.Log("Alterar Exibicao do Texto da Pergunta");
	}

	public void SelecionarBotaoPergunta(PerguntaBotao botao)
	{
		DesselecionarBotoes(botao);

		botao.SelecionarBotao();

		opcaoSelecionada = botao == botaoSim ? 0 : 1;

		ExibirFundoConfirmarVoto();

		confirmacaoVotoLiberada = true;

		Debug.Log("Selecionar Pergunta do Botao");
	}

	private void DesselecionarBotoes(PerguntaBotao botao)
	{
		PerguntaBotao desselecionarBotao = botao == botaoSim ? botaoNao : botaoSim;

		desselecionarBotao.DesselecionarBotao();
	}

	// Jogadores

	private void AtualizarTextoJogadores()
	{
		pessoasOnlineText.text = string.Format("{0:000}", online);
	}

	// Confirmação de Voto

	private void ChecarConfirmacaoVoto()
	{
		if (!confirmacaoVotoLiberada)
			return;

		Vector3 acceleration = Input.acceleration;
		lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
		Vector3 deltaAcceleration = acceleration - lowPassValue;

		if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold ||
			Input.GetKeyDown(KeyCode.Z))
			ConfirmarVoto();

		Debug.Log("Checar Confirmação de Voto");
	}

	private void ConfirmarVoto()
	{
		confirmacaoVotoLiberada = false;

		OcultarPergunta();

		OcultarFundoConfirmarVoto();

		ExibirCronometroVotacao();

		StartCoroutine(EnviarConfirmacaoVoto());
	}

	private void ExibirFundoConfirmarVoto()
	{
		confirmarPerguntaGameObject.SetActive(true);

		fundoAnimarPergunta.SetActive(true);
	}

	private void OcultarFundoConfirmarVoto()
	{
		confirmarPerguntaGameObject.SetActive(false);

		fundoAnimarPergunta.SetActive(false);
	}

	private IEnumerator EnviarConfirmacaoVoto()
	{
		bool sucesso = false;

		while (!sucesso)
		{
			Debug.Log("Tentar a validação do voto.");

			WWW www = new WWW(Json.urlBase + "votacoes/votar/" + Jogador.instancia.id + "/" + opcaoSelecionada);

			yield return www;

			if (www.error == null)
			{
				Retorno retorno = JsonUtility.FromJson<Retorno>(www.text);

				sucesso = retorno.retorno == 1 ? true : false;
			}
			else
			{
				Debug.Log("WWW Error: " + www.error);
			}

			yield return new WaitForSeconds(0.1f);
		}
	}

	// Cronômetro de Votação

	private float tempoInicioVotacao = 0;

	private void IniciarCronometroVotacao()
	{
		tempoInicioVotacao = Time.time;
	}

	private void ExibirBarraCronometroVotacao()
	{
		cronometroBarraRectTransform.gameObject.SetActive(true);
	}

	private void ExibirCronometroVotacao()
	{
		tempoRestanteVotacaoGameObject.SetActive(true);
	}

	private void OcultarCronometroVotacao()
	{
		tempoRestanteVotacaoGameObject.SetActive(false);
		cronometroBarraRectTransform.gameObject.SetActive(false);
	}

	private IEnumerator AtualizarCronometroVotacao()
	{
		while (true)
		{
			float duracao = Mathf.Max(0, duracaoVotacao - (Time.time - tempoInicioVotacao));

			tempoRestanteVotacaoText.text = string.Format("{0:00.00}", duracao);

			cronometroBarraRectTransform.anchorMax = new Vector2(
				duracao / duracaoVotacao,
				0
			);

			if (duracao == 0)
			{
				EncerrarVotacao();

				break;
			}

			yield return null;
		}
	}

	// Banco de Dados

	private IEnumerator IniciarChecagemVotacaoAberta()
	{
		Debug.Log("Checagem de Votacao Aberta iniciada");

		while (true)
		{
			if (!votacaoAberta)
			{
				if (Jogador.instancia && Jogador.instancia.id > 0)
				{
					WWW www = new WWW(Json.urlBase + "votacoes/aberta");

					yield return www;

					if (www.error == null)
					{
						Votacao retorno = JsonUtility.FromJson<Votacao>(www.text);

						if (retorno.votacaoId > 0)
							IniciarVotacao(retorno.estagio, retorno.duracao);
					}
					else
					{
						Debug.Log("WWW Error: " + www.error);
					}
				}

				yield return new WaitForSeconds(0.2f);
			}
			else
			{
				yield return new WaitForSeconds(1f);
			}
		}
	}

	private void AtualizarJogadoresOnline()
	{
		StartCoroutine(GravarJogadoresOnline());
	}

	private IEnumerator GravarJogadoresOnline()
	{
		WWW www = new WWW(Json.urlBase + "jogadores/online");

		yield return www;

		if (www.error == null)
		{
			Retorno retorno = JsonUtility.FromJson<Retorno>(www.text);

			online = retorno.retorno;

			AtualizarTextoJogadores();

			// Debug.Log("Jogadores Online: " + online);
		}
		else
		{
			Debug.Log("WWW Error: " + www.error);
		}
	}

	private IEnumerator AtualizarStatusCidade()
	{
		while (true)
		{
			WWW www = new WWW(Json.urlBase + "votacoes/status");

			yield return www;

			if (www.error == null)
			{
				Status retorno = JsonUtility.FromJson<Status>(www.text);

				economia = retorno.economia;
				emprego = retorno.emprego;
				felicidade = retorno.felicidade;
				educacao = retorno.educacao;
				saude = retorno.saude;
				evento = retorno.evento;
				jornal = retorno.jornal;

				Debug.Log("Atualizar Status Cidade");
			}
			else
			{
				Debug.Log("WWW Error: " + www.error);
			}
		}
	}

	private void AtualizarBarrasStatus()
	{
		float max = 3;

		economiaBarra.anchorMax = new Vector2(
			Mathf.Lerp(economiaBarra.anchorMax.x, economia / max, 0.1f),
			1
		);

		empregoBarra.anchorMax = new Vector2(
			Mathf.Lerp(empregoBarra.anchorMax.x, emprego / max, 0.1f),
			1
		);

		felicidadeBarra.anchorMax = new Vector2(
			Mathf.Lerp(felicidadeBarra.anchorMax.x, felicidade / max, 0.1f),
			1
		);

		educacaoBarra.anchorMax = new Vector2(
			Mathf.Lerp(educacaoBarra.anchorMax.x, educacao / max, 0.1f),
			1
		);

		saudeBarra.anchorMax = new Vector2(
			Mathf.Lerp(saudeBarra.anchorMax.x, saude / max, 0.1f),
			1
		);
	}

	private void AtualizarBarrasFinaisStatus()
	{
		if (!atualizarBarraFinal)
			return;

		float max = 3;

		economiaBarraFinal.anchorMax = new Vector2(
			Mathf.Lerp(economiaBarraFinal.anchorMax.x, economia / max, 0.1f),
			1
		);

		empregoBarraFinal.anchorMax = new Vector2(
			Mathf.Lerp(empregoBarraFinal.anchorMax.x, emprego / max, 0.1f),
			1
		);

		felicidadeBarraFinal.anchorMax = new Vector2(
			Mathf.Lerp(felicidadeBarraFinal.anchorMax.x, felicidade / max, 0.1f),
			1
		);

		educacaoBarraFinal.anchorMax = new Vector2(
			Mathf.Lerp(educacaoBarraFinal.anchorMax.x, educacao / max, 0.1f),
			1
		);

		saudeBarraFinal.anchorMax = new Vector2(
			Mathf.Lerp(saudeBarraFinal.anchorMax.x, saude / max, 0.1f),
			1
		);
	}

	private void ExibirTelaFinal()
	{
		telaFinal.SetActive(true);

		Invoke("AtivarAtualizacaoBarrasFinaisStatus", 1f);
	}

	private void AtivarAtualizacaoBarrasFinaisStatus()
	{
		atualizarBarraFinal = true;
	}
}
