using System;
using System.Threading;
using Renci.SshNet;
using UnityEngine;
public class RemoteServerConnect : MonoBehaviour {
    //public static SshClient ssh;
    /*
    void Start() {
        Debug.Log("sshStart");
        try {
            var hostNameOrIpAddr = "**********";
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

    public void SshRun(string commandString) {
        //commandString = "./ev3dev-lang-cpp/build/demos/forward";
        Debug.Log(commandString);
        SshCommand cmd = ssh.CreateCommand(commandString);
        // コマンドを実行する
        //Console.WriteLine("[CMD] {0}", commandString);
        cmd.Execute();
        // 結果を変数に入れる
        //これで入出力処理できそう
        //var stdOut = cmd.Result;
        //var stdErr = cmd.Error;
        // 接続終了
    }
*/
    public void Quit() {
        //ssh.Disconnect();
        Debug.Log("ssh Disconnect");
        #if UNITY_EDITOR
          UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_STANDALONE
          UnityEngine.Application.Quit();
        #endif
    }

    void Update() {
        if (Input.GetKey(KeyCode.Escape)) Quit();
    }
}
