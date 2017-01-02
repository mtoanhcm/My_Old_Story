using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Helpers
{
	public class DictionaryHelper
	{
		//private static DictionaryHelper instance = new DictionaryHelper("Dic");
		//public static DictionaryHelper Instance
		//{
		//	get { return instance; }
		//	set { instance = value; }
		//}

		private HashSet<string> dics;
		public DictionaryHelper(string fileName)
		{	
			this.dics = new HashSet<string>();
			TextAsset textAsset = Resources.Load(fileName) as TextAsset;
			StreamReader file = new StreamReader(new MemoryStream(textAsset.bytes), Encoding.UTF8);

			string line = null;
			while ((line = file.ReadLine()) != null)
			{
				if (line != "")
				{
					this.dics.Add(line);
				}
				line = null;
			}

			file.Close();
		}

		public bool CheckText(string str)
		{
			return this.dics.Any (x => x == str);
		}
	}
}
