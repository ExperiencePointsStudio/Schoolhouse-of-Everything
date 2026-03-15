using UnityEngine;
using UnityEngine.AI; // Необходим для использования NavMeshAgent

public class NPC_Chaser : MonoBehaviour
{
    // Ссылка на объект игрока в сцене
    public Transform player;

    // Исходная позиция, куда будет телепортирован игрок при поимке
    private Vector3 startPosition;

    // Компонент для навигации NPC (если используется NavMesh)
    private NavMeshAgent agent;

    // Дистанция, на которой NPC "ловит" игрока
    public float catchDistance = 2.0f;

    void Start()
    {
        // Находим игрока по тегу "Player", если он не задан вручную в инспекторе
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("Player object not found! Please tag the player as 'Player'.");
                enabled = false; // Отключаем скрипт, если игрок не найден
                return;
            }
        }

        // Запоминаем исходную позицию игрока (предполагаем, что игрок уже на месте старта)
        // Если стартовая точка другая, можно создать отдельный пустой GameObject и использовать его позицию
        startPosition = player.position;

        // Получаем компонент NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on NPC! NPC will not move.");
        }
    }

    void Update()
    {
        if (player == null || agent == null) return;

        // NPC бежит за игроком
        agent.SetDestination(player.position);

        // Проверяем дистанцию до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < catchDistance)
        {
            // Игрок пойман! Телепортируем его на исходную позицию
            TeleportPlayerToStart();
        }
    }

    void TeleportPlayerToStart()
    {
        // Для корректной телепортации игрока, особенно если используется CharacterController
        // (как часто бывает с игровыми персонажами в Unity), его нужно временно отключить.
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.position = startPosition;
            cc.enabled = true;
        }
        else
        {
            // Если CharacterController нет (например, используется Rigidbody или просто Transform)
            player.position = startPosition;
            // При использовании Rigidbody может потребоваться сбросить скорость:
            // Rigidbody rb = player.GetComponent<Rigidbody>();
            // if (rb != null) rb.velocity = Vector3.zero;
        }

        Debug.Log("Игрок пойман и телепортирован на старт!");
    }
}
