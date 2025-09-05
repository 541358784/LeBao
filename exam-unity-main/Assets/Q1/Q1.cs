using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

/**
界面上有三个输入框，分别对应 X,Y,Z 的值，请实现 {@link Q1.onGenerateBtnClick} 函数，生成一个 10 × 10 的可控随机矩阵，并显示到界面上，矩阵要求如下：
1. {@link COLORS} 中预定义了 5 种颜色
2. 每个点可选 5 种颜色中的 1 种
3. 按照从左到右，从上到下的顺序，依次为每个点生成颜色，(0, 0)为左上⻆点，(9, 9)为右下⻆点，(0, 9)为右上⻆点
4. 点(0, 0)随机在 5 种颜色中选取
5. 其他各点的颜色计算规则如下，设目标点坐标为(m, n）：
    a. (m, n - 1)所属颜色的概率为基准概率加 X%
    b. (m - 1, n)所属颜色的概率为基准概率加 Y%
    c. 如果(m, n - 1)和(m - 1, n)同色，则该颜色的概率为基准概率加 Z%
    d. 其他颜色平分剩下的概率
*/

public class Q1 : MonoBehaviour
{
    private static readonly Color[] COLORS = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        new Color(1f, 0.5f, 0f) // Orange
    };

    // 每个格子的大小
    private const float GRID_ITEM_SIZE = 75f;
    private const int ROW_COUNT = 10;
    private const int COLUMN_COUNT = 10;

    [SerializeField]
    private InputField xInputField = null;

    [SerializeField]
    private InputField yInputField = null;

    [SerializeField]
    private InputField zInputField = null;

    [SerializeField]
    private Transform gridRootNode = null;

    [SerializeField]
    private GameObject gridItemPrefab = null;

    public void OnGenerateBtnClick()
    {
        // TODO: 请在此处开始作答
        //输入过长不考虑了
        if (!double.TryParse(xInputField.text, out double x)) x = 0;
        if (!double.TryParse(yInputField.text, out double y)) y = 0;
        if (!double.TryParse(zInputField.text, out double z)) z = 0;
        float xProb = (float)x / 100f;
        float yProb = (float)y / 100f;
        float zProb = (float)z / 100f;
        int[,] colorMatrix = GenerateColorMatrix(xProb, yProb, zProb);
        UpdateGridColors(colorMatrix);
    }

    private Image[,] grids = new Image[ROW_COUNT, COLUMN_COUNT];

    private void UpdateGridColors(int[,] colorMatrix)
    {
        var rectTransform = gridRootNode as RectTransform;
        var width = rectTransform.sizeDelta.x;
        var height = rectTransform.sizeDelta.y;
        var offset = new Vector2(width / 2f, height / 2f);
        var avgWidth = width / COLUMN_COUNT;
        var avgHeight = height / ROW_COUNT;
        for (int i = 0; i < ROW_COUNT; i++)
        {
            for (int j = 0; j < COLUMN_COUNT; j++)
            {
                var color = COLORS[colorMatrix[i, j]];
                if (!grids[i, j])
                {
                    GameObject imageGameObject = new GameObject("ColorRectangle"+i+"_"+j);
                    Image image = imageGameObject.AddComponent<Image>();
                    grids[i, j] = image;
                    imageGameObject.transform.SetParent(gridRootNode);
                    var imageRect = imageGameObject.transform as RectTransform;
                    var anchorPosition = new Vector2(j * avgWidth + avgWidth / 2,
                        (ROW_COUNT - i) * avgHeight - avgHeight / 2);
                    anchorPosition -= offset;
                    imageRect.anchoredPosition = anchorPosition;
                    imageRect.sizeDelta = new Vector2(GRID_ITEM_SIZE, GRID_ITEM_SIZE);
                }
                grids[i, j].color = color;
            }
        }
    }
    private int[,] GenerateColorMatrix(float xProb, float yProb, float zProb)
    {
        int[,] matrix = new int[ROW_COUNT, COLUMN_COUNT];
        
        float baseProb = 1f/COLORS.Length;
        
        for (int i = 0; i < ROW_COUNT; i++)
        {
            for (int j = 0; j < COLUMN_COUNT; j++)
            {
                var colorPool = new List<int>();
                for (var ii = 0; ii < COLORS.Length; ii++)
                {
                    colorPool.Add(ii);
                }
                var colorWeight = new List<float>();
                var colorList = new List<int>();
                bool hasLeft = j > 0;
                bool hasTop = i > 0;
                
                if (hasLeft && hasTop && matrix[i, j - 1] == matrix[i - 1, j])
                {
                    int sameColor = matrix[i, j - 1];
                    colorPool.Remove(sameColor);
                    colorList.Add(sameColor);
                    colorWeight.Add(baseProb+zProb);
                }
                else
                {
                    if (hasLeft)
                    {
                        int leftColor = matrix[i, j - 1];
                        colorPool.Remove(leftColor);
                        colorList.Add(leftColor);
                        colorWeight.Add(baseProb+xProb);
                    }
                
                    if (hasTop)
                    {
                        int topColor = matrix[i - 1, j];
                        colorPool.Remove(topColor);
                        colorList.Add(topColor);
                        colorWeight.Add(baseProb+yProb);
                    }   
                }

                var totalWeight = 0f;
                foreach (var weight in colorWeight)
                {
                    totalWeight += weight;
                }

                if (totalWeight > 1f || colorPool.Count == 0)//概率超过1时会按比例均分概率，题目中未给边界处理方法，我就当这样处理了
                {
                }
                else
                {
                    var leftWeight = 1f - totalWeight;
                    var avgWeight = leftWeight / colorPool.Count;
                    for (var ii = 0; ii < colorPool.Count; ii++)
                    {
                        colorList.Add(colorPool[ii]);
                        colorWeight.Add(avgWeight);
                        totalWeight += avgWeight;
                    }
                }
                var randomWeight = Random.Range(0f, totalWeight);
                var sc = false;
                for (var ii = 0; ii < colorWeight.Count; ii++)
                {
                    randomWeight -= colorWeight[ii];
                    if (randomWeight <= 0)
                    {
                        matrix[i, j] = colorList[ii];
                        sc = true;
                        break;
                    }
                }
                if (!sc)
                    matrix[i, j] = colorList.Last();//浮点数运算可能有误差
            }
        }
        
        return matrix;
    }
}
