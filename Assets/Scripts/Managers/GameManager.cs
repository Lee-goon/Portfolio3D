using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

delegate void UpdateFunction(float deltaTime);
delegate void StartFunction();
delegate void DestroyFunction();

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance => instance;

    ResourceManager resource;
    public ResourceManager Resource => resource;

    SoundManager sound;
    public SoundManager Sound => sound;

    SaveManager save;
    public SaveManager Save => save;

    OptionManager option;
    public OptionManager Option => option;

    ControllerManager controller;
    public ControllerManager Controller => controller;

    UiManager ui;
    public UiManager Ui => ui;

    bool isGameStart = false;
    public static bool IsGameStart => instance && instance.isGameStart;



    private IEnumerator Start()
    {
        this.MakeSingleton(ref instance);
        resource = new ResourceManager();
        yield return resource.Instantiate();

        sound = new SoundManager();
        yield return sound.Instantiate();

        save = new SaveManager();
        yield return save.Instantiate();

        option = new OptionManager();
        yield return option.Instantiate();

        controller = new ControllerManager();
        yield return controller.Instantiate();

        ui = new UiManager();
        yield return ui.Instantiate();

        isGameStart = true;
    }
}
