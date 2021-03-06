﻿using System.Collections.Generic;
using AtomosZ.RPG.Actors.Battle;
using AtomosZ.UI;
using UnityEngine;
using UnityEngine.Events;

namespace AtomosZ.RPG.UI.Battle
{
	/// <summary>
	/// A single target action.
	/// </summary>
	public class ChooseTargetSelectionItem : ISelectionItem
	{
		public string name;
		public UnityAction<GameObject> onTargetSelect;
		public TargetType targetType;


		public ChooseTargetSelectionItem(string name, TargetType targetType, UnityAction<GameObject> onTargetSelect)
		{
			this.name = name;
			this.targetType = targetType;
			this.onTargetSelect = onTargetSelect;
		}

		public string GetName()
		{
			return name;
		}

		public ISelectionType GetSelectionType()
		{
			return ISelectionType.ItemSelected;
		}
	}

	public class ChooseTargetsSeletionItem<T> : ISelectionItem
		where T : List<GameObject>
	{
		public T targetList;


		public string GetName()
		{
			throw new System.NotImplementedException();
		}

		public ISelectionType GetSelectionType()
		{
			throw new System.NotImplementedException();
		}
	}
}
