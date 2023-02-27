using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenController : MonoBehaviour
{
    public Vector2 position;
    public float zoom;
    public float zoom_speed;
    private Vector2 ref_mouse_position;
    // public Vector2 main_texture_size;
    // Start is called before the first frame update
    void Start()
    {
        zoom = 1f;
        position = 0.5f * Vector2.one;
    }

    // Update is called once per frame
    void Update()
    {
        // Zoom control using mouse scroll wheel
        if (Input.mouseScrollDelta.y > 0)
        {
            zoom *= zoom_speed;
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            zoom /= zoom_speed;
        }

        // Movement control using mouse right click drag
        if (Input.GetMouseButtonDown(1))
        {
            ref_mouse_position = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1))
        {
            Vector2 mouse_delta = (Vector2)Input.mousePosition - ref_mouse_position;

            UpdatePosition(mouse_delta);

            ref_mouse_position = Input.mousePosition;
        }
    }

    void UpdatePosition(Vector2 mouse_delta)
    {
        // Update the position variable of the screen
        position += mouse_delta / Screen.width / zoom;
    }

    public void DrawTransformed(RenderTexture src, RenderTexture dest)
    {
        // Draw the screen
        float scale = 1f / zoom;
        Vector2 scale_vec = new Vector2(scale, scale * (float)Screen.height / (float)Screen.width);
        Vector2 offset = Vector2.one - position;
        Graphics.Blit(src, dest, scale_vec, offset - 0.5f * scale_vec);
    }
}
