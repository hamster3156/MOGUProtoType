using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ItemController : MonoBehaviour
{
    [SerializeField]
    private Transform targetTransform;

    [SerializeField] 
    private Rigidbody targetRigidbody;

    [SerializeField] 
    private RectTransform targetRectTransform;

    [SerializeField] 
    private Camera mainCamera;

    [SerializeField]
    private RawImage itemRawImage;

    [SerializeField]
    private float throwForce = 10f;

    [SerializeField]
    private float throwDistance = -6;

    [SerializeField]
    private float throwUpwardForce = 5f;

    [SerializeField]
    private float stopMoveTimer = 0.5f;

    [SerializeField]
    private float cameraDistanceZ = 5;

    public float startDistance = 10f; // スケーリング開始距離
    public float hiddenDistance = 2f; // 完全に最小スケールにする距離
    public float maxScale = 0.01f; // 最大スケール
    public float minScale = 0.001f; // 最小スケール
    public float lerpSpeed = 0.1f; // 補間速度

    private Vector3 lastMousePosition;
    private Vector3 throwDirection;

    public bool canDrop = false;

    public bool isThrow = false;
    
    [SerializeField]
    private PlayerController playerController;

    [SerializeField] private Vector3 initPos;

    public void OnThrow(bool isThrow)
    {
        this.isThrow = isThrow;
    }

    private void Start()
    {
        initPos = transform.position;

        Init();
    }

    private void FixedUpdate()
    {
        Rotation();
        OnFollowTransformMousePos();
        ReSize();
    }

    private void Rotation()
    {
        transform.rotation = Camera.main.transform.rotation;
    }

    private void ReSize()
    {
        if(!canDrop)
        {
            return;
        }

        // メインカメラとUIオブジェクトのZ軸の距離を計算する
        float zDistance = Mathf.Abs(mainCamera.transform.position.z - targetRectTransform.position.z);

        // 距離に基づいて目標スケールを計算する
        float targetScale;
        if (zDistance <= hiddenDistance)
        {
            targetScale = maxScale;
        }
        else if (zDistance <= startDistance)
        {
            float t = (zDistance - hiddenDistance) / (startDistance - hiddenDistance);
            targetScale = Mathf.Lerp(maxScale, minScale, t);
        }
        else
        {
            targetScale = minScale;
        }

        // 現在のスケールから目標スケールへの補間を行う
        float currentScale = targetRectTransform.localScale.x; // x, y, z のどれも同じなので x を使う
        float newScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * lerpSpeed);

        // スケールを適用する
        targetRectTransform.localScale = new Vector3(newScale, newScale, newScale);
    }

    public void OnFollowTransformMousePos()
    {
        if (!isThrow)
        {
            return;
        }

        if (canDrop)
        {
            return;
        }

        if(Input.GetMouseButton(0))
        {
            ActiveKinematic(true);
            
            // スクリーン座標のXの回転軸の数値が0じゃないので修正している
            Vector3 cameraPosition = Input.mousePosition;

            // カメラからオブジェクトを離す距離
            cameraPosition.z = 10;
            Vector3 point = Camera.main.ScreenToWorldPoint(cameraPosition);

            // 今回はカメラのX軸の数値が違うので、その分の修正を行う
            var rotationX = Camera.main.transform.eulerAngles.x;

            // Y軸の角度をrotaitionX分ずらして修正する
            float radiun = point.z / Mathf.Cos(rotationX * Mathf.Deg2Rad);
            float modY = radiun * Mathf.Sin(rotationX * Mathf.Deg2Rad);
            Vector3 screenPos = new Vector3(point.x, point.y + modY, throwDistance);
            targetTransform.transform.position = screenPos;

            //lastMousePosition = Input.mousePosition;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            ActiveKinematic(false);
            ThrowItem();
            canDrop = true;
        }
    }

    public void ActiveKinematic(bool isActive)
    {
        targetRigidbody.isKinematic = isActive;
    }

    void ThrowItem()
    {
        // カメラの前方向に加える力
        var throwForwardVelocity = mainCamera.transform.forward * throwForce;
        var throwUpwardVelocity = Vector3.up * throwUpwardForce;
        var throwVelocity = throwForwardVelocity + throwUpwardVelocity;
        targetRigidbody.AddForce(throwVelocity, ForceMode.VelocityChange);
    }

    public void SetRawImage(RawImage setRawImage)
    {
       itemRawImage.texture = setRawImage.texture;
    }

    public void OnActiveObject(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void Init()
    {
        canDrop = false;
        transform.position = initPos;
        targetRigidbody.isKinematic = true;
        targetRectTransform.localScale = new Vector3(maxScale, maxScale, maxScale);
    }

    private async void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            try
            {
                var cancelTokenSource = new CancellationTokenSource();
                await UniTask.Delay(TimeSpan.FromSeconds(stopMoveTimer), cancellationToken: cancelTokenSource.Token);
                targetRigidbody.velocity = Vector3.zero;
                targetRigidbody.angularVelocity = Vector3.zero;
                cancelTokenSource.Dispose();
                //Debug.Log("Ground");

                playerController.TargetMovePos(transform);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}
