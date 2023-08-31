public IEnumerator Upload(Texture2D img, string imgName)
    {
        WWWForm formimg = new WWWForm();

        //uwrimg.SetRequestHeader("Authorization", "Bearer " + token);
        formimg.AddField("Accept", "application/json");
        formimg.AddField("Content-Type", "multipart/form-data");
        formimg.AddBinaryData("file", img.EncodeToPNG(), imgName, "image/png");
        using (UnityWebRequest uwrimg = UnityWebRequest.Post(url + "/upload", formimg))
        {
            uwrimg.downloadHandler = new DownloadHandlerBuffer();

            yield return uwrimg.SendWebRequest();
            if (uwrimg.result == UnityWebRequest.Result.ProtocolError || uwrimg.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(uwrimg.error);
            }
            else
            {
                Debug.Log(uwrimg.downloadHandler.text);
            }
        }
    }