using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

     
public class Mapping : MonoBehaviour {
    //GameObject ConnectSsh;
    public static SshClient ssh;
    public List<AxleInfo> axleInfos; 
    public float maxMotorTorque;
    private float feasibleTime; // [s]
    private float ElapsedTime;
    public float leftMotor;
    public float rightMotor;
    public float distance2wall;
    public Rigidbody rigid;
    public GameObject Body;
    public GameObject Pole;
    public bool start1 = false;

    public void Start() {
        maxMotorTorque = 300;
        feasibleTime = 1.0f;
        ElapsedTime = 1.0f;
        //leftMotor = 3 * maxMotorTorque;
        //rightMotor = -3 * maxMotorTorque;
        leftMotor = -maxMotorTorque/2;
        rightMotor = -maxMotorTorque/2;
        //ConnectSsh = GameObject.Find("sshScript");
        Debug.Log("sshStart");
        try {
            var hostNameOrIpAddr = "192.168.11.4";
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
        } catch( Exception ex ) {   
            Debug.Log(ex);
            throw ex;
        }
    }
    
    public float SshRun(string commandString) {
        SshCommand cmd = ssh.CreateCommand(commandString);
        cmd.Execute();
        var stdOut = float.Parse(cmd.Result);
        string stdErr = cmd.Error;
        return stdOut;
    }

    public void MakeWall(float distance) {
        //Debug.Log(this.transform.position );
        //計算がうまく行ってなさそう
        if(distance < 10) return;
        Vector3 wallPosition = this.transform.forward * distance; //分母で縮小サイズ決めれる
        //Debug.Log(wallPosition);
        Instantiate (  Pole,
            this.transform.position + wallPosition, //相対位置
            Quaternion.identity );        
    }

    public async Task MoveEV3 () {
        string run_path = "./ev3dev-lang-cpp/build/demos/forward";
        await Task.Run( () => { distance2wall = SshRun( run_path ); });
        Debug.Log(distance2wall);
        MakeWall( distance2wall );
        start1 = true;
    }

    public void OutputMotor ( ) {
        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = leftMotor;
                axleInfo.rightWheel.motorTorque = rightMotor;
            }
        }
    }

    public void BrakeMotor ( ) {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    public void FixedUpdate ( ) {
        ElapsedTime += Time.deltaTime;
        if(ElapsedTime > feasibleTime) {
            MoveEV3( );
            //BrakeMotor( );
            ElapsedTime += Time.deltaTime;
            ElapsedTime = 0;
        }else{
            if(start1) OutputMotor();
        }
    }
}
