using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CameraPanelManager : MonoBehaviour
{
    [SerializeField]
    private RectTransform arrowRectTransform;

    [SerializeField]
    private float moveArrowPosY;

    [SerializeField]
    private float moveArrowDuration;

    private float initArrowPosY;

    [SerializeField]
    private RectTransform trashRectTransform;

    [SerializeField]
    private RectTransform photoRawImageRectTransform;

    [SerializeField]
    private RectTransform xRectTransform;

    private bool isChangeTrashSize;
    private bool sizeChanged = false; // �T�C�Y���ύX���ꂽ���ǂ�����ǐՂ���V�����ϐ�

    // Start is called before the first frame update
    void Start()
    {
        arrowRectTransform
            .DOAnchorPosY(moveArrowPosY, moveArrowDuration)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // Update is called once per frame
    void Update()
    {
        if (photoRawImageRectTransform.IsOverlapping(xRectTransform) && !sizeChanged)
        {
            isChangeTrashSize = true;
            ChangeTrashScale();
        }
        else if (!photoRawImageRectTransform.IsOverlapping(xRectTransform) && sizeChanged)
        {
            isChangeTrashSize = false;
            ChangeTrashScale();
        }
    }

    private void ChangeTrashScale()
    {
        if (isChangeTrashSize)
        {
            trashRectTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            sizeChanged = true; // �T�C�Y���ύX���ꂽ���Ƃ��L�^
        }
        else
        {
            trashRectTransform.localScale = new Vector3(1f, 1f, 1f);
            sizeChanged = false; // �T�C�Y�����ɖ߂������Ƃ��L�^
        }
    }
}
