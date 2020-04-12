// GENERATED AUTOMATICALLY FROM 'Assets/TestControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @TestControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @TestControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""TestControls"",
    ""maps"": [
        {
            ""name"": ""Debug"",
            ""id"": ""d5676941-191b-4c7d-8d28-19f17414937d"",
            ""actions"": [
                {
                    ""name"": ""KillUnit"",
                    ""type"": ""Button"",
                    ""id"": ""88b12747-f7d4-44ae-8232-1daffac1a8d8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""d4848aa3-58ad-4448-af8d-34eff474789f"",
                    ""path"": ""<Keyboard>/#(k)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControlScheme"",
                    ""action"": ""KillUnit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Gameplay"",
            ""id"": ""f853cb56-7530-4402-9449-ca3c300856d8"",
            ""actions"": [
                {
                    ""name"": ""ShapeSelection"",
                    ""type"": ""Button"",
                    ""id"": ""2da440b2-c329-4841-b248-77d6e9237da4"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""2c8dd21a-0d36-4762-a6d7-e5b3f04da8e6"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": ""DefaultControlScheme"",
                    ""action"": ""ShapeSelection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""DefaultControlScheme"",
            ""bindingGroup"": ""DefaultControlScheme"",
            ""devices"": [
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Debug
        m_Debug = asset.FindActionMap("Debug", throwIfNotFound: true);
        m_Debug_KillUnit = m_Debug.FindAction("KillUnit", throwIfNotFound: true);
        // Gameplay
        m_Gameplay = asset.FindActionMap("Gameplay", throwIfNotFound: true);
        m_Gameplay_ShapeSelection = m_Gameplay.FindAction("ShapeSelection", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Debug
    private readonly InputActionMap m_Debug;
    private IDebugActions m_DebugActionsCallbackInterface;
    private readonly InputAction m_Debug_KillUnit;
    public struct DebugActions
    {
        private @TestControls m_Wrapper;
        public DebugActions(@TestControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @KillUnit => m_Wrapper.m_Debug_KillUnit;
        public InputActionMap Get() { return m_Wrapper.m_Debug; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DebugActions set) { return set.Get(); }
        public void SetCallbacks(IDebugActions instance)
        {
            if (m_Wrapper.m_DebugActionsCallbackInterface != null)
            {
                @KillUnit.started -= m_Wrapper.m_DebugActionsCallbackInterface.OnKillUnit;
                @KillUnit.performed -= m_Wrapper.m_DebugActionsCallbackInterface.OnKillUnit;
                @KillUnit.canceled -= m_Wrapper.m_DebugActionsCallbackInterface.OnKillUnit;
            }
            m_Wrapper.m_DebugActionsCallbackInterface = instance;
            if (instance != null)
            {
                @KillUnit.started += instance.OnKillUnit;
                @KillUnit.performed += instance.OnKillUnit;
                @KillUnit.canceled += instance.OnKillUnit;
            }
        }
    }
    public DebugActions @Debug => new DebugActions(this);

    // Gameplay
    private readonly InputActionMap m_Gameplay;
    private IGameplayActions m_GameplayActionsCallbackInterface;
    private readonly InputAction m_Gameplay_ShapeSelection;
    public struct GameplayActions
    {
        private @TestControls m_Wrapper;
        public GameplayActions(@TestControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @ShapeSelection => m_Wrapper.m_Gameplay_ShapeSelection;
        public InputActionMap Get() { return m_Wrapper.m_Gameplay; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameplayActions set) { return set.Get(); }
        public void SetCallbacks(IGameplayActions instance)
        {
            if (m_Wrapper.m_GameplayActionsCallbackInterface != null)
            {
                @ShapeSelection.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnShapeSelection;
                @ShapeSelection.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnShapeSelection;
                @ShapeSelection.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnShapeSelection;
            }
            m_Wrapper.m_GameplayActionsCallbackInterface = instance;
            if (instance != null)
            {
                @ShapeSelection.started += instance.OnShapeSelection;
                @ShapeSelection.performed += instance.OnShapeSelection;
                @ShapeSelection.canceled += instance.OnShapeSelection;
            }
        }
    }
    public GameplayActions @Gameplay => new GameplayActions(this);
    private int m_DefaultControlSchemeSchemeIndex = -1;
    public InputControlScheme DefaultControlSchemeScheme
    {
        get
        {
            if (m_DefaultControlSchemeSchemeIndex == -1) m_DefaultControlSchemeSchemeIndex = asset.FindControlSchemeIndex("DefaultControlScheme");
            return asset.controlSchemes[m_DefaultControlSchemeSchemeIndex];
        }
    }
    public interface IDebugActions
    {
        void OnKillUnit(InputAction.CallbackContext context);
    }
    public interface IGameplayActions
    {
        void OnShapeSelection(InputAction.CallbackContext context);
    }
}
