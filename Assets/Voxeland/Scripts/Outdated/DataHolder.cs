using UnityEngine;
using System.Collections;

namespace Voxeland {

	public class DataHolder : MonoBehaviour 
	{
		public IntervalTree data;
		//public VoxelandOctree data;
		
		static public DataHolder Create (VoxelandTerrain land)
		{
			GameObject hlObj = new GameObject("DataHolder");
			hlObj.transform.parent = land.transform;
			//hlObj.transform.hideFlags=HideFlags.HideInHierarchy;
			hlObj.transform.localPosition = new Vector3(0,0,0);
			hlObj.transform.localScale = new Vector3(1,1,1);
			if (land.hideChunks) hlObj.transform.hideFlags = HideFlags.HideInHierarchy;
			
			return hlObj.AddComponent<DataHolder>();
		}
	}

}//namespace