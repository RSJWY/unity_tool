 public IEnumerator Upload(Texture2D img, string imgName)
    {
        if (token=="")
        {
            LogShow.Inst.SetText("token不存在！！拒绝上传");
            Debug.LogError("token不存在！！拒绝上传");
            yield break;
        }
        if (url == "")
        {
            LogShow.Inst.SetText("Url不存在！！拒绝上传");
            Debug.LogError("Url不存在！！拒绝上传");
            yield break;
        }

        WWWForm formimg = new WWWForm();
        JObject jArray = null;

        //请求表设置
        //formimg.AddField("Authorization", "Bearer " + token);
        formimg.AddField("Accept", "application/json");
        formimg.AddField("Content-Type", "multipart/form-data");
        //添加图片
        formimg.AddBinaryData("file", img.EncodeToPNG(), imgName, "image/png");
        //执行上传  
        using (UnityWebRequest uwrimg = UnityWebRequest.Post(url + "/upload", formimg))
        {
            /*无进度
            uwrimg.SetRequestHeader("Authorization", "Bearer " + token);//添加秘钥
            uwrimg.downloadHandler = new DownloadHandlerBuffer();//保存返回数据
            Invoke(nameof(ResetPhotograph), 60f);//60秒内未上传成功，返回拍照模式
            yield return uwrimg.SendWebRequest();//等待上传完成
            CancelInvoke();//完成则取消计时器
            //确认上传是否完成
            if (uwrimg.result == UnityWebRequest.Result.ProtocolError || uwrimg.result == UnityWebRequest.Result.ProtocolError)
            {
                //发生错误
                Debug.Log(uwrimg.error);
                LogShow.Inst.SetText(uwrimg.error);//提示错误码
                yield return new WaitForSeconds(1);
                Msg.inst.Send(PhotographEnum.ExitQRCode);//重新拍照
                yield break;
            }
            else
            {
                //解析获取的数据
                Debug.Log(uwrimg.downloadHandler.text);
                jArray = JsonConvert.DeserializeObject<JObject>(uwrimg.downloadHandler.text);
            }

            if (jArray["status"].ToString() == "True" || jArray["status"].ToString() == "true")
            {
                //上传成功，获取链接
                string QRCodeUrl= jArray["data"]["links"]["url"].ToString();
                Debug.Log(QRCodeUrl);
                LogShow.Inst.SetText(jArray["message"].ToString());
                yield return new WaitForSeconds(0.5f);
                ShowQRCode.Inst.SetQRCode(QRCodeUrl);
            }
            else
            {
                //上传失败
                Debug.Log("上传超时");
                LogShow.Inst.SetText(UnicodeToString(jArray["message"].ToString()));//显示提示信息
                yield return new WaitForSeconds(1);
                Msg.inst.Send(PhotographEnum.ExitQRCode);//重新拍照
            }
            */
            uwrimg.SetRequestHeader("Authorization", "Bearer " + token);//添加秘钥
            uwrimg.downloadHandler = new DownloadHandlerBuffer();//保存返回数据
            Invoke(nameof(ResetPhotograph), 60f);//60秒内未上传成功，返回拍照模式
            StartCoroutine(UploadProgress(uwrimg));//启动进度显示
            yield return uwrimg.SendWebRequest();//等待上传完成
            CancelInvoke();//完成则取消计时器
            //确认上传是否完成
            if (uwrimg.result == UnityWebRequest.Result.ProtocolError || uwrimg.result == UnityWebRequest.Result.ProtocolError)
            {
                //发生错误
                Debug.LogWarning(string.Format("上传发生错误，错误代码：{0}", uwrimg.error));
                LogShow.Inst.SetText(uwrimg.error);//提示错误码
                yield return new WaitForSeconds(3f);
                Msg.inst.Send(PhotographEnum.ExitQRCode);//重新拍照
                yield break;
            }
            else
            {
                //解析获取的数据
                Debug.Log(string.Format("接收到服务器返回的数据：{0}", uwrimg.downloadHandler.text));
                jArray = JsonConvert.DeserializeObject<JObject>(uwrimg.downloadHandler.text);
            }

            if (jArray["status"].ToString() == "True" || jArray["status"].ToString() == "true")
            {
                //上传成功，获取链接
                string QRCodeUrl = jArray["data"]["links"]["url"].ToString();
                Debug.Log(string.Format("上传成功！！解析直链链接：{0}", uwrimg.downloadHandler.text));
                LogShow.Inst.SetText(jArray["message"].ToString());//显示上传成功
                yield return new WaitForSeconds(0.5f);
                ShowQRCode.Inst.SetQRCode(QRCodeUrl);
            }
            else
            {
                //上传失败
                Debug.LogWarning(string.Format("上传失败！！失败信息：{0}", jArray["message"].ToString()));
                LogShow.Inst.SetText(jArray["message"].ToString());//显示提示信息
                yield return new WaitForSeconds(3f);
                Msg.inst.Send(PhotographEnum.ExitQRCode);//重新拍照
            }
        }
    }
    IEnumerator GetToken()
    {
        JObject jArray = null;
        //获取
        //创建请求体
        WWWForm form = new WWWForm();
        form.AddField("email", "1446176202@qq.com");
        form.AddField("password", "HanLan1446176202");
        //获取token
        using (UnityWebRequest uwr = UnityWebRequest.Post(url + "/tokens", form))
        {
            uwr.SetRequestHeader("Accept", "application/json");
            //uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = new DownloadHandlerBuffer();

            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                Debug.Log(uwr.downloadHandler.text);
                jArray = JsonConvert.DeserializeObject<JObject>(uwr.downloadHandler.text);
            }

            if (jArray["status"].ToString() == "True" || jArray["status"].ToString() == "true")
            {
                string token = jArray["data"]["token"].ToString();
            }

        }
    }