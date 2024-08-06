using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class CameraController : MonoBehaviour, IOverlapAction
{
    [SerializeField]
    private GameObject nomalPanelGameObject;

    [SerializeField]
    private GameObject cameraPanelGameObject;

    [SerializeField]
    private GameObject eatPanelGameObject;

    [SerializeField]
    private GameObject itemGameObject;

    [SerializeField]
    private RawImage photoRawImage;

    [SerializeField]
    private TMP_InputField firstFilePathInputFiled;

    [SerializeField]
    private RectTransform photoRectTransform, circleRectTransform, xRectTransform;

    [SerializeField]
    private UnityEvent setTexture2DEvent;

    [SerializeField]
    private UnityEvent throwItemEvent;

    private Texture2D savePhotoTexture2D;

    [SerializeField]
    private RectTransform folkRectTransform;

    [SerializeField]
    private RectTransform spoonRectTransform;

    [SerializeField]
    private float tablewareRotation;

    [SerializeField]
    private float tablewareRtationSpeed;

    [SerializeField]
    private RectTransform cameraPanelObjRect;

    [SerializeField]
    private float cameraPanelMoveSpd;

    [SerializeField]
    private CalendarDate calendarDate;

    private float initCameraPanelPosY;

    private float initFolkRotation;
    private float initSpoonRotation;

    private void Start()
    {
        initFolkRotation = folkRectTransform.localEulerAngles.z;
        initSpoonRotation = spoonRectTransform.localEulerAngles.z;
        initCameraPanelPosY = cameraPanelObjRect.anchoredPosition.y;
    }

    public async void CircleOverlap()
    {
        //OnActiveCameraPanel(false);
        cameraPanelObjRect.DOAnchorPosY(initCameraPanelPosY, cameraPanelMoveSpd);

        OnActiveNomalGameObject(false);
        eatPanelGameObject.SetActive(true);
        itemGameObject.SetActive(true);
        setTexture2DEvent?.Invoke();

        calendarDate.InstancePicture((Texture2D)photoRawImage.texture);

        using (var cts = new CancellationTokenSource())
        {
            // CancellationTokenSource����CancellationToken���擾
            var cancellationToken = cts.Token;

            // UniTask.Delay��ҋ@
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);

            // �C�x���g�𔭉�
            throwItemEvent?.Invoke();
        }
    }

    public void XOverlap()
    {
        //OnActiveCameraPanel(false);
        cameraPanelObjRect.DOAnchorPosY(initCameraPanelPosY, cameraPanelMoveSpd);
    }

    public void OnActiveNomalGameObject(bool isActive)
    {
        nomalPanelGameObject.SetActive(isActive);
    }

    public void OnActiveCameraPanel(bool isActive)
    {
        cameraPanelGameObject.SetActive(isActive);
    }

    public async UniTask OnOpenCameraPanel()
    {
        using (var cts = new CancellationTokenSource())
        {
            var cancellationToken = cts.Token;
            TablewareRotation();
            await UniTask.Delay(TimeSpan.FromSeconds(tablewareRtationSpeed), cancellationToken: cancellationToken);
        }

        //OnActiveCameraPanel(true);
        cameraPanelObjRect.DOAnchorPosY(1020, cameraPanelMoveSpd);

        NativeCamera.Permission permission = NativeCamera.TakePicture((string path) =>
        {
            if (path != null)
            {
                // �J�����p�l����\������
                photoRawImage.gameObject.SetActive(true);

                // �e�N�X�`�������[�h���ĕ\��
                // TODO: �ʐ^��ۑ����鎞��1024px�ŗǂ����ǁA�Q�[�����ŕ\�����鎞��512px���炢�ŗǂ�����
                photoRawImage.texture = NativeCamera.LoadImageAtPath(path, 512, false);
                photoRawImage.SetNativeSize();

                // �ۑ��p�̃e�N�X�`����ϐ��Ɋi�[����
                savePhotoTexture2D = NativeCamera.LoadImageAtPath(path, 1024, false);
            }
            else
            {
                XOverlap();
            }
        });
    }

    public void OnSavePhotoTexture2D()
    {
        // Texture2D��ǂݍ��݉\�Ȍ`���ɂ���
        var readableTexture = CreateReadableTexture2D(savePhotoTexture2D);

        // �t�@�C�����ɔN����������t���ĕۑ�����
        string filePath = $"{firstFilePathInputFiled.text}_{DateTime.Now.ToString("yyyyMMdd_HHmm")}";

        // �w�肵���t�@�C�����ɕۑ�����B�����t�@�C�������݂��Ȃ���΃t�@�C�����쐬����B
        NativeGallery.SaveImageToGallery(readableTexture, "MOGU_Picturec", filePath);
    }

    public void Test()
    {
        Debug.Log("a");
    }

    public void OnFollowMousePosition(Transform targetTransform)
    {
        var mousePos = Input.mousePosition;
        mousePos.z = 3;
        targetTransform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }

    public void OnFollowMousePosition(RectTransform targetRectTransform)
    {
        Vector2 movePos;

        // �}�E�X�̃X�N���[�����W���L�����o�X��RectTransform�̃��[�J�����W�ɕϊ�
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)targetRectTransform.parent,
            Input.mousePosition,
            null, // �����ł�null���w�肵�Ă��܂����A�K�v�ɉ����ăJ�������w�肵�Ă��������B
            out movePos);

        // �v�Z���ꂽ���[�J�����W��RectTransform�ɓK�p
        targetRectTransform.localPosition = movePos;
    }

    /// <summary>
    /// Texture2D��ǂݍ��݉\�Ȍ`���ɂ��ĕԂ�
    /// </summary>
    /// <param name="savePhotoTexture2D">�ۑ�����ʐ^</param>
    private Texture2D CreateReadableTexture2D(Texture2D savePhotoTexture2D)
    {
        var width = savePhotoTexture2D.width;
        var height = savePhotoTexture2D.height;

        var midRenderTexture = RenderTexture.GetTemporary(width, height, 0, GraphicsFormat.R8G8B8A8_SRGB);

        Graphics.Blit(savePhotoTexture2D, midRenderTexture);
        var previous = RenderTexture.active;
        RenderTexture.active = midRenderTexture;

        var readableTexture = new Texture2D(width, height);
        readableTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        readableTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(midRenderTexture);
        return readableTexture;
    }

    public void OnInitializeRectTransform(RectTransform targetTransform)
    {
        targetTransform.localPosition = Vector3.zero;
    }

    public void OnPhotoRectTransformOverlap()
    {
        RectTransfromOverlap(photoRectTransform, circleRectTransform, CircleOverlap);
        RectTransfromOverlap(photoRectTransform, xRectTransform, XOverlap);
    }

    private void RectTransfromOverlap(RectTransform targetRectTransform,
      RectTransform overlapRectTransform, Action overlapAction)
    {
        if (targetRectTransform.IsOverlapping(overlapRectTransform))
        {
            overlapAction?.Invoke();
        }
    }

    public void TablewareRotation()
    {
        folkRectTransform
            .DOLocalRotate(new Vector3(0, 0, -tablewareRotation), tablewareRtationSpeed)
            .SetLoops(2, LoopType.Yoyo);

        spoonRectTransform
            .DOLocalRotate(new Vector3(0, 0, tablewareRotation), tablewareRtationSpeed)
            .SetLoops(2, LoopType.Yoyo);
    }
}

public interface IOverlapAction
{
    void CircleOverlap();

    void XOverlap();
}