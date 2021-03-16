using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class CarAgent : Agent
{
    public Transform Wall;
    public Transform ParkingSwitch;
    public List<AxleInfo> axleInfos; 
    public bool useVecObs;
    private float maxMotorTorque;
    private float leftMotor;
    private float rightMotor;
    private int feasibleTime;
    private int ElapsedTime;
    private int Count;
    private const float NOTHING = 1000f;    // 計測不能
    private const float maxDistance = 30f;      // 計測可能な最大距離
    private Rigidbody AgentRb;
    //private DistanceSensor distanceSensor; //距離センサーのスクリプト
    private TouchSensor touchSensor;


    public override void Initialize() {
        AgentRb = GetComponent<Rigidbody>();
        touchSensor = ParkingSwitch.GetComponent<TouchSensor>();
        //distanceSensor = GetComponent<DistanceSensor>();
        maxMotorTorque = 300.0f;
        feasibleTime = 30;
        ElapsedTime = 1;
        leftMotor = 0.0f;
        rightMotor = 0.0f;
        SetEV3(); // 記述なしでも良さそう
        SetWall();
    }

    public override void OnEpisodeBegin( ) { //壁の再設置
        SetEV3();
        SetWall();
        Count = 0;
        //Debug.Log(touchSensor.parking);
    }

    public override void CollectObservations( VectorSensor sensor ) { //センサからの判定
        float distance_ev3towall = DistanceSensor();
        //Debug.Log(distance_ev3towall);
        sensor.AddObservation(distance_ev3towall);
        //sensor.AddObservation();
    }
 
    public override void OnActionReceived( ActionBuffers actionBuffers ) {
        ElapsedTime++;
        Count++;
        //Debug.Log(ElapsedTime);
        if(ElapsedTime > feasibleTime) {
            BrakeMotor( );
            ChangeMotor( actionBuffers );
            ElapsedTime = 0;
        } else {
            //CrossKeyController( ); //SShで使うやつ
            OutputMotor( );
        }
        //if(!useVecObs) return;
        //時間でペナルティ。到着で報酬
        SetReward(-0.0001f);
        float distance_sensor = DistanceSensor();
        if(distance_sensor < 10) SetReward(0.01f/distance_sensor);
        if(touchSensor.parking) {
            touchSensor.parking = false;
            SetReward(3.0f);
            EndEpisode();
        }
        if(JudgeDistance() > 15.0f || Count > 10000) {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if(Input.GetAxis("Horizontal") < -0.3) discreteActionsOut[0] = 0;
        else if(Input.GetAxis("Horizontal") > 0.3) discreteActionsOut[0] = 1;
        else if(Input.GetAxis("Vertical") < -0.3) discreteActionsOut[0] = 2;
        if(Input.GetAxis("Vertical") > 0.3) discreteActionsOut[0] = 3;
    }

    public void BrakeMotor( ) {
        AgentRb.velocity = Vector3.zero;
        AgentRb.angularVelocity = Vector3.zero;
    }

    public void OutputMotor( ) {
        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = leftMotor;
                axleInfo.rightWheel.motorTorque = rightMotor;
            }
        }
    }

    public void ChangeMotor( ActionBuffers actionBuffers ) {
        if(actionBuffers.DiscreteActions[0] == 0) { //左
            leftMotor = -maxMotorTorque;
            rightMotor = maxMotorTorque;
        } else if(actionBuffers.DiscreteActions[0] == 1) { //右
            leftMotor = maxMotorTorque;
            rightMotor = -maxMotorTorque;
        } else if(actionBuffers.DiscreteActions[0] == 2) { //後
            leftMotor = maxMotorTorque;
            rightMotor = maxMotorTorque;
            /*
        } else if(actionBuffers.DiscreteActions[0] == 3) { // 前
           leftMotor = -maxMotorTorque;
            rightMotor = -maxMotorTorque;
            */
        } else { //止
            leftMotor = 0;
            rightMotor = 0;
        }
    }

    // 距離計測
    public float DistanceSensor() {
        // 前方ベクトル計算
        float distance;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        // 距離計算
        RaycastHit hit;
        if ( Physics.Raycast(transform.position, fwd, out hit, maxDistance) ) {
            distance = hit.distance;
        } else {
            distance = NOTHING;
        }
        return distance;
    }

    public float JudgeDistance() {
        return Vector3.Distance( Wall.position, this.transform.position );
    }

    public void SetWall() { //EV3から見える範囲に壁を置きたい
        Wall.position = this.transform.position 
                    + new Vector3( Random.Range( -7, 7 ), 0, Random.Range( -7, 7 ) );
    }

    public void SetEV3() {
        this.transform.position = new Vector3( 0.0f, -0.5f, 0.0f);
        this.transform.rotation = Quaternion.Euler(0.0f,0.0f,0.0f);
    }
}