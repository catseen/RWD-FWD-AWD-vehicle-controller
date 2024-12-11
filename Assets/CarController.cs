using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CarController : MonoBehaviour
{
    //CAR SETUP
    [Space(20)]
    [Space(10)]
    [Range(20, 300)]
    public int maxSpeed = 250;
    [Range(10, 120)]
    public int maxReverseSpeed = 45;
    [Range(1, 50)]
    public int accelerationMultiplier = 2;
    [Space(10)]
    [Range(10, 45)]
    public int maxSteeringAngle = 27;
    [Range(0.1f, 1f)]
    public float steeringSpeed = 0.5f;
    [Space(10)]
    [Range(100, 600)]
    public int brakeForce = 350;
    [Range(1, 10)]
    public int decelerationMultiplier = 2;
    [Range(1, 10)]
    public int handbrakeDriftMultiplier = 5;
    [Space(10)]
    public Vector3 bodyMassCenter;
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    [Space(10)]
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    [Space(10)]
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    [Space(10)]
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    //TOUCH CONTROLS
    [Space(20)]
    [Space(10)]
    public bool useTouchControls = false;
    public GameObject throttleButton;
    TouchInputHandler throttlePTI;
    public GameObject reverseButton;
    TouchInputHandler reversePTI;
    public GameObject turnRightButton;
    TouchInputHandler turnRightPTI;
    public GameObject turnLeftButton;
    TouchInputHandler turnLeftPTI;
    public GameObject handbrakeButton;
    TouchInputHandler handbrakePTI;

    //PARTICLES
    [Space(20)]
    [Space(10)]
    public bool useEffects = false;
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    //[Space(10)]
    public ParticleSystem FRWParticleSystem;
    public ParticleSystem FLWParticleSystem;
    [Space(10)]
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;
    [Space(10)]
    public TrailRenderer FLWTireSkid;
    public TrailRenderer FRWTireSkid;

    //SOUNDS
    [Space(20)]
    [Space(10)]
    public bool useSounds = false;
    public AudioSource carEngineSound; 
    public AudioSource tireScreechSound;
    float initialCarEngineSoundPitch;

    //DATA/VARIABLES
    [SerializeField, HideInInspector]
    public CarDriveType carDriveType;
    [HideInInspector]
    public float carSpeed;
    [HideInInspector]
    public bool isDrifting;
    [HideInInspector]
    public bool isTractionLocked;

    Rigidbody carRigidbody;
    float steeringAxis;
    float throttleAxis;
    float driftingAxis;
    float localVelocityZ;
    float localVelocityX;
    bool deceleratingCar;

    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;

    void Start()
    {
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;
        Canvas canvas = GetComponentInChildren<Canvas>();

        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;
        FRwheelFriction = new WheelFrictionCurve();
        FRwheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;
        RLwheelFriction = new WheelFrictionCurve();
        RLwheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;
        RRwheelFriction = new WheelFrictionCurve();
        RRwheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;

        if (useTouchControls)
        {
            canvas.enabled = true;

            if (throttleButton != null && turnRightButton != null && turnLeftButton != null && handbrakeButton != null)
            {
                throttlePTI = throttleButton.GetComponent<TouchInputHandler>();
                reversePTI = reverseButton.GetComponent<TouchInputHandler>();
                turnLeftPTI = turnLeftButton.GetComponent<TouchInputHandler>();
                turnRightPTI = turnRightButton.GetComponent<TouchInputHandler>();
                handbrakePTI = handbrakeButton.GetComponent<TouchInputHandler>();
            }
            else
            {
                //canvas.enabled = false;
                String ex = "Touch controls are not completely set up. You must drag and drop your scene buttons in the" +
                " PrometeoCarController component.";
                Debug.LogWarning(ex);
            }
        }
        //else
            //canvas.enabled = false;
    }

    void Update()
    {
        // Скорость и локальная скорость
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        // Определяем управление
        bool throttle = useTouchControls ? throttlePTI.buttonPressed : Input.GetKey(KeyCode.W);
        bool reverse = useTouchControls ? reversePTI.buttonPressed : Input.GetKey(KeyCode.S);
        bool turnLeft = useTouchControls ? turnLeftPTI.buttonPressed : Input.GetKey(KeyCode.A);
        bool turnRight = useTouchControls ? turnRightPTI.buttonPressed : Input.GetKey(KeyCode.D);
        bool handbrake = useTouchControls ? handbrakePTI.buttonPressed : Input.GetKey(KeyCode.Space);
        bool handbrakeReleased = useTouchControls ? !handbrakePTI.buttonPressed : Input.GetKeyUp(KeyCode.Space);

        // Управление движением
        if (throttle || reverse || handbrake)
        {
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
        }

        // Сброс газа
        ThrottleOff();

        if (throttle)
        {
            GoForward();
        }
        else if (reverse)
        {
            GoReverse();
        }

        if (turnLeft)
        {
            TurnLeft();
        }
        else if (turnRight)
        {
            TurnRight();
        }
        else if (steeringAxis != 0f)
        {
            ResetSteeringAngle();
        }

        if (handbrake)
        {
            Handbrake();
        }
        else if (handbrakeReleased)
        {
            RecoverTraction();
        }

        // Замедление
        bool noInput = !throttle && !reverse && !handbrake;
        if (noInput && !deceleratingCar)
        {
            InvokeRepeating("DecelerateCar", 0f, 0.1f);
            deceleratingCar = true;
        }

        // Звук
        if (useSounds)
        {
            if (carEngineSound != null)
            {
                initialCarEngineSoundPitch = carEngineSound.pitch;
            }

            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else
        {
            if (carEngineSound != null) carEngineSound.Stop();
            if (tireScreechSound != null) tireScreechSound.Stop();
        }


        // Эффекты
        if (!useEffects)
        {
            RLWParticleSystem?.Stop();
            RRWParticleSystem?.Stop();
            if (RLWTireSkid != null) RLWTireSkid.emitting = false;
            if (RRWTireSkid != null) RRWTireSkid.emitting = false;
        }

        // Анимация колёс
        AnimateWheelMeshes();
    }

    public void TurnLeft()
    {
        steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        if (steeringAxis < -1f)
        {
            steeringAxis = -1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void TurnRight()
    {
        steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        if (steeringAxis > 1f)
        {
            steeringAxis = 1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void ResetSteeringAngle()
    {
        if (steeringAxis < 0f)
        {
            steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        }
        else if (steeringAxis > 0f)
        {
            steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        }
        if (Mathf.Abs(frontLeftCollider.steerAngle) < 1f)
        {
            steeringAxis = 0f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    void AnimateWheelMeshes()
    {
        try
        {
            Quaternion FLWRotation;
            Vector3 FLWPosition;
            frontLeftCollider.GetWorldPose(out FLWPosition, out FLWRotation);
            frontLeftMesh.transform.position = FLWPosition;
            frontLeftMesh.transform.rotation = FLWRotation;

            Quaternion FRWRotation;
            Vector3 FRWPosition;
            frontRightCollider.GetWorldPose(out FRWPosition, out FRWRotation);
            frontRightMesh.transform.position = FRWPosition;
            frontRightMesh.transform.rotation = FRWRotation;

            Quaternion RLWRotation;
            Vector3 RLWPosition;
            rearLeftCollider.GetWorldPose(out RLWPosition, out RLWRotation);
            rearLeftMesh.transform.position = RLWPosition;
            rearLeftMesh.transform.rotation = RLWRotation;

            Quaternion RRWRotation;
            Vector3 RRWPosition;
            rearRightCollider.GetWorldPose(out RRWPosition, out RRWRotation);
            rearRightMesh.transform.position = RRWPosition;
            rearRightMesh.transform.rotation = RRWRotation;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public void GoForward()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if (throttleAxis > 1f)
        {
            throttleAxis = 1f;
        }
        if (localVelocityZ < -1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.RoundToInt(carSpeed) < maxSpeed)
            {
                frontLeftCollider.brakeTorque = 0;
                frontRightCollider.brakeTorque = 0;
                rearLeftCollider.brakeTorque = 0;
                rearRightCollider.brakeTorque = 0;
                float driveTorqueMultiplier = (carDriveType == CarDriveType.AllWheelDrive) ? 50f : 250f;
                float torque = accelerationMultiplier * driveTorqueMultiplier * throttleAxis;

                switch (carDriveType)
                {
                    case CarDriveType.FrontWheelDrive:
                        frontLeftCollider.motorTorque = frontRightCollider.motorTorque = torque;
                        rearLeftCollider.motorTorque = rearRightCollider.motorTorque = 0;
                        //Logger.Log("Передний");
                        break;

                    case CarDriveType.RearWheelDrive:
                        frontLeftCollider.motorTorque = frontRightCollider.motorTorque = 0;
                        rearLeftCollider.motorTorque = rearRightCollider.motorTorque = torque;
                        //Logger.Log("Задний");
                        break;

                    case CarDriveType.AllWheelDrive:
                        frontLeftCollider.motorTorque = frontRightCollider.motorTorque =
                        rearLeftCollider.motorTorque = rearRightCollider.motorTorque = torque;
                        //Logger.Log("Полный");
                        break;
                }
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void GoReverse()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }

        throttleAxis = throttleAxis - (Time.deltaTime * 3f);
        if (throttleAxis < -1f)
        {
            throttleAxis = -1f;
        }
        if (localVelocityZ > 1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
            {
                float driveTorqueMultiplier = carDriveType switch
                {
                    CarDriveType.FrontWheelDrive => 200f,
                    CarDriveType.RearWheelDrive => 200f,
                    CarDriveType.AllWheelDrive => 50f,
                    _ => 50f
                };

                float torque = accelerationMultiplier * driveTorqueMultiplier * throttleAxis;

                frontLeftCollider.brakeTorque = frontRightCollider.brakeTorque =
                    rearLeftCollider.brakeTorque = rearRightCollider.brakeTorque = 0;

                switch (carDriveType)
                {
                    case CarDriveType.FrontWheelDrive:
                        frontLeftCollider.motorTorque = frontRightCollider.motorTorque = torque;
                        rearLeftCollider.motorTorque = rearRightCollider.motorTorque = 0;
                        break;

                    case CarDriveType.RearWheelDrive:
                        frontLeftCollider.motorTorque = frontRightCollider.motorTorque = 0;
                        rearLeftCollider.motorTorque = rearRightCollider.motorTorque = torque;
                        break;

                    case CarDriveType.AllWheelDrive:
                        frontLeftCollider.motorTorque = frontRightCollider.motorTorque =
                        rearLeftCollider.motorTorque = rearRightCollider.motorTorque = torque;
                        break;
                }
            }
            else
            {
                frontLeftCollider.motorTorque = frontRightCollider.motorTorque =
                rearLeftCollider.motorTorque = rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void ThrottleOff()
    {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    public void DecelerateCar()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f)
            {
                throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            }
            else if (throttleAxis < 0f)
            {
                throttleAxis = throttleAxis + (Time.deltaTime * 10f);
            }
            if (Mathf.Abs(throttleAxis) < 0.15f)
            {
                throttleAxis = 0f;
            }
        }
        carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * decelerationMultiplier)));
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
        if (carRigidbody.linearVelocity.magnitude < 0.25f)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    public void Brakes()
    {
        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;
    }

    public void Handbrake()
    {
        // Отменяем восстановление сцепления
        CancelInvoke(nameof(RecoverTraction));

        // Увеличиваем силу дрифта со временем
        driftingAxis += Time.deltaTime * 2f; // Ускоренное накопление
        driftingAxis = Mathf.Clamp(driftingAxis, 0.1f, 1f);

        // Определяем, достаточно ли боковой скорости для дрифта
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;

        // Применяем увеличенный extremumSlip для всех колёс
        float slipFL = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        float slipFR = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        float slipRL = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        float slipRR = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;

        // Устанавливаем значения фрикции
        FLwheelFriction.extremumSlip = slipFL;
        frontLeftCollider.sidewaysFriction = FLwheelFriction;

        FRwheelFriction.extremumSlip = slipFR;
        frontRightCollider.sidewaysFriction = FRwheelFriction;

        RLwheelFriction.extremumSlip = slipRL;
        rearLeftCollider.sidewaysFriction = RLwheelFriction;

        RRwheelFriction.extremumSlip = slipRR;
        rearRightCollider.sidewaysFriction = RRwheelFriction;

        // Флаг блокировки сцепления
        isTractionLocked = true;

        // Визуальные эффекты дрифта
        DriftCarPS();

        // Отладка
        Debug.Log($"[Handbrake] driftingAxis: {driftingAxis:F2}, SlipFL: {slipFL:F2}, IsDrifting: {isDrifting}");
    }

    public void DriftCarPS()
    {
        if (useEffects)
        {
            try
            {
                bool shouldPlay = isDrifting;
                bool shouldEmit = (isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f;

                switch (carDriveType)
                {
                    case CarDriveType.FrontWheelDrive:
                        if (FLWParticleSystem != null) HandleParticle(FLWParticleSystem, shouldPlay);
                        if (FRWParticleSystem != null) HandleParticle(FRWParticleSystem, shouldPlay);
                        if (FLWTireSkid != null) FLWTireSkid.emitting = shouldEmit;
                        if (FRWTireSkid != null) FRWTireSkid.emitting = shouldEmit;
                        break;

                    case CarDriveType.RearWheelDrive:
                        if (RLWParticleSystem != null) HandleParticle(RLWParticleSystem, shouldPlay);
                        if (RRWParticleSystem != null) HandleParticle(RRWParticleSystem, shouldPlay);
                        if (RLWTireSkid != null) RLWTireSkid.emitting = shouldEmit;
                        if (RRWTireSkid != null) RRWTireSkid.emitting = shouldEmit;
                        break;

                    case CarDriveType.AllWheelDrive:
                        if (FLWParticleSystem != null) HandleParticle(FLWParticleSystem, shouldPlay);
                        if (FRWParticleSystem != null) HandleParticle(FRWParticleSystem, shouldPlay);
                        if (RLWParticleSystem != null) HandleParticle(RLWParticleSystem, shouldPlay);
                        if (RRWParticleSystem != null) HandleParticle(RRWParticleSystem, shouldPlay);

                        if (FLWTireSkid != null) FLWTireSkid.emitting = shouldEmit;
                        if (FRWTireSkid != null) FRWTireSkid.emitting = shouldEmit;
                        if (RLWTireSkid != null) RLWTireSkid.emitting = shouldEmit;
                        if (RRWTireSkid != null) RRWTireSkid.emitting = shouldEmit;
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else if (!useEffects)
        {
            var particleSystems = new[] { RLWParticleSystem, RRWParticleSystem, FLWParticleSystem, FRWParticleSystem };
            var tireSkids = new[] { RLWTireSkid, RRWTireSkid, FLWTireSkid, FRWTireSkid };

            foreach (var ps in particleSystems) ps?.Stop();
            foreach (var skid in tireSkids) if (skid != null) skid.emitting = false;
        }
    }

    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f)
        {
            driftingAxis = 0f;
        }

        if (FLwheelFriction.extremumSlip > FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            Invoke("RecoverTraction", Time.deltaTime);

        }
        else if (FLwheelFriction.extremumSlip < FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            driftingAxis = 0f;
        }
    }

    private void HandleParticle(ParticleSystem ps, bool shouldPlay)
    {
        if (shouldPlay)
            ps.Play();
        else
            ps.Stop();
    }
}

public enum CarDriveType
{
    FrontWheelDrive,
    RearWheelDrive,
    AllWheelDrive
}
