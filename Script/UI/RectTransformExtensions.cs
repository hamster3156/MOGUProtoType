//  RectTransformExtensions.cs
//  http://kan-kikuchi.hatenablog.com/entry/RectTransform_IsOverlapping
//
//  Created by kan.kikuchi on 2023.05.25.

using UnityEngine;

/// <summary>
/// RectTransformの拡張メソッドを宣言する拡張クラス
/// </summary>
public static class RectTransformExtensions
{

    /// <summary>
    /// RectTransformが別のRectTransformと重なっているかどうか
    /// </summary>
    public static bool IsOverlapping(this RectTransform rect1, RectTransform rect2)
    {
        //各RectTransformの角を格納する配列を作成
        var rect1Corners = new Vector3[4];
        var rect2Corners = new Vector3[4];

        //RectTransformの角のワールド座標を取得
        rect1.GetWorldCorners(rect1Corners);
        rect2.GetWorldCorners(rect2Corners);

        //すべての角にチェック
        for (var i = 0; i < 4; i++)
        {
            //rect1の角がrect2の内部にあるか
            if (IsPointInsideRect(rect1Corners[i], rect2Corners))
            {
                return true;
            }
            //rect2の角がrect1の内部にあるか
            if (IsPointInsideRect(rect2Corners[i], rect1Corners))
            {
                return true;
            }
        }

        //重なっていない
        return false;
    }

    //指定座標が矩形の内部にあるか
    private static bool IsPointInsideRect(Vector3 point, Vector3[] rectCorners)
    {
        var inside = false;

        //rectCornersの各頂点に対して、pointがrect内にあるかを確認
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