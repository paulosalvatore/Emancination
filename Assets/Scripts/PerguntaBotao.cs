using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerguntaBotao : MonoBehaviour,
	IPointerClickHandler
{
	public Sprite spriteSelected;
	private Sprite spriteDefault;

	private Image image;

	private void Awake()
	{
		image = GetComponent<Image>();
		spriteDefault = image.sprite;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Jogo.instancia.SelecionarBotaoPergunta(this);
	}

	public void SelecionarBotao()
	{
		image.sprite = spriteSelected;
	}

	public void DesselecionarBotao()
	{
		image.sprite = spriteDefault;
	}
}
