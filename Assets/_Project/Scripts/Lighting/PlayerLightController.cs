using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLightController : MonoBehaviour
{
    public GameObject flashlight;
    public GameObject torch;

    void Start()
    {
        if (flashlight) flashlight.SetActive(false);
        if (torch) torch.SetActive(false);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.fKey.wasPressedThisFrame) { SetFlash(true);  SetTorch(false); }
        if (kb.gKey.wasPressedThisFrame) { SetFlash(false); SetTorch(true); }
        if (kb.xKey.wasPressedThisFrame) { SetFlash(false); SetTorch(false); }
    }

    void SetFlash(bool on){ if (flashlight) flashlight.SetActive(on); }
    void SetTorch(bool on){ if (torch) torch.SetActive(on); }
}