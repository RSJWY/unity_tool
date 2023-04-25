public class UpdateCatalogAndAssets : MonoBehaviour
{
	List<object> cusKeys = new List<object>();
	private void Start()
	{
		checkUpdate(() => { print("资源加载完事了，剩下的正常使用即可")});
        //加载资源的几个API
        //
	  AsyncOperationHandle res = Addressables.InstantiateAsync("xxx");
		AsyncOperationHandle res2 = Addressables.InstantiateAsync("xxx");
		Addressables.LoadAssetAsync<Sprite>("xxx");
    Addressables.Release(res);
	}
 
	void checkUpdate(System.Action pFinish)
	{
		StartCoroutine(Initialize(() =>{
			StartCoroutine(checkUpdateSize((oSize, oList) =>{
				if (oList.Count > 0){
					 StartCoroutine(DoUpdate(oList, () =>{
						pFinish();
					}));
				}
				else
				{
					pFinish();
				}
			}));
		}));
	}
	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="pOnFinish"></param>
	/// <returns></returns>
	IEnumerator Initialize(System.Action pOnFinish)
	{
		//初始化Addressable
		var init = Addressables.InitializeAsync();
		yield return init;
		//Caching.ClearCache();
		// Addressables.ClearResourceLocators();
		pOnFinish.Invoke();
	}
	/// <summary>
	/// 检查更新文件大小
	/// </summary>
	/// <param name="pOnFinish"></param>
	/// <returns></returns>
	IEnumerator checkUpdateSize(System.Action<long, List<string>> pOnFinish)
	{
		//Debug.LogError(" checkUpdateSize >>>");
		long sizeLong = 0;
		List<string> catalogs = new List<string>();
		AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);
		yield return checkHandle;
		if (checkHandle.Status == AsyncOperationStatus.Succeeded)
		{
			catalogs = checkHandle.Result;
		}
		/*IEnumerable<IResourceLocator> locators = Addressables.ResourceLocators;
		List<object> keys = new List<object>();
		//暴力遍历所有的key
		foreach (var locator in locators)
		{
			foreach (var key in locator.Keys)
			{
				keys.Add(key);
			}
		}*/
		pOnFinish.Invoke(sizeLong, catalogs);
	}
	/// <summary>
	/// 下载更新逻辑
	/// </summary>
	/// <param name="catalogs"></param>
	/// <param name="pOnFinish"></param>
	/// <returns></returns>
	IEnumerator DoUpdate(List<string> catalogs, System.Action pOnFinish)
	{
		//Debug.LogError(" DocatalogUpdate >>>");
		var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
		yield return updateHandle;
		foreach (var item in updateHandle.Result)
		{
			cusKeys.AddRange(item.Keys);
		}
		Addressables.Release(updateHandle);
		StartCoroutine(DownAssetImpl(pOnFinish));
	}
	public IEnumerator DownAssetImpl(Action pOnFinish)
	{
		var downloadsize = Addressables.GetDownloadSizeAsync(cusKeys);
		yield return downloadsize;
		Debug.Log("start download size :" + downloadsize.Result);
 
		if (downloadsize.Result > 0)
		{
			var download = Addressables.DownloadDependenciesAsync(cusKeys, Addressables.MergeMode.Union);
			yield return download;
 
			//await download.Task;
			Debug.Log("download result type " + download.Result.GetType());
			foreach (var item in download.Result as List<UnityEngine.ResourceManagement.ResourceProviders.IAssetBundleResource>)
			{
 
				var ab = item.GetAssetBundle();
				Debug.Log("ab name " + ab.name);
				foreach (var name in ab.GetAllAssetNames())
				{
					Debug.Log("asset name " + name);
				}
			}
			Addressables.Release(download);
		}
		Addressables.Release(downloadsize);
		pOnFinish?.Invoke();
	}
}
-----------------------------------
©著作权归作者所有：来自51CTO博客作者洞悉ONE的原创作品，请联系作者获取转载授权，否则将追究法律责任
Unity3D之资源管理——Addressables管理详解
https://blog.51cto.com/myselfdream/6180916