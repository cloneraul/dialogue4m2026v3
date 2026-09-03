using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // Posição do Jogador
    public float[] playerPosition = new float[3];

    // Cena Atual
    public string currentSceneName;

    // Lista de IDs das moedas já coletadas no mapa
    public List<string> collectedCoinIDs = new List<string>();

    // Quantidade total de moedas no inventário
    public int totalCoins = 0;

    // Converte Vector3 para array de float ao salvar
    public void SetPlayerPosition(Vector3 position)
    {
        playerPosition[0] = position.x;
        playerPosition[1] = position.y;
        playerPosition[2] = position.z;
    }

    // Retorna a posição em formato Vector3 ao carregar
    public Vector3 GetPlayerPosition()
    {
        return new Vector3(playerPosition[0], playerPosition[1], playerPosition[2]);
    }
}