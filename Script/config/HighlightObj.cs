using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物体高亮
/// </summary>
public class HighlightObj : MonoBehaviour
{
    Material mat;//高亮材质球
    GameObject mat_root;//材质物体

    //public GameObject higlig_obj
    //{
    //    get
    //    {
    //        return mat_root;
    //    }
    //}
    private void Awake()
    {
        mat = Resources.Load<Material>("Material/HighlightGreen");//加载材质球
        if (mat_root == null)
        {
            mat_root = new GameObject("mat_root");//新建总物体
            mat_root.transform.SetParent(transform);

            mat_root.transform.localPosition = Vector3.zero;
            mat_root.transform.localEulerAngles = Vector3.zero;
        }
    }
    /// <summary>
    /// 设置高亮状态
    /// </summary>
    /// <param name="isHL">布尔值，默认为fasle</param>
    public void SetShowHL(bool isHL=false)
    {
        mat_root.SetActive(isHL);
    }
    // Start is called before the first frame update
    void Start()
    {
        SetHigLight();
    }

    void SetHigLight()
    {
        //基础物体
        MeshFilter[] _mfarr = transform.GetComponentsInChildren<MeshFilter>();
        if (_mfarr != null)
        {
            //数组转list
            List<MeshFilter> _mf = new List<MeshFilter>(_mfarr);
            GameObject _mf_obj;
            for (int i = 0; i < _mf.Count; i++)
            {
                _mf_obj = new GameObject(_mf[i].gameObject.name);//名字同步
                _mf_obj.transform.SetParent(mat_root.transform);
                _mf_obj.AddComponent<MeshFilter>().mesh = _mf[i].mesh;//复制一份材质球
                _mf_obj.AddComponent<MeshRenderer>().material = mat;//增加发光材质

                _mf_obj.transform.position = _mf[i].transform.position;
                _mf_obj.transform.rotation = _mf[i].transform.rotation;

                if (_mf.Count == 1)
                {
                    //存在当前物体没有子物体时，如果scale被修改，需要同步状态
                    _mf_obj.transform.localScale = _mf[i].transform.localScale;
                }
                else
                {
                    if (_mf[i].gameObject == gameObject)//如果当前材质物体是挂载在当前物体上
                    {
                        _mf_obj.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        _mf_obj.transform.localScale = _mf[i].transform.localScale;
                    }
                }
               
            }
        }
        //特殊物体
        SkinnedMeshRenderer[] _smrarr = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (_smrarr != null)
        {
            //数组转list
            List<SkinnedMeshRenderer> _smf = new List<SkinnedMeshRenderer>(_smrarr);
            GameObject _smr_obj;
            for (int i = 0; i < _smf.Count; i++)
            {
                _smr_obj = new GameObject(_smf[i].gameObject.name);//名字同步
                _smr_obj.transform.SetParent(mat_root.transform);
                //赋材质
                SkinnedMeshRenderer _smr = _smr_obj.AddComponent<SkinnedMeshRenderer>();
                _smr.sharedMesh = _smf[i].sharedMesh;
                _smr.rootBone = _smf[i].rootBone;
                _smr.bones = _smf[i].bones;
                //多材质球
                Material[] _temp = new Material[_smrarr[i].materials.Length];
                for (int j = 0; j < _temp.Length; j++)
                {
                    _temp[j] = mat;
                }
                _smr.materials = _temp;

                _smr_obj.transform.position = _smf[i].transform.position;
                _smr_obj.transform.rotation = _smf[i].transform.rotation;
                if (_smf[i].gameObject == gameObject)//如果当前材质物体是挂载在当前物体上
                {
                    _smr_obj.transform.localScale = Vector3.one;
                }
                else
                {
                    _smr_obj.transform.localScale = _smf[i].transform.localScale;
                }
            }
        }

        mat_root.SetActive(false);

    }
    // Update is called once per frame
    void Update()
    {

    }
}

