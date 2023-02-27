using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode]
public class SimulatorController : MonoBehaviour
{
    [SerializeField] private RenderTexture main_texture;
    [SerializeField] private Material material;
    [SerializeField] private Texture2D initial_texture;
    [SerializeField] private ScreenController screen_controller;
    [SerializeField] private float spring_force = 0.1f;
    [SerializeField] private float damping_factor = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        screen_controller = GetComponent<ScreenController>();

        InitializeTexture(ref main_texture, initial_texture);
        if (initial_texture == null)
        {
            Debug.LogError("No initial texture");
        }
        if (main_texture == null)
        {
            Debug.LogError("Failed to initialize texture");
        }
        if (material == null)
        {
            Debug.LogError("No material");
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    // OnRenderImage is called after all rendering is complete to render image
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        RenderTexture temp = RenderTexture.GetTemporary(main_texture.width, main_texture.height, 0, main_texture.format, RenderTextureReadWrite.Default);
        temp.filterMode = FilterMode.Point;

        Vector4 mouse_vector = GetMouseVector();
        material.SetVector("_MousePos", mouse_vector);
        material.SetFloat("_SpringForce", spring_force);
        material.SetFloat("_DampingFactor", damping_factor);

        Graphics.Blit(main_texture, temp, material);
        Graphics.Blit(temp, main_texture);
        screen_controller.DrawTransformed(temp, dest);

        RenderTexture.ReleaseTemporary(temp);
    }

    // Initialize texture
    void InitializeTexture(ref RenderTexture tex, Texture2D init_tex = null)
    {
        // Create texture with screen dimensions
        int width, height;
        int depth = 0;
        if (init_tex != null)
        {
            width = init_tex.width;
            height = init_tex.height;
            tex = new RenderTexture(width, height, depth, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
            Graphics.Blit(init_tex, tex);
        }
        else
        {
            width = Screen.width;
            height = Screen.height;
            tex = new RenderTexture(width, height, depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        }

    }

    Vector4 GetMouseVector()
    {
        float mouse_button_state = 0;
        if (Input.GetMouseButton(0))
        {
            mouse_button_state = 1;
        }
        Vector2 mouse_pos = Input.mousePosition;
        Vector2 main_texture_size = new Vector2(main_texture.width, main_texture.height);
        Vector2 screen_offset = screen_controller.position - Vector2.one / 2f;
        Debug.Log(mouse_pos);

        float scale = main_texture.width / (float)Screen.width / screen_controller.zoom;
        Debug.Log("Scale: " + scale);
        mouse_pos.x -= (Screen.width - main_texture.width / scale) / 2f;
        mouse_pos.y -= (Screen.height - main_texture.height / scale) / 2f;
        mouse_pos = mouse_pos * scale - screen_offset * main_texture_size;
        Vector4 mouse_vector = new Vector4(mouse_pos.x, mouse_pos.y, mouse_button_state, 0);
        return mouse_vector;
    }
}
