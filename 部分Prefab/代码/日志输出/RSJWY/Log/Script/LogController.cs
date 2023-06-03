using log4net.Repository.Hierarchy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RSJWY.Log
{
    /// <summary>
    /// 日志管理器，请不要多继承本类，用制作好的预制体！！
    /// </summary>
    public class LogController : MonoBehaviour
    {
        private void Awake()
        {
            Logger.Instance.Init();
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            Logger.Instance.Close();
        }

      
    }
}

