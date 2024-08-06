//  RectTransformExtensions.cs
//  http://kan-kikuchi.hatenablog.com/entry/RectTransform_IsOverlapping
//
//  Created by kan.kikuchi on 2023.05.25.

using UnityEngine;

/// <summary>
/// RectTransform�̊g�����\�b�h��錾����g���N���X
/// </summary>
public static class RectTransformExtensions
{

    /// <summary>
    /// RectTransform���ʂ�RectTransform�Əd�Ȃ��Ă��邩�ǂ���
    /// </summary>
    public static bool IsOverlapping(this RectTransform rect1, RectTransform rect2)
    {
        //�eRectTransform�̊p���i�[����z����쐬
        var rect1Corners = new Vector3[4];
        var rect2Corners = new Vector3[4];

        //RectTransform�̊p�̃��[���h���W���擾
        rect1.GetWorldCorners(rect1Corners);
        rect2.GetWorldCorners(rect2Corners);

        //���ׂĂ̊p�Ƀ`�F�b�N
        for (var i = 0; i < 4; i++)
        {
            //rect1�̊p��rect2�̓����ɂ��邩
            if (IsPointInsideRect(rect1Corners[i], rect2Corners))
            {
                return true;
            }
            //rect2�̊p��rect1�̓����ɂ��邩
            if (IsPointInsideRect(rect2Corners[i], rect1Corners))
            {
                return true;
            }
        }

        //�d�Ȃ��Ă��Ȃ�
        return false;
    }

    //�w����W����`�̓����ɂ��邩
    private static bool IsPointInsideRect(Vector3 point, Vector3[] rectCorners)
    {
        var inside = false;

        //rectCorners�̊e���_�ɑ΂��āApoint��rect���ɂ��邩���m�F
        for (int i = 0, j = 3; i < 4; j = i++)
        {
            if (((rectCorners[i].y > point.y) != (rectCorners[j].y > point.y)) &&
                (point.x < (rectCorners[j].x - rectCorners[i].x) * (point.y - rectCorners[i].y) / (rectCorners[j].y - rectCorners[i].y) + rectCorners[i].x))
            {
                inside = !inside;
            }
        }

        return inside;
    }
}