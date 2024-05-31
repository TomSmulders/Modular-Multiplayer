using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ModularPlayer : MonoBehaviour
{
    #region classes

    #region movement
    [System.Serializable]
    public class ModularPlayer_Movement_Settings
    {
        [HideInInspector]
        public Vector3 velocity;
        [HideInInspector]
        public Vector3 input;
        [HideInInspector]
        public bool jumped;
        [HideInInspector]
        public bool isGrounded,isRunning,isSneaking;

        [Header("Walking/Running")]
        public float maxSpeed;
        public float accelerationSpeed;
        public float decelerationSpeed;
        public float runningMultiplier;
        public float sneakingMultiplier;

        [Header("Jumping/Gravtiy")]
        public float jumpForce;
        public float gravityScale;
        [HideInInspector]
        public float gravityMultiplier = 1;

        [Header("Direction Multipliers")]
        public DirectionMultiplierMode directionMultiplierMode = DirectionMultiplierMode.identical;
        public ModularPlayer_DirectionMultipliers_FourAxis directionMultipliers;
        public ModularPlayer_DirectionMultipliers_EightAxis directionMultipliers_Diagonals;


        [System.Serializable]
        public class ModularPlayer_DirectionMultipliers_EightAxis
        {
            public float frontMultiplier;
            public float backMultiplier;
            public float sideMultiplier;
            public float diagonalMultiplier;
        }
        [System.Serializable]
        public class ModularPlayer_DirectionMultipliers_FourAxis
        {
            public float frontMultiplier;
            public float backMultiplier;
            public float sideMultiplier;
        }



        public enum DirectionMultiplierMode { identical, Seperate, Seperate_With_Diagonals };
    }

    #endregion

    #region cameraMotion
    [System.Serializable]
    public class ModularPlayer_CameraMotion_Settings
    {
        public GameObject cameraHolder;
        public Camera camera;

        [HideInInspector]
        public Vector2 rotation;
        [HideInInspector]
        public Vector2 input;

        public Vector2 sensitivity;
        public bool rotateCamNotPlayer;

        public CameraPhysicsMode cameraPhysicsMode = CameraPhysicsMode.none; 
        public enum CameraPhysicsMode { none , basic_1 , basic_2, realistic , Ultra_Realistic , Side_To_Side }
        
    }

    #endregion

    #region stamina
    [System.Serializable]
    public class ModularPlayer_Stamina_Settings
    {
        [HideInInspector]
        public float currentStamina;
        [HideInInspector]
        public float isEmpty;


        public float maxStamina;
        public float staminaGainPerSecond;
        public float staminaLossPerSecond;
        public float staminaEmptyGainMultiplier;
    }
    #endregion

    #region animation
    [System.Serializable]
    public class ModularPlayer_Animation_Settings
    {
        public bool enableAnimations;
        public List<ModularPlayer_OnAnimation> animationCalls = new List<ModularPlayer_OnAnimation>();

        [System.Serializable]
        public class ModularPlayer_OnAnimation
        {
            public AnimationCall animationEvent = AnimationCall.idle;
            public Animation animation;
            public bool loopAnimation;
        }
        public enum AnimationCall { idle , onJump , onWalking, onRunning , onSneaking , onHit , onDamaged , onDeath , onSpawn }
    }
    #endregion

    #region player info
    [System.Serializable]
    public class ModularPlayer_PlayerData_Settings
    {
        //Scriptable objects
        //In these scriptable objects you will have a ModularPlayer_PlayerInfo_Class
        //this class can decise data types between , Json, String , float
        //
        //Make sure this script loads 

        [Header("Set the Name to the key you want to use in your scripts")]
        public List<PlayerData_Class<string>> stringData = new List<PlayerData_Class<string>>();
        public List<PlayerData_Class<int>> intData = new List<PlayerData_Class<int>>();
        public List<PlayerData_Class<float>> floatData = new List<PlayerData_Class<float>>();
        public List<PlayerData_Class<Color>> colorData = new List<PlayerData_Class<Color>>();
        public List<PlayerData_Class<GameObject>> gameObjectData = new List<PlayerData_Class<GameObject>>();

        [System.Serializable]
        public class PlayerData_Class<T>
        {
            public string name = "Key-Name";
            public T value;
            public PlayerData_Class(string _key , T _value)
            {
                this.name = _key;
                this.value = _value;
            }
        }

        #region Set + Get functions

        public void SetStringData(string _key, string _data) { stringData.Add(new PlayerData_Class<string>(_key, _data)); }
        public void SetIntData(string _key, int _data) { intData.Add(new PlayerData_Class<int>(_key, _data)); }
        public void SetFloatData(string _key, float _data) { floatData.Add(new PlayerData_Class<float>(_key, _data)); }
        public void SetColorData(string _key, Color _data) { colorData.Add(new PlayerData_Class<Color>(_key, _data)); }
        public void SetGameObjectData(string _key, GameObject _data) { gameObjectData.Add(new PlayerData_Class<GameObject>(_key, _data)); }

        public string GetStringData(string _key)
        {
            foreach (var _dataItem in stringData)
            {
                if(_dataItem.name == _key) { return _dataItem.value; }
            }
            Debug.LogWarning(_key + " : Does not exist, returning empty string");
            return string.Empty;
        }
        public int GetIntData(string _key)
        {
            foreach (var _dataItem in intData)
            {
                if (_dataItem.name == _key) { return _dataItem.value; }
            }
            Debug.LogWarning(_key + " : Does not exist, returning 0");
            return 0;
        }
        public float GetFloatData(string _key)
        {
            foreach (var _dataItem in floatData)
            {
                if (_dataItem.name == _key) { return _dataItem.value; }
            }
            Debug.LogWarning(_key + " : Does not exist, returning 0");
            return 0;
        }
        public Color GetColorData(string _key)
        {
            foreach (var _dataItem in colorData)
            {
                if (_dataItem.name == _key) { return _dataItem.value; }
            }
            Debug.LogWarning(_key + " : Does not exist, returning color(0, 0, 0)");
            return new Color(0,0,0);
        }
        public GameObject GetGameObjectData(string _key)
        {
            foreach (var _dataItem in gameObjectData)
            {
                if (_dataItem.name == _key) { return _dataItem.value; }
            }
            Debug.LogWarning(_key + " : Does not exist, returning Null");
            return null;
        }

        #endregion
    }
    #endregion

    #region settings

    [System.Serializable]
    public class ModularPlayer_Settings
    {
        [HideInInspector]
        public Rigidbody rb;

        [HideInInspector]
        public ModularPlayer_PlayerInputs playerInputs;
    }

    #endregion

    #endregion


    #region variables

    public ModularPlayer_Movement_Settings movement;
    public ModularPlayer_CameraMotion_Settings cameraMotion;
    public ModularPlayer_Stamina_Settings stamina;
    public ModularPlayer_Animation_Settings animations;
    public ModularPlayer_PlayerData_Settings data;

    public ModularPlayer_Settings settings;

    #endregion


    #region Unity Functions

    private void Awake()
    {
        settings.playerInputs = new ModularPlayer_PlayerInputs();

        if(cameraMotion.camera == null && cameraMotion.cameraHolder != null)
        {
            cameraMotion.camera = cameraMotion.cameraHolder.GetComponentInChildren<Camera>();
        }
    }
    private void OnEnable()
    {
        settings.playerInputs.Enable();
    }
    private void OnDisable()
    {
        settings.playerInputs.Disable();
    }
    private void Start()
    {
        settings.rb = GetComponent<Rigidbody>();
        settings.rb.freezeRotation = true;
        settings.rb.useGravity = false;
        Cursor.lockState = CursorLockMode.Locked;
     }


    private void Update()
    {
        GetCameraMotionInputs();
        Gravity();
    }
    private void FixedUpdate()
    {
        Movement();
        MoveCameraMotion();
    }

    #endregion

    #region Get Inputs

    public void OnMove(InputAction.CallbackContext context)
    {
        movement.input = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        movement.jumped = context.action.triggered;
    }
    void GetCameraMotionInputs()
    {
        cameraMotion.input = new Vector2(settings.playerInputs.ModularPlayer_InputActionMap.LookX.ReadValue<float>(),                                 
            settings.playerInputs.ModularPlayer_InputActionMap.LookY.ReadValue<float>());
    }

    #endregion


    #region movement code

    private void Gravity()
    {
        Vector3 gravity = (movement.gravityScale * movement.gravityMultiplier) * -Vector3.up;
        settings.rb.AddForce(gravity, ForceMode.Acceleration);
    }

    private void Movement()
    {
        movement.isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);

        Vector3 inputVector = new Vector3(movement.input.x, 0f, movement.input.y);

        float speedMultiplier = 1f;
        if (movement.isRunning)
            speedMultiplier = movement.runningMultiplier;
        else if (movement.isSneaking)
            speedMultiplier = movement.sneakingMultiplier;

        Vector3 targetVelocity = inputVector * movement.maxSpeed * speedMultiplier;

        Debug.Log(targetVelocity);

        if (inputVector.magnitude > 0)
        {
            movement.velocity = Vector3.Lerp(movement.velocity, targetVelocity, movement.accelerationSpeed * Time.deltaTime);
        }
        else
        {
            movement.velocity = Vector3.Lerp(movement.velocity, Vector3.zero, movement.decelerationSpeed * Time.deltaTime);
        }

        if (movement.jumped && movement.isGrounded)
        {
            settings.rb.AddForce(transform.up * movement.jumpForce, ForceMode.Impulse);
            movement.jumped = false;
            movement.gravityMultiplier = 1f;
        }

        if (movement.isGrounded)
        {
            movement.gravityMultiplier = 0.1f;
            movement.jumped = false;
        }
        else
        {
            movement.gravityMultiplier = 1f;
        }

        Vector3 _directionF = settings.rb.rotation * new Vector3(movement.velocity.x, 0, movement.velocity.z);
        settings.rb.MovePosition(settings.rb.position + (_directionF * Time.fixedDeltaTime));
    }

    #endregion

    #region camera motion code

    void MoveCameraMotion()
    {
        cameraMotion.rotation.x -= cameraMotion.input.y * Time.deltaTime * cameraMotion.sensitivity.x;
        cameraMotion.rotation.x = Math.Clamp(cameraMotion.rotation.x, -90f, 90f);

        if (cameraMotion.rotateCamNotPlayer)
        {
            Vector3 rot = cameraMotion.camera.transform.eulerAngles;
            cameraMotion.camera.transform.eulerAngles = new Vector3(cameraMotion.rotation.x, rot.y + cameraMotion.input.x * Time.deltaTime * cameraMotion.sensitivity.y, 0);
        }
        else
        {
            transform.eulerAngles += new Vector3(0, cameraMotion.input.x * Time.deltaTime * cameraMotion.sensitivity.y, 0);
            cameraMotion.camera.transform.localRotation = Quaternion.Euler(cameraMotion.rotation.x,0,0);
        }
    }
    #endregion
}
