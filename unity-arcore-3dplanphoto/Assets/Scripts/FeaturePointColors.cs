using System;
using GoogleARCore;
using GoogleARCore.Examples.ComputerVision;
using UnityEngine;

[RequireComponent(typeof(TextureReader))]
public class FeaturePointColors : MonoBehaviour
{
    // Scale output image dimensions for performance
    const int k_DimensionsInverseScale = 2;

    public GameObject cubePrefab;
    public int poolSize;

    byte[] m_PixelByteBuffer = new byte[0];
    int m_PixelBufferSize;
    Material[] m_PixelMaterials;
    GameObject[] m_PixelObjects;
    Color[] m_PixelColors;

    void Awake() {
        if (cubePrefab.GetComponent<Renderer>() == null) {
            Debug.LogError("No renderer on pixel prefab!");
            enabled = false;
            return;
        }

        var textureReader = GetComponent<TextureReader>();
        textureReader.ImageFormat = TextureReaderApi.ImageFormatType.ImageFormatColor;
        textureReader.OnImageAvailableCallback += OnImageAvailable;

        var landscape = ScreenIsLandscape();
        var scaledScreenWidth = Screen.width / k_DimensionsInverseScale;
        var scaledScreenHeight = Screen.height / k_DimensionsInverseScale;

        // It's arbitrary whether the output image should be portrait or landscape, as long as
        // you know how to interpret it for each potential screen orientation.
        textureReader.ImageWidth = landscape ? scaledScreenWidth : scaledScreenHeight;
        textureReader.ImageHeight = landscape ? scaledScreenHeight : scaledScreenWidth;

        m_PixelObjects = new GameObject[poolSize];
        m_PixelMaterials = new Material[poolSize];
        for (var i = 0; i < poolSize; ++i) {
            var pixelObj = Instantiate(cubePrefab, transform);
            m_PixelObjects[i] = pixelObj;
            m_PixelMaterials[i] = pixelObj.GetComponent<Renderer>().material;
            pixelObj.SetActive(false);
        }
    }

    static bool ScreenIsLandscape() {
        return Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight;
    }

    void OnImageAvailable(TextureReaderApi.ImageFormatType format, int width, int height, IntPtr pixelBuffer, int bufferSize) {
        if (format != TextureReaderApi.ImageFormatType.ImageFormatColor)
            return;

        // Adjust buffer size if necessary.
        if (bufferSize != m_PixelBufferSize || m_PixelByteBuffer.Length == 0) {
            m_PixelBufferSize = bufferSize;
            m_PixelByteBuffer = new byte[bufferSize];
            m_PixelColors = new Color[width * height];
        }

        // Move raw data into managed buffer.
        System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, m_PixelByteBuffer, 0, bufferSize);

        // Interpret pixel buffer differently depending on which orientation the device is.
        // We need to get pixel colors into a friendly format - an array
        // laid out row by row from bottom to top, and left to right within each row.
        var bufferIndex = 0;
        for (var y = 0; y < height; ++y) {
            for (var x = 0; x < width; ++x) {
                int r = m_PixelByteBuffer[bufferIndex++];
                int g = m_PixelByteBuffer[bufferIndex++];
                int b = m_PixelByteBuffer[bufferIndex++];
                int a = m_PixelByteBuffer[bufferIndex++];
                var color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                int pixelIndex;
                switch (Screen.orientation) {
                    case ScreenOrientation.LandscapeRight:
                        pixelIndex = y * width + width - 1 - x;
                        break;
                    case ScreenOrientation.Portrait:
                        pixelIndex = (width - 1 - x) * height + height - 1 - y;
                        break;
                    case ScreenOrientation.LandscapeLeft:
                        pixelIndex = (height - 1 - y) * width + x;
                        break;
                    default:
                        pixelIndex = x * height + y;
                        break;
                }
                m_PixelColors[pixelIndex] = color;
            }
        }

        FeaturePointCubes();
    }

    void FeaturePointCubes() {
        foreach (var pixelObj in m_PixelObjects) {
            pixelObj.SetActive(false);
        }

        var index = 0;
        var pointsInViewCount = 0;
        var camera = Camera.main;
        var scaledScreenWidth = Screen.width / k_DimensionsInverseScale;
        while (index < Frame.PointCloud.PointCount && pointsInViewCount < poolSize) {
            // If a feature point is visible, use its screen space position to get the correct color for its cube
            // from our friendly-formatted array of pixel colors.
            var point = Frame.PointCloud.GetPoint(index);
            var screenPoint = camera.WorldToScreenPoint(point);
            if (screenPoint.x >= 0 && screenPoint.x < camera.pixelWidth &&
                screenPoint.y >= 0 && screenPoint.y < camera.pixelHeight) {
                var pixelObj = m_PixelObjects[pointsInViewCount];
                pixelObj.SetActive(true);
                pixelObj.transform.position = point;
                var scaledX = (int)screenPoint.x / k_DimensionsInverseScale;
                var scaledY = (int)screenPoint.y / k_DimensionsInverseScale;
                m_PixelMaterials[pointsInViewCount].color = m_PixelColors[scaledY * scaledScreenWidth + scaledX];
                pointsInViewCount++;
            }
            index++;
        }
    }
}