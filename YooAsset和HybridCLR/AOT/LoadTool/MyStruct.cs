using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class MyStruct 
{
   
}
[System.Serializable]
public struct Progressbar
{
    public Image jindutiao;
    public Text baifenbi;

    public void Update(string nowname,float num){
        Updatejindu(num);
    }
    public void Reset() {
        jindutiao.fillAmount=0;
        baifenbi.text="--%";
    }
    public void Updatejindu(float num){
        if(num<=0){
            baifenbi.text=$"{0}%";
            jindutiao.fillAmount=0;
        }
        else if(num>=1){
            baifenbi.text=$"{100}%";
            jindutiao.fillAmount=1;
        }
        else{
            baifenbi.text=$"{(int)num*100}%";
            jindutiao.fillAmount=num;
        }
    }
}