using OpenAI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private CameraData cameraData;

    [SerializeField]
    private float randomMoveRadius = 5f;

    [SerializeField]
    private float randomMoveInterval = 5f;

    private NavMeshAgent navMeshAgent;
    private float currentRandomMoveInterval = 0f;

    [SerializeField]
    private ChatGptController chatGptController;

    [SerializeField]
    private RawImage rawImage;

    [SerializeField]
    private GameObject eatPanel;

    [SerializeField]
    private GameObject nomalPanel;

    public bool canEat = false;

    [SerializeField]
    private CalendarDate calendarDate;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        currentRandomMoveInterval = randomMoveInterval;
    }

    void Update()
    {
        if (canEat)
        {
            return;
        }

        currentRandomMoveInterval += Time.deltaTime;

        if(currentRandomMoveInterval >= randomMoveInterval)
        {
            RandomMovePos();
            currentRandomMoveInterval = 0f;
        }
    }

    private void RandomMovePos()
    {
        // ƒ‰ƒ“ƒ_ƒ€‚ÅˆÚ“®‚·‚é”ÍˆÍ“à‚ğŒˆ‚ß‚é
        Vector3 randomDirction = Random.insideUnitSphere * randomMoveRadius;
        randomDirction += transform.position;

        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(randomDirction, out navMeshHit, randomMoveRadius, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(navMeshHit.position);
        }
        else
        {
            Debug.Log("ˆÚ“®æ‚ªŒ©‚Â‚©‚ç‚È‚©‚Á‚½");
        }
    }

    public void TargetMovePos(Transform targetTransform)
    {
        canEat = true;
        navMeshAgent.SetDestination(targetTransform.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {
            var itemController = collision.gameObject.GetComponent<ItemController>();
            itemController.Init();
            itemController.isThrow = false;
            collision.gameObject.SetActive(false);

            Texture2D texture = rawImage.texture as Texture2D;
            chatGptController.ReqestImage(texture);
            eatPanel.SetActive(false);
            nomalPanel.SetActive(true);
            canEat = false;
        }
    }
}
