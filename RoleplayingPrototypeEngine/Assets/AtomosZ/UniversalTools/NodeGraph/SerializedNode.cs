using UnityEngine;

namespace AtomosZ.UniversalTools.NodeGraph.Schemas
{
	public interface ISerializedEntity
	{
		string GetGUID();
		object GetData();
	}

	[System.Serializable]
	public class SerializedNode<T> : ISerializedEntity
	{
		/// <summary>
		/// A personal identifier for a node. Used in editor for deletion.
		/// Only needs to be unique to the script it belongs to.
		/// </summary>
		public string GUID;
		/// <summary>
		/// This is how we get around JsonUtility not being able to deal with inheritance.
		/// It may be easier in the long run just to write own json serialization instead of
		/// using basic, but incredibly handy, JsonUtility.FromJson() and JsonUtility.ToJson().
		/// Also this creates a bunch of ugly generic classes.
		/// </summary>
		public T data;

		//					//
		// Editor variables	//
		//					//

		/// <summary>
		/// Position in graph. Editor Only.
		/// </summary>
		public Vector2 position;


		public string GetGUID()
		{
			return GUID;
		}

		public object GetData()
		{
			return data;
		}
	}
}