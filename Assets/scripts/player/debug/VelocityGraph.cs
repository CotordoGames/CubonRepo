using UnityEngine;
using UnityEngine.UI;

public class VelocityGraph : MonoBehaviour
{
    public Rigidbody2D targetRb;
    public RawImage graphImage;

    [Header("Graph Settings")]
    public int graphWidth = 300;
    public int graphHeight = 100;

    [Tooltip("how much velocity affects graph height")]
    public float velocityScale = 2f;

    [Tooltip("how many frames are stored")]
    public int historyLength = 300;

    private Texture2D graphTexture;
    private float[] velocityHistory;
    private int historyIndex;

    private Color32[] clearPixels;

    void Start()
    {
        velocityHistory = new float[historyLength];

        graphTexture = new Texture2D(graphWidth, graphHeight);
        graphTexture.filterMode = FilterMode.Point;

        clearPixels = new Color32[graphWidth * graphHeight];

        for(int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = new Color32(0, 0, 0, 192);
        }

        graphImage.texture = graphTexture;

        if(targetRb == null)
        {
            targetRb = GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        RecordVelocity();
        DrawGraph();
    }

    private void RecordVelocity()
    {
        velocityHistory[historyIndex] = targetRb.linearVelocityX;

        historyIndex++;

        if(historyIndex >= historyLength)
        {
            historyIndex = 0;
        }
    }

    private void DrawGraph()
    {
        graphTexture.SetPixels32(clearPixels);

        // center line
        for(int x = 0; x < graphWidth; x++)
        {
            graphTexture.SetPixel(x, graphHeight / 2, Color.gray);
        }

        for(int i = 1; i < historyLength; i++)
        {
            int index1 = (historyIndex + i - 1) % historyLength;
            int index2 = (historyIndex + i) % historyLength;

            float vel1 = velocityHistory[index1];
            float vel2 = velocityHistory[index2];

            int x1 = Mathf.FloorToInt((i - 1) / (float)historyLength * graphWidth);
            int x2 = Mathf.FloorToInt(i / (float)historyLength * graphWidth);

            int y1 = Mathf.FloorToInt(graphHeight / 2 + vel1 * velocityScale);
            int y2 = Mathf.FloorToInt(graphHeight / 2 + vel2 * velocityScale);

            DrawLine(x1, y1, x2, y2, Color.cyan);
        }

        graphTexture.Apply();
    }

    private void DrawLine(int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while(true)
        {
            if(x0 >= 0 && x0 < graphWidth &&
               y0 >= 0 && y0 < graphHeight)
            {
                graphTexture.SetPixel(x0, y0, color);
            }

            if(x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = err * 2;

            if(e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if(e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}