namespace Mapbox.Editor {

	using System;
	using System.Collections.Generic;
	using UnityEngine;

	[Serializable]
	public class TreeElement {

		[SerializeField] private int m_ID;
		[SerializeField] private string m_Name;
		[SerializeField] private int m_Depth;
		[NonSerialized] private TreeElement m_Parent;
		[NonSerialized] private List<TreeElement> m_Children;

		public int depth
		{
			get => m_Depth;
			set => m_Depth = value;
		}

		public TreeElement parent
		{
			get => m_Parent;
			set => m_Parent = value;
		}

		public List<TreeElement> children
		{
			get => m_Children;
			set => m_Children = value;
		}

		public bool hasChildren => children != null && children.Count > 0;

		public string name
		{
			get => m_Name;
			set => m_Name = value;
		}

		public int id
		{
			get => m_ID;
			set => m_ID = value;
		}

		public TreeElement() { }

		public TreeElement(string name, int depth, int id) {
			m_Name = name;
			m_ID = id;
			m_Depth = depth;
		}

	}

}