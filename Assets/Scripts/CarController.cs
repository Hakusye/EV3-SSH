using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}
public class CarController : MonoBehaviour {
    //GameObject ConnectSsh;
    public static SshClient ssh;
    public List<AxleInfo> axleInfos; 
    private float maxMotorTorque;
    private float feasibleTime; // [s]
    private float ElapsedTime;
    public float leftMotor;
    public float rightMotor;
    public string crossKeyInput;
    public Rigidbody rigid;
    public float distance2wall;

    public void Start() {
        maxMotorTorque = 300;
        feasibleTime = 1.0f;
        ElapsedTime = 0.0f;
        //ConnectSsh = GameObject.Find("sshScript");
        crossKeyInput = "stop";

        Debug.Log("sshStart");
        try {
            var hostNameOrIpAddr = "*********"; //EV3のIPアドレスを入れる
            var portNo = 22;
            var userName = "robot";
            var passWord = "maker";

            // コネクション情報
            ConnectionInfo info = new ConnectionInfo(hostNameOrIpAddr, portNo, userName,
                new AuthenticationMethod[] {
                    new PasswordAuthenticationMethod(userName, passWord)
                    }
            );
            ssh = new SshClient(info);
            ssh.Connect();
            if(ssh.IsConnected) {
                Debug.Log("[OK] SSH Connection succeeded!!");
            } else {
                Debug.Log("[NG] SSH Connection failed!!");
                return;
            }
        }
        catch(Exception ex) {   
            Debug.Log(ex);
            throw ex;
        }
    }
    
public float SshRun(string commandString) {
    Debug.Log(commandString);
    SshCommand cmd = ssh.CreateCommand(commandString);
    cmd.Execute();
    var stdOut = float.Parse(cmd.Result);
    string stdErr = cmd.Error;
    return stdOut;
}

    public void MoveEV3 (string direction_path) {
        string root_path = "./ev3dev-lang-cpp/build/demos/";
        string run_path = root_path + direction_path;
        Task.Run( () => {
            //ConnectSsh.GetComponent<RemoteServerConnect>().SshRun( run_path );
            distance2wall = SshRun(run_path); //4方向への移動後は距離データを渡す
            crossKeyInput = "stop";
        });
    }
    
    public void ChangeMotor() {
        if( crossKeyInput == "left" ) {
            leftMotor = -3 * maxMotorTorque;
            rightMotor = 3 * maxMotorTorque;
        }
        else if( crossKeyInput == "right" ) {
            leftMotor = 3 * maxMotorTorque;
            rightMotor = -3 * maxMotorTorque;
        }
        else if( crossKeyInput == "back" ) {
            leftMotor = maxMotorTorque;
            rightMotor = maxMotorTorque;
        }
        else if( crossKeyInput == "forward" ) {
            leftMotor = -maxMotorTorque;
            rightMotor = -maxMotorTorque;
        }else {
            leftMotor = 0;
            rightMotor = 0;
        }
        if(crossKeyInput != "stop") {
            MoveEV3(crossKeyInput);
            Debug.Log(distance2wall);
        } 
    }

    public void CrossKeyController() {
        if(Input.GetKeyDown(KeyCode.LeftArrow)) crossKeyInput = "left";
        if(Input.GetKeyDown(KeyCode.RightArrow)) crossKeyInput = "right";
        if(Input.GetKeyDown(KeyCode.UpArrow)) crossKeyInput = "back";
        if(Input.GetKeyDown(KeyCode.DownArrow)) crossKeyInput = "forward";        
    }

    public void OutputMotor( ) {
        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = leftMotor;
                axleInfo.rightWheel.motorTorque = rightMotor;
            }
        }
    }

    public void BrakeMotor( ) {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    public void FixedUpdate() {
        ElapsedTime += Time.deltaTime;
        if(ElapsedTime > feasibleTime) {
            BrakeMotor( );
            ChangeMotor( );
            ElapsedTime += Time.deltaTime;
            ElapsedTime = 0;
        } else {
            CrossKeyController( );
            OutputMotor( );
        }
    }
}